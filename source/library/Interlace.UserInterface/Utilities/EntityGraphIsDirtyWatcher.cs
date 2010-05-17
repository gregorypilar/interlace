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
using System.ComponentModel;
using System.Text;

using SD.LLBLGen.Pro.ORMSupportClasses;

#endregion

namespace Interlace.Utilities
{
    /// <summary>
    /// Binds to an entity and watches the IsDirty flag of the entity and all connected entities.
    /// </summary>
    /// <remarks>
    /// <para>The dirty watcher finds all related entities (even when cycles are present) of
    /// a root entity, following any number of relations. It then watches the entities
    /// to see if any have their IsDirty flag set, and any RemovedEntityTracker collections to
    /// see if they contain any entities.</para>
    /// </remarks>
    public class EntityGraphIsDirtyWatcher
    {
        IEntity2 _boundTo;
        IEnumerable<IEntity2> _boundToGraph;
        bool _cachedIsDirty;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityGraphIsDirtyWatcher"/> class.
        /// </summary>
        public EntityGraphIsDirtyWatcher()
        {
            _boundTo = null;
        }

        /// <summary>
        /// Gets or sets the root object of of object tree that is being watched.
        /// </summary>
        /// <value>The bound to root object.</value>
        public IEntity2 BoundTo
        {
            get { return _boundTo; }
            set
            {
                if (value == _boundTo) return;

                if (_boundTo != null)
                {
                    UnhookEvents();
                    _boundToGraph = null;
                }

                _boundTo = value;

                if (_boundTo != null)
                {
                    _boundToGraph = EntityGraphUtilities.GetAllEntitiesInGraph(_boundTo);
                    HookEvents();
                }

                if (BoundToChanged != null) BoundToChanged(this, EventArgs.Empty);

                UpdateIsDirty();
            }
        }

        /// <summary>
        /// Occurs when the bound to object is changed.
        /// </summary>
        public event EventHandler BoundToChanged;

        void HookEvents()
        {
            foreach (IEntity2 entity in _boundToGraph)
            {
                entity.EntityContentsChanged += new EventHandler(entity_EntityContentsChanged);
                entity.PropertyChanged += new PropertyChangedEventHandler(entity_PropertyChanged);
                entity.AfterSave += new EventHandler(entity_AfterSave);

                foreach (IEntityCollection2 collection in entity.GetMemberEntityCollections())
                {
                    collection.ListChanged += new ListChangedEventHandler(collection_ListChanged);

                    if (collection.RemovedEntitiesTracker != null)
                    {
                        collection.RemovedEntitiesTracker.ListChanged += 
                            new ListChangedEventHandler(RemovedEntitiesTracker_ListChanged);
                    }
                }
            }
        }

        void entity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (IEntityRelation relation in _boundTo.GetAllRelations())
            {
                if (relation.MappedFieldName == e.PropertyName)
                {
                    // If they've updated a 1:1 related entity, then the graph needs to
                    // be updated to make sure we're not watching the old entity
                    UnhookEvents();
                    _boundToGraph = EntityGraphUtilities.GetAllEntitiesInGraph(_boundTo);
                    HookEvents();

                    // We've updated the graph, so no need to keep looking through the relations.
                    break;
                }
            }

            UpdateIsDirty();
        }

        void RemovedEntitiesTracker_ListChanged(object sender, ListChangedEventArgs e)
        {
            UpdateIsDirty();
        }

        void collection_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType != ListChangedType.ItemChanged)
            {
                UnhookEvents();
                _boundToGraph = EntityGraphUtilities.GetAllEntitiesInGraph(_boundTo);
                HookEvents();

                UpdateIsDirty();
            }
        }

        void UnhookEvents()
        {
            foreach (IEntity2 entity in _boundToGraph)
            {
                foreach (IEntityCollection2 collection in entity.GetMemberEntityCollections())
                {
                    collection.ListChanged -= new ListChangedEventHandler(collection_ListChanged);

                    if (collection.RemovedEntitiesTracker != null)
                    {
                        collection.RemovedEntitiesTracker.ListChanged += 
                            new ListChangedEventHandler(RemovedEntitiesTracker_ListChanged);
                    }
                }

                entity.EntityContentsChanged -= new EventHandler(entity_EntityContentsChanged);
                entity.PropertyChanged -= new PropertyChangedEventHandler(entity_PropertyChanged);
                entity.AfterSave -= new EventHandler(entity_AfterSave);
            }
        }

        void entity_EntityContentsChanged(object sender, EventArgs e)
        {
            UpdateIsDirty();
        }

        void entity_AfterSave(object sender, EventArgs e)
        {
            UpdateIsDirty();
        }

        void UpdateIsDirty()
        {
            _cachedIsDirty = false;

            if (_boundToGraph != null)
            {
                foreach (IEntity2 entity in _boundToGraph) 
                {
                    _cachedIsDirty = _cachedIsDirty || entity.IsDirty;

                    foreach (IEntityCollection2 collection in entity.GetMemberEntityCollections())
                    {
                        if (collection.RemovedEntitiesTracker != null)
                        {
                            _cachedIsDirty = _cachedIsDirty || (collection.RemovedEntitiesTracker.Count > 0);
                        }
                    }

                    // Optimization: If it's dirty, there's no reason to continue through the loop.  The
                    // graph is dirty.
                    if (_cachedIsDirty) break;
                }
            }

            if (IsDirtyChanged != null) IsDirtyChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets a value indicating whether any objects in the bound to object tree are dirty.
        /// </summary>
        /// <value><c>true</c> if there are dirty entities; otherwise, <c>false</c>.</value>
        public bool IsDirty
        {
            get { return _cachedIsDirty; }
        }

        /// <summary>
        /// Occurs when the IsDirty value changes.
        /// </summary>
        public event EventHandler IsDirtyChanged;
    }
}
