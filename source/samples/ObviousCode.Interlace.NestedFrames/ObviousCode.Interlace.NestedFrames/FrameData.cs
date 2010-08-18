using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ObviousCode.Interlace.NestedFrames
{
    public class FrameData : IFrame
    {
        byte[] _data;
        byte[] _header;

        IList<IFrame> _nested;

        public FrameData(byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                _header = reader.ReadBytes(4);
                _data = reader.ReadBytes(BitConverter.ToInt32(_header, 0));
            }
        }

        public FrameData(byte[] header, byte[] message)
        {
            _header = header;
            _data = message;
        }

        public byte[] Header
        {
            get { return _header; }
        }

        public byte[] Data
        {
            get { return _data; }
        }

        #region IFrame Members

        public IList<IFrame> NestedFrames
        {
            get
            {
                if (_nested == null)
                {
                    using (MemoryStream stream = new MemoryStream(CombineHeaderAndMessage(_header, _data)))
                    {
                        using (NestedFrameReader reader = new NestedFrameReader(stream))
                        {
                            _nested = reader.Frames;
                        }
                    }
                }

                return _nested;
            }
        }

        #endregion

        public static byte[] CombineHeaderAndMessage(byte[] header, byte[] message)
        {
            if (header.Length != 4)
            {
                throw new InvalidOperationException("Header must be 4 bytes");
            }
            using (MemoryStream framestream = new MemoryStream(new byte[4 + message.Length], 0, 4 + message.Length))
            {
                framestream.Seek(0, SeekOrigin.Begin);
                framestream.Write(header, 0, 4);
                framestream.Write(message, 0, message.Length);

                return framestream.ToArray();
            }
        }

    }
}
