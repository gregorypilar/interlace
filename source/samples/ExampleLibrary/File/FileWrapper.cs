using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ObviousCode.Interlace.BitTunnelLibrary.File;

namespace ExampleLibrary.File
{
    public class FileWrapper : INotifyPropertyChanged
    {        
        public FileWrapper(FileDescriptor file)
        {
            Id = Guid.NewGuid().ToString();

            File = file;         

            File.HashGenerationCompleted += new EventHandler(File_HashGenerationCompleted);
            File.HashGenerationStarting += new EventHandler(File_HashGenerationStarting);
        }

        void File_HashGenerationStarting(object sender, EventArgs e)
        {
            OnHashGenerationStarting();
        }        

        void File_HashGenerationCompleted(object sender, EventArgs e)
        {
            OnHashGenerationCompleted();
        }

        protected virtual void OnHashGenerationCompleted() { }

        protected virtual void OnHashGenerationStarting() { }

        public FileDescriptor File { get; set; }

        public string Id { get; private set; }

        protected void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
