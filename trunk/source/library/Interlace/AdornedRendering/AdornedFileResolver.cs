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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

using Interlace.AdornedPasteUp.Documents;
using Interlace.AdornedPasteUp.Rendering;
using Interlace.AdornedText;
using Interlace.PropertyLists;

#endregion

namespace Interlace.AdornedRendering
{
    public class AdornedFileResolver : IAdornedReferenceResolver
    {
        ITemporaryFileManager _temporaryFileManager;
        string _referencesDirectory;

        public AdornedFileResolver(ITemporaryFileManager temporaryFileManager, string referencesDirectory)
        {
            _temporaryFileManager = temporaryFileManager;
            _referencesDirectory = referencesDirectory;
        }

        public Uri ResolveReference(Uri adornedAddress)
        {
            string path = adornedAddress.PathAndQuery;
            string relativePath = Path.Combine(_referencesDirectory, path);
            string manifestPath = Path.GetFullPath(relativePath + ".ati");

            PropertyDictionary manifest;

            if (File.Exists(manifestPath))
            {
                manifest = LoadManifest(manifestPath);
            }
            else
            {
                manifest = GenerateImpliedManifest(manifestPath);
            }

            if (manifest.HasDictionaryFor("master"))
            {
                PropertyDictionary master = manifest.DictionaryFor("master");

                return ResolveMasterReference(master, manifestPath);
            }
            else if (manifest.HasDictionaryFor("pasteUp"))
            {
                return ResolvePasteUpReference(manifest, manifestPath);
            }
            else
            {
                throw new AdornedReferenceResolutionException(string.Format(
                    "The manifest for reference \"{0}\" does not have any recognised sections.", manifestPath));
            }
        }

        public Uri ResolveMasterReference(PropertyDictionary master, string manifestPath)
        {
            if (master == null || string.IsNullOrEmpty(master.StringFor("fileName")))
            {
                throw new AdornedReferenceResolutionException(string.Format(
                    "The manifest for reference \"{0}\" does not have a master section or the master " +
                    "section does not contain a \"fileName\" setting.",
                    manifestPath));
            }

            string imageFileName = Path.Combine(_referencesDirectory, master.StringFor("fileName"));

            if (!File.Exists(imageFileName))
            {
                throw new AdornedReferenceResolutionException(string.Format(
                    "A file (\"{0}\") referenced in the reference manifest \"{1}\" could not be found.",
                    imageFileName, manifestPath));
            }

            string newUrl = ResolveFormat(Path.GetFullPath(imageFileName), master);
            string newerUrl = ResolveFormat(newUrl.ToString(), master);
            string reallyNewUrl = ResolveFormat(newerUrl.ToString(), master);

            return new Uri(reallyNewUrl);
        }

        public Uri ResolvePasteUpReference(PropertyDictionary manifest, string manifestPath)
        {
            Document document = Document.Deserialize(manifest, Path.GetDirectoryName(manifestPath));

            PasteUpRenderer renderer = new PasteUpRenderer(document);

            string outputPath = _temporaryFileManager.CreateAnonymousFilePath(".png");

            using (Bitmap bitmap = renderer.Render())
            {
                bitmap.Save(outputPath);
            }

            return new Uri(outputPath);
        }

        string[] _imageFileExtensions = new string[] { ".jpg", ".png" };

        private PropertyDictionary GenerateImpliedManifest(string manifestPath)
        {
            // Search for image files to fake a manifest from; return in the loop if one is found:
            foreach (string extension in _imageFileExtensions)
            {
                Debug.Assert(extension.StartsWith("."));

                string candidateName = Path.ChangeExtension(manifestPath, extension);

                if (File.Exists(candidateName))
                {
                    PropertyDictionary fileDictionary = new PropertyDictionary();
                    PropertyDictionary masterDictionary = new PropertyDictionary();

                    fileDictionary.SetValueFor("master", masterDictionary);

                    masterDictionary.SetValueFor("fileName", Path.GetFileName(candidateName));

                    return fileDictionary;
                }
            }

            // Nothing was found; complain:
            throw new AdornedReferenceResolutionException(string.Format(
                "The manifest file expected to be at \"{0}\" could not be found, and no images with " +
                "the same name were found.", 
                manifestPath));
        }

        private static PropertyDictionary LoadManifest(string manifestPath)
        {
            PropertyDictionary manifest;
            try
            {
                manifest = PropertyDictionary.FromFile(manifestPath);
            }
            catch (PropertyListException ex)
            {
                throw new AdornedReferenceResolutionException(string.Format(
                    "An error occurred reading the reference manifest file: {0}", ex.Message), ex);
            }
            return manifest;
        }

        public Uri ResolveInline(string contentType, string content)
        {
            if (contentType != "latex")
            {
                throw new AdornedReferenceResolutionException(string.Format(
                    "The inline block content type \"{0}\" is unknown or not supported.", contentType));
            }

            string newFilePath = _temporaryFileManager.CreateAnonymousFilePath(".tex");

            using (StreamWriter writer = new StreamWriter(newFilePath))
            {
                writer.WriteLine(@"\batchmode");
                writer.WriteLine(@"\documentclass[fleqn]{article}");
                writer.WriteLine(@"\mathindent=0em");
                writer.WriteLine(@"\usepackage[a4paper,landscape]{geometry}");
                writer.WriteLine(@"\usepackage[fleqn]{amsmath}");
                writer.WriteLine(@"\usepackage[active,delayed,pdftex,tightpage]{preview}");
                writer.WriteLine(@"\setlength\PreviewBorder{5pt}");
                writer.WriteLine(@"\begin{document}");
                writer.WriteLine(@"\begin{preview}");

                writer.WriteLine(content.Trim());

                writer.WriteLine(@"\end{preview}");
                writer.WriteLine(@"\end{document}");

                writer.Close();
            }

            PropertyDictionary properties = new PropertyDictionary();
            properties.SetValueFor("resolution", 120);

            string newUrl = ResolveFormat(newFilePath, properties);
            string newerUrl = ResolveFormat(newUrl.ToString(), properties);

            return new Uri(newerUrl);
        }

        static readonly string _dotCommand = "C:\\Program Files\\Graphviz2.16\\bin\\dot.exe";
        static readonly string _ghostscriptCommand = "C:\\Program Files\\gs\\gs8.60\\bin\\gswin32c.exe";
        static readonly string _texCommand = "C:\\Program Files\\MiKTeX 2.7\\miktex\\bin\\pdflatex.exe";

        public string ResolveFormat(string path, PropertyDictionary properties)
        {
            string extension = Path.GetExtension(path).ToLower();

            if (extension == ".png")
            {
                return path;
            }

            if (extension == ".jpg")
            {
                return path;
            }

            if (extension == ".dot")
            {
                string newFilePath = _temporaryFileManager.CreateAnonymousFilePath(".ps");

                string arguments = string.Format("\"{0}\" -Tps2 \"-o{1}\"",
                    path, newFilePath);

                RunUtility(_dotCommand, arguments);

                return newFilePath;
            }

            if (extension == ".ps")
            {
                string newFilePath = _temporaryFileManager.CreateAnonymousFilePath(".pdf");

                string arguments = string.Format(
                    "-dFirstPage=1 -dLastPage=1 -sDEVICE=pdfwrite -dBATCH -dNOPAUSE -dUseCropBox " +
                    "-sOutputFile=\"{1}\" \"{0}\"",
                    path, newFilePath);

                RunUtility(_ghostscriptCommand, arguments);

                return newFilePath;
            }

            if (extension == ".pdf" || extension == ".ai")
            {
                string newFilePath = _temporaryFileManager.CreateAnonymousFilePath(".png");

                int resolution = properties.IntegerFor("resolution", 72);

                string arguments = string.Format(
                    "-dFirstPage=1 -dLastPage=1 -sDEVICE=png16m -dBATCH -dNOPAUSE " +
                    "-dTextAlphaBits=4 -dGraphicsAlphaBits=4 -dUseCropBox -r{2} " +
                    "-sOutputFile=\"{1}\" \"{0}\"", 
                    path, newFilePath, resolution);

                RunUtility(_ghostscriptCommand, arguments);

                return newFilePath;
            }

            if (extension == ".tex")
            {
                string newFilePath = _temporaryFileManager.CreateAnonymousFilePath(".pdf", ".log", ".aux");

                string arguments = string.Format("-output-directory=\"{0}\" -job-name=\"{1}\" \"{2}\"", 
                    Path.GetDirectoryName(newFilePath), Path.GetFileNameWithoutExtension(newFilePath), path);

                RunUtility(_texCommand, arguments);

                return newFilePath;
            }

            return null;
        }

        static void RunUtility(string command, string arguments)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo(command, arguments);
                processStartInfo.CreateNoWindow = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.RedirectStandardOutput = true;

                int exitCode;
                string errorString;

                using (Process process = Process.Start(processStartInfo))
                {
                    process.WaitForExit();
                    exitCode = process.ExitCode;

                    errorString = process.StandardOutput.ReadToEnd();

                    if (!string.IsNullOrEmpty(errorString)) errorString += "\n";

                    errorString += process.StandardError.ReadToEnd();
                }

                if (exitCode != 0)
                {
                    throw new AdornedReferenceResolutionException(string.Format(
                        "The image conversion utility (at \"{0}\") failed with an exit code.",
                        command), errorString);
                }
            }
            catch (Win32Exception ex)
            {
                throw new AdornedReferenceResolutionException(string.Format(
                    "An error occurred executing an image conversion utility (at \"{0}\"). The " +
                    "error was: {1}", command, ex.Message), ex);
            }
        }
    }
}
