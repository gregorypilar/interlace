
using System;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using System.Text;
using System.Threading;
using KnockServer;
using System.IO;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using System.Collections.Generic;

namespace TelexplorerServer.Mounting
{
	public class FileMounter : ConsoleCommand
	{        
        public FileMounter() : base(10, true, "prepf", "mountf", "unmountf", "delf")
        {
            
        }        

        #region ICommand Members

        public override bool HandleCommand(CommandContext context)
        {
            string files = string.IsNullOrEmpty(Arguments) ? "*.*" : Arguments;

            if (UsedCommand == "prepf" || UsedCommand == "mountf")
            {
                foreach (string fileName in (files).Split(','))
                {
                    string fullName = Path.Combine(context.CurrentPath, fileName.Trim());

                    if (File.Exists(fullName))
                    {
                        PrepareFiles(context, FileDescriptor.Create(fullName, false));
                    }
                    else if (File.Exists(fileName.Trim()))
                    {
                        PrepareFiles(context, FileDescriptor.Create(fileName.Trim(), false));
                    }
                    else if (PrepareFilesFromWildCards(context, fileName))
                    {

                    }
                    else
                    {
                        Console.WriteLine("Unknown File: \"{0}\"", fileName);
                    }
                }

                return true;
            }
            else
            {
                foreach (string fileName in (files).Split(','))
                {                    
                    if (fileName.Contains("*"))
                    {
                        RemoveFilesByWildCards(context, fileName);
                    }
                    else
                    {
                        string fullName = new FileInfo(Path.Combine(context.CurrentPath, fileName.Trim())).FullName;

                        RemoveFiles(context, fullName);
                    }
                }

                return true;
            }
        }        

        #endregion

        private bool PrepareFilesFromWildCards(CommandContext context, string searchPattern)
        {
            bool found = false;

            DirectoryInfo info = new DirectoryInfo(context.CurrentPath);

            foreach (FileInfo file in info.GetFiles(searchPattern))
            {
                found = true;
                PrepareFiles(context, FileDescriptor.Create(file, false));
            }

            return found;
        }

        private void RemoveFiles(CommandContext context, params string[] files)
        {
            foreach (string file in files)
            {
                FileDescriptor descriptor = MountedFileCache.Cache.GetFileDescriptor(file);

                if (descriptor == null)
                {
                    Console.WriteLine("{0} not mounted. Ignoring", file);
                    continue;
                }

                bool fileRemoved = false;

                EventHandler<FileListModificationEventArgs> handler = delegate(object sender, FileListModificationEventArgs e)
                {
                    StringBuilder builder = new StringBuilder();

                    foreach (FileModificationDescriptor item in e.Modifications)
                    {
                        if (item.Mode == FileModificationMode.Remove)
                        {
                            builder.AppendFormat("Removing File: \"{0}\"{1}", item.FileFullName, Environment.NewLine);
                        }
                    }

                    Console.Write(builder.ToString());

                    fileRemoved = true;
                };

                context.LocalClient.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(handler);

                context.LocalClient.RemoveFiles(new FileDescriptor[] { descriptor });

                while (!fileRemoved)
                {
                    Thread.Sleep(100);
                }

                context.LocalClient.FileListUpdateReceived -= new EventHandler<FileListModificationEventArgs>(handler);
            }
        }

        private void RemoveFilesByWildCards(CommandContext context, string searchPattern)
        {
            bool found = false;

            DirectoryInfo info = new DirectoryInfo(context.CurrentPath);

            foreach (FileInfo file in info.GetFiles(searchPattern))
            {
                found = true;
                RemoveFiles(context, new string[] { file.FullName });
            }

            if (!found)
            {
                Console.WriteLine("No file found for pattern {0}", searchPattern);
            }
        }

        private void PrepareFiles(CommandContext context, params FileDescriptor[] files)
        {
            foreach (FileDescriptor file in files)
            {
                MountedFileCache.Cache.AddFile(file);
            }
        }
    }
}
