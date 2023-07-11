using System;
using System.Collections.Generic;
using System.Windows;

namespace Foxconn.Editor.Configuration
{
    public class JRect : NotifyProperty
    {
        private int _x { get; set; }
        private int _y { get; set; }
        private int _width { get; set; }
        private int _height { get; set; }



        public int X
        {
            get => _x;
            set
            {
                _x = value;
                NotifyPropertyChanged(nameof(X));
            }
        }

        public int Y
        {
            get => _y;
            set
            {
                _y = value;
                NotifyPropertyChanged(nameof(Y));
            }
        }

        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                NotifyPropertyChanged(nameof(Width));
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                NotifyPropertyChanged(nameof(Height));
            }
        }

        public System.Drawing.Rectangle Rectangle => new System.Drawing.Rectangle((int)X, (int)Y, (int)Width, (int)Height);

        public JRect()
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
        }

        public JRect(int X, int Y, int Width, int Height)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
        }

        public JRect(Rect rect)
        {
            X = (int)rect.X;
            Y = (int)rect.Y;
            Width = (int)rect.Width;
            Height = (int)rect.Height;
        }

        public JRect(JRect rect)
        {
            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;
        }

        public bool Equal(Rect rect)
        {
            return X == rect.X && Y == rect.Y && Width == rect.Width && Height == rect.Height;
        }

        public bool Equal(JRect rect)
        {
            return X == rect.X && Y == rect.Y && Width == rect.Width && Height == rect.Height;
        }

        public JRect Clone()
        {
            return new JRect(X, Y, Width, Height);
        }

        public static implicit operator List<object>(JRect v)
        {
            throw new NotImplementedException();
        }
    }
}
