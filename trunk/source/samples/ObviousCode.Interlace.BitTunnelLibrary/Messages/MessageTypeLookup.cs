using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.MessageInstances;

namespace ObviousCode.Interlace.BitTunnelLibrary.Messages
{
    public class MessageTypeLookup
    {
        static Dictionary<MessageKeys, Type> _types;

        static MessageTypeLookup()
        {
            LoadTypes();
        }

        private static void LoadTypes()
        {
            _types = new Dictionary<MessageKeys, Type>();

            _types[MessageKeys.FileList] = typeof(FileListMessage);
            _types[MessageKeys.FileListModifications] = typeof(FileModificationMessage);
            _types[MessageKeys.Identification] = typeof(IdentificationMessage);
            _types[MessageKeys.FileRequest] = typeof(FileRequestMessage);
            _types[MessageKeys.FileChunk] = typeof(FileChunkMessage);
            _types[MessageKeys.SimpleMessage] = typeof(SimpleMessage);
        }

        public static Type GetMessageType(MessageKeys key)
        {
            return _types[key];
        }
    }
}
