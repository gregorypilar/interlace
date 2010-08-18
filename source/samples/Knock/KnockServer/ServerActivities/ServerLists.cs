using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using KnockServer.Mounting;
using System.Threading;

namespace KnockServer.ServerActivities
{
    public class ServerLists : ConsoleCommand
    {
        public ServerLists() : base(10, true, "list", "listf")
        {

        }
        
        #region ICommand Members

        public override bool HandleCommand(CommandContext context)
        {
            bool listed = false;
            string command = context.Command;

            if (HasArguments && Arguments == "all")
            {
                DirectoryMounter mounter = new DirectoryMounter();

                CommandContext context2 = context.Clone();
                context2.Command = "listd";

                mounter.HandlesCommand(context2);
                mounter.HandleCommand(context2);
            }

            EventHandler<FileListEventArgs> handler = delegate(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileListEventArgs e)
                {
                    FullFileListReceived(e.FileList);

                    listed = true;
                };

            context.LocalClient.FullFileListReceived += new EventHandler<FileListEventArgs>(handler);

            context.LocalClient.RequestFullFileList();

            while (!listed)
            {
                Thread.Sleep(100);
            }

            context.LocalClient.FullFileListReceived -= new EventHandler<FileListEventArgs>(handler);

            return true;
        } 

        public int Priority
        {
            get { return 10; }
        }

        #endregion

        void FullFileListReceived(IList<FileDescriptor> list)
        {
            if (list.Count == 0)
            {
                Console.WriteLine("No Files Prepared");
            }
            else
            {
                List<FileDescriptor> files = new List<FileDescriptor>(list);

                files.Sort(
                    delegate(FileDescriptor lhs, FileDescriptor rhs)
                    {
                        return lhs.FileFullName.CompareTo(rhs.FileFullName);
                    });

                Console.WriteLine();
                Console.WriteLine("Available Files");
                Console.WriteLine("---------------");

                foreach (FileDescriptor file in files)
                {                    
                    Console.WriteLine(file.FileFullName);
                }

                Console.WriteLine();
            }
        }
    }
}
