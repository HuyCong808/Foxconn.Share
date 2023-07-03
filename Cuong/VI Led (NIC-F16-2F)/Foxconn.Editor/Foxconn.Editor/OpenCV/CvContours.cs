using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Foxconn.Editor.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Foxconn.Editor.OpenCV
{
    public class CvContours : NotifyProperty
    {
        private ValueRange _threshold { get; set; }
        private ValueRange _width { get; set; }
        private ValueRange _height { get; set; }
        private JPoint _P0 { get; set; }
        private JPoint _P1 { get; set; }
        private JPoint _P2 { get; set; }
        private JPoint _P3 { get; set; }

        public ValueRange Threshold
        {
            get => _threshold;
            set
            {
                _threshold = value;
                NotifyPropertyChanged(nameof(Threshold));
            }
        }

        public ValueRange Width
        {
            get => _width;
            set
            {
                _width = value;
                NotifyPropertyChanged(nameof(Width));
            }
        }

        public ValueRange Height
        {
            get => _height;
            set
            {
                _height = value;
                NotifyPropertyChanged(nameof(_height));
            }
        }

        public JPoint P0
        {
            get => _P0;
            set
            {
                _P0 = value;
                NotifyPropertyChanged(nameof(P0));
            }
        }

        public JPoint P1
        {
            get => _P1;
            set
            {
                _P1 = value;
                NotifyPropertyChanged(nameof(P1));
            }
        }

        public JPoint P2
        {
            get => _P2;
            set
            {
                _P2 = value;
                NotifyPropertyChanged(nameof(P2));
            }
        }

        public JPoint P3
        {
            get => _P3;
            set
            {
                _P3 = value;
                NotifyPropertyChanged(nameof(P3));
            }
        }

        public CvContours()
        {
            _threshold = new ValueRange(180, 255, 0, 255);
            _width = new ValueRange(10, 1000, 0, 2448);
            _height = new ValueRange(10, 1000, 0, 2048);
            _P0 = new JPoint();
            _P1 = new JPoint();
            _P2 = new JPoint();
            _P3 = new JPoint();
        }

        public CvResult Run(Image<Bgr, byte> src, Image<Bgr, byte> dst = null, Rectangle ROI = new Rectangle(), int contourType = 1)
        {
            CvResult cvRet = new CvResult();
            try
            {
                if (contourType == 1)
                {
                    cvRet = RunText(src, dst, ROI);
                }
                else if (contourType == 2)
                {
                    cvRet = RunBox(src, dst, ROI);
                }
                else
                {
                    cvRet = RunTextBox(src, dst, ROI);
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

        public CvResult RunText(Image<Bgr, byte> src, Image<Bgr, byte> dst = null, Rectangle ROI = new Rectangle())
        {
            CvResult cvRet = new CvResult();
            try
            {
                using (var gray = ThresholdMask(src))
                using (var contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(gray, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);
                    var lstPoint = new List<PointF>();
                    for (int i = 0; i < contours.Size; i++)
                    {
                        var rect = CvInvoke.BoundingRectangle(contours[i]);
                        if (rect.Width > _width.Lower && rect.Width < _width.Upper && rect.Height > _height.Lower && rect.Height < _height.Upper)
                        {
                            var points = contours[i].ToArray();
                            foreach (var item in points)
                            {
                                lstPoint.Add(new PointF(item.X, item.Y));
                            }
                            if (dst != null)
                            {
                                CvInvoke.DrawContours(dst, contours, i, new MCvScalar(255, 0, 0), 2);
                            }
                        }
                    }
                    if (lstPoint.Count > 0)
                    {
                        PointF[] hull = CvInvoke.ConvexHull(lstPoint.ToArray(), true);
                        RotatedRect area = CvInvoke.MinAreaRect(hull);
                        System.Windows.Point[] temp = ConvertVertices(area.GetVertices());
                        System.Windows.Point[] vertices = OrderVertices(temp);
                        System.Windows.Point[] points = CopyPoints(vertices);
                        System.Windows.Point center = CenterRect(vertices);
                        if (dst != null)
                        {
                            dst.Draw(area, new Bgr(0, 0, 255), 2);
                        }
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            var p = new System.Windows.Point
                            {
                                X = vertices[i].X + ROI.X,
                                Y = vertices[i].Y + ROI.Y
                            };
                            cvRet.Points[i] = p;
                        }
                        var c = new System.Windows.Point
                        {
                            X = center.X + ROI.X,
                            Y = center.Y + ROI.Y
                        };
                        cvRet.Result = vertices.Length == 4;
                        cvRet.Center = c;
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

        public CvResult RunBox(Image<Bgr, byte> src, Image<Bgr, byte> dst = null, Rectangle ROI = new Rectangle())
        {
            CvResult cvRet = new CvResult();
            try
            {
                using (var gray = ThresholdMask(src))
                using (var contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(gray, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
                    if (contours.Size > 0)
                    {
                        int largest_idx = -1;
                        double maxSize = 0;
                        for (int i = 0; i < contours.Size; i++)
                        {
                            double size = CvInvoke.ContourArea(contours[i]);
                            if (size > maxSize)
                            {
                                maxSize = size;
                                largest_idx = i;
                            }
                        }
                        if (dst != null)
                        {
                            CvInvoke.DrawContours(dst, contours, largest_idx, new MCvScalar(255, 0, 0), 2);
                        }
                        RotatedRect area = CvInvoke.MinAreaRect(contours[largest_idx]);
                        System.Windows.Point[] temp = ConvertVertices(area.GetVertices());
                        System.Windows.Point[] vertices = OrderVertices(temp);
                        System.Windows.Point[] points = CopyPoints(vertices);
                        System.Windows.Point center = CenterRect(vertices);
                        if (dst != null)
                        {
                            dst.Draw(area, new Bgr(0, 0, 255), 2);
                        }
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            var p = new System.Windows.Point
                            {
                                X = vertices[i].X + ROI.X,
                                Y = vertices[i].Y + ROI.Y
                            };
                            cvRet.Points[i] = p;
                        }
                        var c = new System.Windows.Point
                        {
                            X = center.X + ROI.X,
                            Y = center.Y + ROI.Y
                        };
                        cvRet.Result = vertices.Length == 4;
                        cvRet.Center = c;
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

        public CvResult RunTextBox(Image<Bgr, byte> src, Image<Bgr, byte> dst = null, Rectangle ROI = new Rectangle())
        {
            CvResult cvRet = new CvResult();
            try
            {
                using (var gray = ThresholdMask(src))
                using (var contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(gray, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);
                    var lstPoint = new List<PointF>();
                    for (int i = 0; i < contours.Size; i++)
                    {
                        var rect = CvInvoke.BoundingRectangle(contours[i]);
                        if (rect.Width > _width.Lower && rect.Width < _width.Upper && rect.Height > _height.Lower && rect.Height < _height.Upper)
                        {
                            var points = contours[i].ToArray();
                            foreach (var item in points)
                            {
                                lstPoint.Add(new PointF(item.X, item.Y));
                            }

                            if (dst != null)
                            {
                                CvInvoke.DrawContours(dst, contours, i, new MCvScalar(255, 0, 0), 2);
                            }
                        }
                    }
                    if (lstPoint.Count > 0)
                    {
                        var sortedX = lstPoint.OrderBy(i => i.X).ToList();
                        var sortedY = lstPoint.OrderBy(i => i.Y).ToList();

                        System.Windows.Point top_left = new System.Windows.Point()
                        {
                            X = sortedX[0].X,
                            Y = sortedY[0].Y
                        };

                        System.Windows.Point bottom_right = new System.Windows.Point()
                        {
                            X = sortedX[sortedX.Count - 1].X,
                            Y = sortedY[sortedX.Count - 1].Y
                        };

                        var temp = new System.Windows.Point[]
                        {
                            new System.Windows.Point() {X = top_left.X, Y = top_left.Y},
                            new System.Windows.Point() {X = bottom_right.X, Y = top_left.Y},
                            new System.Windows.Point() {X = bottom_right.X, Y = bottom_right.Y},
                            new System.Windows.Point() {X = top_left.X, Y = bottom_right.Y},
                        };
                        System.Windows.Point[] vertices = OrderVertices(temp);
                        System.Windows.Point[] points = CopyPoints(vertices);
                        System.Windows.Point center = CenterRect(vertices);
                        if (dst != null)
                        {
                            PointF[] hull = CvInvoke.ConvexHull(lstPoint.ToArray(), true);
                            RotatedRect area = CvInvoke.MinAreaRect(hull);
                            dst.Draw(area, new Bgr(0, 0, 255), 2);
                        }
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            var p = new System.Windows.Point
                            {
                                X = vertices[i].X + ROI.X,
                                Y = vertices[i].Y + ROI.Y
                            };
                            cvRet.Points[i] = p;
                        }
                        var c = new System.Windows.Point
                        {
                            X = center.X + ROI.X,
                            Y = center.Y + ROI.Y
                        };
                        cvRet.Result = vertices.Length == 4;
                        cvRet.Center = c;
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

        private System.Windows.Point[] ConvertVertices(PointF[] src)
        {
            System.Windows.Point[] dst = new System.Windows.Point[src.Length];
            for (int i = 0; i < dst.Length; i++)
            {
                dst[i] = new System.Windows.Point
                {
                    X = src[i].X,
                    Y = src[i].Y
                };
            }
            return dst;
        }

        private System.Windows.Point[] OrderVertices(System.Windows.Point[] src)
        {
            int id = 0;
            double distance = 0;
            for (int i = 0; i < src.Length; i++)
            {
                double _distance = Math.Sqrt(Math.Pow(src[i].X, 2) + Math.Pow(src[i].Y, 2));
                if (_distance > distance)
                {
                    distance = _distance;
                    id = i;
                }
            }

            System.Windows.Point[] dst = new System.Windows.Point[src.Length];
            for (int i = 0; i < dst.Length; i++)
            {
                // Because the top left and bottom right away distance two index so plus 2
                int _id = (id + src.Length + i + 2) % 4;
                dst[i] = src[_id];
            }

            return dst;
        }

        private System.Windows.Point[] CopyPoints(System.Windows.Point[] src)
        {
            if (src != null)
            {
                System.Windows.Point[] dst = new System.Windows.Point[src.Length];
                for (int i = 0; i < src.Length; i++)
                {
                    dst[i] = src[i];
                }
                return dst;
            }
            else
            {
                return null;
            }
        }

        private System.Windows.Point CenterRect(System.Windows.Point[] points)
        {
            System.Windows.Point top_left = new System.Windows.Point()
            {
                X = points[0].X,
                Y = points[0].Y
            };

            System.Windows.Point bottom_right = new System.Windows.Point()
            {
                X = points[2].X,
                Y = points[2].Y
            };

            return new System.Windows.Point
            {
                X = top_left.X + (bottom_right.X - top_left.X) / 2,
                Y = top_left.Y + (bottom_right.Y - top_left.Y) / 2
            };
        }
    }
}
