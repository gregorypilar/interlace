using System;
using System.Collections.Generic;
using Interlace.ReactorCore;
using Interlace.ReactorService;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Messages;

namespace ObviousCode.Interlace.BitTunnelLibrary.Services
{
    /// <summary>
    /// REFACTOR to push queue into individual file builders, no need to queue up at this level as two separate files 
    /// will not be clashing for an Open Stream
    /// </summary>
    public class FileRebuilderService : IService
    {
        public event EventHandler<FileTransferCompletedEventArgs> TransferCompleted;
        public event EventHandler<FileRequestEventArgs> NextChunkRequested;
        
        Dictionary<string, FileRebuilder> _fileBuilders;
        IServiceHost _host;
        Queue<FileChunkMessage> _waitingChunks;

        TimerHandle _timerHandle;

        AppSettings _settings;


        public FileRebuilderService(AppSettings settings)
        {
            _settings = settings;

        }

        #region IService Members

        public void Close(IServiceHost host)
        {
            //Clean up and delete any open files - possibly allow restart in future
            foreach (KeyValuePair<string, FileRebuilder> kvp in _fileBuilders)
            {
                kvp.Value.Dispose();
            }

            _waitingChunks.Clear();
        }

        public void Open(IServiceHost host)
        {
            _host = host;            
            _fileBuilders = new Dictionary<string,FileRebuilder>();

            _waitingChunks = new Queue<FileChunkMessage>();

            _timerHandle = host.Reactor.AddTimer(DateTime.Now, CheckForBuildTask, null);
        }
        
        public void CheckForBuildTask(DateTime fireAt, object state)
        {
            if (_waitingChunks.Count == 0)
            {
                _timerHandle = _host.Reactor.AddTimer(new TimeSpan(0, 0, 0, 0, _settings.FileChunkPollWait), CheckForBuildTask, null);
                return;
            }

            FileChunkMessage next = _waitingChunks.Dequeue();

            if (!WriteChunkToFile(next))
            {
                throw new InvalidOperationException("Chunk failed, this should be logged and contain some meaningful data in future");
            }

            //if (NextChunkRequested != null)
            //{                
            //    NextChunkRequested(this, )
            //}

            _timerHandle = _host.Reactor.AddTimer(DateTime.Now, CheckForBuildTask, null);
        }

        #endregion  

        public void ReceiveChunk(FileChunkMessage message)
        {
            if (_timerHandle != null)
            {
                _timerHandle.Cancel();

                _waitingChunks.Enqueue(message);

                _timerHandle = _host.Reactor.AddTimer(DateTime.Now, CheckForBuildTask, null);
            }
        }

        private bool WriteChunkToFile(FileChunkMessage chunk)
        {
            if (_host == null) throw new InvalidOperationException("Service not started");

            if (chunk.IsStartChunk)
            {
                PrepareForTransfer(chunk.Header.Hash);
            }

            if (!_fileBuilders.ContainsKey(chunk.Header.Hash))
            {
                return false;
            }

            FileRebuilder builder = _fileBuilders[chunk.Header.Hash];

            builder.ReceiveChunk(chunk);

            if (chunk.IsEndChunk && TransferCompleted != null)
            {
                FileTransferCompletedEventArgs args = new FileTransferCompletedEventArgs(chunk.Header.Hash, builder.FileName);

                //Close so subscribers can move file
                _fileBuilders[chunk.Header.Hash].Close();

                TransferCompleted(this, args);

                _fileBuilders[chunk.Header.Hash].Dispose();
                _fileBuilders.Remove(chunk.Header.Hash);                
            }
            else
            {                
                if (NextChunkRequested != null)
                {
                    FileRequestEventArgs args = new FileRequestEventArgs(null);
                    args.Hash = chunk.Header.Hash;
                    args.ChunkIndex = chunk.ChunkIndex + 1;

                    NextChunkRequested(this, args);
                }
            }

            return true;
        }  

        private bool PrepareForTransfer(string hash)
        {
            if (!AddBuilder(hash)) return false;
           
            return true;
        }        
        
        private bool AddBuilder(string hash)
        {            
            if (_fileBuilders.ContainsKey(hash)) return false;
            
            _fileBuilders[hash] = new FileRebuilder(_settings, hash);

            return true;
        }
    }
}
