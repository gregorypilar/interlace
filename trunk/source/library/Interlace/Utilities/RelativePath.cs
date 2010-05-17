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

namespace Interlace.Utilities
{
    public static class RelativePath
    {
        static List<DirectoryInfo> GetPathChain(DirectoryInfo directory)
        {
            List<DirectoryInfo> chain = new List<DirectoryInfo>();

            DirectoryInfo current = directory;

            do 
            {
                chain.Add(current);

                current = current.Parent;
            } 
            while (current != null);

            chain.Reverse();

            return chain;
        }

        static List<DirectoryInfo> GetCommonChain(List<DirectoryInfo> left, List<DirectoryInfo> right)
        {
            List<DirectoryInfo> common = new List<DirectoryInfo>();

            for (int i = 0; i < Math.Min(left.Count, right.Count); i++)
            {
                if (left[i].FullName != right[i].FullName) break;

                common.Add(left[i]);
            }

            return common;
        }

        public static string FromAbsolute(string absolutePath, string currentDirectory)
        {
            if (!Path.IsPathRooted(absolutePath)) throw new ArgumentException("The paths must be absolute.", "absolutePath");
            if (!Path.IsPathRooted(currentDirectory)) throw new ArgumentException("The paths must be absolute.", "absolutePath");

            List<DirectoryInfo> pathComponents = GetPathChain(new DirectoryInfo(absolutePath));
            List<DirectoryInfo> currentComponents = GetPathChain(new DirectoryInfo(currentDirectory));

            List<DirectoryInfo> commonComponents = GetCommonChain(pathComponents, currentComponents);

            if (commonComponents.Count == 0) return absolutePath;

            int backComponents = currentComponents.Count - commonComponents.Count;

            string path = "";

            for (int i = 0; i < backComponents; i++)
            {
                path = Path.Combine(path, "..");
            }

            for (int i = 0; i < pathComponents.Count - commonComponents.Count; i++)
            {
                DirectoryInfo component = pathComponents[commonComponents.Count + i];

                path = Path.Combine(path, component.Name);
            }

            return path;
        }
    }
}
