using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ObviousCode.Interlace.BitTunnelLibrary.Messages;
using System.Diagnostics;

namespace ObviousCode.Interlace.BitTunnelLibrary.File
{
    public class FileRebuilder : IDisposable
    {        
        //public event EventHandler ReadyForNextChunk;
        //public event EventHandler FileCompleted;
        
        AppSettings _settings;
        bool _completed;
        string _fileName;
        FileStream _stream;

        public FileRebuilder(AppSettings settings, string hash)
        {
            _settings = settings;
            Hash = hash;
            _completed = false;
            _fileName = Path.Combine(_settings.WorkingPath.FullName,
                    string.Format("{0}.btt", Guid.NewGuid().ToString().Replace("-", "")));

            _stream = new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.Write);
        }
      
        public string Hash { get; set; }
        public string FileName { get { return _fileName; } }

        #region IDisposable Members

        public void Dispose()
        {
            _stream.Close();                        

            //If incomplete, clean up (may keep to restart download in future)
            if (System.IO.File.Exists(_fileName) && _settings.DeleteUnfinishedFilesOnFileBuilderDispose && !_completed)
            {
                System.IO.File.Delete(_fileName);
            }            
        }

        #endregion

        internal void Close()
        {
            _stream.Close();
        }

        internal void ReceiveChunk(FileChunkMessage chunk)
        {
            if (chunk.IsStartChunk)
            {
                PrepareForBuild(chunk);
            }

            _stream.Write(chunk.Chunk, 0, chunk.Chunk.Length);            
            
            if (chunk.IsEndChunk)
            {
                Debug.Write("Finished");
                CompleteBuild();
            }            
        }

        private void PrepareForBuild(ObviousCode.Interlace.BitTunnelLibrary.Messages.FileChunkMessage chunk)
        {
            //possibly have a small metadata file to describe what files are what in case of a crash - allow for restart. 
            //Requires File Descriptor to be added to file chunk message header - not sure if I want to do that
            
        }

        private void CompleteBuild()
        {
            _completed = true;
        }        
    }
}