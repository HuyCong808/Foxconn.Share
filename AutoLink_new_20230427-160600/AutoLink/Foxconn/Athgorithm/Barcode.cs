using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using ZXing;
using ZXing.Common;

namespace Foxconn.Athgorithm
{
    public class Barcode
    {
        public static string Decode(Image<Bgr, byte> src, int step = 2)
        {
            string data = string.Empty;
            if (src != null)
            {
                if (src.Width > 0 && src.Height > 0)
                {
                    using (var gray = src.Convert<Gray, byte>())
                    {
                        for (int s = -5; s < 3; s++)
                        {
                            double scale = Math.Pow(2, s);
                            int width = Convert.ToInt32(scale * src.Width);
                            int height = Convert.ToInt32(scale * src.Height);
                            if (width > 0 && height > 0)
                            {
                                using (var imgScale = gray.Resize(width, height, Emgu.CV.CvEnum.Inter.Linear))
                                {
                                    for (int i = 0; i < 26; i += step)
                                    {
                                        double gamma = 0.5 + i * 0.1;
                                        using (var imgGamma = new Image<Gray, byte>(imgScale.Size))
                                        {
                                            CvInvoke.ConvertScaleAbs(imgScale, imgGamma, gamma, 0);
                                            using (Bitmap bitmap = imgGamma.ToBitmap())
                                            {
                                                var reader = new BarcodeReader
                                                {
                                                    AutoRotate = true,
                                                    TryInverted = true,
                                                };
                                                var result = reader.Decode(bitmap);
                                                if (result != null)
                                                {
                                                    data = result.Text;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(data))
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (data == string.Empty)
            {
                data = "NOT FOUND";
            }
            return data;
        }
    }
}
