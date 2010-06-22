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

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Interlace.Pinch.Analysis;
using Interlace.Pinch.Dom;
using Interlace.Pinch.Generation;
using Interlace.PropertyLists;

#endregion

namespace Interlace.Pinch.BuildTasks
{
    public class CompilePinch : Task
    {
        ITaskItem[] _sources;
        ITaskItem[] _results;

        [Required]
        public ITaskItem[] Sources
        { 	 
            get { return _sources; }
            set { _sources = value; }
        }

        [Output]
        public ITaskItem[] Results
        {
            get { return _results; }
            set { _results = value; }
        }

        public override bool Execute()
        {
            List<ITaskItem> results = new List<ITaskItem>();

            foreach (ITaskItem source in _sources)
            {
                string sourcePath = source.ItemSpec;
                string destinationPath = Path.ChangeExtension(sourcePath, ".cs");

                bool upToDate = File.Exists(destinationPath) && 
                    File.GetLastWriteTimeUtc(sourcePath) <= File.GetLastWriteTimeUtc(destinationPath);

                if (!upToDate)
                {
                    Document document = Document.Parse(sourcePath);

                    Compilation compilation = new Compilation();
                    compilation.AddDocument(document);
                    compilation.Resolve();
                    compilation.Number();

                    Language language = Language.Cs;

                    language.CreateDomImplementationHelpers(document, PropertyDictionary.EmptyDictionary());

                    Generator generator = new Generator(language);

                    generator.Generate(document, destinationPath, Path.GetDirectoryName(destinationPath));

                    Log.LogMessage(MessageImportance.Low, "Pinch compiled {0}", sourcePath);
                }

                results.Add(new TaskItem(destinationPath));
            }

            _results = results.ToArray();

            return true;
        }
    }
}
