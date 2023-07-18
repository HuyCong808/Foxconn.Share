using Foxconn.Editor.Enums;
using Foxconn.Editor.OpenCV;

namespace Foxconn.Editor.Configuration
{
    public class SMD : NotifyProperty
    {
        private int _FOV_ID { get; set; }
        private int _id { get; set; }
        private string _name { get; set; }
        private bool _isEnabled { get; set; }
        private SMDAlgorithm _algorithm { get; set; }
        private SMDType _SMDType { get; set; }
        private CvCodeRecognition _codeRecognition { get; set; }
        private CvTemplateMatching _templateMatching { get; set; }
        private CvHSVExtraction _hsvExtraction { get; set; }
        private CvLuminanceExtraction _luminanceExtraction { get; set; }
        private CvLuminanceExtractionQty _luminanceExtractionQty { get; set; }
        private JRect _ROI { get; set; }


        public int FOV_ID
        {
            get => _FOV_ID;
            set
            {
                _FOV_ID = value;
                NotifyPropertyChanged(nameof(FOV_ID));
            }
        }

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                NotifyPropertyChanged(nameof(Id));
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

        public SMDAlgorithm Algorithm
        {
            get => _algorithm;
            set
            {
                _algorithm = value;
                NotifyPropertyChanged(nameof(Algorithm));
            }
        }

        public SMDType SMDType
        {
            get => _SMDType;
            set
            {
                _SMDType = value;
                NotifyPropertyChanged(nameof(SMDType));
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

        public CvTemplateMatching TemplateMatching
        {
            get => _templateMatching;
            set
            {
                _templateMatching = value;
                NotifyPropertyChanged(nameof(TemplateMatching));
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
        public JRect ROI
        {
            get => _ROI;
            set
            {
                _ROI = value;
                NotifyPropertyChanged(nameof(ROI));
            }
        }

        public SMD()
        {
            _FOV_ID = 0;
            _id = -1;
            _name = "";
            _isEnabled = true;
            _algorithm = 0;
            _SMDType = 0;
            _codeRecognition = new CvCodeRecognition();
            _templateMatching = new CvTemplateMatching();
            _hsvExtraction = new CvHSVExtraction();
            _luminanceExtraction = new CvLuminanceExtraction();
            _luminanceExtractionQty = new CvLuminanceExtractionQty();
            _ROI = new JRect(0, 0, 0, 0);
        }


    }
}
