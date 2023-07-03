using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.Editor.Configuration;
using System;
using System.Diagnostics;
using System.Drawing;

namespace Foxconn.Editor.OpenCV
{
    public class CvFeatureMatching : NotifyProperty
    {
        private Image<Bgr, byte> _template { get; set; }
        private ValueRange _OKRange { get; set; }
        private bool _isEnableReverseSearch { get; set; }
        private JPoint _center { get; set; }

        public Image<Bgr, byte> Template
        {
            get => _template;
            set
            {
                _template = value;
                NotifyPropertyChanged(nameof(Template));
            }
        }

        public ValueRange OKRange
        {
            get => _OKRange;
            set
            {
                _OKRange = value;
                NotifyPropertyChanged(nameof(OKRange));
            }
        }

        public bool IsEnableReverseSearch
        {
            get => _isEnableReverseSearch;
            set
            {
                _isEnableReverseSearch = value;
                NotifyPropertyChanged(nameof(IsEnableReverseSearch));
            }
        }

        public JPoint Center
        {
            get => _center;
            set
            {
                _center = value;
                NotifyPropertyChanged(nameof(Center));
            }
        }

        public CvFeatureMatching()
        {
            _template = null;
            _OKRange = new ValueRange(80, 100, 0, 100);
            _isEnableReverseSearch = false;
            _center = new JPoint();
        }

        public CvResult Run(Image<Bgr, byte> src, Image<Bgr, byte> dst = null, Rectangle ROI = new Rectangle())
        {
            CvResult cvRet = new CvResult();
            try
            {

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                src?.Dispose();
            }
            return cvRet;
        }
    }
}
