using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.TunnelSerialiser;
using ObviousCode.Interlace.TunnelSerialiser.Attributes;

namespace ObviousCode.Interlace.BitTunnelLibrary.File
{
    [Tunnel]
    public class FileDescriptor
    {
        static object hashLock = new object();
        
        public event EventHandler HashGenerationStarting;
        public event EventHandler HashGenerationCompleted;
        public event EventHandler<ExceptionEventArgs> HashGenerationFailed;

        FileInfo _file;
        string _fileName;
        string _fileFullName;
        long _size;

        public FileDescriptor()
        {
            //FileId = Guid.NewGuid().ToString();
        }
        public static FileDescriptor Create(string fileName, bool generateHash)
        {
            return Create(new FileInfo(fileName), generateHash);
        }

        public static FileDescriptor Create(FileInfo file)
        {
            return Create(file, true);
        }        

        public static FileDescriptor Create(FileInfo file, bool generateHash)
        {
            FileDescriptor descriptor = new FileDescriptor();

            descriptor.FileFullName = file.FullName;

            descriptor.FileName = file.Name;

            descriptor._file = file;

            descriptor.DirectoryName = file.DirectoryName;                            

            if (file.Exists)
            {
                descriptor._size = file.Length;
            }

            if (generateHash)
            {
                descriptor.GenerateHash();
            }

            return descriptor;
        }

        public void GenerateHash()
        {
            if (Exists)
            {
                Thread thread = new Thread(new ThreadStart(delegate()
                {
                    try
                    {
                        lock (hashLock)
                        {
                            using (Stream stream = 
                                _file.Length > 0?
                                (Stream) _file.OpenRead():
                                (Stream) new MemoryStream(Guid.NewGuid().ToByteArray()))
                            {
                                using (MD5CryptoServiceProvider hasher = new MD5CryptoServiceProvider())
                                {

                                    if (HashGenerationStarting != null)
                                    {
                                        HashGenerationStarting(this, EventArgs.Empty);
                                    }

                                    Hash = BitConverter.ToString(hasher.ComputeHash(stream));

                                    if (HashGenerationCompleted != null)
                                    {
                                        HashGenerationCompleted(this, EventArgs.Empty);
                                    }
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if (HashGenerationFailed != null)
                        {
                            HashGenerationFailed(this, new ExceptionEventArgs(ex));
                        }
                        else throw ex;
                    }
                }));
                thread.Start();
            }
        }

        public FileStream OpenForRead()
        {
            //File may have been returned from stream, so File Info requires to be rebuilt
            if (_file == null)
            {
                if (!Exists) throw new InvalidOperationException();
                else
                {
                    _file = new FileInfo(FileFullName);

                    if (!_file.Exists)
                    {
                        //possibly handle with event or better exception to inform app that file has been deleted
                        throw new InvalidOperationException("Cannot locate requested file");
                    }
                }
            }

            return _file.OpenRead();
        }

        public bool Exists
        {
            //If marshaled over network, then file info may be null, so test from Hash, otherwise, go directly from file info
            get { return _file == null ? !string.IsNullOrEmpty(Hash ) : _file.Exists; }
        }

        /// <summary>
        /// MD5 hash of file bytes
        /// </summary>
        [Tunnel]
        public string Hash { get; set; }

        /// <summary>
        /// Guid of file
        /// </summary>
        //[Tunnel]
        //public string FileId { get; set; }

        ///// <summary>
        ///// Id of client that hosts files
        ///// </summary>
        [Tunnel]
        public string OriginId { get; set; }

        /// <summary>
        /// FileName location on Originating client
        /// </summary>
        [Tunnel]
        public string FileFullName
        {
            get { return _fileFullName;}
            set 
            { 
                _fileFullName = value;
                _fileName = new FileInfo(value).Name;
            }
        }

        /// <summary>
        /// File Name
        /// </summary>
        [Tunnel]
        public string FileName
        {
            get
            {
                if (!string.IsNullOrEmpty(_fileName)) return _fileName;
                if (_file != null) return _file.Name;
                if (!string.IsNullOrEmpty(FileFullName)) return new FileInfo(FileFullName).Name;
                return "";
            }
            set { _fileName = value; }
        }

        public string Extension
        {
            get
            {
                EnsureFileInfoIsCreated();

                return _file.Extension;
            }
        }

        private void EnsureFileInfoIsCreated()
        {
            if (_file == null) _file = new FileInfo(_fileFullName);
        }

        [Tunnel]
        public long Size
        {
            get { return _size; }
            set { _size = value; }
        }

        [Tunnel]
        public string DirectoryName { get; private set; }

        /// <summary>
        /// Create FileDescriptor without Origin Id or File full path name
        /// </summary>
        internal static FileDescriptor CreateGenericReference(FileDescriptor descriptor)
        {
            FileDescriptor newInstance =
                Serialiser.Restore<FileDescriptor>(
                    Serialiser.Tunnel(descriptor)
                );

            newInstance.OriginId = null;
            newInstance.FileName = null;

            return newInstance;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            FileDescriptor descriptor = (FileDescriptor)obj;
            if ((System.Object)descriptor == null)
            {
                return false;
            }

            return (obj as FileDescriptor).Hash == Hash;
        }

        public static bool operator ==(FileDescriptor lhs, FileDescriptor rhs)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if ((object)lhs == null ||
                (object)rhs == null)
            {
                return false;
            }

            return lhs.Hash == rhs.Hash;
        }

        public static bool operator !=(FileDescriptor lhs, FileDescriptor rhs)
        {
            return !(lhs == rhs);
        }

        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }
    }
}
