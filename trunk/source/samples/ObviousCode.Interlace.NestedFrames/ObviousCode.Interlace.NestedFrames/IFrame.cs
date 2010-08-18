using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObviousCode.Interlace.NestedFrames
{
    public interface IFrame
    {
        IList<IFrame> NestedFrames { get; }
        byte[] Data { get; }
        byte[] Header { get; }
    }
}
