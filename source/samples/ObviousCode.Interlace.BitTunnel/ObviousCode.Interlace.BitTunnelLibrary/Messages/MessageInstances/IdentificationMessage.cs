using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.NestedFrames;
using ObviousCode.Interlace.BitTunnelLibrary.Identification;
using ObviousCode.Interlace.TunnelSerialiser;

namespace ObviousCode.Interlace.BitTunnelLibrary.Messages
{
    public class IdentificationMessage : BaseMessage
    {
        public IdentificationMessage()
            : base(new IdentificationHeader())
        {

        }
        
        public override void Load(byte[] data)
        {
            using (NestedFrameReader reader = new NestedFrameReader(data))
            {
                IFrame parentFrame = reader.Frames[1];
                IFrame dataFrame = parentFrame.NestedFrames[0];

                using (NestedFrameReader innerReader = new NestedFrameReader(dataFrame.Data))
                {
                    Client = Serialiser.Restore<ConnectedClient>(innerReader.Frames[0].Data);
                }                
            }
        }

        public ConnectedClient Client { get; private set; }

        class IdentificationHeader : BaseHeader
        {
            public IdentificationHeader()
                : base(MessageKeys.Identification)
            {

            }
        }
    }
}
