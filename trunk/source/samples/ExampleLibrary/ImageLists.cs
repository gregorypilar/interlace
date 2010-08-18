using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ExampleLibrary
{
    public static class ImageLists
    {
        static ImageList _fileImages_Small;
        static ImageList _fileImages_Large;

        public static ImageList FileImages_Small
        {
            get
            {
                if (_fileImages_Small == null)
                {
                    _fileImages_Small = new ImageList();
                    _fileImages_Small.ImageSize = new System.Drawing.Size(16, 16);
                    _fileImages_Small.ColorDepth = ColorDepth.Depth32Bit;

                    _fileImages_Small.Images.Add("Document", Images.Document);
                    _fileImages_Small.Images.Add("Audio", Images.Audio);
                    _fileImages_Small.Images.Add("Image", Images.Image);
                    _fileImages_Small.Images.Add("ISO", Images.ISO);
                    _fileImages_Small.Images.Add("Movie", Images.Movie);
                    _fileImages_Small.Images.Add("Text", Images.Text);
                    _fileImages_Small.Images.Add("System", Images.System);
                }

                return _fileImages_Small;
            }
        }

        public static ImageList FileImages_Large
        {
            get
            {
                if (_fileImages_Large == null)
                {
                    _fileImages_Large = new ImageList();
                    _fileImages_Large.ImageSize = new System.Drawing.Size(16, 16);
                    _fileImages_Large.ColorDepth = ColorDepth.Depth32Bit;

                    _fileImages_Large.Images.Add("Document", Images.Document);
                    _fileImages_Large.Images.Add("Audio", Images.Audio);
                    _fileImages_Large.Images.Add("Image", Images.Image);
                    _fileImages_Large.Images.Add("ISO", Images.ISO);
                    _fileImages_Large.Images.Add("Movie", Images.Movie);
                    _fileImages_Large.Images.Add("Text", Images.Text);
                    _fileImages_Large.Images.Add("System", Images.System);
                }

                return _fileImages_Large;
            }
        }
    }
}
