using Newtonsoft.Json;
using System.Windows;

namespace Foxconn.AutoWeight.Configuration
{
    public class JPoint
    {
        private double _X { get; set; }
        private double _Y { get; set; }

        public double X
        {
            get => _X;
            set => _X = value;

        }

        public double Y
        {
            get => _Y;
            set => _Y = value;
        }

        [JsonIgnore]

        public Point Point = new Point();

        public JPoint()
        {
            X= 0;
            Y= 0;
        }

        public JPoint(double X, double Y)
        {
            this.X = X;
            this.Y= Y;
        }

        public JPoint (Point p)
        {
            X = p.X;
            Y = p.Y;
        }

        public JPoint(JPoint p)
        {
            X= p.X;
            Y= p.Y;
        }
        public bool Equal(Point p)
        {
            return X == p.X && Y == p.Y;
        }

        public bool Equal(JPoint p)
        {
            return X == p.X && Y == p.Y;
        }

        public JPoint Clone()
        {
            return new JPoint(X, Y);
        }

        public static JPoint[] FromArray(Point[] p)
        {
            if (p != null)
            {
                JPoint[] jp = new JPoint[p.Length];
                for (int i = 0; i < p.Length; i++)
                {
                    jp[i] = new JPoint(p[i]);
                }
                return jp;
            }
            else
            {
                JPoint[] jp = new JPoint[0];
                return jp;
            }
        }
    }
}
