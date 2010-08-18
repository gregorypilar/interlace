using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TelexplorerServer.Mounting;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using KnockServer.ServerActivities;

namespace KnockServer.Mounting
{
    public class DirectoryMounter : ConsoleCommand
    {        
        public DirectoryMounter() : base (10, true, "prepd", "mountd", "listd", "mounts")
        {            
        }

        public override bool HandleCommand(CommandContext context)
        {
            if (UsedCommand == "listd")
            {
                ListDirectories(context);
            }
            else if (UsedCommand == "mounts")
            {
                ServerLists fileLister = new ServerLists();

                CommandContext listContext = context.Clone();

                listContext.Command = "list";

                fileLister.HandleCommand(listContext);

                ListDirectories(context);

            }
            else
            {
                MountDirectories(context);
            }

            return true;
        }

        private void MountDirectories(CommandContext context)
        {
            if (HasArguments)
            {
                Console.WriteLine("Argument based directory mounting not yet implemented");
            }
            else
            {
                MountDirectory(context);
            }
            
        }

        private void MountDirectory(CommandContext context)
        {
            DirectoryInfo info = new DirectoryInfo(context.CurrentPath);

            if (MountedFileCache.Cache.ContainsDirectory(info.FullName))
            {
                Console.WriteLine("{0} already mounted.", info.FullName);
                return;
            }

            MountedFileCache.Cache.AddDirectory(info);                  
        }

        private void ListDirectories(CommandContext context)
        {
            if (MountedFileCache.Cache.DirectoryCount == 0)
            {
                Console.WriteLine("No mounted directories");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Mounted Directories");
            Console.WriteLine("-------------------");

            foreach (string key in MountedFileCache.Cache.DirectoryNames)
            {
                Console.WriteLine(key);
            }

            Console.WriteLine();
        }
    }
}
