using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Diagnostics;
using System.Drawing;

namespace Foxconn.Editor.OpenCV
{
    public class CvLuminanceExtraction : NotifyProperty
    {
        private ValueRange _threshold { get; set; }
        private ValueRange _OKRange { get; set; }
        private bool _isEnabledReverseSearch { get; set; }

        public ValueRange Threshold
        {
            get => _threshold;
            set
            {
                _threshold = value;
                NotifyPropertyChanged(nameof(Threshold));
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

        public CvLuminanceExtraction()
        {
            _threshold = new ValueRange(180, 255, 0, 255);
            _OKRange = new ValueRange(80, 100, 0, 100);
            _isEnabledReverseSearch = false;
        }

        public CvResult Run(Image<Bgr, byte> src, Image<Bgr, byte> dst = null, Rectangle ROI = new Rectangle())
        {
            CvResult cvRet = new CvResult();
            try
            {
                using (var mask = ThresholdMask(src))
                {
                    int total = src.Width * src.Height;
                    int qty = CvInvoke.CountNonZero(mask);
                    double score = (double)qty / total;
                    if (IsMatched(score, _OKRange))
                    {
                        cvRet.Result = _isEnabledReverseSearch ? false : true;
                    }
                    else
                    {
                        cvRet.Result = _isEnabledReverseSearch ? true : false;
                    }
                    cvRet.Score = score;
                    cvRet.Qty = qty;
                    if (dst != null)
                    {
                        DrawResult(ref dst, cvRet.Result);
                    }
                }
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

        public CvResult Preview(Image<Bgr, byte> src, Image<Bgr, byte> dst = null, Rectangle ROI = new Rectangle())
        {
            CvResult cvRet = new CvResult();
            try
            {
                using (var mask = ThresholdMask(src))
                {
                    int total = src.Width * src.Height;
                    int qty = CvInvoke.CountNonZero(mask);
                    double score = (double)qty / total;
                    if (IsMatched(score, _OKRange))
                    {
                        cvRet.Result = _isEnabledReverseSearch ? false : true;
                    }
                    else
                    {
                        cvRet.Result = _isEnabledReverseSearch ? true : false;
                    }
                    cvRet.Score = score;
                    cvRet.Qty = qty;
                    if (dst != null)
                    {
                        DrawResult(ref dst, cvRet.Result);
                        CvInvoke.CvtColor(mask, dst, ColorConversion.Gray2Bgr);
                    }
                }
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

        private Image<Gray, byte> ThresholdMask(Image<Bgr, byte> src)
        {
            double low = _threshold.Lower;
            double up = _threshold.Upper;
            Image<Gray, byte> dst = null;
            using (var gray = src.Convert<Gray, byte>())
            {
                if (low < up)
                {
                    dst = gray.InRange(new Gray(low), new Gray(up));
                }
                else
                {
                    dst = new Image<Gray, byte>(gray.Size);
                    CvInvoke.BitwiseOr(gray.InRange(new Gray(low), new Gray(255)), gray.InRange(new Gray(0), new Gray(up)), dst);
                }
            }
            return dst;
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

        private void DrawResult(ref Image<Bgr, byte> dst, bool bRet)
        {
            if (dst != null)
            {
                Rectangle rect = new Rectangle(0, 0, dst.Width, dst.Height);
                Bgr color = bRet ? new Bgr(75, 215, 50) : new Bgr(58, 69, 255);
                dst.Draw(rect, color, 3);
            }
        }
    }
}
