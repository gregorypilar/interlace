using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObviousCode.Interlace.Chatroom.Library
{
    public class ChatroomKeys
    {
        //Control messages
        public const string Broadcast = "bcs";
        public const string LoginRequest = "lgr";
        public const string MessageType = "typ";
        public const string LogoutNotification = "lg-";

        //Data messages
        public const string Message = "msg";
        public const string SenderName = "shd";
        public const string SenderId = "sid";
        public const string ChatterListUpdate = "clu";

        //return messages
        
        public const string LoginSuccess = "lg+";
        public const string LoginFail_UserNameInUse = "lfu";
        public const string LoginFail_TooManyClients = "lf>";
        public const string MalformedRequest = "bad";
        public const string LoginFail_ServerNotConnected = "lfx";
    }
}
