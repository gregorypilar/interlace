using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using System.IO;
using ObviousCode.Interlace.BitTunnelLibrary.Messages;

namespace ObviousCode.Interlace.BitTunnelUtilities.Messages
{
    public class MessageLogger : IMessageLogger
    {
        DirectoryInfo _logpath;
        List<MessageWrapper> _messages;

        public MessageLogger(DirectoryInfo logpath)
        {
            _messages = new List<MessageWrapper>();

            if (!logpath.Exists)
            {
                logpath.Create();
            }

            _logpath = logpath;
        }

        #region IMessageLogger Members

        public void ViewMessage(string source, MessageAction action, BaseMessage message, DateTime timeStamp)
        {
            MessageWrapper wrapper = MessageWrapper.GetMessageWrapper(source, action, message, timeStamp);

            _messages.Add(wrapper);
        }

        public void Flush()
        {            
            using(FileStream stream = new FileStream(Path.Combine(_logpath.FullName, "BitTunnelMessages.log"), FileMode.Create, FileAccess.Write))
            {
                using(StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    foreach (MessageWrapper wrapper in _messages)
                    {
                        try
                        {
                            writer.WriteLine("{0}: {1}-({2}), {3} {4}", wrapper.Time, wrapper.Source, wrapper.Action, wrapper.Key, wrapper.Message);
                        }
                        catch(Exception e)
                        {
                            throw e;
                        }
                    }
                }
            }

            
        }

        #endregion
    }
}
