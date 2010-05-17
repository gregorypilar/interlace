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
using System.IO;
using System.Text;

#endregion

namespace Interlace.ReactorUtilities
{
    public class RemoteFileReceiver : IRemoteFileReceiver
    {
        List<byte[]> _chunks;
        byte[] _completedFile;

        public byte[] CompletedFile
        {
            get
            {
                if (_completedFile == null)
                {
                    throw new InvalidOperationException("An attempt was made to access the completed file property before the file was completed.");
                }

                return _completedFile;
            }
        }

        #region IRemoteFileReceiver Members

        public VoidDeferred BeginSending()
        {
            _chunks = new List<byte[]>();
            _completedFile = null;

            return VoidDeferred.Success();
        }

        public VoidDeferred Send(byte[] data)
        {
            if (_chunks == null)
            {
                throw new InvalidOperationException("You must call BeginSending before you can call Send.");
            }

            _chunks.Add(data);

            return VoidDeferred.Success();
        }

        public VoidDeferred EndSending()
        {
            // Assemble the chunks.
            using (MemoryStream stream = new MemoryStream())
            {
                foreach(byte[] chunk in _chunks)
                {
                    stream.Write(chunk, 0, chunk.Length);
                }

                _completedFile = stream.ToArray();
            }

            if (FileCompleted != null) FileCompleted(this, EventArgs.Empty);

            return VoidDeferred.Success();
        }

        public VoidDeferred CancelSending()
        {
            _chunks = null;

            return VoidDeferred.Success();
        }

        #endregion

        public event EventHandler FileCompleted;
    }
}
