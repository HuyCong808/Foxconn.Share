using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Foxconn.AOI.Editor.Configuration;
using System;
using System.Drawing;

namespace Foxconn.AOI.Editor.OpenCV
{
    public class CvMarkTracing : NotifyProperty
    {
        private ValueRange _threshold { get; set; }
        private ValueRange _OKRange { get; set; }
        private double _radius { get; set; }
        private double _angle { get; set; }
        private JPoint _center { get; set; }

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

        public double Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                NotifyPropertyChanged(nameof(Radius));
            }
        }

        public double Angle
        {
            get => _angle;
            set
            {
                _angle = value;
                NotifyPropertyChanged(nameof(Angle));
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

        public CvMarkTracing()
        {
            _threshold = new ValueRange(127, 255, 0, 255);
            _OKRange = new ValueRange(80, 100, 0, 100);
            _angle = 0;
            _radius = 0;
            _center = new JPoint();
        }

        public CvResult Run(Image<Bgr, byte> src, Image<Bgr, byte> dst = null, Rectangle ROI = new Rectangle())
        {
            CvResult cvRet = new CvResult();
            try
            {
                double radius = _radius;
                using (var gray = src.Convert<Gray, byte>())
                {
                    using (var mask = ThresholdMask(gray))
                    {
                        (double score, Point[] contours) = ContourMaxScore(mask, radius);
                        if (contours.Length > 0)
                        {
                            CircleF circle = CvInvoke.MinEnclosingCircle(new VectorOfPoint(contours));
                            cvRet.Result = IsMatched(Math.Round(score, 6), _OKRange);
                            cvRet.Score = Math.Round(score, 6);
                            cvRet.Center = new System.Windows.Point(circle.Center.X + ROI.X, circle.Center.Y + ROI.Y);
                            cvRet.Radius = circle.Radius;
                            if (dst != null)
                            {
                                dst.Draw(new Rectangle(0, 0, ROI.Width, ROI.Height), new Bgr(40, 205, 65), 2);
                                dst.Draw(circle, new Bgr(0, 0, 255), 2);
                                CvInvoke.Circle(dst, Point.Round(circle.Center), 2, new MCvScalar(0, 0, 255), 2);
                                CvInvoke.PutText(dst, $"{circle.Center.X + ROI.X:#.####}, {circle.Center.Y + ROI.Y:#.####}", Point.Round(circle.Center), FontFace.HersheyPlain, 2, new MCvScalar(0, 255, 0), 2);
                            }
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

        private (double, Point[]) ContourMaxScore(Image<Gray, byte> src, double radius)
        {
            double maxScore = 0;
            Point[] p = new Point[0];
            using (var contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(src, contours, null, RetrType.Ccomp, ChainApproxMethod.ChainApproxSimple);
                double sc = Math.PI * radius * radius;
                int maxScoreID = -1;
                for (int i = 0; i < contours.Size; i++)
                {
                    CircleF circle = CvInvoke.MinEnclosingCircle(contours[i]);
                    int numNonZero = 0;
                    using (var gray = new Image<Gray, byte>(src.Size))
                    {
                        CvInvoke.Circle(gray, new Point((int)circle.Center.X, (int)circle.Center.Y), Convert.ToInt32(circle.Radius), new MCvScalar(255), -1);
                        CvInvoke.BitwiseAnd(gray, src, gray);
                        numNonZero = CvInvoke.CountNonZero(gray);
                    }
                    double s = circle.Area;
                    double score1 = Math.Min(s, sc) / Math.Max(s, sc);
                    double score2 = Math.Min(numNonZero, sc) / Math.Max(numNonZero, sc);
                    double score = Math.Min(score1, score2);
                    if (maxScore < score)
                    {
                        maxScore = score;
                        maxScoreID = i;
                        p = contours[i].ToArray();
                    }
                }
            }
            return (maxScore, p);
        }

        private Image<Gray, byte> ThresholdMask(Image<Gray, byte> src)
        {
            double low = _threshold.Lower;
            double up = _threshold.Upper;
            Image<Gray, byte> dst;
            if (low < up)
            {
                dst = src.InRange(new Gray(low), new Gray(up));
            }
            else
            {
                dst = src.InRange(new Gray(up), new Gray(255));
                using (var image = src.InRange(new Gray(0), new Gray(low)))
                {
                    CvInvoke.BitwiseOr(dst, image, dst);
                }
            }
            return dst;
        }

        private bool IsMatched(double s, ValueRange OKRange)
        {
            s *= 100;
            if (OKRange.Lower < OKRange.Upper)
            {
                if (s >= OKRange.Lower && s <= OKRange.Upper)
                {
                    return true;
                }
            }
            else
            {
                if (s >= OKRange.Lower || s < OKRange.Upper)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
