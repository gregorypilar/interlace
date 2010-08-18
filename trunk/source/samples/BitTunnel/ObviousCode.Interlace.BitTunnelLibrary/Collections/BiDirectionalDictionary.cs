using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObviousCode.Interlace.BitTunnelLibrary.Collections
{
    public class BiDirectionalDictionary<L, R>
    {
        Dictionary<R, L> _lhsByRhs;
        Dictionary<L, R> _rhsByLhs;        

        public BiDirectionalDictionary(L lhs, R rhs)
        {
            _lhsByRhs = new Dictionary<R, L>();
            _rhsByLhs = new Dictionary<L, R>();       
        }

        public void Add(L lhs, R rhs)
        {
            _lhsByRhs[rhs] = lhs;
            _rhsByLhs[lhs] = rhs;
        }
        
        public L this[R key]
        {
            get { return _lhsByRhs[key]; }
            set { Add(value, key); }
        }

        public R this[L key]
        {
            get { return _rhsByLhs[key]; }
            set { Add(key, value); }
        }
    }
}
