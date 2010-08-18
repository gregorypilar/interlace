using System;
using System.Collections.Generic;
using System.Text;
using Interlace.ReactorService;
using ObviousCode.Interlace.ChatroomServer.Plugins;
using ObviousCode.Interlace.ChatroomServer.Protocols;

namespace ObviousCode.Interlace.ChatroomServer
{
    public class ChatroomService : IService
    {
        ChatroomSettings _settings;
        IChatroomServerPlugin[] _plugins;

        public ChatroomService(ChatroomSettings settings, params IChatroomServerPlugin[] plugins)
        {
            _settings = settings;
            _plugins = plugins;
        }

        #region IService Members

        public void Close(IServiceHost host)
        {

        }

        public void Open(IServiceHost host)
        {
            host.Reactor.ListenStream(new ChatroomServerProtocolFactory(_settings, _plugins), _settings.Port);
        }

        #endregion
    }
}
