using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KnockServer.Events
{
    public enum MountType { File, Directory }

    public class MountDeletedEventArgs : EventArgs
    {
        public MountDeletedEventArgs(string path, MountType type)
        {
            Path = path;
            Type = type;
        }

        public string Path { get; set; }
        public MountType Type { get; set; }
    }
}
