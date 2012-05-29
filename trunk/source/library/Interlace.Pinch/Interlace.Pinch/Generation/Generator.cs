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

using Antlr.StringTemplate;
using Antlr.StringTemplate.Language;

using Interlace.Pinch.Analysis;
using Interlace.Pinch.Dom;
using Interlace.PropertyLists;
using System.Text.RegularExpressions;

#endregion

namespace Interlace.Pinch.Generation
{
    public class Generator
    {
        Language _language;
        Document _document;
        string _documentPath;
        string _destinationPath;
        DateTime _mostRecentSourceFile;

        static Regex _suffixRegex = new Regex(@"(\.(java|cs|cpp|python))?\.instance$", RegexOptions.IgnoreCase);

        protected Generator(Language language, Document document, string documentPath, string destinationPath, DateTime mostRecentSourceFile)
        {
            _language = language;
            _document = document;
            _documentPath = documentPath;
            _destinationPath = destinationPath;
            _mostRecentSourceFile = mostRecentSourceFile;
        }

        public static void Generate(Language language, string documentPath, string destinationPath)
        {
            if (!File.Exists(documentPath))
            {
                throw new ApplicationException(string.Format(
                    "The specified input file \"{0}\" does not exist.", documentPath));
            }

            PropertyDictionary documentOptions;
            string actualDocumentPath;

            if (_suffixRegex.IsMatch(documentPath))
            {
                documentOptions = PropertyDictionary.FromFile(documentPath);
                actualDocumentPath = _suffixRegex.Replace(documentPath, ".pinch");

                if (!File.Exists(actualDocumentPath))
                {
                    throw new ApplicationException(string.Format(
                        "The instance configuration \"{0}\" exists, but not the expected corresponding specification file \"{1}\".", 
                        documentPath, actualDocumentPath));
                }
            }
            else
            {
                actualDocumentPath = documentPath;

                documentOptions = PropertyDictionary.EmptyDictionary();
                actualDocumentPath = documentPath;
            }

            DateTime documentModified = File.GetLastWriteTimeUtc(documentPath);
            DateTime actualDocumentModified = File.GetLastWriteTimeUtc(actualDocumentPath);

            DateTime mostRecent = documentModified > actualDocumentModified ? documentModified : actualDocumentModified;

            Document document = Document.Parse(actualDocumentPath);

            Compilation compilation = new Compilation();
            compilation.AddDocument(document);
            compilation.Resolve();
            compilation.Number();

            language.CreateDomImplementationHelpers(document, documentOptions);

            Generator generator = new Generator(language, document, actualDocumentPath, destinationPath, mostRecent);

            language.GenerateFiles(generator, document);
        }

        public string BaseName
        {
            get 
            {
                return Path.GetFileNameWithoutExtension(_documentPath);
            }
        }

        public string DestinationPath
        {
            get 
            {
                return _destinationPath;
            }
        }

        public void GenerateFile(string outputFilePath, string templateString, string templateName, string rootName, object root)
        {
            if (_mostRecentSourceFile < File.GetLastWriteTimeUtc(outputFilePath)) return;

            StringTemplateGroup group;

            using (StringReader reader = new StringReader(templateString))
            {
                group = new StringTemplateGroup(reader, typeof(AngleBracketTemplateLexer));
            }

            StringTemplate template = group.GetInstanceOf(templateName);

            template.SetAttribute(rootName, root);

            using (StreamWriter writer = new StreamWriter(outputFilePath, false))
            {
                template.Write(new AutoIndentWriter(writer));
            }
        }
    }
}
