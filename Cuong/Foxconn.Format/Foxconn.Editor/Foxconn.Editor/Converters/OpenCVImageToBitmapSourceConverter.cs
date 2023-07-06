using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Foxconn.Editor.Converters
{
    public class OpenCVImageToBitmapSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var bitmap = (Image<Bgr, byte>)value;
                return Convert(bitmap.ToBitmap(), release: true);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static BitmapSource Convert(System.Drawing.Bitmap bitmap, bool release = true)
        {
            var pixelFormat = bitmap.PixelFormat;
            var format = PixelFormats.Bgr24;
            switch (pixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    format = PixelFormats.Gray8;
                    break;
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    format = PixelFormats.Bgr24;
                    break;
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    format = PixelFormats.Bgra32;
                    break;
                default:
                    break;
            }
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                format, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            bitmap.UnlockBits(bitmapData);
            if (release)
            {
                bitmap.Dispose();
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return bitmapSource;
        }
    }
}
