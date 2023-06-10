using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.TestUI.Editor;
using System;
using System.Drawing;


namespace Foxconn.TestUI.OpenCV
{
    public class CvTemplateMatching : NotifyProperty
    {
        private Image<Bgr, byte> _template { get; set; }
        private ValueRange _OKRange { get; set; }
        private bool _isEnabledReverseSearch { get; set; }
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

        public bool IsEnabledReverseSearch
        {
            get => _isEnabledReverseSearch;
            set
            {
                _isEnabledReverseSearch = value;
                NotifyPropertyChanged(nameof(IsEnabledReverseSearch));
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

        public CvTemplateMatching()
        {
            _template = null;
            _OKRange = new ValueRange(80, 100, 0, 100);
            _isEnabledReverseSearch = false;
            _center = new JPoint();
        }

        public CvResult Run(Image<Bgr, byte> src, Image<Bgr, byte> dst = null, Rectangle ROI = new Rectangle())
        {
            CvResult cvRet = new CvResult();
            try
            {
                using (var image = src.Convert<Gray, byte>())
                using (var template = _template.Convert<Gray, byte>())
                {
                    (Point loc, double s) = Matching(image, template);
                    if (IsMatched(s, _OKRange))
                    {
                        if (_isEnabledReverseSearch)
                        {
                            cvRet.Result = false;
                            cvRet.Score = s;
                            cvRet.Center = new System.Windows.Point(loc.X + ROI.X + _template.Size.Width / 2, loc.Y + ROI.Y + _template.Size.Height / 2);
                            DrawResult(ref dst, cvRet.Result, new Rectangle(loc, _template.Size));
                         //   AutoRun.Current.UpdateLogInfo(Convert.ToString("Score :" + cvRet.Score));


                        }
                        else
                        {
                            cvRet.Result = true;
                            cvRet.Score = s;
                            cvRet.Center = new System.Windows.Point(loc.X + ROI.X + _template.Size.Width / 2, loc.Y + ROI.Y + _template.Size.Height / 2);
                            DrawResult(ref dst, cvRet.Result, new Rectangle(loc, _template.Size));
                         //   AutoRun.Current.UpdateLogInfo(Convert.ToString("Score :" + cvRet.Score));

                        }
                    }
                    else
                    {
                        if (_isEnabledReverseSearch)
                        {
                            cvRet.Result = true;
                            cvRet.Score = s;
                            cvRet.Center = new System.Windows.Point(0, 0);
                            DrawResult(ref dst, cvRet.Result, Rectangle.Empty);
                        }
                        else
                        {
                            cvRet.Result = false;
                            cvRet.Score = s;
                            cvRet.Center = new System.Windows.Point(0, 0);
                            DrawResult(ref dst, cvRet.Result, Rectangle.Empty);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               // AutoRun.Current.UpdateLogError(ex.Message);
            }
            finally
            {
                src?.Dispose();
            }
            return cvRet;
        }

        private (Point, double) Matching(Image<Gray, byte> image, Image<Gray, byte> template)
        {
            if (image.Width < template.Width || image.Height < template.Height)
                return (new Point(), 0);
            Point minLoc = new Point();
            Point maxLoc = new Point();
            double minVal = 0;
            double maxVal = 0;
            using (Mat s = new Mat())
            {
                CvInvoke.MatchTemplate(image, template, s, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);
                CvInvoke.MinMaxLoc(s, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
            }
            return (maxLoc, maxVal);
        }

        private bool IsMatched(double s, ValueRange OKrange)
        {
            s *= 100;
            if (OKrange.Lower < OKrange.Upper)
            {
                if (s >= OKrange.Lower && s <= OKrange.Upper)
                {
                    return true;
                }
            }
            else
            {
                if (s >= OKrange.Lower || s < OKrange.Upper)
                {
                    return true;
                }
            }
            return false;
        }

        private void DrawResult(ref Image<Bgr, byte> dst, bool result, Rectangle match)
        {
            if (dst != null)
            {
                var rect = new Rectangle(0, 0, dst.Width, dst.Height);
                var green = new Bgr(75, 215, 50);
                var red = new Bgr(58, 69, 255);
                var blue = new Bgr(255, 132, 10);
                if (match != Rectangle.Empty)
                {
                    dst.Draw(match, blue, 2);
                }
                dst.Draw(rect, result ? green : red, 3);
            }
        }
    }
}
