using System;
using Interlace.ReactorService;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
 
namespace ObviousCode.Interlace.BitTunnelLibrary.Interfaces
{
    public interface IBitTunnelService : IService
    {
        event EventHandler ConnectionCompleted;
        
        event EventHandler<ExceptionEventArgs> LostConnection;
        event EventHandler<MessageEventArgs> MessageReceived;
        event EventHandler<MessageEventArgs> MessageSending;

        IInstance Instance { get; set; }
        AppSettings Settings { get; }
    }
}
