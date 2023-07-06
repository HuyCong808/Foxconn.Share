using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
namespace Foxconn.Editor.OpenCV
{
    public class CvHSVExtraction : NotifyProperty
    {
        private ValueRange _hue { get; set; }
        private ValueRange _saturation { get; set; }
        private ValueRange _value { get; set; }
        private ValueRange _OKRange { get; set; }
        private double _score { get; set; }
        private bool _isEnabledReverseSearch { get; set; }

        public ValueRange Hue
        {
            get => _hue;
            set
            {
                _hue = value;
                NotifyPropertyChanged(nameof(Hue));
            }
        }

        public ValueRange Saturation
        {
            get => _saturation;
            set
            {
                _saturation = value;
                NotifyPropertyChanged(nameof(Saturation));
            }
        }
        public ValueRange Value
        {
            get => _value;
            set
            {
                _value = value;
                NotifyPropertyChanged(nameof(Value));
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

        public double Score
        {
            get => _score;
            set
            {
                _score = value;
                NotifyPropertyChanged(nameof(Score));
            }
        }

        public bool IsEnabledReserveSearch
        {
            get => _isEnabledReverseSearch;
            set
            {
                _isEnabledReverseSearch = value;
                NotifyPropertyChanged(nameof(IsEnabledReserveSearch));
            }

        }
        public CvHSVExtraction()
        {
            _hue = new ValueRange(0, 150, 0, 255);
            _saturation = new ValueRange(0, 200, 0, 255);
            _value = new ValueRange(50, 255, 0, 255);
            _OKRange = new ValueRange(80, 100, 0, 100);
            _score = 0;
            _isEnabledReverseSearch = false;
        }

        public CvResult Run(Image<Bgr, byte> src, Image<Bgr, byte> dst = null, Rectangle ROI = new Rectangle())
        {
            CvResult cvRet = new CvResult();
            try
            {
                using (var mask = ThresholdHSV(src))
                {
                    int countMask = src.Width * src.Height;
                    int countHSV = CvInvoke.CountNonZero(mask);
                    double score = (double)countHSV / countMask;
                    if (IsMatched(score, _OKRange))
                    {
                        cvRet.Result = _isEnabledReverseSearch ? false : true;
                    }
                    else
                    {
                        cvRet.Result = _isEnabledReverseSearch ? true : false;
                    }
                    cvRet.Score = score;
                    cvRet.Qty = countHSV;
                    if (dst != null)
                    {
                        DrawResult(ref dst, cvRet.Result);
                    }
                }
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

        public CvResult Preview(Image<Bgr, byte> src, Image<Bgr, byte> dst = null, Rectangle ROI = new Rectangle())
        {
            CvResult cvRet = new CvResult();
            try
            {
                using (var mask = ThresholdHSV(src))
                {
                    int countMask = src.Width * src.Height;
                    int countHSV = CvInvoke.CountNonZero(mask);
                    double score = (double)countHSV / countMask;
                    if (IsMatched(score, _OKRange))
                    {
                        cvRet.Result = _isEnabledReverseSearch ? false : true;
                    }
                    else
                    {
                        cvRet.Result = _isEnabledReverseSearch ? true : false;
                    }
                    cvRet.Score = score;
                    cvRet.Qty = countHSV;
                    if (dst != null)
                    {
                        DrawResult(ref dst, cvRet.Result);
                        CvInvoke.ColorChange(dst, mask, dst, 255, 255, 255);
                        using (Image<Bgr, byte> maskRGB = new Image<Bgr, byte>(src.Size))
                        {
                            CvInvoke.CvtColor(mask, maskRGB, ColorConversion.Gray2Bgr);
                            CvInvoke.BitwiseAnd(src, maskRGB, dst);
                        }
                    }
                }
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


        public (ValueRange, ValueRange, ValueRange) HSVRange(Image<Bgr, byte> src)
        {
            var Hue = new ValueRange(0, 0, 0, 255);
            var Saturation = new ValueRange(0, 0, 0, 255);
            var Value = new ValueRange(0, 0, 0, 255);
            using (var dst = new Mat())
            {
                CvInvoke.CvtColor(src, dst, ColorConversion.Bgr2Hsv);
                Mat[] channels = dst.Split();
                RangeF H = channels[0].GetValueRange();
                RangeF S = channels[1].GetValueRange();
                RangeF V = channels[2].GetValueRange();
                Hue.Lower = (int)H.Min;
                Hue.Upper = (int)H.Max;
                Saturation.Lower = (int)S.Min;
                Saturation.Upper = (int)S.Max;
                Value.Lower = (int)V.Min;
                Value.Upper = (int)V.Max;
            };
            return (Hue, Saturation, Value);
        }

        private Image<Gray, byte> ThresholdHSV(Image<Bgr, byte> src)
        {
            double lowerH = _hue.Lower;
            double upperH = _hue.Upper;
            double lowerS = _saturation.Lower;
            double upperS = _saturation.Upper;
            double lowerV = _value.Lower;
            double upperV = _value.Upper;
            Image<Gray, byte> dst = null;
            using (var hsv = src.Convert<Hsv, byte>())
            {
                if (lowerH < upperH)
                {
                    dst = hsv.InRange(new Hsv(lowerH, Math.Min(lowerS, upperS), Math.Min(lowerV, upperV)), new Hsv(upperH, Math.Max(lowerS, upperS), Math.Max(lowerV, upperV)));
                }
                else
                {
                    dst = hsv.InRange(new Hsv(lowerH, Math.Min(lowerS, upperS), Math.Min(lowerV, upperV)), new Hsv(179, Math.Max(lowerS, upperS), Math.Max(lowerV, upperV)));
                    using (var dst2 = hsv.InRange(new Hsv(0, Math.Min(lowerS, upperS), Math.Min(lowerV, upperV)), new Hsv(upperH, Math.Max(lowerS, upperS), Math.Max(lowerV, upperV))))
                    {
                        CvInvoke.BitwiseOr(dst, dst2, dst);
                    }
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
            Rectangle rect = new Rectangle(0, 0, dst.Width, dst.Height);
            Bgr color = bRet ? new Bgr(75, 215, 50) : new Bgr(58, 69, 255);
            dst.Draw(rect, color, 3);
        }

    }
}
