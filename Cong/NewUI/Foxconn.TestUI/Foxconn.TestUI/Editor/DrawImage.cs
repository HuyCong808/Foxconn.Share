using System;
using System.Collections.Generic;
using System.Drawing;

namespace Foxconn.TestUI.Editor
{
    /// <summary>
    /// This class is alternative Rectangle of system.Drawing for save Rectangle in mongodb
    /// </summary>
    public class BRectangle
    {
        public BRectangle() { }
        public BRectangle(int X, int Y, int Width, int Height)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
        }
        public BRectangle(BRectangle rect)
        {
            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;
        }
        public BRectangle(Rectangle rect)
        {
            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;
        }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsEmpty => Width == 0 && Height == 0;
        public static BRectangle Empty => new BRectangle();
        public Rectangle Rectangle => new Rectangle(X, Y, Width, Height);
        public Point Localtion => new Point(X, Y);

        public bool Equal(BRectangle rect)
        {
            return X == rect.X && Y == rect.Y && Width == rect.Width && Height == rect.Height;
        }

        public bool Equal(Rectangle rect)
        {
            return X == rect.X && Y == rect.Y && Width == rect.Width && Height == rect.Height;
        }

        public BRectangle Clone()
        {
            return new BRectangle(X, Y, Width, Height);
        }

        public static implicit operator List<object>(BRectangle v)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// This class is used for exchange struct System.Drawing.Point => because System.Drawing.Point cant be deserialized by Bson
    /// </summary>
    public class BPoint : ICloneable
    {
        public BPoint() { }
        public BPoint(int X, int Y) { this.X = X; this.Y = Y; }
        public BPoint(Point p) { X = p.X; Y = p.Y; }
        public BPoint(BPoint p) { X = p.X; Y = p.Y; }
        public int X { get; set; }
        public int Y { get; set; }
        public Point Point => new Point(X, Y);
        /// <summary>
        /// Check Equal with another BPoint
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Equal(BPoint p)
        {
            return X == p.X && Y == p.Y;
        }
        /// <summary>
        /// Check Equal with another Point
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Equal(Point p)
        {
            return X == p.X && Y == p.Y;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
        public static BPoint[] FromArray(Point[] lstP)
        {
            if (lstP != null)
            {
                BPoint[] lst = new BPoint[lstP.Length];
                for (int i = 0; i < lstP.Length; i++)
                {
                    lst[i] = new BPoint(lstP[i]);
                }
                return lst;
            }
            else
            {
                BPoint[] lst = new BPoint[0];
                return lst;
            }
        }
    }
}
