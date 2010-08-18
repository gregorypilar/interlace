using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.MessageInstances;
using System.Threading;
using System.Net;
using ObviousCode.Interlace.BitTunnelLibrary;

namespace TestBitTunnel
{
    [TestFixture]
    public class FileListMessageTests
    {
        AppSettings _settings;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _settings = new AppSettings();
            _settings.Port = 1234;
            _settings.ServerAddress = IPAddress.Parse("127.0.0.1");
            _settings.ClientConnectionTimeout = 1000;            
        }

        [Test]
        public void WhenEmptyFileListSent_ShouldBeReceivedAsEmptyFileList()
        {
            using(ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using(ClientInstance client = new ClientInstance(_settings))
                {
                    client.Connect();                                        
                }
            }
        }
    }
}
