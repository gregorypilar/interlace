using System;
using System.Collections.Generic;
using System.Text;

namespace Interlace.Collections
{
    public abstract class IndexedAdapterCollection<TAdapter, TValue, TKey> : AdapterCollection<TAdapter, TValue, TKey>
    {
        Dictionary<TKey, TAdapter> _index = new Dictionary<TKey, TAdapter>();

        protected override void OnAdded(TAdapter item)
        {
            TKey key = GetKeyFromAdapter(item);

            if (!_index.ContainsKey(key))
            {
                _index[key] = item;
            }

            base.OnAdded(item);
        }

        protected override void OnRemoved(TAdapter item)
        {
            base.OnRemoved(item);

            TKey key = GetKeyFromAdapter(item);

            if (_index.ContainsKey(key))
            {
                if (object.ReferenceEquals(_index[key], item))
                {
                    _index.Remove(key);
                }
            }
        }

        public TAdapter GetAdapter(TKey key)
        {
            if (_index.ContainsKey(key))
            {
                return _index[key];
            }
            else
            {
                return default(TAdapter);
            }
        }
    }
}
