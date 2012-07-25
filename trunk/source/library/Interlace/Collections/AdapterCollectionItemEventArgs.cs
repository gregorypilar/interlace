using System;
using System.Collections.Generic;
using System.Text;

namespace Interlace.Collections
{
    public class AdapterCollectionItemEventArgs<TAdapter> : EventArgs
    {
        readonly TAdapter _adapter;

        public AdapterCollectionItemEventArgs(TAdapter adapter)
        {
            _adapter = adapter;
        }

        public TAdapter Adapter
        { 	 
            get { return _adapter; }
        }
    }
}
