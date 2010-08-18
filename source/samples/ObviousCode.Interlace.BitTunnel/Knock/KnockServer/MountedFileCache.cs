using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using KnockServer.Events;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using TelexplorerServer.Mounting;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using System.Threading;

namespace KnockServer
{
    public class MountedFileCache
    {

        public event EventHandler PromptRequested;

        public event EventHandler<MountDeletedEventArgs> FileMountDeleted;
        public event EventHandler<MountDeletedEventArgs> DirectoryMountDeleted;

        Dictionary<string, FileSystemWatcher> _watchers;

        Dictionary<string, List<string>> _relevantFiles;
        Dictionary<string, List<string>> _relevantDirectories;
        Dictionary<string, FileDescriptor> _cachedDescriptors;
        List<string> _cachedDirectories;

        string AllFilesTag = "all-files";
        private static MountedFileCache _cache;

        private MountedFileCache()
        {
            _watchers = new Dictionary<string, FileSystemWatcher>();

            _relevantDirectories = new Dictionary<string, List<string>>();
            _relevantFiles = new Dictionary<string, List<string>>();

            _cachedDescriptors = new Dictionary<string, FileDescriptor>();
            _cachedDirectories = new List<string>();
        }

        public static MountedFileCache Cache
        {
            get
            {
                if (_cache == null) _cache = new MountedFileCache();

                return _cache;
            }
        }

        public ClientInstance Client { private get; set; }
        public ServerInstance Server { private get; set; } 

        public bool AddFile(FileDescriptor file)
        {
            return AddFile(file, false);
        }

        private bool AddFile(FileDescriptor file, bool force)
        {
            bool hashed = false;
            Exception hashException = null;
            int loops = 0;

            if (!force && (ContainsFile(file)))
            {
                Console.WriteLine("{0} already mounted. Ignoring.", file.FileFullName);
                return false;
            }

            //Hashing operations

            Console.WriteLine("Preparing \"{0}\"", file.FileFullName);            

            file.HashGenerationCompleted += new EventHandler(delegate(object sender, EventArgs e)
            {
                hashed = true;
            });

            file.HashGenerationFailed += new EventHandler<ExceptionEventArgs>(delegate(object sender, ExceptionEventArgs e)
            {
                hashException = e.ThrownException;
            });

            file.GenerateHash();

            Console.Write("Hashing ..");

            while (!hashed && hashException == null)
            {
                if (loops++ % 10 == 0)
                {
                    Console.Write(".");
                }

                Thread.Sleep(100);
            }

            Console.WriteLine();

            if (hashException != null)
            {
                Console.WriteLine(hashException.Message);
                Console.WriteLine("Unable to hash {0}. Ignoring", file.FileName);

                return false;
            }

            if(_cachedDescriptors.Count(d => d.Value.Hash == file.Hash) > 0)
            {
                Console.WriteLine("Identically hashed file already mounted. Ignoring.");
                return false;
            }

            //Caching operations

            EnsureRequiredWatcherExists(file.DirectoryName);

            EnsureRelevantFileListExists(file.DirectoryName);

            if (!_relevantFiles[file.DirectoryName].Contains(file.FileFullName))
            {
                _relevantFiles[file.DirectoryName].Add(file.FileFullName);
            }            

            //mounting

            bool fileAvailable = false;

            EventHandler<FileListModificationEventArgs> handler = delegate(object sender, FileListModificationEventArgs e)
            {
                StringBuilder builder = new StringBuilder();

                foreach (FileModificationDescriptor item in e.Modifications)
                {
                    if (item.Mode == FileModificationMode.New)
                    {
                        builder.AppendFormat("Available File: \"{0}\"{1}", item.FileFullName, Environment.NewLine);
                    }
                }

                Console.Write(builder.ToString());
                
                _cachedDescriptors[e.Modifications[0].FileFullName] = e.Modifications[0].ToFileDescriptor();

                fileAvailable = true;
            };

            Client.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(handler);

            Client.AddFiles(new FileDescriptor[] { file });

            DateTime then = DateTime.Now;

            while (!fileAvailable && ((TimeSpan)(DateTime.Now - then)).TotalSeconds < 30)
            {
                Thread.Sleep(100);
            }

            if (!fileAvailable)
            {
                ConsoleColor oldColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("File Add Request not responded after 30 seconds. Check network and consider restarting.");
                Console.WriteLine("Terminating request ...");
                Console.ForegroundColor = oldColour;

                return false;
            }

            Client.FileListUpdateReceived -= new EventHandler<FileListModificationEventArgs>(handler);

            return true;
        }
        
        public bool AddDirectory(DirectoryInfo directory)
        {
            if (directory.Parent != null)
            {
                EnsureRequiredWatcherExists(directory.Parent.FullName);

                EnsureRelevantDirectoryListExists(directory.Parent.FullName);

                if (!_relevantDirectories[directory.Parent.FullName].Contains(directory.FullName))//it shouldn't
                {
                    _relevantDirectories[directory.Parent.FullName].Add(directory.FullName);                    
                }
                else throw new InvalidOperationException();

                _relevantDirectories[directory.Parent.FullName].Add(directory.FullName);
            }            

            EnsureRelevantFileListExists(directory.FullName);            

            foreach (FileInfo file in directory.GetFiles())
            {
                AddFile(FileDescriptor.Create(file, false));
            }

            _relevantFiles[directory.FullName].Insert(0, AllFilesTag);
            
            _cachedDirectories.Add(directory.FullName);

            return true;
        }        

        public FileDescriptor GetFileDescriptor(string path)
        {
            return (_cachedDescriptors.ContainsKey(path)) ? _cachedDescriptors[path] : (FileDescriptor)null;
        }

        private void RenameFile(string oldFileName, string newFileName)
        {
            FileDescriptor descriptor = GetFileDescriptor(oldFileName);
            
            descriptor.FileFullName = newFileName;

            if (descriptor == null) return;

            bool fileRenamed = false;

            EventHandler<FileListModificationEventArgs> handler = delegate(object sender, FileListModificationEventArgs e)
            {
                StringBuilder builder = new StringBuilder();

                if (e.Modifications[0].Mode == FileModificationMode.Renamed)
                {
                    Console.Write("Renamed File: \"{0}\"{1}", e.Modifications[0].FileFullName, Environment.NewLine);

                    _cachedDescriptors.Remove(oldFileName);
                    _cachedDescriptors[e.Modifications[0].FileFullName] = e.Modifications[0].ToFileDescriptor();                

                    fileRenamed = true;
                }                                
            };

            Client.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(handler);

            Client.RenameFiles(new FileDescriptor[] { descriptor });

            DateTime then = DateTime.Now;

            while (!fileRenamed && ((TimeSpan)(DateTime.Now - then)).TotalSeconds < 30)
            {
                Thread.Sleep(100);
            }

            if (!fileRenamed)
            {
                ConsoleColor oldColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("File Rename Request not responded after 30 seconds. Check network and consider restarting.");
                Console.WriteLine("Terminating request ...");
                Console.ForegroundColor = oldColour;

                return;
            }

            Client.FileListUpdateReceived -= new EventHandler<FileListModificationEventArgs>(handler);           
        }

        private void RemoveFile(string path)
        {
            FileDescriptor descriptor = GetFileDescriptor(path);

            if (descriptor == null) return;

            if (_relevantFiles[descriptor.DirectoryName].Contains(descriptor.FileFullName))//it should
            {
                _relevantFiles.Remove(path);
            }
            else throw new InvalidOperationException();

            if (_relevantFiles[descriptor.DirectoryName].Count == 0)
            {
                _relevantFiles.Remove(descriptor.DirectoryName);
            }

            FileDescriptor toRemove = GetFileDescriptor(path);

            _cachedDescriptors.Remove(path);

            bool fileRemoved = false;

            EventHandler<FileListModificationEventArgs> handler = delegate(object sender, FileListModificationEventArgs e)
            {
                StringBuilder builder = new StringBuilder();

                foreach (FileModificationDescriptor item in e.Modifications)
                {
                    if (item.Mode == FileModificationMode.Remove)
                    {
                        builder.AppendFormat("Removed File: \"{0}\"{1}", item.FileFullName, Environment.NewLine);
                    }
                }

                Console.Write(builder.ToString());

                _cachedDescriptors[e.Modifications[0].FileFullName] = e.Modifications[0].ToFileDescriptor();

                fileRemoved = true;
            };

            Client.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(handler);

            Client.RemoveFiles(new FileDescriptor[] { toRemove });

            DateTime then = DateTime.Now;

            while (!fileRemoved && ((TimeSpan)(DateTime.Now - then)).TotalSeconds < 30)
            {
                Thread.Sleep(100);
            }

            if (!fileRemoved)
            {
                ConsoleColor oldColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("File Remove Request not responded after 30 seconds. Check network and consider restarting.");
                Console.WriteLine("Terminating request ...");
                Console.ForegroundColor = oldColour;

                return;
            }

            Client.FileListUpdateReceived -= new EventHandler<FileListModificationEventArgs>(handler);           
        }

        private bool ContainsFile(FileDescriptor file)
        {
            if (_relevantFiles.ContainsKey(file.DirectoryName))
            {
                return (
                    _relevantFiles[file.DirectoryName].Contains(AllFilesTag)
                    || _relevantFiles[file.DirectoryName].Contains(file.FileFullName));
            }

            return false;

        }                 

        internal bool ContainsDirectory(string fullName)
        {
            return _cachedDirectories.Contains(fullName);
        }

        private void EnsureRelevantDirectoryListExists(string directoryName)
        {
            if (!_relevantDirectories.ContainsKey(directoryName))
            {
                _relevantDirectories[directoryName] = new List<string>();
            }
        }

        private void EnsureRelevantFileListExists(string directoryName)
        {
            if (!_relevantFiles.ContainsKey(directoryName))
            {
                _relevantFiles[directoryName] = new List<string>();
            }
        }

        private void EnsureRequiredWatcherExists(string watcherKey)
        {
            if (!_watchers.ContainsKey(watcherKey))
            {
                FileSystemWatcher watcher = new FileSystemWatcher(watcherKey);

                watcher.Created += new FileSystemEventHandler(FileSystemWatcher_ItemCreated);
                watcher.Deleted += new FileSystemEventHandler(FileSystemWatcher_ItemDeleted);
                watcher.Renamed += new RenamedEventHandler(FileSystemWatcher_ItemRenamed);

                watcher.EnableRaisingEvents = true;

                _watchers[watcherKey] = watcher;
            }
        }


        void FileSystemWatcher_ItemRenamed(object sender, RenamedEventArgs e)
        {
            RenameFile(e.OldFullPath, e.FullPath);

            if (PromptRequested != null)
            {
                PromptRequested(this, EventArgs.Empty);
            }
        }
        
        void FileSystemWatcher_ItemDeleted(object sender, FileSystemEventArgs e)
        {
            RemoveFile(e.FullPath);

            if (PromptRequested != null)
            {
                PromptRequested(this, EventArgs.Empty);
            }
        }

        void FileSystemWatcher_ItemCreated(object sender, FileSystemEventArgs e)
        {
            AddFile(FileDescriptor.Create(e.FullPath, false), true);
            
            if (PromptRequested != null)
            {
                PromptRequested(this, EventArgs.Empty);
            }
        }


        public int DirectoryCount
        {
            get
            {
                return _cachedDirectories.Count;
            }
        }

        public IEnumerable<string> DirectoryNames
        {
            get
            {
                return _cachedDirectories;
            }
        }
    }
}
