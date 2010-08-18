using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ObviousCode.Interlace.ChatroomClient
{
    public class ChatroomClientSettings 
    {
        int _port;
        IPAddress _address;

        public ChatroomClientSettings()
        {
            
        }

        public string IPAddress
        {
            get
            {
                return _address.ToString();
            }
            set
            {
                //TODO: Implement error handling on Parse (or tryParse)
                _address = System.Net.IPAddress.Parse(value);
            }
        }

        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
            }
        }
    }
}
