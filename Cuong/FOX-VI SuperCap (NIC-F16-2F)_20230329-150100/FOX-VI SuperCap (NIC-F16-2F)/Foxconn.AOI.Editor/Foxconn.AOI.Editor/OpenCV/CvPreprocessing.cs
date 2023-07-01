namespace Foxconn.AOI.Editor.OpenCV
{
    public class CvPreprocessing : NotifyProperty
    {
        private ThresholdImage _threshold { get; set; }
        private BlurImage _blur { get; set; }
        private BlobImage _blob { get; set; }

        public ThresholdImage Threshold
        {
            get => _threshold;
            set
            {
                _threshold = value;
                NotifyPropertyChanged(nameof(Threshold));
            }
        }

        public BlurImage Blur
        {
            get => _blur;
            set
            {
                _blur = value;
                NotifyPropertyChanged(nameof(Blur));
            }
        }

        public BlobImage Blob
        {
            get => _blob;
            set
            {
                _blob = value;
                NotifyPropertyChanged(nameof(Blob));
            }
        }

        public CvPreprocessing()
        {
            _threshold = new ThresholdImage();
            _blur = new BlurImage();
            _blob = new BlobImage();
        }
    }

    public class ThresholdImage : NotifyProperty
    {
        private bool _enable { get; set; }
        private int _value { get; set; }
        private bool _inverted { get; set; }

        public bool Enable
        {
            get => _enable;
            set
            {
                _enable = value;
                NotifyPropertyChanged(nameof(Enable));
            }
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                NotifyPropertyChanged(nameof(Value));
            }
        }

        public bool Inverted
        {
            get => _inverted;
            set
            {
                _inverted = value;
                NotifyPropertyChanged(nameof(Inverted));
            }
        }

        public ThresholdImage()
        {
            _enable = false;
            _value = 180;
            _inverted = false;
        }
    }

    public class BlurImage : NotifyProperty
    {
        private bool _enable { get; set; }
        private int _value { get; set; }

        public bool Enable
        {
            get => _enable;
            set
            {
                _enable = value;
                NotifyPropertyChanged(nameof(Enable));
            }
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                NotifyPropertyChanged(nameof(Value));
            }
        }

        public BlurImage()
        {
            _enable = false;
            _value = 3;
        }
    }

    public class BlobImage : NotifyProperty
    {
        private bool _enable { get; set; }
        private int _value { get; set; }
        private int _dilate { get; set; }
        private int _erode { get; set; }

        public bool Enable
        {
            get => _enable;
            set
            {
                _enable = value;
                NotifyPropertyChanged(nameof(Enable));
            }
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                NotifyPropertyChanged(nameof(Value));
            }
        }

        public int Dilate
        {
            get => _dilate;
            set
            {
                _dilate = value;
                NotifyPropertyChanged(nameof(Dilate));
            }
        }

        public int Erode
        {
            get => _erode;
            set
            {
                _erode = value;
                NotifyPropertyChanged(nameof(Erode));
            }
        }

        public BlobImage()
        {
            _enable = false;
            _value = 3;
            _dilate = 1;
            _erode = 2;
        }
    }
}
