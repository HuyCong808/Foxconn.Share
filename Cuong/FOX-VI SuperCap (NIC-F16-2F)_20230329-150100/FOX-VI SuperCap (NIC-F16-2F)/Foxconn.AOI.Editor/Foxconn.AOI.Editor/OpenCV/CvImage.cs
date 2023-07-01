using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace Foxconn.AOI.Editor.OpenCV
{
    public class CvImage
    {
        public static void Save(string path, string filename, Mat image, int quality = 80)
        {
            if (path == "")
            {
                path = @$"{AppDomain.CurrentDomain.BaseDirectory}logs\{DateTime.Now:yyyy-MM-dd}\";
            }
            if (filename == "")
            {
                filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
            }
            FileExplorer.CreateDirectory(path);
            var saved = CvInvoke.Imwrite(path + filename, image, new KeyValuePair<ImwriteFlags, int>(ImwriteFlags.JpegQuality, quality));
            if (saved)
            {
                Console.WriteLine($"Saved: {path + filename}");
            }
            else
            {
                Console.WriteLine($"Cannot save: {path + filename}");
            }
        }

        public static void Clear(string path, int maxDayCount = 31)
        {
            if (path == "")
            {
                path = @$"{AppDomain.CurrentDomain.BaseDirectory}logs\";
            }
            FileExplorer.CreateDirectory(path);
            foreach (string item in Directory.GetDirectories(path))
            {
                string s = new DirectoryInfo(item).Name;
                if (DateTime.TryParseExact(s, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out DateTime old))
                {
                    DateTime now = DateTime.Now;
                    if ((now.Date - old.Date).Days > maxDayCount)
                    {
                        FileExplorer.DeleteDirectory(item);
                    }
                }
            }
        }

        public static Image<Gray, byte> Preprocessing(Image<Bgr, byte> src, CvPreprocessing param)
        {
            using (var gray = src.Convert<Gray, byte>())
            {
                if (param.Threshold.Enable)
                {
                    using (var blur = param.Blur.Enable ? gray.SmoothGaussian(param.Blur.Value) : gray)
                    {
                        using (var threshold = !param.Threshold.Inverted ?
                            blur.ThresholdBinary(new Gray(param.Threshold.Value), new Gray(255)) :
                            blur.ThresholdBinaryInv(new Gray(param.Threshold.Value), new Gray(255)))
                        {
                            if (param.Blob.Enable)
                            {
                                var element = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(param.Blob.Value, param.Blob.Value), new Point(-1, -1));
                                CvInvoke.Dilate(threshold, threshold, element, new Point(-1, -1), param.Blob.Dilate, BorderType.Reflect, default);
                                CvInvoke.Erode(threshold, threshold, element, new Point(-1, -1), param.Blob.Erode, BorderType.Reflect, default);
                            }
                            return threshold.Clone();
                        }
                    }
                }
                return gray.Clone();
            }
        }

        public (ValueRange, ValueRange, ValueRange) ValueHSV(Image<Bgr, byte> src)
        {
            var Hue = new ValueRange(0, 0, 0, 255);
            var Saturation = new ValueRange(0, 0, 0, 255);
            var Value = new ValueRange(0, 0, 0, 255);
            using (var dst = new Mat())
            {
                CvInvoke.CvtColor(src, dst, ColorConversion.Bgr2Hsv);
                Mat[] channels = dst.Split();
                RangeF H = channels[0].GetValueRange();
                RangeF S = channels[1].GetValueRange();
                RangeF V = channels[2].GetValueRange();
                Hue.Lower = (int)H.Min;
                Hue.Upper = (int)H.Max;
                Saturation.Lower = (int)S.Min;
                Saturation.Upper = (int)S.Max;
                Value.Lower = (int)V.Min;
                Value.Upper = (int)V.Max;
            };
            return (Hue, Saturation, Value);
        }

        private Image<Gray, byte> ThresholdHSV(Image<Bgr, byte> src, ValueRange hue, ValueRange saturation, ValueRange value)
        {
            double lowerH = hue.Lower;
            double upperH = hue.Upper;
            double lowerS = saturation.Lower;
            double upperS = saturation.Upper;
            double lowerV = value.Lower;
            double upperV = value.Upper;
            Image<Gray, byte> dst = null;
            using (var hsv = src.Convert<Hsv, byte>())
            {
                if (lowerH < upperH)
                {
                    dst = hsv.InRange(new Hsv(lowerH, Math.Min(lowerS, upperS), Math.Min(lowerV, upperV)), new Hsv(upperH, Math.Max(lowerS, upperS), Math.Max(lowerV, upperV)));
                }
                else
                {
                    dst = hsv.InRange(new Hsv(lowerH, Math.Min(lowerS, upperS), Math.Min(lowerV, upperV)), new Hsv(179, Math.Max(lowerS, upperS), Math.Max(lowerV, upperV)));
                    using (var dst2 = hsv.InRange(new Hsv(0, Math.Min(lowerS, upperS), Math.Min(lowerV, upperV)), new Hsv(upperH, Math.Max(lowerS, upperS), Math.Max(lowerV, upperV))))
                    {
                        CvInvoke.BitwiseOr(dst, dst2, dst);
                    }
                }
            }
            return dst;
        }

        private Image<Gray, byte> Threshold(Image<Bgr, byte> src, ValueRange threshold)
        {
            double low = threshold.Lower;
            double up = threshold.Upper;
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

        public static Image<Gray, byte> Threshold(Image<Gray, byte> src, ValueRange threshold)
        {
            double low = threshold.Lower;
            double up = threshold.Upper;
            Image<Gray, byte> dst;
            if (low < up)
            {
                dst = src.InRange(new Gray(low), new Gray(up));
            }
            else
            {
                dst = src.InRange(new Gray(up), new Gray(255));
                using (var image = src.InRange(new Gray(0), new Gray(low)))
                {
                    CvInvoke.BitwiseOr(dst, image, dst);
                }
            }
            return dst;
        }

        public static bool IsMatched(double s, ValueRange OKRange)
        {
            s *= 100;
            if (OKRange.Lower < OKRange.Upper)
            {
                if (s >= OKRange.Lower && s <= OKRange.Upper)
                {
                    return true;
                }
            }
            else
            {
                if (s >= OKRange.Lower || s < OKRange.Upper)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
