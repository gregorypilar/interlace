#region Using Directives and Copyright Notice

// Copyright (c) 2010, Bit Plantation
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the Bit Plantation nor the
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
using System.ComponentModel;
using System.Collections;

#endregion

namespace Interlace.Collections
{
    public abstract class SortedAdapterCollection<TAdapter, TValue, TKey> : IBindingList, IList<TAdapter> where TAdapter : class
    {
        SortedList<TKey, TAdapter> _list = new SortedList<TKey, TAdapter>();

        protected abstract TKey GetKeyFromValue(TValue value);
        protected abstract TKey GetKeyFromAdapter(TAdapter adapter);
        protected abstract TAdapter CreateAdapter(TValue value);
        protected abstract void UpdateAdapter(TValue value, TAdapter adapter);

        public void Merge(IList<TValue> source) 
        {
            // Build a dictionary of existing items (that we later mutate in a gross way):
            Dictionary<TKey, int> existingIndicies = new Dictionary<TKey, int>();

            int i = 0;

            foreach (TAdapter item in this)
            {
                existingIndicies[GetKeyFromAdapter(item)] = i;

                i++;
            }

    		// Add new entities, and update existing entities:
    		foreach (TValue sourceItem in source) 
            {
                int foundIndex;

                TKey sourceKey = GetKeyFromValue(sourceItem);

                bool found = existingIndicies.TryGetValue(sourceKey, out foundIndex);

                if (found)
                {
                    UpdateAdapter(sourceItem, _list.Values[foundIndex]);

                    existingIndicies.Remove(sourceKey);
                }
                else
                {
                    Add(sourceKey, CreateAdapter(sourceItem));
                }
            }

        	// Remove missing entities:
    		List<int> indiciesToRemove = new List<int>(existingIndicies.Values);

            indiciesToRemove.Sort();

            int indiciesRemoved = 0;

            foreach (int index in indiciesToRemove)
            {
                RemoveAt(index - indiciesRemoved);

                indiciesRemoved++;
            }
        }

        public void Update(IList<TValue> partialSource)
        {
            // Add new entities, and update existing entities:
            foreach (TValue sourceItem in partialSource)
            {
                TKey sourceKey = GetKeyFromValue(sourceItem);

                int foundIndex = _list.IndexOfKey(sourceKey);

                if (foundIndex != -1)
                {
                    UpdateAdapter(sourceItem, _list.Values[foundIndex]);
                }
                else
                {
                    Add(sourceKey, CreateAdapter(sourceItem));
                }
            }
        }

        public TAdapter FindByKey(TKey key)
        {
            int index = _list.IndexOfKey(key);

            if (index != -1)
            {
                return _list.Values[index];
            }
            else
            {
                return null;
            }
        }

        public event EventHandler<AdapterCollectionItemEventArgs<TAdapter>> ItemAdding;
        public event EventHandler<AdapterCollectionItemEventArgs<TAdapter>> ItemRemoving;

        void HandleItemAdding(TKey key, TAdapter adapter)
        {
            if (adapter is INotifyPropertyChanged)
            {
                INotifyPropertyChanged implementation = adapter as INotifyPropertyChanged;

                implementation.PropertyChanged += new PropertyChangedEventHandler(implementation_PropertyChanged);
            }

            if (ItemAdding != null) ItemAdding(this, new AdapterCollectionItemEventArgs<TAdapter>(adapter));
        }

        void HandleItemRemoving(TKey key, TAdapter adapter)
        {
            if (adapter is INotifyPropertyChanged)
            {
                INotifyPropertyChanged implementation = adapter as INotifyPropertyChanged;

                implementation.PropertyChanged -= new PropertyChangedEventHandler(implementation_PropertyChanged);
            }

            if (ItemAdding != null) ItemRemoving(this, new AdapterCollectionItemEventArgs<TAdapter>(adapter));
        }

        void implementation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ListChanged == null) return;

            TAdapter adapter = sender as TAdapter;

            if (adapter == null) return;

            TKey key = GetKeyFromAdapter(adapter);

            int index = _list.IndexOfKey(key);

            if (index == -1) return;

            ListChanged(this, new ListChangedEventArgs(ListChangedType.ItemChanged, index));
        }

        #region IBindingList Members

        public void AddIndex(PropertyDescriptor property)
        {
        }

        public object AddNew()
        {
            throw new InvalidOperationException();
        }

        public bool AllowEdit
        {
            get { return true; }
        }

        public bool AllowNew
        {
            get { return false; }
        }

        public bool AllowRemove
        {
            get { return true; }
        }

        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            throw new NotSupportedException();
        }

        public int Find(PropertyDescriptor property, object key)
        {
            throw new NotSupportedException();
        }

        public bool IsSorted
        {
            get { throw new NotSupportedException(); }
        }

        public void RemoveIndex(PropertyDescriptor property)
        {
        }

        public void RemoveSort()
        {
            throw new NotSupportedException();
        }

        public ListSortDirection SortDirection
        {
            get { throw new NotSupportedException(); }
        }

        public PropertyDescriptor SortProperty
        {
            get { throw new NotSupportedException(); }
        }

        public bool SupportsChangeNotification
        {
            get { return true; }
        }

        public bool SupportsSearching
        {
            get { return false; }
        }

        public bool SupportsSorting
        {
            get { return false; }
        }

        public event ListChangedEventHandler ListChanged;

        #endregion

        public void Add(TKey key, TAdapter adapter)
        {
            if (_list.ContainsKey(key))
            {
                throw new ArgumentException("An element with the same key already exists.", "value");
            }

            HandleItemAdding(key, adapter);

            _list.Add(key, adapter);

            if (ListChanged != null) ListChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        #region IList Members

        public int Add(object value)
        {
            TAdapter adapter = value as TAdapter;

            TKey key = GetKeyFromAdapter(adapter);

            Add(key, adapter);

            return _list.IndexOfKey(key);
        }

        public void Clear()
        {
            foreach (KeyValuePair<TKey, TAdapter> pair in _list)
            {
                HandleItemRemoving(pair.Key, pair.Value);
            }

            _list.Clear();

            if (ListChanged != null) ListChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        public bool Contains(object value)
        {
            return _list.ContainsValue(value as TAdapter);
        }

        public int IndexOf(object value)
        {
            return _list.IndexOfValue(value as TAdapter);
        }

        public void Insert(int index, object value)
        {
            Add(value);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            TAdapter adapter = value as TAdapter;

            if (adapter == null) 
            {
                throw new ArgumentException("An object of the adapter type was expected.", "value");
            }

            TKey key = GetKeyFromAdapter(adapter);

            if (!_list.ContainsKey(key)) return;

            HandleItemRemoving(key, adapter);

            _list.Remove(key);

            if (ListChanged != null) ListChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        public void RemoveAt(int index)
        {
            if (0 <= index && index < _list.Count)
            {
                HandleItemRemoving(_list.Keys[index], _list.Values[index]);
            }

            _list.RemoveAt(index);

            if (ListChanged != null) ListChanged(this, new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
        }

        public object this[int index]
        {
            get
            {
                return _list.Values[index];
            }
            set
            {
                TAdapter oldAdapter = _list.Values[index];
                TAdapter newAdapter = value as TAdapter;

                if (object.ReferenceEquals(oldAdapter, newAdapter)) return;

                if (newAdapter == null) 
                {
                    throw new ArgumentException("An object of the adapter type was expected.");
                }

                HandleItemRemoving(_list.Keys[index], oldAdapter);
                HandleItemAdding(_list.Keys[index], newAdapter);

                _list.Values[index] = newAdapter;

                if (ListChanged != null) ListChanged(this, new ListChangedEventArgs(ListChangedType.ItemChanged, index));
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                array.SetValue(_list.Values[i], index + i);
            }
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { throw new NotSupportedException(); }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return _list.Values.GetEnumerator();
        }

        #endregion

        #region IList<TAdapter> Members

        public int IndexOf(TAdapter item)
        {
            return _list.IndexOfValue(item);
        }

        public void Insert(int index, TAdapter item)
        {
            Add(item);
        }

        TAdapter IList<TAdapter>.this[int index]
        {
            get
            {
                return _list.Values[index];
            }
            set
            {
                TAdapter oldAdapter = _list.Values[index];
                TAdapter newAdapter = value;

                if (object.ReferenceEquals(oldAdapter, newAdapter)) return;

                if (newAdapter == null) 
                {
                    throw new ArgumentException("An object of the adapter type was expected.");
                }

                HandleItemRemoving(_list.Keys[index], oldAdapter);
                HandleItemAdding(_list.Keys[index], newAdapter);

                _list.Values[index] = newAdapter;

                if (ListChanged != null) ListChanged(this, new ListChangedEventArgs(ListChangedType.ItemChanged, index));
            }
        }

        #endregion

        #region ICollection<TAdapter> Members

        public void Add(TAdapter item)
        {
            if (item == null) 
            {
                throw new ArgumentException("An object of the adapter type was expected.", "value");
            }

            TKey key = GetKeyFromAdapter(item);

            Add(key, item);
        }

        public bool Contains(TAdapter item)
        {
            return _list.ContainsValue(item);
        }

        public void CopyTo(TAdapter[] array, int arrayIndex)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                array[arrayIndex + i] = _list.Values[i];
            }
        }

        public bool Remove(TAdapter item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IEnumerable<TAdapter> Members

        IEnumerator<TAdapter> IEnumerable<TAdapter>.GetEnumerator()
        {
            return _list.Values.GetEnumerator();
        }

        #endregion
    }
}
