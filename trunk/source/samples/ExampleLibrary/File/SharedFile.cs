using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using System.ComponentModel;
using ExampleLibrary.File;

namespace BitTunnelExampleLibrary.File
{
    public class SharedFile : FileWrapper
    {
        public enum FileState { Local = 0, Hashing, Sending, Available, Removing }
        FileState _state;
        
        public SharedFile(FileDescriptor file) : base(file)
        {
            
            
        }        

        protected override void OnHashGenerationCompleted()
        {
            State = FileState.Sending;
        }

        protected override void OnHashGenerationStarting()
        {
            State = FileState.Hashing;
        }

        public void SetFileAvailable()
        {
            State = FileState.Available;
        }

        public void SetFileAsRemoving()
        {
            if (State != FileState.Available)
            {
                throw new NotImplementedException("Currently only Files in an available state can be removed");
            }
            State = FileState.Removing;
        }

        public FileState State
        {
            get
            { return _state; }
            set
            {
                _state = value;
                NotifyPropertyChanged("State");
            }
        }

        public double Size
        {
            get
            {
                return (double)(File.Size / 1024);
            }
        }
    }
}
