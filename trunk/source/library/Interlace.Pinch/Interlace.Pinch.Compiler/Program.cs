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

using Interlace.Pinch.Analysis;
using Interlace.Pinch.Dom;
using Interlace.Pinch.Generation;

#endregion

namespace Interlace.Pinch.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Language language = Language.Cs;

                string documentPath = null;
                string destinationPath = null;

                int nonOptions = 0;

                for (int argumentIndex = 0; argumentIndex < args.Length; argumentIndex++)
                {
                    string argument = args[argumentIndex];

                    if (argument.ToLower() == "--cs")
                    {
                        language = Language.Cs;
                    }
                    else if (argument.ToLower() == "--java")
                    {
                        language = Language.Java;
                    }
                    else if (argument.ToLower() == "--cpp")
                    {
                        language = Language.Cpp;
                    }
                    else if (argument.StartsWith("-"))
                    {
                        throw new ApplicationException(
                            string.Format("The option \"{0}\" is not supported.", argument));
                    }
                    else
                    {
                        if (nonOptions == 0)
                        {
                            documentPath = Path.GetFullPath(argument);

                            nonOptions++;
                        }
                        else if (nonOptions == 1)
                        {
                            destinationPath = Path.GetFullPath(argument);

                            nonOptions++;
                        }
                        else
                        {
                            throw new ApplicationException(
                                string.Format("An unexpected argument was specified."));
                        }
                    }
                }

                if (documentPath == null)
                {
                    throw new ApplicationException("A file to process must be specified.");
                }

                if (destinationPath == null)
                {
                    destinationPath = Path.GetDirectoryName(documentPath);
                }

                Generator.Generate(language, documentPath, destinationPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                Environment.ExitCode = -1;
            }
        }
    }
}
