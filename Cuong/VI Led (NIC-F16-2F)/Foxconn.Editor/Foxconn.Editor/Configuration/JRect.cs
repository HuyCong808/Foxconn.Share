using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Foxconn.Editor.Configuration
{
    /// <summary>
    /// This class is alternative Rect of System.Windows.Rect for save Rect in Json
    /// </summary>
    public class JRect : NotifyProperty
    {
        private double _X { get; set; }
        private double _Y { get; set; }
        private double _Width { get; set; }
        private double _Height { get; set; }

        public double X
        {
            get => _X;
            set
            {
                _X = value;
                NotifyPropertyChanged(nameof(X));
            }
        }

        public double Y
        {
            get => _Y;
            set
            {
                _Y = value;
                NotifyPropertyChanged(nameof(Y));
            }
        }

        public double Width
        {
            get => _Width;
            set
            {
                _Width = value;
                NotifyPropertyChanged(nameof(Width));
            }
        }

        public double Height
        {
            get => _Height;
            set
            {
                _Height = value;
                NotifyPropertyChanged(nameof(Height));
            }
        }

        [JsonIgnore]
        public Rect Rect => new Rect(X, Y, Width, Height);

        [JsonIgnore]
        public System.Drawing.Rectangle Rectangle => new System.Drawing.Rectangle((int)X, (int)Y, (int)Width, (int)Height);

        public JRect()
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
        }

        public JRect(double X, double Y, double Width, double Height)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
        }

        public JRect(Rect rect)
        {
            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;
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
