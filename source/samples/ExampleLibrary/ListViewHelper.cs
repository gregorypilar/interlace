using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ExampleLibrary.File;
using System.IO;

namespace ExampleLibrary
{
    public class ListViewHelper
    {
        public enum ComparisonKey { Hash, Id, FileFullName }
        ListView _listView;
        ComparisonKey _comparison;
        Dictionary<string, string> _imageKeyByExtension;

        public ListViewHelper(ListView listView, ComparisonKey defaultKey)
        {
            _listView = listView;

            _listView.SmallImageList = ImageLists.FileImages_Small;
            _listView.LargeImageList = ImageLists.FileImages_Large;

            LoadImageDictionary();

            _comparison = defaultKey;
        }

        private void LoadImageDictionary()
        {
            _imageKeyByExtension = new Dictionary<string, string>();

            _imageKeyByExtension[".mp3"] = "Audio";
            _imageKeyByExtension[".wav"] = "Audio";
            _imageKeyByExtension[".wma"] = "Audio";
            _imageKeyByExtension[".ogg"] = "Audio";

            _imageKeyByExtension[".bmp"] = "Image";
            _imageKeyByExtension[".gif"] = "Image";
            _imageKeyByExtension[".jpg"] = "Image";
            _imageKeyByExtension[".png"] = "Image";

            _imageKeyByExtension[".iso"] = "ISO";

            _imageKeyByExtension[".mov"] = "Movie";
            _imageKeyByExtension[".mp4"] = "Movie";
            _imageKeyByExtension[".mpg"] = "Movie";
            _imageKeyByExtension[".avi"] = "Movie";
            _imageKeyByExtension[".wmv"] = "Movie";

            _imageKeyByExtension[".txt"] = "Text";

            _imageKeyByExtension[".bat"] = "System";
            _imageKeyByExtension[".sys"] = "System";
        }

        public void RemoveFileFromListView(string key)
        {
            ListViewItem item = GetListViewItem(key);

            if (item == null) throw new InvalidOperationException("Cannot locate file to remove");

            _listView.Items.Remove(item);
        }

        private string GetImageKey(string extension)
        {
            if (_imageKeyByExtension.ContainsKey(extension.ToLower()))
            {
                return _imageKeyByExtension[extension.ToLower()];
            }
            else
            {
                return "Document";
            }
        }

        public void AddFileToList(FileWrapper file, params string[] subItems)
        {            
            FileInfo info = new FileInfo(file.File.FileFullName);
            
            ListViewItem item = new ListViewItem(subItems);
            item.ImageKey = GetImageKey(file.File.Extension);
            item.Tag = file;

            _listView.Items.Add(item);
        }        

        public ListViewItem GetListViewItem(string key)
        {
            return GetListViewItem(key, _comparison);
        }

        public ListViewItem GetListViewItem(string value, ComparisonKey key)
        {
            foreach (ListViewItem item in _listView.Items)
            {
                if (key == ComparisonKey.Hash)
                {
                    if ((item.Tag as FileWrapper).File.Hash == value)
                    {
                        return item;
                    }
                }
                else if (key == ComparisonKey.Id)
                {
                    if ((item.Tag as FileWrapper).Id == value)
                    {
                        return item;
                    }
                }
                else if (key == ComparisonKey.FileFullName)
                {
                    if ((item.Tag as FileWrapper).File.FileFullName == value)
                    {
                        return item;
                    }
                }
                else throw new InvalidOperationException();
            }

            return null;
        }
    }
}
