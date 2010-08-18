using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace ObviousCode.Interlace.NestedFrames
{
    public class NestedFrameReader : IDisposable
    {
        MemoryStream _stream;
        IList<IFrame> _frames;

        public NestedFrameReader(byte[] data)
        {
            _stream = new MemoryStream(data);

            LoadFrames(_stream);
        }

        public NestedFrameReader(MemoryStream stream)
        {
            LoadFrames(stream);
        }

        private void LoadFrames(MemoryStream stream)
        {
            long dataLeft;

            _frames = new List<IFrame>();

            dataLeft = stream.Length;
            using (BinaryReader reader = new BinaryReader(stream))
            {
                while (dataLeft > 0)
                {
                    byte[] header = reader.ReadBytes(4);
                    uint length = (uint)IPAddress.NetworkToHostOrder(
                       (int)BitConverter.ToUInt32(header, 0));


                    byte[] messageBytes = reader.ReadBytes((int)length);

                    _frames.Add(new FrameData(header, messageBytes));

                    dataLeft -= length + 4;
                }
            }
        }

        public IList<IFrame> Frames
        {
            get
            {
                return _frames;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Dispose();
            }
        }

        #endregion
    }
}
