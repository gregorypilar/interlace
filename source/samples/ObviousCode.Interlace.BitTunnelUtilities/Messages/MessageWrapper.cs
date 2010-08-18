using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.Messages;
using ObviousCode.Interlace.BitTunnelLibrary;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using System.IO;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.MessageInstances;
using ObviousCode.Interlace.BitTunnelLibrary.File;

namespace ObviousCode.Interlace.BitTunnelUtilities.Messages
{
    public class MessageWrapper
    {
        static Dictionary<MessageKeys, Func<BaseMessage, BaseMessage>> _messageTranslators;
        static Dictionary<MessageKeys, Func<BaseMessage, string>> _messageWriter;
        
        static MessageReaderProtocol _protocol = new MessageReaderProtocol();

        static MessageWrapper()
        {
            _messageTranslators = new Dictionary<MessageKeys, Func<BaseMessage, BaseMessage>>();
            _messageWriter = new Dictionary<MessageKeys, Func<BaseMessage, string>>();

            _messageTranslators[MessageKeys.Identification] = TranslateIdentificationMessage;
            _messageTranslators[MessageKeys.FileList] = TranslateFileListMessage;
            _messageTranslators[MessageKeys.FileListModifications] = TranslateFileModificationMessage;
            _messageTranslators[MessageKeys.FileRequest] = TranslateFileRequestMessage;
            _messageTranslators[MessageKeys.FileChunk] = TranslateFileChunkMessage;

            _messageWriter[MessageKeys.Identification] = GetIdentificationMessage;
            _messageWriter[MessageKeys.FileList] = GetFileListMessage;
            _messageWriter[MessageKeys.FileListModifications] = GetFileModificationMessage;
            _messageWriter[MessageKeys.FileRequest] = GetFileRequestMessage;
            _messageWriter[MessageKeys.FileChunk] = GetFileChunkMessage;
        }

        private static BaseMessage TranslateIdentificationMessage(BaseMessage message)
        {
            if (!(message is IdentificationMessage)) throw new InvalidOperationException();

            if (message.MessageData == null || message.MessageData.Length == 0) return message;

            using(IdentificationMessage id = _protocol.TranslateMessage<IdentificationMessage>(message))
            {
                return id;
            }
        }

        private static string GetIdentificationMessage(BaseMessage message)
        {
            if (!(message is IdentificationMessage)) throw new InvalidOperationException();

            return string.Format("Server {0}, Client {1}, {2}:{3}",
                (message as IdentificationMessage).Client.ServerPublicName,
                (message as IdentificationMessage).Client.PublicName,
                (message as IdentificationMessage).Client.IPAddress,
                (message as IdentificationMessage).Client.Port);
        }

        private static BaseMessage TranslateFileListMessage(BaseMessage message)
        {
            if (!(message is FileListMessage)) throw new InvalidOperationException();

            if (message.MessageData == null || message.MessageData.Length == 0) return message;

            using(FileListMessage list = _protocol.TranslateMessage<FileListMessage>(message))
            {
                return list;
            }
        }

        private static string GetFileListMessage(BaseMessage message)
        {
            if (!(message is FileListMessage)) throw new InvalidOperationException();

            StringBuilder builder = new StringBuilder();

            if ((message as FileListMessage).FileList == null || (message as FileListMessage).FileList.Count == 0) return "No files available";

            builder.AppendLine();

            foreach (FileDescriptor file in (message as FileListMessage).FileList)
            {
                builder.AppendLine(string.Format("{0}: {1}", file.Hash, file.FileFullName));
            }

            return builder.ToString();
        }

        private static BaseMessage TranslateFileModificationMessage(BaseMessage message)
        {
            if (!(message is FileModificationMessage)) throw new InvalidOperationException();

            if (message.MessageData == null || message.MessageData.Length == 0) return message;

            using (FileModificationMessage modifications = _protocol.TranslateMessage<FileModificationMessage>(message))
            {
                return modifications;
            }
        }

        private static string GetFileModificationMessage(BaseMessage message)
        {
            if (!(message is FileModificationMessage)) throw new InvalidOperationException();

            StringBuilder builder = new StringBuilder();
            
            builder.AppendLine();

            foreach (FileModificationDescriptor modification in (message as FileModificationMessage).Modifications)
            {
                builder.AppendLine(string.Format("{0} {1}: {2}", modification.Mode, modification.Hash, modification.FileFullName));
            }

            return builder.Length == 0 ? "No files listed" : builder.ToString();
        }

        private static BaseMessage TranslateFileRequestMessage(BaseMessage message)
        {
            if (!(message is FileRequestMessage)) throw new InvalidOperationException();

            if (message.MessageData == null || message.MessageData.Length == 0) return message;

            using (FileRequestMessage request = _protocol.TranslateMessage<FileRequestMessage>(message))
            {
                return request;
            }
        }

        private static string GetFileRequestMessage(BaseMessage message)
        {
            FileRequestMessage request = message as FileRequestMessage;

            return string.Format("{0} - {1} ({2})",
                request.Header.Response,
                request.RequestedFile.FileName,
                request.Header.Id);
        }

        private static BaseMessage TranslateFileChunkMessage(BaseMessage message)
        {
            if (!(message is FileChunkMessage)) throw new InvalidOperationException();

            if (message.MessageData == null || message.MessageData.Length == 0) return message;

            using (FileChunkMessage chunk = _protocol.TranslateMessage<FileChunkMessage>(message))
            {
                return chunk;
            }
        }

        private static string GetFileChunkMessage(BaseMessage message)
        {
            FileChunkMessage chunk = message as FileChunkMessage;

            return string.Format("File ({0}): Chunk {1} of {2} - Start {3} End {4}",                
                chunk.Header.Hash,
                chunk.ChunkIndex + 1,
                chunk.Header.ChunkCount,
                chunk.IsStartChunk,
                chunk.IsEndChunk
                );
        }
        public string Source { get; set; }
        public MessageAction Action { get; set; }
        public string Time { get; set; }
        public MessageKeys Key { get; set; }
        
        public string Message { get; set; }
        
        
        public static MessageWrapper GetMessageWrapper(string source, MessageAction action, BaseMessage message, DateTime timeStamp)
        {            
            MessageWrapper wrapper = new MessageWrapper();            

            wrapper.Action = action;
            wrapper.Time = string.Format("{0}:{1} {2}.{3}", timeStamp.Hour, timeStamp.Minute, timeStamp.Second, timeStamp.Millisecond);
            wrapper.Key = message.Key;
            wrapper.Source = source;

            if (_messageTranslators.ContainsKey(wrapper.Key))
            {
                BaseMessage toUse = _messageTranslators[wrapper.Key](message);
                
                if (_messageWriter.ContainsKey(wrapper.Key))
                {
                    wrapper.Message = _messageWriter[wrapper.Key](toUse);
                }
            }

            return wrapper;
        }
    }
}
