using Foxconn.AOI.Editor.Enums;
using Foxconn.AOI.Editor.OpenCV;

namespace Foxconn.AOI.Editor.Configuration
{
    public class SMD : NotifyProperty
    {
        private int _FOV_ID { get; set; }
        private int _id { get; set; }
        private string _name { get; set; }
        private bool _isEnabled { get; set; }
        private JRect _ROI { get; set; }
        private SMDType _type { get; set; }
        private SMDAlgorithm _algorithm { get; set; }
        private CvContours _contours { get; set; }
        private CvMarkTracing _markTracing { get; set; }
        private CvFeatureMatching _featureMatching { get; set; }
        private CvTemplateMatching _templateMatching { get; set; }
        private CvCodeRecognition _codeRecognition { get; set; }
        private CvHSVExtraction _hsvExtraction { get; set; }
        private CvLuminanceExtraction _luminanceExtraction { get; set; }
        private CvLuminanceExtractionQty _luminanceExtractionQty { get; set; }
        private CvDeepLearning _deepLearning { get; set; }

        public int FOV_ID
        {
            get => _FOV_ID;
            set
            {
                _FOV_ID = value;
                NotifyPropertyChanged(nameof(FOV_ID));
            }
        }

        public int ID
        {
            get => _id;
            set
            {
                _id = value;
                NotifyPropertyChanged(nameof(ID));
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                NotifyPropertyChanged(nameof(IsEnabled));
            }
        }

        public JRect ROI
        {
            get => _ROI;
            set
            {
                _ROI = value;
                NotifyPropertyChanged(nameof(ROI));
            }
        }

        public SMDType Type
        {
            get => _type;
            set
            {
                _type = value;
                NotifyPropertyChanged(nameof(Type));
            }
        }

        public SMDAlgorithm Algorithm
        {
            get => _algorithm;
            set
            {
                _algorithm = value;
                NotifyPropertyChanged(nameof(Algorithm));
            }
        }

        public CvContours Contours
        {
            get => _contours;
            set
            {
                _contours = value;
                NotifyPropertyChanged(nameof(Contours));
            }
        }

        public CvMarkTracing MarkTracing
        {
            get => _markTracing;
            set
            {
                _markTracing = value;
                NotifyPropertyChanged(nameof(MarkTracing));
            }
        }

        public CvFeatureMatching FeatureMatching
        {
            get => _featureMatching;
            set
            {
                _featureMatching = value;
                NotifyPropertyChanged(nameof(FeatureMatching));
            }
        }

        public CvTemplateMatching TemplateMatching
        {
            get => _templateMatching;
            set
            {
                _templateMatching = value;
                NotifyPropertyChanged(nameof(TemplateMatching));
            }
        }

        public CvCodeRecognition CodeRecognition
        {
            get => _codeRecognition;
            set
            {
                _codeRecognition = value;
                NotifyPropertyChanged(nameof(CodeRecognition));
            }
        }

        public CvHSVExtraction HSVExtraction
        {
            get => _hsvExtraction;
            set
            {
                _hsvExtraction = value;
                NotifyPropertyChanged(nameof(HSVExtraction));
            }
        }

        public CvLuminanceExtraction LuminanceExtraction
        {
            get => _luminanceExtraction;
            set
            {
                _luminanceExtraction = value;
                NotifyPropertyChanged(nameof(LuminanceExtraction));
            }
        }

        public CvLuminanceExtractionQty LuminanceExtractionQty
        {
            get => _luminanceExtractionQty;
            set
            {
                _luminanceExtractionQty = value;
                NotifyPropertyChanged(nameof(LuminanceExtractionQty));
            }
        }

        public CvDeepLearning DeepLearning
        {
            get => _deepLearning;
            set
            {
                _deepLearning = value;
                NotifyPropertyChanged(nameof(DeepLearning));
            }
        }

        public SMD()
        {
            _FOV_ID = 0;
            _id = 0;
            _name = "SMD";
            _isEnabled = true;
            _ROI = new JRect(0, 0, 0, 0);
            _type = SMDType.Unknow;
            _algorithm = SMDAlgorithm.Unknow;
            _contours = new CvContours();
            _markTracing = new CvMarkTracing();
            _featureMatching = new CvFeatureMatching();
            _templateMatching = new CvTemplateMatching();
            _codeRecognition = new CvCodeRecognition();
            _hsvExtraction = new CvHSVExtraction();
            _luminanceExtraction = new CvLuminanceExtraction();
            _luminanceExtractionQty = new CvLuminanceExtractionQty();
            _deepLearning = new CvDeepLearning();
        }

        public void Dispose()
        {
            if (_featureMatching != null)
                _featureMatching.Template?.Dispose();
            if (_templateMatching != null)
                _templateMatching.Template?.Dispose();
        }
    }
}
