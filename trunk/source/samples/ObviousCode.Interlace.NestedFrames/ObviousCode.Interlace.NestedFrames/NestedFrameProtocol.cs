using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Interlace.ReactorUtilities;

namespace ObviousCode.Interlace.NestedFrames
{
    public abstract class NestedFrameProtocol : FrameProtocol
    {
        protected abstract override void HandleReceivedFrame(byte[] data);

        public void Send(byte[] data)
        {
            //Data frame
            using (MemoryStream stream = PrepareSendFrame())
            {
                stream.Write(data, 0, data.Length);

                CompleteSendFrame(stream);
            }
        }

        protected byte[] BuildFrame(byte[] data)
        {
            using (MemoryStream stream = PrepareSendFrame())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    try
                    {
                        writer.Write(data, 0, data.Length);
                    }
                    catch(Exception e)
                    {

                    }

                    uint networkOrderLength = (uint)IPAddress.HostToNetworkOrder((int)(stream.Length - 4));

                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Write(BitConverter.GetBytes(networkOrderLength), 0, 4);

                    return stream.ToArray();
                }
            }
        }
    }
}
