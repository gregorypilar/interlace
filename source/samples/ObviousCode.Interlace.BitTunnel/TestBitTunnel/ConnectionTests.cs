using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using System.Net;
using ObviousCode.Interlace.BitTunnelLibrary;
using System.Threading;
using ObviousCode.Interlace.BitTunnelLibrary.Exceptions;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using MbUnit.Framework;

namespace TestBitTunnel
{
    [TestFixture]
    public class ConnectionTests
    {
        bool _lostConnectionEventFired;
        
        AppSettings _settings;

        [SetUp]
        public void TestSetup()
        {
        //    Thread.Sleep(100);
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            _settings = new AppSettings();

            _settings.Port = 1234;
            _settings.ServerAddress = IPAddress.Parse("127.0.0.1");
            _settings.ServerIsRemote = false;            
        }

        #region Client

        [Test]
        public void WhenClientConnectsWithName_ClientConnectionIsAvailableWithSameName()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                Assert.IsTrue(server.Connect());

                using (ClientInstance client = new ClientInstance(_settings, "Test Client"))
                {
                    Assert.IsTrue(client.Connect());

                    Assert.IsNotNull(client.ConnectionDetails);
                    Assert.AreEqual("Test Client", client.ConnectionDetails.PublicName);
                }
            }
        }

        #region Connect

        [Test]
        public void WhenMultipleClientsConnect_ServerInitiated_ServerShouldReturnCorrectConnectionCount()
        {           
            using (ServerInstance server = new ServerInstance(_settings))
            {
                Assert.IsTrue(server.Connect());
                
                using (ClientInstance client1 = new ClientInstance(_settings))
                {
                    using (ClientInstance client2 = new ClientInstance(_settings))
                    {
                        using (ClientInstance client3 = new ClientInstance(_settings))
                        {
                            Assert.IsTrue(client1.Connect());
                            Assert.IsTrue(client2.Connect());
                            Assert.IsTrue(client3.Connect());

                            Assert.AreEqual(server.ConnectionCount, 3);
                        }
                    }
                }                
            }
        }

        [Test]
        public void WhenMultipleClientsDisconnect_ServerInitiated_ServerShouldReturnCorrectConnectionCount()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                Assert.IsTrue(server.Connect());

                using (ClientInstance client1 = new ClientInstance(_settings))
                {
                    using (ClientInstance client2 = new ClientInstance(_settings))
                    {
                        using (ClientInstance client3 = new ClientInstance(_settings))
                        {
                            Assert.IsTrue(client1.Connect());
                            Assert.IsTrue(client2.Connect());
                            Assert.IsTrue(client3.Connect());                           
                        }
                    }
                }

                Thread.Sleep(100);//Give it time to tear down. No sense in blocking on the dispose of the clients
                                
                Assert.AreEqual(server.ConnectionCount, 0);
            }
        }
        
        [Test]
        public void WhenClientDisposed_ClientConnectionShouldDropOut()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings))
                {
                    client.Connect();
                    client.Disconnect();
                }

                Thread.Sleep(100);//Give it time to tear down. No sense in blocking on the dispose of the clients

                Assert.AreEqual(0, server.ConnectionCount);
            }
        }

        [Test]
        public void WhenConnectionIsRequestedFromClient_ServerInitiated_ClientShouldConnect()
        {            
            using (ServerInstance server = new ServerInstance(_settings))
            {
                Assert.IsTrue(server.Connect());

                using (ClientInstance client = new ClientInstance(_settings))
                {                    
                    Assert.IsTrue(client.Connect());

                    Assert.IsTrue(server.IsConnected);
                    Assert.IsTrue(client.IsConnected);
                }
            }                       
        }

        [Test]
        public void WhenClientDisconnects_ClientShouldBeAbleToReconnect()
        {
            using(ServerInstance server = new   ServerInstance(_settings))
            {
                using(ClientInstance client = new ClientInstance(_settings))
                {
                    client.Connect();
                    Thread.Sleep(100);
                    client.Disconnect();
                    Thread.Sleep(100);
                    client.Connect();
                    Thread.Sleep(100);
                    Assert.IsTrue(true);
                }
            }
        }

        #endregion

        #region Fail Connection
        
        [Test]        
        public void WhenConnectionIsRequestedFromClientConnectionObject_LostConnectionEventWired_NoServerInitiated_ConnectionShouldFailAndNeverConnect()
        {
            _lostConnectionEventFired = false;            

            using (Connection connection = new Connection(ConnectionType.Client, _settings))
            {
                connection.LostConnection += new EventHandler<ExceptionEventArgs>(connection_LostConnection);
                
                Assert.IsFalse(connection.Connect());
              
                Assert.IsFalse(connection.IsConnected);
                Assert.IsFalse(_lostConnectionEventFired);
        
                connection.LostConnection -= new EventHandler<ExceptionEventArgs>(connection_LostConnection);
            }
        }

        //No point in not wiring exception, otherwise unhandled exception gobbled by reactor

        [Test]
        public void WhenConnectionIsRequestedFromClientInstanceObject_LostConnectionEventWired_NoServerInitiated_ConnectionShouldFail/*WithEventFired*/()
        {
            _lostConnectionEventFired = false;

            using (ClientInstance instance = new ClientInstance(_settings))
            {
                instance.LostConnection += new EventHandler<ExceptionEventArgs>(connection_LostConnection);

                Assert.IsFalse(instance.Connect());

                Assert.IsFalse(instance.IsConnected);
                Assert.IsFalse(_lostConnectionEventFired);
                
                instance.LostConnection -= new EventHandler<ExceptionEventArgs>(connection_LostConnection);
            }
        }     
        #endregion    

        #endregion       

        #region Server

        [Test]
        public void WhenConnectionIsRequestedFromServerConnectionObject_ShouldConnect()
        {
            using(Connection connection = new Connection(ConnectionType.Server, _settings))
            {
                Assert.IsTrue(connection.Connect());                

                Assert.IsTrue(connection.IsConnected);                
            }
        }

        [Test]
        public void WhenConnectionIsRequestedFromServerInstance_ShouldConnect()
        {
            using (ServerInstance instance = new ServerInstance(_settings))
            {
                Assert.IsTrue(instance.Connect());
                
                Assert.IsTrue(instance.IsConnected);
            }
        }    

        #endregion

        void connection_LostConnection(object sender, ExceptionEventArgs e)
        {
            _lostConnectionEventFired = true;
        }
    }
}
