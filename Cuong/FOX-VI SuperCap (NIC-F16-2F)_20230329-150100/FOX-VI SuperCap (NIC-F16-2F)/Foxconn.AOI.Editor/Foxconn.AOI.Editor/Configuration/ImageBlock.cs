using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.IO;

namespace Foxconn.AOI.Editor.Configuration
{
    public class ImageBlock
    {
        private int _id { get; set; }
        private string _name { get; set; }
        private Image<Bgr, byte> _image { get; set; }
        private Rectangle _location { get; set; }
        private string _filename { get; set; }

        public int ID
        {
            get => _id;
            set => _id = value;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public Image<Bgr, byte> Image
        {
            get => _image;
            set => _image = value;
        }

        public Rectangle Location
        {
            get => _location;
            set => _location = value;
        }

        public string Filename
        {
            get => _filename;
            set => _filename = value;
        }

        public ImageBlock()
        {
            _id = 0;
            _name = string.Empty;
            _image = null;
            _location = new Rectangle();
            _filename = string.Empty;
        }

        public ImageBlock(int id, string name, Image<Bgr, byte> image)
        {
            _id = id;
            _name = name;
            if (image != null)
            {
                _image = image.Copy();
            }
            _location = new Rectangle(); ;
            _filename = @$"data\images\Image_{name}.png";
        }

        public ImageBlock(int id, string name, Image<Bgr, byte> image, Rectangle location)
        {
            _id = id;
            _name = name;
            if (image != null)
            {
                _image = image.Copy();
            }
            _location = location;
            _filename = @$"data\images\{id}_{name}.png";
        }

        public void Dispose()
        {
            _image?.Dispose();
        }

        public bool Load(string path)
        {
            try
            {
                string tempPath = $"{path}/{_filename}";
                if (File.Exists(tempPath))
                {
                    _image = new Image<Bgr, byte>(tempPath);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Save(string path)
        {
            try
            {
                string tempPath = $"{path}/{_filename}";
                FileInfo fileInfo = new FileInfo(tempPath);
                if (!Directory.Exists(fileInfo.DirectoryName))
                {
                    Directory.CreateDirectory(fileInfo.DirectoryName);
                }
                if (_image != null)
                {
                    CvInvoke.Imwrite(fileInfo.FullName, _image);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
