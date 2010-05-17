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
using System.Reflection;
using System.Text;

#endregion

namespace Interlace.Collections
{
    public class Bucket<T> : IComparable<Bucket<T>>
    {
        List<T> _items;

        string _propertyName;
        IComparable _comparisonObject;

        BucketLumpType _lumpType;

        Type _itemType;

        object _tag;

        internal Bucket(string propertyName, IComparable comparisonObject, BucketLumpType lumpType)
        {
            _items = new List<T>();
            _itemType = typeof(T);

            _propertyName = propertyName;
            _comparisonObject = comparisonObject;
            _lumpType = lumpType;
        }

        public virtual bool ItemFitsInBucket(T item)
        {
            PropertyInfo property = _itemType.GetProperty(_propertyName);

            if (property == null) throw new InvalidOperationException(string.Format("The property {0} does not exist on this object.", _propertyName));

            object fieldValue = property.GetValue(item, null);
            IComparable comparisonValue = fieldValue as IComparable;

            if (comparisonValue == null) throw new InvalidOperationException("The values returned from the property must implement IComparable.");

            // This seems counterintuitive, but when we're doing a less than lump,
            // the comparison is a greater than.
            if (_lumpType == BucketLumpType.IncludeItemsGreaterThanComparison)
            {
                return _comparisonObject.CompareTo(comparisonValue) <= 0;
            }
            else
            {
                return _comparisonObject.CompareTo(comparisonValue) >= 0;
            }
        }

        public void AddItemToBucket(T item)
        {
            _items.Add(item);
        }

        public void AddItemToBucketIfAppropriate(T item)
        {
            if (ItemFitsInBucket(item)) AddItemToBucket(item);
        }

        public void AddItemsToBucketIfAppropriate(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                AddItemToBucketIfAppropriate(item);
            }
        }

        public List<T> Items
        {
            get { return _items; }
        }

        #region IComparable<Bucket<T>> Members

        public virtual int CompareTo(Bucket<T> other)
        {
            // Remainder Buckets always go at the end of the list.
            if (other is RemainderBucket<T>) return -1;

            int comparisonResult = _comparisonObject.CompareTo(other._comparisonObject);

            // If they're less than bucketing, then we want the 
            // sort the other direction.
            if (_lumpType == BucketLumpType.IncludeItemsGreaterThanComparison)
            {
                return -comparisonResult;
            }
            else
            {
                return comparisonResult;
            }
        }

        #endregion

        public IComparable ComparisonObject
        {
            get { return _comparisonObject; }
        }

        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }
    }
}
