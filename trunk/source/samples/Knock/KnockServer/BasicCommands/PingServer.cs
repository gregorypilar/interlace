using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnel.Connectivity;

namespace KnockServer.BasicCommands
{
    public class PingServer : ConsoleCommand
    {
        public PingServer() : base(10, false, "ping server", "status", "ping")
        {
                
        }

        #region ICommand Members        

        public override bool HandleCommand(CommandContext context)
        {
            if (HandlesCommand(context))
            {
                Console.WriteLine("Server is connected: {0}", context.Server.Connection.IsConnected);
                Console.WriteLine("Local Client is connected: {0}", context.LocalClient.Connection.IsConnected);
            }

            return false;
        }

        #endregion
    }
}
