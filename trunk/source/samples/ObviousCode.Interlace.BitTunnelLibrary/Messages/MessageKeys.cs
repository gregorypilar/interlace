using System;

namespace ObviousCode.Interlace.BitTunnelLibrary
{
    public enum MessageKeys : byte
    {
        FileChunk = 0x0000,
        FileList = 0x0001,
        FileListModifications = 0x0002,
        FileRequest = 0x0003,
        Identification = 0x0004,
        SimpleMessage = 0x00FF
    }
}