using System;
using System.Collections.Generic;
using System.IO;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using ObviousCode.Interlace.NestedFrames;
using ObviousCode.Interlace.TunnelSerialiser;

namespace ObviousCode.Interlace.BitTunnelLibrary.Messages
{
    public abstract class BaseMessage : IMessage
    {
        IHeader _header;
        
        BinaryWriter _writer;
        MemoryStream _stream;
        int _count;

        public BaseMessage(IHeader header)
        {
            _header = header;
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);
            _count = 0;
        }

        public MessageKeys Key
        {
            get
            {
                return _header.Key;
            }
        }

        public void AddFrame(byte[] frame) 
        {
            _writer.Write(frame);
            _count++;
        }

        #region IMessage Members

        public byte[] MessageData
        {
            get
            {
                return _stream.ToArray();
            }
            private set
            {
                _stream = new MemoryStream(value);
            }
        }

        public abstract void Load(byte[] data);

        public byte[] HeaderData
        {
            get
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        _header.Count = _count;

                        writer.Write(Serialiser.Tunnel(_header, true));
                    }
                    return stream.ToArray();
                }
            }
        }
        #endregion        

        public static IMessage Translate(byte[] messageData)
        {
            using (MemoryStream stream = new MemoryStream(messageData))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    MessageKeys key = (MessageKeys)reader.ReadByte();

                    Type type = MessageTypeLookup.GetMessageType(key);

                    IMessage message = (Activator.CreateInstance(type) as IMessage);
                    
                    message.Load(reader.ReadBytes(messageData.Length - 1));
                    
                    return message;
                }
            }
         
            throw new NotImplementedException();
        }

        protected IHeader HeaderFromBase
        {
            get { return _header; }
            set { _header = value; }
        }


        #region IDisposable Members

        public void Dispose()
        {
            _stream.Dispose();
            _writer.Close();
        }

        #endregion
    }

    public abstract class BaseMessage<THeader, T> : BaseMessage where THeader : IHeader
    {
        public BaseMessage(THeader header) : base(header)
        {

        }        
     
        public override void Load(byte[] data)
        {
            RestoredItems = new List<T>();

            using (NestedFrameReader frameReader = new NestedFrameReader(data))
            {
                IList<IFrame> frames = frameReader.Frames;

                THeader header = Serialiser.Restore<THeader>(frames[0].Data, true);

                HeaderFromBase = header;

                using (MemoryStream innerStream = new MemoryStream(frames[1].Data))
                {
                    using (NestedFrameReader reader = new NestedFrameReader(innerStream))
                    {
                        if (reader.Frames.Count != header.Count)
                        {
                            throw new InvalidOperationException("FileListHeader Count does not match message count");
                        }

                        foreach (IFrame frame in reader.Frames)
                        {
                            RestoredItems.Add(RestoreFrameData(frame));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allow sub classes to define how frame is restored. By default, assumes a Tunneled class
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        protected virtual T RestoreFrameData(IFrame frame)
        {
            return Serialiser.Restore<T>(frame.Data, true);
        }

        public THeader Header 
        {
            get { return (THeader)HeaderFromBase; }
        }

        protected List<T> RestoredItems { get; private set; }
    }
}
