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

#endregion

namespace Interlace.Pinch.Generation
{
    public class Generator
    {
        Language _language;

        public Generator(Language language)
        {
            _language = language;
        }

        public static void Generate(Language language, string documentPath, string destinationPath)
        {
            Document document = Document.Parse(documentPath);

            Compilation compilation = new Compilation();
            compilation.AddDocument(document);
            compilation.Resolve();
            compilation.Number();

            language.CreateDomImplementationHelpers(document);

            Generator generator = new Generator(language);

            generator.Generate(document, documentPath, destinationPath);
        }

        public void Generate(Document document, string documentPath, string destinationPath)
        {
            foreach (LanguageOutput output in _language.GetLanguageOutputs(Path.GetFileNameWithoutExtension(documentPath), destinationPath))
            {
                switch (output.TemplateKind)
                {
                    case LanguageOutputTemplateKind.StringTemplate:
                        GenerateFromStringTemplate(document, output);
                        break;

                    case LanguageOutputTemplateKind.Velocity:
                        throw new NotSupportedException();
                }
            }
        }

        private void GenerateFromStringTemplate(Document document, LanguageOutput output)
        {
            StringTemplateGroup group;

            using (StringReader reader = new StringReader(output.Template))
            {
                group = new StringTemplateGroup(reader, typeof(AngleBracketTemplateLexer));
            }

            StringTemplate template = group.GetInstanceOf(output.TemplateName);

            template.SetAttribute("Document", document);

            using (StreamWriter writer = new StreamWriter(output.OutputFilePath, false))
            {
                template.Write(new AutoIndentWriter(writer));
            }
        }
    }
}
