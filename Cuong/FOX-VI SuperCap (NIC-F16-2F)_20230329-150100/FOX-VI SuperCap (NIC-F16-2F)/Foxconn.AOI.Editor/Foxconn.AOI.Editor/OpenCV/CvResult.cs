using System.Windows;

namespace Foxconn.AOI.Editor.OpenCV
{
    public class CvResult
    {
        public bool Result { get; set; }
        public string Content { get; set; }
        public double Score { get; set; }
        public double Qty { get; set; }
        public Point Center { get; set; }
        public double Radius { get; set; }
        public Point[] Points { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }

        public CvResult()
        {
            Result = false;
            Content = string.Empty;
            Score = 0;
            Qty = 0;
            Center = new Point(0, 0);
            Radius = 0;
            Points = new Point[4];
            OffsetX = 0;
            OffsetY = 0;
        }
    }
}
