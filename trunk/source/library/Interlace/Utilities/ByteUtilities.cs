#region Using Directives and Copyright Notice

// Copyright (c) 2007-2010, Computer Consultancy Pty Ltd
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the Computer Consultancy Pty Ltd nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL COMPUTER CONSULTANCY PTY LTD BE LIABLE 
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY 
// OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace Interlace.Utilities
{
    public class ByteUtilities
    {
        public static bool CompareBytes(byte[] lhs, byte[] rhs)
        {
            if (lhs.Length != rhs.Length) return false;

            for (int i = 0; i < lhs.Length; i++)
            {
                if (lhs[i] != rhs[i]) return false;
            }

            return true;
        }

        public static byte[] TruncateBytes(byte[] data, int newLength)
        {
            if (data.Length > newLength)
            {
                byte[] newArray = new byte[newLength];
                Array.Copy(data, newArray, newLength);
                return newArray;
            }
            else if (data.Length == newLength)
            {
                return data;
            }
            else
            {
                return JoinBytes(data, new byte[newLength - data.Length]);
            }
        }

        public static byte[] JoinBytes(params byte[][] byteArrays)
        {
            // Get the total length:
            int totalLength = 0;

            foreach (byte[] array in byteArrays)
            {
                totalLength += array.Length;
            }

            // Create the new array and copy the arrays in:
            byte[] newArray = new byte[totalLength];

            int offset = 0;

            foreach (byte[] array in byteArrays)
            {
                Array.Copy(array, 0, newArray, offset, array.Length);

                offset += array.Length;
            }

            return newArray;
        }

        public static byte[] GetStringHash(string s)
        {
            HashAlgorithm hasher = new MD5CryptoServiceProvider();
            return hasher.ComputeHash(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] GetKeyFromString(string s, int bits)
        {
            byte[] hash = GetStringHash(s);
            Debug.Assert(bits / 8 <= hash.Length);

            return TruncateBytes(hash, bits / 8);
        }

        public static byte[] EncryptBytes(byte[] data, byte[] key)
        {
            return EncryptOrDecryptBytes(data, key, true);
        }

        public static byte[] DecryptBytes(byte[] data, byte[] key)
        {
            return EncryptOrDecryptBytes(data, key, false);
        }

        private static byte[] EncryptOrDecryptBytes(byte[] data, byte[] key, bool encrypt)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.BlockSize = 64;
            des.KeySize = 64;
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.Zeros;
            des.Key = key;

            Debug.Assert(key.Length == (des.KeySize / 8));

            ICryptoTransform transform;

            if (encrypt)
            {
                transform = des.CreateEncryptor();
            }
            else
            {
                transform = des.CreateDecryptor();
            }

            MemoryStream result = new MemoryStream();

            using (CryptoStream stream = new CryptoStream(result, transform, CryptoStreamMode.Write))
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }

            return result.ToArray();
        }
    }
}
