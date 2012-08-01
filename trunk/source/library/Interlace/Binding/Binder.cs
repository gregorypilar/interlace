#region Using Directives and Copyright Notice

// Copyright (c) 2007-2010, Computer Consultancy Pty Ltd
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the Computer Consultancy Pty Ltd nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL COMPUTER CONSULTANCY PTY LTD BE LIABLE 
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY 
// OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Interlace.Binding
{
    using AutoBinders;
    using ViewConverters;
    using Views;

    /// <summary>
    /// A binder connects the properties of a single object (the model) with a number of properties
    /// of other objects (the views).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A single binder handles a single one to many relationship between objects. The binder is attached 
    /// to one single object (the "bound-to" object) and can watch any number of properties. Each property
    /// is watched by a <see cref="BinderModel"/> instance, which can get and set the property and
    /// which subscribes to the change events of the property, if they exist. For details on how
    /// the event is found, see the <see cref="BinderModel"/> class documentation.
    /// </para>
    /// <para>
    /// The bound-to object can also be changed at runtime, and all views will be notified appropriately.
    /// Also, if the <see pref="BoundTo"/> property is set to null, views are notified and can
    /// change the screen to show that no object is bound. When the <see pref="BoundTo"/> property
    /// is modified, views are notified immediately.
    /// </para>
    /// <para>
    /// The <see cref="BinderController"/> connects the model instance and the multiple views that may be 
    /// attached to a property.
    /// </para>
    /// <para>
    /// There are several types of view, all derived from <see cref="BinderViewBase"/>. A view
    /// receives notifications when the model property it is connected to changes. Views may also
    /// send updates to the model if the objects connected to the view are modified; the
    /// binding can be read only, write only or bi-directional depending on the implementation
    /// of <see cref="BinderViewBase"/>.
    /// </para>
    /// </remarks>
    public class Binder
    {
        object _boundTo;
        string _autoBindViewPrefix = "";
        Dictionary<string, BinderController> _properties;

        static List<IAutoBindingFactory> _autoBindings;
        static List<IAutoConverterFactory> _autoConverters;

        static Binder()
        {
            _autoBindings = new List<IAutoBindingFactory>();
            _autoBindings.Add(new CheckBoxAutoBindingFactory());
            _autoBindings.Add(new TextBoxBaseAutoBindingFactory());
            _autoBindings.Add(new ComboBoxBaseAutoBindingFactory());

            _autoConverters = new List<IAutoConverterFactory>();
            _autoConverters.Add(new BasicAutoConverterFactory());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Binder"/> class, with no initial bound-to
        /// object.
        /// </summary>
        public Binder()
        {
            _boundTo = null;
            _properties = new Dictionary<string, BinderController>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Binder"/> class, with <paramref name="boundTo"/>
        /// as the initial bound-to object.
        /// </summary>
        /// <param name="boundTo">The bound to.</param>
        public Binder(object boundTo)
        {
            _boundTo = boundTo;
            _properties = new Dictionary<string, BinderController>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Binder"/> class, and creates 
        /// a binding on an underlying binder to update this binders BoundTo property.
        /// </summary>
        /// <param name="boundToBinder">The binder to create the binding on.</param>
        /// <param name="propertyName">Name of the property to bind to.</param>
        public Binder(Binder boundToBinder, string propertyName)
        : this()
        {
            boundToBinder.AddBinding(propertyName, new PropertyView(this, "BoundTo", null));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Binder"/> class, and creates 
        /// a binding on an underlying binder to update this binders BoundTo property.
        /// </summary>
        /// <param name="boundToBinder">The binder to create the binding on.</param>
        /// <param name="propertyName">Name of the property to bind to.</param>
        /// <param name="readOnly">A boolean indicating whether the binding should be bidirectional or read only.</param>
        public Binder(Binder boundToBinder, string propertyName, bool readOnly)
        : this()
        {
            boundToBinder.AddBinding(propertyName, new PropertyView(this, "BoundTo", null, readOnly));
        }

        /// <summary>
        /// The prefix that is removed from the name of an auto-bound control.
        /// </summary>
        /// <remarks>
        /// If multiple fields on a form are auto-bound to fields with the same name, the
        /// names of the controls will clash. Each binder can have a prefix set
        /// to avoid name clashes. The prefix must be set when the auto-binding is made; 
        /// changing the prefix after bindings are created has no effect on the previously
        /// created bindings.
        /// </remarks>
        public string AutoBindViewPrefix
        {
            get { return _autoBindViewPrefix; }
            set { _autoBindViewPrefix = value; }
        }

        private string FixAutoBindName(string name)
        {
            string underscorelessName = name;

            if (!string.IsNullOrEmpty(name) && name[0] == '_')
            {
                underscorelessName = name.Substring(1, 1).ToUpper() + name.Substring(2);
            }

            if (underscorelessName.ToLower().StartsWith(_autoBindViewPrefix.ToLower()))
            {
                return underscorelessName.Substring(_autoBindViewPrefix.Length);
            }
            else
            {
                return underscorelessName;
            }
        }

        /// <summary>
        /// Creates a binding, making inferences based on the name and type of the control.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Auto-binding attempts to use the name and type of the control to determine
        /// the field to bind to and the type of view to use.
        /// </para>
        /// <para>
        /// The field name is usually taken from the name of the control (after some simple
        /// transformations), but other behaviours are possible. The name is transformed with the 
        /// following steps:
        /// </para>
        /// <list type="bullet">
        /// <item><description>If present, the single leading underscore is stripped
        /// off. The character immediately following the underscore is converted to an 
        /// uppercase character. For example, "_name" would be changed to "Name".</description></item>
        /// <item><description>If set and present at the beginning of the control name, 
        /// the auto-bind prefix is removed from the name. For example, "PersonAge" would
        /// be changed to "Person".</description></item>
        /// </list>
        /// <para>Auto-binding makes no attempt to check the bound object for existance of the
        /// property. In most cases this is not possible; the type of the bound object is 
        /// not always known when bindings are created, since it is possible to be unbound
        /// or bound to different types of objects.</para>
        /// </remarks>
        /// <param name="viewer">The view control to be bound.</param>
        /// <returns>The newly bound view.</returns>
        public BinderViewBase AutoBind(object viewer)
        {
            return AutoBind(viewer, ViewConverterBase.Null);
        }

        public BinderViewBase AutoBind(object viewer, ViewConverterBase converter)
        {
            foreach (IAutoBindingFactory factory in _autoBindings)
            {
                if (factory.CanAutoBind(viewer))
                {
                    BinderViewBase view = factory.CreateView(viewer);
                    
                    AddBinding(FixAutoBindName(factory.GetAutoBindName(viewer)), view, converter);

                    return view;
                }
            }

            throw new InvalidOperationException("No suitable autobinder was found.");
        }

        public BinderViewBase AutoBind(object viewer, BinderHint hint)
        {
            foreach (IAutoBindingFactory factory in _autoBindings)
            {
                if (factory.CanAutoBind(viewer))
                {
                    BinderViewBase view = factory.CreateView(viewer);

                    // Find a converter:
                    ViewConverterBase converter = ViewConverterBase.Null;

                    foreach (IAutoConverterFactory converterFactory in _autoConverters)
                    {
                        if (converterFactory.CanProvideConverter(view, hint))
                        {
                            converter = converterFactory.ProvideConverter(view, hint);

                            break;
                        }

                        // (If a converter can't be provided, just use the exact one.)
                    }
                    
                    AddBinding(FixAutoBindName(factory.GetAutoBindName(viewer)), view, converter);

                    return view;
                }
            }

            throw new InvalidOperationException("No suitable autobinder was found.");
        }

        public void EnsureControllerExistsForProperty(string propertyName)
        {
            if (!_properties.ContainsKey(propertyName))
            {
                IBinderModel model;

                switch (propertyName)
                {
                    case "@Count":
                        model = new CountBinderModel();
                        break;

                    default:
                        model = new PropertyBinderModel(propertyName);
                        break;
                }

                _properties[propertyName] = new BinderController(model);
            }
        }

        public void SetTracing(string propertyName, bool enabled)
        {
            EnsureControllerExistsForProperty(propertyName);

            _properties[propertyName].TracingEnabled = enabled;
        }

        public void AddBinding(string propertyName, BinderViewBase view)
        {
            AddBinding(propertyName, view, ViewConverterBase.Null);
        }

        public void AddBinding(string propertyName, BinderViewBase view,
            ViewConverterBase converter)
        {
            view.Converter = converter;

            EnsureControllerExistsForProperty(propertyName);

            BinderController controller = _properties[propertyName];
            controller.AddView(view);

            if (_boundTo != null) controller.ConnectBoundToObject(_boundTo);
        }

        public event EventHandler BoundToChanged;

        public object BoundTo
        {
            get { return _boundTo; }
            set
            {
                _boundTo = value;

                if (_boundTo != null)
                {
                    foreach (BinderController controller in _properties.Values)
                    {
                        controller.ConnectBoundToObject(_boundTo);
                    }
                }
                else
                {
                    foreach (BinderController controller in _properties.Values)
                    {
                        controller.DisconnectBoundToObject();
                    }
                }

                if (BoundToChanged != null) BoundToChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets a list of all views, ordered to some logical (and stable) ordering.
        /// </summary>
        /// <remarks>
        /// Views are assigned a partial ordering based on (for real controls) the tab
        /// order. The first control in the ordering is picked first when error processing
        /// is deciding which control to complain about first.
        /// </remarks>
        /// <returns>An ordered list of all views.</returns>
        private List<BinderViewBase> GetOrderedViewsList()
        {
            List<BinderViewBase> views = new List<BinderViewBase>();

            foreach (BinderController controller in _properties.Values)
            {
                views.AddRange(controller.Views);
            }

            views.Sort((Comparison<BinderViewBase>)delegate(BinderViewBase lhs, BinderViewBase rhs)
            {
                return lhs.OrderingIndex.CompareTo(rhs.OrderingIndex);
            });

            return views;
        }

        /// <summary>
        /// Finds the first view that is in an error state, and returns an error handle from it.
        /// </summary>
        /// <returns>
        /// An error handle, or null if no error has occurred. 
        /// </returns>
        public BinderViewError FirstError
        {
            get 
            {
                List<BinderViewBase> views = GetOrderedViewsList();

                foreach (BinderViewBase view in views)
                {
                    BinderViewError error = view.FindFirstError();

                    if (error != null) return error;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether any views in the binder have errors.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if one or more errors are present; otherwise, <c>false</c>.
        /// </value>
        public bool HasErrors
        {
            get
            {
                return FirstError != null;
            }
        }
    }
}
