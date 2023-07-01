using Foxconn.AOI.Editor.Effects;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Holly.AOI.Editor.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(ImageSource))]
    internal sealed class IconExtension : MarkupExtension
    {
        private static Dictionary<Key, ImageSource> images = new Dictionary<Key, ImageSource>();
        private object _icon;
        private object _color;
        private object _effect;
        private BitmapEffect _bitmapEffect;
        private int _width = 0;
        private int _height = 0;

        public BitmapEffect BitmapEffect
        {
            get => _bitmapEffect;
            set => _bitmapEffect = value;
        }

        public object Effect
        {
            get => _effect;
            set => _effect = value;
        }

        public int Width
        {
            get => _width;
            set => _width = value;
        }

        public int Height
        {
            get => _height;
            set => _height = value;
        }

        public object Icon
        {
            get => _icon;
            set => _icon = value;
        }

        public object Color
        {
            get => _color;
            set => _color = value;
        }

        private static void RenderIcon(DrawingContext drawingContext, Brush iconBrush, Brush fillColor, Size size)
        {
            drawingContext.PushOpacityMask(iconBrush);
            drawingContext.DrawRectangle(fillColor, null, new Rect(size));
            drawingContext.Pop();
        }

        private static Brush GetColorBrush(IServiceProvider serviceProvider, object resourceKey)
        {
            return resourceKey != null && new StaticResourceExtension(resourceKey).ProvideValue(serviceProvider) is Brush brush ? brush : Brushes.White;
        }

        private static Brush GetIconBrush(IServiceProvider serviceProvider, object resourceKey)
        {
            return resourceKey != null && new StaticResourceExtension(resourceKey).ProvideValue(serviceProvider) is Brush brush ? brush : null;
        }

        private Point GetSize()
        {
            return new Point(_width > 0 ? _width : 16, _height > 0 ? _height : 16);
        }

        public static Effect GetEffect(IServiceProvider serviceProvider, object resourceKey)
        {
            return resourceKey != null && new StaticResourceExtension(resourceKey).ProvideValue(serviceProvider) is Effect effect ? effect : null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Brush iconBrush = GetIconBrush(serviceProvider, _icon);
            if (iconBrush == null)
                return null;
            Brush colorBrush = GetColorBrush(serviceProvider, _color);
            Effect effect = GetEffect(serviceProvider, _effect);
            Point size = GetSize();
            Key key = new Key()
            {
                Color = colorBrush,
                Icon = iconBrush,
                Width = _width,
                Height = _height,
                Effect = effect
            };
            ImageSource imageSource;
            if (images.TryGetValue(key, out imageSource))
                return imageSource;
            DrawingVisual visual = new DrawingVisual();
            visual.SetVisualEffect(effect);
            DrawingContext drawingContext = visual.RenderOpen();
            RenderIcon(drawingContext, iconBrush, colorBrush, new Size((double)size.X, (double)size.Y));
            drawingContext.Close();
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)size.X, (int)size.Y, 96.0, 96.0, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(visual);
            renderTargetBitmap.Freeze();
            images.Add(key, renderTargetBitmap);
            return renderTargetBitmap;
        }

        private struct Key
        {
            public Brush Icon;
            public Brush Color;
            public int Width;
            public int Height;
            public Effect Effect;

            public override int GetHashCode()
            {
                return Icon.GetHashCode() ^ Color.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                Key key = (Key)obj;
                return Equals(key.Icon, Icon) && Equals(key.Color, Color) && key.Width == Width && key.Height == Height && Equals(key.Effect, Effect);
            }
        }
    }
}
