using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.NestedFrames;
using ObviousCode.Interlace.BitTunnelLibrary.Messages;
using System.IO;

namespace ObviousCode.Interlace.BitTunnelUtilities.Messages
{
    public class MessageReaderProtocol : NestedFrameProtocol 
    {
        protected override void HandleReceivedFrame(byte[] data)
        {
            throw new NotImplementedException();
        }

        public T TranslateMessage<T>(BaseMessage message) where T : BaseMessage
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(BitConverter.GetBytes((byte)message.Key)[0]);
                    writer.Write(BuildFrame(message.HeaderData));
                    writer.Write(BuildFrame(message.MessageData));

                    return (T)BaseMessage.Translate(stream.ToArray());
                }
            }

        }
    }
}
