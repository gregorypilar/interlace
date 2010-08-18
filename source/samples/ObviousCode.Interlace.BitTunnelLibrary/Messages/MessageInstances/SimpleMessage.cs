using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;
using ObviousCode.Interlace.TunnelSerialiser.Attributes;
using ObviousCode.Interlace.NestedFrames;
using ObviousCode.Interlace.TunnelSerialiser;

namespace ObviousCode.Interlace.BitTunnelLibrary.Messages.MessageInstances
{
    [Tunnel]
    public enum MessageType { None, FullFileListRequest }

    public class SimpleMessage : BaseMessage
    {
        public SimpleMessage() : base(new SimpleMessageHeader()) 
        { 
            
        }
            
        public override void Load(byte[] data)
        {
            using (NestedFrameReader frameReader = new NestedFrameReader(data))
            {
                IList<IFrame> frames = frameReader.Frames;

                SimpleMessageHeader header = Serialiser.Restore<SimpleMessageHeader>(frames[0].Data, true);

                HeaderFromBase = header;
            }
        }        

        public MessageType MessageType
        {
            get
            {
                return (HeaderFromBase as SimpleMessageHeader).MessageType;
            }
            set
            {
                (HeaderFromBase as SimpleMessageHeader).MessageType = value;
            }
        }
    }
}
