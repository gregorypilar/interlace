using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.TunnelSerialiser.Attributes;

namespace ObviousCode.Interlace.BitTunnelLibrary.File
{        
    [Tunnel]
    public class FileModificationDescriptor
    {
        public FileModificationDescriptor()
        {

        }

        public FileModificationDescriptor(FileDescriptor descriptor, FileModificationMode mode)
        {
            Exists = descriptor.Exists;
            FileFullName = descriptor.FileFullName;            
            Hash = descriptor.Hash;
            Mode = mode;
            OriginId = descriptor.OriginId;
        }

        [Tunnel]
        public bool Exists { get; private set; }

        [Tunnel]
        public string Hash { get; private set; }

        [Tunnel]
        public string FileFullName { get; private set; }

        [Tunnel]
        public string FileId { get; private set; }

        [Tunnel]
        public string OriginId { get; private set; }

        [Tunnel]
        public FileModificationMode Mode { get; private set; }

        public FileDescriptor ToFileDescriptor()
        {
            FileDescriptor descriptor = FileDescriptor.Create(FileFullName, false);
            
            descriptor.FileFullName = FileFullName;
            descriptor.Hash = Hash;
            descriptor.OriginId = OriginId;

            return descriptor;
        }

        internal static bool TryCreate(FileDescriptor file, out FileModificationDescriptor modification)
        {
            try
            {
                modification = new FileModificationDescriptor(file, FileModificationMode.New);

                return true;
            }
            catch (Exception e)
            {
                modification = null;

                return false;
            }
        }
    }
}
