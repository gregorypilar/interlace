using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.Protocols;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;
using Interlace.Utilities;
using ObviousCode.Interlace.BitTunnelLibrary.Messages;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary;
using System.Timers;

namespace ObviousCode.Interlace.BitTunnelServer
{
    internal class FileServerNegotiator
    {
        enum State { Init, Waiting, Timeout, FileAvailable }
        public event EventHandler TimedOut;        

        object _receiveLock = new object();

        State _state = State.Init;

        BitTunnelServerProtocolFactory _factory;
        FileDescriptor _file;
        Action<FileDescriptor, FileRequestMode> _sendResponseCallback;
        Action<FileDescriptor, FileChunkMessage> _sendChunkCallback;
        List<BitTunnelServerProtocol> _protocols;
        BitTunnelServerProtocol _servingProtocol;
        Timer _timer;

        List<string> _yetToAnswer;
        long _chunkIndex = -1;
        public FileServerNegotiator(BitTunnelServerProtocolFactory factory, FileDescriptor file, Action<FileDescriptor, FileRequestMode> sendResponseCallback, Action<FileDescriptor, FileChunkMessage> sendChunkCallback)
        {
            Id = Guid.NewGuid().ToString();

            _factory = factory;
            _file = file;
            _sendChunkCallback = sendChunkCallback;
            _sendResponseCallback = sendResponseCallback;            
        }

        public void Negotiate(long chunkIndex)
        {
            _protocols = new List<BitTunnelServerProtocol>(
                _factory.Protocols.Where(p => p.Files.Contains(_file)
                ));

            _yetToAnswer = new List<string>(_protocols.Select(p => p.Id));

            _chunkIndex = chunkIndex;

            if (_protocols.Count == 0)
            {
                _sendResponseCallback(_file, FileRequestMode.NotAvailable);

                if (TimedOut != null)
                {
                    TimedOut(this, EventArgs.Empty);
                }

                return;
            }

            _state = State.Waiting;

            _servingProtocol = null;

            foreach (BitTunnelServerProtocol protocol in _protocols)
            {
                //Subscribing directly to event - would it be better to pass negotiator id with request 
                //and have protocol -> factory -> service pass it back to the relevant negotiator
                protocol.FileRequestResponseReceived += new EventHandler<FileRequestResponseEventArgs>(protocol_FileRequestResponseReceived);
                //If we have a winner already, no point in continuing
                if (_state == State.FileAvailable) break;

                //Have protocol ask client whether it is willing to server file
                protocol.SendFileAvailabilityRequest(_file, chunkIndex);                
            }

            _timer = new Timer(Settings.ClientFileRequestTimeout);

            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
       }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {            
            CleanUpRequestAvailabilityEvents();
            
            if (TimedOut != null)
            {
                TimedOut(this, EventArgs.Empty);                
            }

            lock(_receiveLock)
            {
                if (_state == State.Waiting)
                {
                    Timeout();
                }
            }
        }

        void protocol_FileRequestResponseReceived(object sender, FileRequestResponseEventArgs e)
        {
            if (e.File.Hash != _file.Hash) return;//different file request            

            lock (_receiveLock)
            {                
                if (_state == State.FileAvailable) return;//we already have a serving protocol accepted, nothing to see here, please move along                             

                if (e.Response == FileRequestMode.Available)
                {
                    _timer.Stop();
                    _state = State.FileAvailable;
                }
            }

            _yetToAnswer.Remove((sender as BitTunnelProtocol).Id);

            if (_state == State.Waiting)
            {
                if (_yetToAnswer.Count == 0)
                {
                    Timeout();
                }

                return;//response was not an accepting protocol, so drop out
            }

            _servingProtocol = sender as BitTunnelServerProtocol;

            CleanUpRequestAvailabilityEvents();
            
            _sendResponseCallback(_file, FileRequestMode.Available);

            _servingProtocol.SendReadyToReceiveFile(_file, Id, _chunkIndex);
        }

        private void Timeout()
        {
            _state = State.Timeout;

            CleanUpRequestAvailabilityEvents();

            if (TimedOut != null)
            {
                TimedOut(this, EventArgs.Empty);
            }

            _sendResponseCallback(_file, FileRequestMode.NotAvailable);
        }

        public void ChunkReceived(FileChunkMessage message)
        {
            _sendChunkCallback(_file, message);
        }

        private void CleanUpRequestAvailabilityEvents()
        {
            foreach (BitTunnelServerProtocol protocol in _protocols)
            {
                protocol.FileRequestResponseReceived -=new EventHandler<FileRequestResponseEventArgs>(protocol_FileRequestResponseReceived);
            }
        }

        /// <summary>
        /// Allow file request negotiations in settings object to be used, otherwise defaults to 1
        /// </summary>
        internal AppSettings Settings { get; set; }

        internal string Id { get; private set; }
        internal string Hash
        {
            get
            { return _file.Hash; }
        }
    }
}