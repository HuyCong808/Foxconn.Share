using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;

namespace Foxconn.AOI.Editor.OpenCV
{
    public class CvDeepLearning : NotifyProperty
    {
        private string _service { get; set; }
        private ValueRange _OKRange { get; set; }

        public string Service
        {
            get => _service;
            set
            {
                _service = value;
                NotifyPropertyChanged(nameof(Service));
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

        public CvDeepLearning()
        {
            _service = string.Empty;
            _OKRange = new ValueRange(80, 100, 0, 100);
        }

        public CvResult Run(Image<Bgr, byte> src, Image<Bgr, byte> dst = null, Rectangle ROI = new Rectangle())
        {
            CvResult cvRet = new CvResult();
            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                src?.Dispose();
            }
            return cvRet;
        }
    }
}
