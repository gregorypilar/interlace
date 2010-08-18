using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers
{
    public class FileListHeader : BaseHeader
    {
        public FileListHeader()
            : base(MessageKeys.FileList)
        {

        }
    }
}
