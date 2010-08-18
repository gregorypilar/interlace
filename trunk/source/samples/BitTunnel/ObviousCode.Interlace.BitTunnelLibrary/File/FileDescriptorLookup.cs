using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.Events;

namespace ObviousCode.Interlace.BitTunnelLibrary.File
{
    public class FileDescriptorLookup
    {
        /// <summary>
        /// Only store 1 instance of a uniquely hashed file
        ///
        /// Consider making this the only use of this class, and make a separate class for the server
        /// so we don't have 2 separate add/remove logic forks
        /// </summary>
        private bool _uniqueHash;

        /// <summary>
        /// FileName Store by MD5?SHA1?SHA256 hash - what is quicker, using MD5 at present
        /// </summary>
        private IDictionary<string, List<FileDescriptor>> _files;

        //Avoid newing up one of these for each file, just reuse old one
        private FileModificationCancelEventArgs _fileCancelArgs;

        public FileDescriptorLookup()
            : this(false)
        {

        }

        public FileDescriptorLookup(bool limitToUniquelyHashedFilesOnly)
            : this(new FileDescriptor[] { }, limitToUniquelyHashedFilesOnly)
        {
        }

        public FileDescriptorLookup(IEnumerable<FileDescriptor> files)
            : this(files, false) { }

        public FileDescriptorLookup(IEnumerable<FileDescriptor> files, bool limitToUniquelyHashedFilesOnly)
        {
            _uniqueHash = limitToUniquelyHashedFilesOnly;

            _files = new Dictionary<string, List<FileDescriptor>>();

            _fileCancelArgs = new FileModificationCancelEventArgs(null);

            foreach (FileDescriptor file in files)
            {
                FileModificationDescriptor modification;

                bool created = FileModificationDescriptor.TryCreate(file, out modification);

                if (!created)
                {
                    //Fire cannot find 
                }

                AddNewFile(modification);
            }
        }

        public int FileInstanceCount
        {
            get
            {
                return _files.Sum(l => l.Value.Count);
            }
        }

        public int UniqueFileCount
        {
            get
            {
                return _files.Count(f => f.Value.Count > 0);
            }
        }

        public int CountOf(FileDescriptor file)
        {
            if (string.IsNullOrEmpty(file.Hash))
            {
                throw new InvalidOperationException("File must contain a valid hash to compare for Count");
            }

            return CountOf(file.Hash);
        }

        public int CountOf(string hash)
        {
            if (!_files.ContainsKey(hash)) return 0;

            return _files[hash].Count;
        }

        /// <summary>
        ///   File has been ignored from lookup as it has a null or empty hash
        /// </summary>
        public event EventHandler<FileListModificationEventArgs> FileIgnored;

        /// <summary>
        /// File Addition in progress. Can pass these up one at a time
        /// </summary>
        public event EventHandler<FileModificationCancelEventArgs> FileAdding;

        /// <summary>
        /// File Addition in progress. Can pass these up one at a time
        /// </summary>
        public event EventHandler<FileModificationCancelEventArgs> FileRemoving;

        /// <summary>
        /// Files Added. One event per ModificationList
        /// </summary>
        public event EventHandler<FileListModificationEventArgs> FilesAdded;

        /// <summary>
        /// Files Removed. One event per ModificationList
        /// </summary>
        public event EventHandler<FileListModificationEventArgs> FilesRemoved;

        /// <summary>
        /// Notify lookup when file list has been modified
        /// </summary>
        /// <param name="files">Modification list to return to clients - uniquely hashed files modified in list - first addition or final removal</param>
        public void UpdateFileList(IEnumerable<FileModificationDescriptor> files)
        {
            foreach (FileModificationDescriptor modification in files)
            {
                switch (modification.Mode)
                {
                    case FileModificationMode.New:

                        AddNewFile(modification);
                        break;

                    case FileModificationMode.Remove:

                        RemoveFile(modification);
                        break;

                    case FileModificationMode.Renamed:

                        ReplaceFile(modification);
                        break; 

                    default:
                        throw new InvalidOperationException(string.Format("Cannot implement modification mode {0}", modification.Mode));
                }
            }
        }

        private void ReplaceFile(FileModificationDescriptor modification)
        {
            RemoveFile(modification);
            AddNewFile(modification);
        }

        private void AddNewFile(FileModificationDescriptor file)
        {
            if (IgnoreEmptyHashedFile(file)) return;

            if (_uniqueHash)
            {
                if (CountOf(file.Hash) > 0) return;
            }
            else
            {
                //if it is identical location, don't store same file twice
                if (_files.ContainsKey(file.Hash) &&
                    _files[file.Hash].Count(f => f.FileFullName == file.FileFullName) > 0)
                {
                    return;
                }
            }


            if (FileAdding != null)
            {
                SetupFileCancelArgs(file);

                FileAdding(this, _fileCancelArgs);

                if (_fileCancelArgs.Cancel) return;
            }

            if (!_files.ContainsKey(file.Hash))
            {
                _files[file.Hash] = new List<FileDescriptor>();
            }

            _files[file.Hash].Add(file.ToFileDescriptor());
        }

        private void RemoveFile(FileModificationDescriptor file)
        {
            if (IgnoreEmptyHashedFile(file)) return;

            if (FileRemoving != null)
            {
                SetupFileCancelArgs(file);

                FileRemoving(this, _fileCancelArgs);

                if (_fileCancelArgs.Cancel) return;
            }

            if (!_files.ContainsKey(file.Hash)) return;

            if (_uniqueHash)
            {
                _files.Remove(file.Hash);
            }
            else
            {
                _files[file.Hash].RemoveAll(f => f.Hash == file.Hash);

                if (_files[file.Hash].Count == 0) 
                {
                    _files.Remove(file.Hash);
                }
            }

            
        }

        private bool IgnoreEmptyHashedFile(FileModificationDescriptor file)
        {
            if (string.IsNullOrEmpty(file.Hash))
            {
                if (FileIgnored != null)
                {
                    FileListModificationEventArgs args = new FileListModificationEventArgs(new FileModificationDescriptor[] { file });

                    FileIgnored(this, args);
                }

                return true;
            }

            return false;
        }

        private void SetupFileCancelArgs(FileModificationDescriptor file)
        {
            //Assume it has been newed up in ctor
            _fileCancelArgs.File = file;

            _fileCancelArgs.Cancel = false;
        }

        /// <summary>
        /// Create and return a new list of currently stored unique files
        /// </summary>
        public IEnumerable<FileDescriptor> GetCurrentUniqueFileList()
        {
            foreach (List<FileDescriptor> descriptorInstances in _files.Values)
            {
                foreach (FileDescriptor descriptor in descriptorInstances)
                {
                    yield return FileDescriptor.CreateGenericReference(descriptor);
                    break;
                }
            }
        }

        public bool Contains(params FileDescriptor[] existingFiles)
        {
            return Contains(
                existingFiles.Select(f => f.Hash).Distinct().ToArray()
                );
        }

        public bool Contains(params string[] existingHashes)
        {
            foreach (string hash in existingHashes)
            {
                if (!_files.ContainsKey(hash)) return false;
            }

            return true;
        }

        public FileDescriptor this[string hash]
        {
            get
            {
                if (!Contains(hash)) throw new InvalidOperationException();

                return _files[hash][0];
            }
        }
    }
}
