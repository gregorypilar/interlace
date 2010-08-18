using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObviousCode.Interlace.TunnelSerialiser.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Enum)]
    public class TunnelAttribute : Attribute
    {        
        public TunnelAttribute()
        {

        }

        public TunnelAttribute(string tunnelName)
        {
            TunnelName = tunnelName;
        }

        public string TunnelName { get; set; }  
    }
}
