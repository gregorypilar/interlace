using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;

namespace ObviousCode.Interlace.BitTunnelLibrary.Messages
{
    public class FileChunkMessage : BaseMessage<FileChunkHeader, byte[]>
    {
        public FileChunkMessage() :
            base(new FileChunkHeader())
        {

        }

        public FileChunkMessage(FileChunkHeader header) : 
            base(header)
        {

        }

        public byte[] Chunk
        {
            get
            {
                if (RestoredItems.Count == 0) throw new InvalidOperationException();

                return RestoredItems[0];
            }
        }

        protected override byte[] RestoreFrameData(ObviousCode.Interlace.NestedFrames.IFrame frame)
        {
            return frame.Data;
        }

        public long ChunkIndex
        {
            get
            {
                return Header.ChunkIndex;
            }
        }

        public bool IsStartChunk 
        {
            get
            {
                return ChunkIndex == 0;
            }
        }

        public bool IsEndChunk
        {
            get
            {
                return ChunkIndex == Header.ChunkCount - 1;
            }
        }
    }
}
