using System;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Foxconn.UI.Controls
{
    public static class ColorSwapper
    {
        public static ImageSource SwapColors(ImageSource imageSource, ColorCallback colorCallback)
        {
            if (colorCallback == null)
                throw new ArgumentNullException(nameof(colorCallback));
            ImageSource imageSource1 = imageSource;
            switch (imageSource)
            {
                case null:
                    return imageSource1;
                case DrawingImage drawingImage:
                    SwapColorsWithoutCloning(((DrawingImage)(imageSource1 = drawingImage.Clone())).Drawing, colorCallback);
                    imageSource1.Freeze();
                    goto case null;
                case BitmapSource bitmapSource:
                    imageSource1 = SwapColors(bitmapSource, colorCallback);
                    goto case null;
                default:
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTable.UnexpectedImageSourceType, imageSource.GetType().Name));
            }
        }

        public static BitmapSource SwapColors(BitmapSource bitmapSource, ColorCallback colorCallback)
        {
            if (colorCallback == null)
                throw new ArgumentNullException(nameof(colorCallback));
            BitmapSource bitmapSource1 = bitmapSource;
            if (bitmapSource != null)
            {
                PixelFormat bgra32 = PixelFormats.Bgra32;
                BitmapPalette bitmapPalette = null;
                double alphaThreshold = 0.0;
                FormatConvertedBitmap formatConvertedBitmap = new FormatConvertedBitmap(bitmapSource, bgra32, bitmapPalette, alphaThreshold);
                int pixelWidth = formatConvertedBitmap.PixelWidth;
                int pixelHeight = formatConvertedBitmap.PixelHeight;
                int stride = 4 * pixelWidth;
                byte[] pixels = new byte[stride * pixelHeight];
                formatConvertedBitmap.CopyPixels(pixels, stride, 0);
                for (int index = 0; index < pixels.Length; index += 4)
                {
                    Color color1 = Color.FromArgb(pixels[index + 3], pixels[index + 2], pixels[index + 1], pixels[index]);
                    Color color2 = colorCallback(color1);
                    if (color2 != color1)
                    {
                        pixels[index] = color2.B;
                        pixels[index + 1] = color2.G;
                        pixels[index + 2] = color2.R;
                        pixels[index + 3] = color2.A;
                    }
                }
                bitmapSource1 = BitmapSource.Create(pixelWidth, pixelHeight, formatConvertedBitmap.DpiX, formatConvertedBitmap.DpiY, bgra32, bitmapPalette, pixels, stride);
                bitmapSource1.Freeze();
            }
            return bitmapSource1;
        }

        public static Drawing SwapColors(Drawing drawing, ColorCallback colorCallback)
        {
            if (colorCallback == null)
                throw new ArgumentNullException(nameof(colorCallback));
            Drawing drawing1 = drawing;
            if (drawing != null)
            {
                drawing1 = drawing.Clone();
                SwapColorsWithoutCloning(drawing1, colorCallback);
                drawing1.Freeze();
            }
            return drawing1;
        }

        public static Brush SwapColors(Brush brush, ColorCallback colorCallback)
        {
            if (colorCallback == null)
                throw new ArgumentNullException(nameof(colorCallback));
            Brush brush1 = brush;
            if (brush != null)
            {
                brush1 = brush.Clone();
                SwapColorsWithoutCloning(brush1, colorCallback);
                brush1.Freeze();
            }
            return brush1;
        }

        private static ImageSource SwapColorsWithoutCloningIfPossible(ImageSource imageSource, ColorCallback colorCallback)
        {
            ImageSource imageSource1 = imageSource;
            switch (imageSource)
            {
                case null:
                    return imageSource1;
                case DrawingImage drawingImage:
                    SwapColorsWithoutCloning(drawingImage.Drawing, colorCallback);
                    goto case null;
                case BitmapSource bitmapSource:
                    imageSource1 = SwapColors(bitmapSource, colorCallback);
                    goto case null;
                default:
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTable.UnexpectedImageSourceType, imageSource.GetType().Name));
            }
        }

        private static void SwapColorsWithoutCloning(Drawing drawing, ColorCallback colorCallback)
        {
            switch (drawing)
            {
                case null:
                    break;
                case DrawingGroup drawingGroup:
                    for (int index = 0; index < drawingGroup.Children.Count; ++index)
                        SwapColorsWithoutCloning(drawingGroup.Children[index], colorCallback);
                    break;
                case GeometryDrawing geometryDrawing:
                    SwapColorsWithoutCloning(geometryDrawing.Brush, colorCallback);
                    if (geometryDrawing.Pen == null)
                        break;
                    SwapColorsWithoutCloning(geometryDrawing.Pen.Brush, colorCallback);
                    break;
                case GlyphRunDrawing glyphRunDrawing:
                    SwapColorsWithoutCloning(glyphRunDrawing.ForegroundBrush, colorCallback);
                    break;
                case ImageDrawing imageDrawing:
                    imageDrawing.ImageSource = SwapColorsWithoutCloningIfPossible(imageDrawing.ImageSource, colorCallback);
                    break;
                case VideoDrawing _:
                    break;
                default:
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTable.UnexpectedDrawingType, drawing.GetType().Name));
            }
        }

        private static void SwapColorsWithoutCloning(Brush brush, ColorCallback colorCallback)
        {
            switch (brush)
            {
                case null:
                    break;
                case SolidColorBrush solidColorBrush:
                    solidColorBrush.Color = colorCallback(solidColorBrush.Color);
                    break;
                case GradientBrush gradientBrush:
                    using (GradientStopCollection.Enumerator enumerator = gradientBrush.GradientStops.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            GradientStop current = enumerator.Current;
                            current.Color = colorCallback(current.Color);
                        }
                        break;
                    }
                case DrawingBrush drawingBrush:
                    SwapColorsWithoutCloning(drawingBrush.Drawing, colorCallback);
                    break;
                case ImageBrush imageBrush:
                    imageBrush.ImageSource = SwapColorsWithoutCloningIfPossible(imageBrush.ImageSource, colorCallback);
                    break;
                case VisualBrush _:
                    break;
                default:
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTable.UnexpectedBrushType, brush.GetType().Name));
            }
        }
    }
}
