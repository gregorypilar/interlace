using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.TunnelSerialiser.Attributes;

namespace ObviousCode.Interlace.BitTunnelLibrary.Identification
{
    [Tunnel]
    public class ConnectedClient
    {
        private string _clientName;

        public ConnectedClient()
        {
            InstanceId = Guid.NewGuid().ToString();
        }

        [Tunnel]
        public string IPAddress { get; set; }
        
        [Tunnel]
		public int Port { get; set; }
		
        [Tunnel]
        public string PublicName 
        { 
            get
            {
                return string.IsNullOrEmpty(_clientName) ?
                    string.Format("{0}:{1}", IPAddress, Port) :
                    _clientName;
            }
            set
            {
                _clientName = value;
            }
        }
		
        [Tunnel]
        public string InstanceId { get; set; }

        [Tunnel]
        public string ServerId { get; set; }
        
        [Tunnel]
        public string ServerPublicName { get; set; }

        //Worthwhile to overload == ?

    }
}
