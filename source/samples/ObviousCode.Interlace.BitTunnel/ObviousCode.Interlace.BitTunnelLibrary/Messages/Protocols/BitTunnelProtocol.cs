using System;
using System.IO;
using Interlace.ReactorCore;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using ObviousCode.Interlace.BitTunnelLibrary.Messages;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;
using ObviousCode.Interlace.NestedFrames;
using ObviousCode.Interlace.TunnelSerialiser;

namespace ObviousCode.Interlace.BitTunnelLibrary.Protocols
{
    public class BitTunnelProtocol : NestedFrameProtocol
    {        
        public string Id { get; private set; }
        public event EventHandler LostConnection;
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<MessageEventArgs> MessageSending;

        public BitTunnelProtocol()
        {
            Id = Guid.NewGuid().ToString();
        }

        protected override void ConnectionLost(CloseReason reason)
        {
            base.ConnectionLost(reason);

            if (LostConnection != null)
            {
                LostConnection(this, EventArgs.Empty);
            }
        }

        protected override void HandleReceivedFrame(byte[] data)
        {
            try
            {
                using (IMessage message = BaseMessage.Translate(data))
                {
                    if (MessageReceived != null)
                    {
                        MessageReceived(this, new MessageEventArgs(message, data));
                    }

                    OnMessageReceived(message);                    
                }
            }
            catch(Exception e)
            {
                throw e;
            }
            
        }

        public void SendMessage(IMessage message)
        {
            if (MessageSending != null)
            {
                MessageSending(this, new MessageEventArgs(message, null));
            }

            OnMessageSending(message);            

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(BitConverter.GetBytes((byte)message.Key)[0]);
                    writer.Write(BuildFrame(message.HeaderData));
                    writer.Write(BuildFrame(message.MessageData));

                    Send(stream.ToArray());
                }
            }
        }

        public void AddValueToMessage(IMessage message, object value)
        {
            byte[] serialised = value is byte[] ? value as byte[] : Serialiser.Tunnel(value, true);//Handle other primitives

            AddDataToMessage(message, serialised);
        }     

        public void AddDataToMessage(IMessage message, byte[] data)
        {
            message.AddFrame(BuildFrame(data));
        }

        protected virtual void OnMessageReceived(IMessage message) { }

        protected virtual void OnMessageSending(IMessage message) { }        

        internal void LoseConnection()
        {
            Connection.LoseConnection();
        }

        public void SendFileAvailabilityRequest(FileDescriptor file, long chunkIndex)
        {
            InternalSendFileRequestResponse(file, FileRequestMode.Request, null, chunkIndex);
        }

        public void SendReadyToReceiveFile(FileDescriptor file, string negotiatorId, long chunkIndex)
        {
            InternalSendFileRequestResponse(file, FileRequestMode.ReadyToReceive, negotiatorId, chunkIndex);
        }

        public void SendFileRequestResponse(FileDescriptor file, FileRequestMode response)
        {
            InternalSendFileRequestResponse(file, response, null, 0);
        }


        //Negotiator Id only required (currently) in ReadyToReceive, so Protocol -> Factory -> Service knows where to send received chunks to
        private void InternalSendFileRequestResponse(FileDescriptor file, FileRequestMode response, string negotiatorId, long chunkIndex)
        {
            FileRequestHeader header = new FileRequestHeader();

            header.Response = response;
            header.Id = negotiatorId;
            header.ChunkIndex = chunkIndex;

            using (FileRequestMessage message = new FileRequestMessage(header))
            {
                AddValueToMessage(message, file);

                SendMessage(message);
            }
        }
    }
}
