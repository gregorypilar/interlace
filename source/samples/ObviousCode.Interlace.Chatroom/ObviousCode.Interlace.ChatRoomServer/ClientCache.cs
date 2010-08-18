using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.ChatroomServer.Protocols;

namespace ObviousCode.Interlace.ChatroomServer
{
    public class ClientCache : IEnumerable<ChatroomServerProtocol>
    {        
        public Dictionary<string, ChatroomServerProtocol> _clients;

        public ClientCache()
        {
            _clients = new Dictionary<string, ChatroomServerProtocol>();
        }

        public int Count
        {
            get
            {
                return _clients.Count;
            }
        }

        public bool ContainsUsername(string username)
        {
            foreach(ChatroomServerProtocol client in _clients.Values)
            {
                if (username.Trim().ToUpperInvariant() == client.Username)
                {
                    return true;
                }
            }

            return false;
        }

        public void AddClient(ChatroomServerProtocol client)
        {
            _clients[client.Key] = client;
        }

        public void RemoveClient(ChatroomServerProtocol client)
        {
            if (_clients.ContainsKey(client.Key))
            {
                _clients.Remove(client.Key);
            }
        }

        public ChatroomServerProtocol this[string key]
        {
            get
            {
                return _clients.ContainsKey(key) ? _clients[key] : null;
            }
        }


        #region IEnumerable<ChatroomServerProtocol> Members

        public IEnumerator<ChatroomServerProtocol> GetEnumerator()
        {
            foreach (ChatroomServerProtocol protocol in _clients.Values)
            {
                yield return protocol;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

}
