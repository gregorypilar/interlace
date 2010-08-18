using System;
using System.Collections.Generic;
using System.Text;

namespace ObviousCode.Interlace.BitTunnelLibrary.File
{
    public enum FileModificationMode
    {
        /// <summary>
        /// Default - should not be used. Indication of unset enum
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// New File
        /// </summary>
        New = 1,
        /// <summary>
        /// Remove File
        /// </summary>
        Remove = 2,
        Renamed = 3,

    }
}
