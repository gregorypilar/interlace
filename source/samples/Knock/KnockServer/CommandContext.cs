using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnel.Connectivity;

namespace KnockServer
{
    public class CommandContext
    {
        public string Command { get; set; }

        public string CurrentPath { get; set; }
        public ServerInstance Server { get; set; }
        public ClientInstance LocalClient { get; set; }

        internal CommandContext Clone()
        {
            CommandContext clone = new CommandContext();

            clone.CurrentPath = CurrentPath;
            clone.Server = Server;
            clone.LocalClient = LocalClient;
            clone.Command = Command;

            return clone;
        }
    }
}
