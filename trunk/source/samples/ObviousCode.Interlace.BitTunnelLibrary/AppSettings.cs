using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using System.IO;

namespace ObviousCode.Interlace.BitTunnelLibrary
{
    public enum ConnectionType { Unknown = 0, Client, Server }

    public class AppSettings
    {
        public AppSettings()
        {
            ClientConnectionTimeout = 1000;
            ClientFileRequestTimeout = 1000;

            FileChunkSize = 1000000;
            FileChunkPollWait = 1000;

            DeleteUnfinishedFilesOnFileBuilderDispose = true;

            ServerAddress = IPAddress.Parse("127.0.0.1");
            Port = 1234;

            WorkingPath = new DirectoryInfo(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BitTunnel"));

            TransferPath = new DirectoryInfo(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Received Files"));
                
            if (!WorkingPath.Exists)
            {
                WorkingPath.Create();
            }
        }

        public bool ServerIsRemote { get; set; }

        /// <summary>
        /// Defaults to 1000ms
        /// </summary>
        public int ClientConnectionTimeout { get; set; }

        /// <summary>
        /// Amount of time server will wait for a client to respond to a file request (both initial request and individual chinks).
        /// if client does not respond in that time, server deems client in withholding file
        /// 
        /// Defaults to 1000ms. 
        /// 
        /// Good for network latency, not large enough for user interaction. Set higher if required.
        /// </summary>
        public int ClientFileRequestTimeout { get; set; }

        public int Port { get; set; }

        public IPAddress ServerAddress { get; set; }

        public IMessageLogger Logger { get; set; }

        public DirectoryInfo WorkingPath { get; set; }

        public DirectoryInfo TransferPath { get; set; }

        /// <summary>
        /// Size of each transfered file chunk in bytes
        /// </summary>
        public int FileChunkSize { get; set; }

        /// <summary>
        /// Amount of milliseconds FileRebuilderService will wait to allow for next chunk to be built
        /// Defaults to 1000
        /// </summary>
        public int FileChunkPollWait { get; set; }

        /// <summary>
        /// Feature to restore downloads not yet implemented, so this should (and defaults to) be true
        /// </summary>
        public bool DeleteUnfinishedFilesOnFileBuilderDispose { get; set; }
    }
}
