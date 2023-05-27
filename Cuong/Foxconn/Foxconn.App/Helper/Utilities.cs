using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Foxconn.App.Helper
{
    public class Utilities
    {
        public static string GetDateModified()
        {
            try
            {
                var mainModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
                if (mainModule != null)
                {
                    var fileName = mainModule.FileName;
                    if (fileName != null)
                    {
                        return File.GetLastWriteTime(fileName).ToString("dddd, dd MMMM yyyy hh:mm:ss tt");
                    }
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetProductVersion()
        {
            try
            {
                var mainModule = System.Diagnostics.Process.GetCurrentProcess().MainModule;
                if (mainModule != null)
                {
                    var fileVersionInfo = mainModule.FileVersionInfo;
                    if (fileVersionInfo != null)
                    {
                        var productVersion = fileVersionInfo.ProductVersion;
                        if (productVersion != null)
                        {
                            return productVersion;
                        }
                    }
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static int ConvertStringToInt(string data)
        {
            if (int.TryParse(data, out int value))
            {
                return value;
            }
            else
            {
                AppUi.ShowMessage("Not number.", System.Windows.MessageBoxImage.Error);
            }
            return -1;
        }

        public static double ConvertStringToDouble(string data)
        {
            if (double.TryParse(data, out double value))
            {
                return value;
            }
            else
            {
                AppUi.ShowMessage("Not number.", System.Windows.MessageBoxImage.Error);
            }
            return -1.0;
        }

        public class Util
        {
            public static Point[] CopyPoints(Point[] src)
            {
                if (src != null)
                {
                    var pdst = new Point[src.Length];
                    for (int i = 0; i < src.Length; i++)
                    {
                        pdst[i] = src[i];
                    }
                    return pdst;
                }
                else
                {
                    return null;
                }
            }

            public static Point[] GetVerticesFromRectangle(Rectangle rect)
            {
                return new Point[4]
                {
                    new Point(rect.X, rect.Y),
                    new Point(rect.Right, rect.Y),
                    new Point(rect.Right, rect.Bottom),
                    new Point(rect.X, rect.Bottom)
                };
            }
            /// <summary>
            /// Calculate angle bewteen two vectors. Unit degree
            /// </summary>
            /// <param name="lstA">Vector made by two point</param>
            /// <param name="lstB">Vector made by two point</param>
            /// <param name="direction">If direction is true: return angle with orientation</param>
            /// <returns>angle between two vectors</returns>
            public static double CalcAngle(Point[] lstA, Point[] lstB, bool direction = true)
            {
                double angle = 0;
                var vt_AB = new Point(lstA[0].X - lstA[1].X, lstA[0].Y - lstA[1].Y);
                var vt_CD = new Point(lstB[0].X - lstB[1].X, lstB[0].Y - lstB[1].Y);
                var cos_anpha = (vt_AB.X * vt_CD.X + vt_AB.Y * vt_CD.Y)
                    / Math.Sqrt(
                        (Math.Pow(vt_AB.X, 2) + Math.Pow(vt_AB.Y, 2))
                        * (Math.Pow(vt_CD.X, 2) + Math.Pow(vt_CD.Y, 2))
                    );
                angle = Math.Acos(cos_anpha) * 180 / Math.PI;
                // check direction: compare by y
                if (direction)
                {
                    // move vector CD -> AB. C is same with A, D -> D2
                    // lstA[0] is A, lstA[1] is B
                    // lstB[0] is C, lstB[1] is D
                    var D2 = new Point()
                    {
                        X = lstB[1].X + (lstA[0].X - lstB[0].X),
                        Y = lstB[1].Y + (lstA[0].Y - lstB[0].Y)
                    };
                    // if D2 has X is smaller than B
                    if (D2.X < lstA[1].X)
                    {
                        angle = -angle;
                    }
                }
                return angle;
            }

            public static PointF GetCircleByThreePoint(Point P1, Point P2, Point P3, ref double Radius)
            {
                // Phuong trinh duong thang p1, p2
                float dt12a = P1.X - P2.X == 0 ? 0 : (P1.Y - P2.Y) / (P1.X - P2.X);
                float dt12b = P2.Y - dt12a * P2.X;
                // Phuong trinh duong thang p2, p3
                float dt23a = P3.X - P2.X == 0 ? 0 : (P3.Y - P2.Y) / (P3.X - P2.X);
                float dt23b = P2.Y - dt12a * P2.X;
                // Tim trung diem
                PointF mid12 = new PointF((float)(P1.X + P2.X) / 2, (float)(P1.Y + P2.Y) / 2);
                PointF mid23 = new PointF((float)(P3.X + P2.X) / 2, (float)(P3.Y + P2.Y) / 2);
                // Tim duong thang vuong goc di qua trung diem
                dt12a = dt12a == 0 ? 0 : -1 / dt12a;
                dt12b = mid12.Y - dt12a * mid12.X;
                dt23a = dt23a == 0 ? 0 : -1 / dt23a;
                dt23b = mid23.Y - dt23a * mid23.X;
                // Tinh tam va ban kinh
                PointF Center = new PointF();
                Center.X = dt12a - dt23a == 0 ? 0 : (dt23b - dt12b) / (dt12a - dt23a);
                Center.Y = dt23a * Center.X + dt23b;
                Radius = (float)Math.Sqrt(Math.Pow(P1.X - Center.X, 2) + Math.Pow(P1.Y - Center.Y, 2));
                return Center;
            }
            /// <summary>
            /// Order 4 vertices of RotatedRect in direction: top left -> top right -> bottom right -> bottom left.
            /// </summary>
            /// <param name="lstPoint"></param>
            /// <returns></returns>
            public static Point[] OrderVertices(Point[] lstPoint)
            {
                int idx = 0; double distance = 0;
                for (int i = 0; i < lstPoint.Length; i++)
                {
                    var dis = Math.Sqrt(Math.Pow(lstPoint[i].X, 2) + Math.Pow(lstPoint[i].Y, 2));
                    if (dis > distance)
                    {
                        distance = dis;
                        idx = i;
                    }
                }

                Point[] orderList = new Point[lstPoint.Length];
                int _idx = idx;
                for (int i = 0; i < orderList.Length; i++)
                {
                    // Because the top left and bottom right alway distacne two index so plus 2
                    _idx = (idx + lstPoint.Length + i + 2) % 4;
                    orderList[i] = lstPoint[_idx];
                }

                return orderList;
            }
            /// <summary>
            /// Order 4 vertices of Poly poinnts in direction: top left -> top right -> bottom right -> bottom left.
            /// </summary>
            /// <param name="lstPoint">4 points</param>
            /// <returns></returns>
            public static Point[] OrderVerticesForPoly(Point[] lstPoint)
            {
                if (lstPoint.Length == 4)
                {
                    var lst = new List<Point>(lstPoint);
                    var sortLst = lst.OrderBy(p => Math.Sqrt(Math.Pow(p.X, 2) + Math.Pow(p.Y, 2))).ToList();
                    Point[] orderList = new Point[lstPoint.Length];
                    orderList[0] = sortLst[0];
                    orderList[2] = sortLst[3];
                    if (Math.Abs(sortLst[1].Y - sortLst[0].Y) < Math.Abs(sortLst[2].Y - sortLst[0].Y))
                    {
                        orderList[1] = sortLst[1];
                        orderList[3] = sortLst[2];
                    }
                    else
                    {
                        orderList[1] = sortLst[2];
                        orderList[3] = sortLst[1];
                    }
                    return orderList;
                }
                else
                {
                    return null;
                }
            }
            /// <summary>
            /// Calculate angle bewteen two vectors
            /// </summary>
            /// <param name="lstA">Vector made by two point</param>
            /// <param name="lstB">Vector made by two point</param>
            /// <param name="direction">If direction is true: return angle with orientation</param>
            /// <returns>angle between two vectors</returns>
            private static double CalcAngle_old(Point[] lstA, Point[] lstB, bool direction = true)
            {
                double angle = 0;
                var vt_AB = new Point(lstA[0].X - lstA[1].X, lstA[0].Y - lstA[1].Y);
                var vt_CD = new Point(lstB[0].X - lstB[1].X, lstB[0].Y - lstB[1].Y);
                var cos_anpha = (vt_AB.X * vt_CD.X + vt_AB.Y * vt_CD.Y)
                    / Math.Sqrt(
                        (Math.Pow(vt_AB.X, 2) + Math.Pow(vt_AB.Y, 2))
                        * (Math.Pow(vt_CD.X, 2) + Math.Pow(vt_CD.Y, 2))
                    );
                angle = Math.Acos(cos_anpha) * 180 / Math.PI;
                // Check direction: compare by y
                if (direction)
                {
                    var ang_x_AB = Math.Atan(Math.Abs((double)vt_AB.Y / vt_AB.X)) * 180 / Math.PI;
                    var ang_x_CD = Math.Atan(Math.Abs((double)vt_CD.Y / vt_CD.X)) * 180 / Math.PI;

                    ang_x_AB = lstA[0].Y < lstA[1].Y ? 180 - ang_x_AB : ang_x_AB;
                    ang_x_CD = lstA[0].Y < lstA[1].Y ? 180 - ang_x_CD : ang_x_CD;

                    if (ang_x_AB >= 90 && ang_x_CD >= 90)
                    {
                        angle = ang_x_CD < ang_x_AB ? -angle : angle;
                    }
                    else if (ang_x_AB < 90 && ang_x_CD >= 90)
                    {
                        angle = -angle;
                    }
                    else if (ang_x_AB < 90 && ang_x_CD < 90)
                    {
                        angle = ang_x_CD < ang_x_AB ? -angle : angle;
                    }
                }
                return angle;
            }
            /// <summary>
            /// Distance two points
            /// </summary>
            /// <param name="pointA"></param>
            /// <param name="pointB"></param>
            /// <returns></returns>
            public static double DistanceTwoPoints(Point pointA, Point pointB)
            {
                return Math.Sqrt(Math.Pow(pointA.X - pointB.X, 2) + Math.Pow(pointA.Y - pointB.Y, 2));
            }
        }
    }
}
