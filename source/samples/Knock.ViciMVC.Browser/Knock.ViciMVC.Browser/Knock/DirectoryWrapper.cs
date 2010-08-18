using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using System.Collections.Generic;
using System.IO;

namespace Knock.ViciMVC.Browser.Knock
{
    public class DirectoryWrapper
    {
        List<DirectoryWrapper> _children;
        List<FileDescriptor> _files;

        string _parentPath = null;
        string _name = null;

        public DirectoryWrapper(string path)
        {
            Path = path;
            
            _files = new List<FileDescriptor>();
            _children = new List<DirectoryWrapper>();
            
            DirectoryInfo info = new DirectoryInfo(path);
            FileInfo file = new FileInfo(path);

            if (info.Parent != null)
            {
                _parentPath = info.Parent.FullName;
            }

            _name = file.Name;
        }

        public string Path { get; private set; }

        public string Name
        {
            get { return _name; }
        }

        public List<FileDescriptor> Files
        {
            get { return _files; }
        }

        public string ParentPath
        {
            get { return _parentPath; }
        }

        public List<DirectoryWrapper> Children
        {
            get { return _children; }
        }

        public static List<DirectoryWrapper> GetDirectories(IList<FileDescriptor> descriptors)
        {
            Dictionary<string, DirectoryWrapper> allDirectories = new Dictionary<string, DirectoryWrapper>();

            //Phase 1 - sort files into appropriate directories
            foreach (FileDescriptor file in descriptors)
            {
                LoadDirectoriesRecursively(file.DirectoryName, allDirectories);

                allDirectories[file.DirectoryName].Files.Add(file);
            }

            BuildDirectoryStructure(allDirectories);

            return allDirectories.Values.Where(v => v.ParentPath == null).ToList();
        }

        private static void BuildDirectoryStructure(Dictionary<string, DirectoryWrapper> allDirectories)
        {
            foreach (DirectoryWrapper wrapper in allDirectories.Values)
            {
                if (wrapper.ParentPath == null) continue;

                if (!allDirectories.ContainsKey(wrapper.ParentPath))
                {
                    throw new InvalidOperationException("Unable to locate parent directory");
                }

                allDirectories[wrapper.ParentPath].Children.Add(wrapper);
            }
        }

        private static void LoadDirectoriesRecursively(string path, Dictionary<string, DirectoryWrapper> allDirectories)
        {
            do
            {
                DirectoryWrapper wrapper = new DirectoryWrapper(path);

                if (!allDirectories.ContainsKey(wrapper.Path))
                {
                    allDirectories[wrapper.Path] = wrapper;
                }

                path = wrapper.ParentPath;
            }
            while (path != null);            
        }
    }
}
