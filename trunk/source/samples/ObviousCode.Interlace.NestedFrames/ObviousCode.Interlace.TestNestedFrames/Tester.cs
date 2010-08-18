using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace ObviousCode.Interlace.TestNestedFrames
{
    public class Tester
    {
        public static void Main(string[] args)
        {
            using (MemoryStream outstream = new MemoryStream())
            {
                outstream.Write(BitConverter.GetBytes(11), 0, 4);
                
                using (BinaryWriter writer = new BinaryWriter(outstream))
                {
                    writer.Write(Encoding.ASCII.GetBytes("Hello World"));
                    writer.Close();
                }

                using (MemoryStream instream = new MemoryStream(outstream.ToArray()))
                {
                    using (BinaryReader reader = new BinaryReader(instream))
                    {
                        byte[] header = reader.ReadBytes(4);                        
                        int length = BitConverter.ToInt32(header, 0);

                        byte[] messageBytes = reader.ReadBytes((int)length);
                        string message = Encoding.ASCII.GetString(messageBytes);

                        Console.WriteLine(message);
                            
                        using (MemoryStream framestream = new MemoryStream(new byte[4 + messageBytes.Length], 0, 4 + messageBytes.Length))
                        {
                            framestream.Seek(0, SeekOrigin.Begin);
                            framestream.Write(header, 0, 4);
                            framestream.Write(messageBytes, 0, messageBytes.Length);
                        }
                        
                    }
                }
            }
        }
    }
}
