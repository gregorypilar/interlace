using System;

namespace ObviousCode.Interlace.BitTunnelLibrary.Interfaces
{
    public interface IMessage : IDisposable
    {
        MessageKeys Key { get; }
        byte[] HeaderData { get; }
        byte[] MessageData { get; }

        void AddFrame(byte[] frame);
        void Load(byte[] data);
    }
}
