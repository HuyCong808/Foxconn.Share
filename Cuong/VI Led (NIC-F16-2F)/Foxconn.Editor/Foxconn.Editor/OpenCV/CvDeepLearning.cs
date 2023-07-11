using Emgu.CV;
using Emgu.CV.Structure;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;

namespace Foxconn.Editor.OpenCV
{
    public class CvDeepLearning : NotifyProperty
    {
        private Image<Bgr, byte> _template { get; set; }
        private ValueRange _OKRange { get; set; }
        private bool _isEnabledReverseSearch { get; set; }

        public Image<Bgr, byte> Template
        {
            get => _template;
            set
            {
                _template = value;
                NotifyPropertyChanged(nameof(Template));
            }
        }

        public ValueRange OKRange
        {
            get => _OKRange;
            set
            {
                _OKRange = value;
                NotifyPropertyChanged(nameof(OKRange));
            }
        }

        public bool IsEnabledReverseSearch
        {
            get => _isEnabledReverseSearch;
            set
            {
                _isEnabledReverseSearch = value;
                NotifyPropertyChanged(nameof(IsEnabledReverseSearch));
            }
        }

        public CvDeepLearning()
        {
            _template = null;
            _OKRange = new ValueRange(80, 100, 0, 100);
            _isEnabledReverseSearch = false;
        }

        public CvResult Run(Image<Bgr, byte> src, Image<Bgr, byte> dst = null, Rectangle ROI = new Rectangle())
        {
            CvResult cvRet = new CvResult();
            try
            {
                double s = Matching(src, dst);
                if (IsMatched(s, _OKRange))
                {
                    if (_isEnabledReverseSearch)
                    {
                        cvRet.Result = false;
                        cvRet.Score = s;
                        DrawResult(ref dst, cvRet.Result);
                    }
                    else
                    {
                        cvRet.Result = true;
                        cvRet.Score = s;
                        DrawResult(ref dst, cvRet.Result);
                    }
                }
                else
                {
                    if (_isEnabledReverseSearch)
                    {
                        cvRet.Result = true;
                        cvRet.Score = s;
                        DrawResult(ref dst, cvRet.Result);
                    }
                    else
                    {
                        cvRet.Result = false;
                        cvRet.Score = s;
                        DrawResult(ref dst, cvRet.Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                src?.Dispose();
            }
            return cvRet;
        }

        private double Matching(Image<Bgr, byte> image, Image<Bgr, byte> template)
        {
            bool pResult = Predict("http://127.0.0.1:5000//predict-binary/test", image.ToBitmap());
            return pResult ? 1 : 0;
        }

        private bool IsMatched(double s, ValueRange OKrange)
        {
            s *= 100;
            if (OKrange.Lower < OKrange.Upper)
            {
                if (s >= OKrange.Lower && s <= OKrange.Upper)
                {
                    return true;
                }
            }
            else
            {
                if (s >= OKrange.Lower || s < OKrange.Upper)
                {
                    return true;
                }
            }
            return false;
        }

        private void DrawResult(ref Image<Bgr, byte> dst, bool result)
        {
            if (dst != null)
            {
                var rect = new Rectangle(0, 0, dst.Width, dst.Height);
                var color = result ? new Bgr(75, 215, 50) : new Bgr(58, 69, 255);
                dst.Draw(rect, color, 3);
            }
        }

        private byte[] BitmapToByteArray(Bitmap bitmap)
        {
            var byteArray = default(byte[]);
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                byteArray = memoryStream.ToArray();
            }
            return byteArray;
        }

        private bool Predict(string requestUri, Bitmap bitmap)
        {
            try
            {
                var boundary = "chenmin666";
                using (var httpClient = new HttpClient())
                using (var content = new MultipartFormDataContent(boundary))
                {
                    content.Add(new ByteArrayContent(BitmapToByteArray(bitmap)), "image", "image.jpg");

                    var responseTask = httpClient.PostAsync(requestUri, content);
                    responseTask.Wait();

                    var response = responseTask.Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var dataTask = response.Content.ReadAsStringAsync();
                        dataTask.Wait();

                        var data = dataTask.Result;
                        var pResult = JsonConvert.DeserializeObject<PredictResult>(data);
                        Console.WriteLine(data);
                        return pResult.success.ToLower() == "true" && pResult.result.ToLower() == "pass";
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
            finally
            {
                bitmap?.Dispose();
            }
        }

        //private async Task<bool> Predict(string requestUri, Bitmap bitmap)
        //{
        //    try
        //    {
        //        var boundary = "chenmin666";
        //        using (var httpClient = new HttpClient())
        //        using (var content = new MultipartFormDataContent(boundary))
        //        {
        //            content.Add(new ByteArrayContent(BitmapToByteArray(bitmap)), "image", "image.jpg");

        //            var response = await httpClient.PostAsync(requestUri, content).ConfigureAwait(false);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        //                var pResult = JsonConvert.DeserializeObject<PredictResult>(data);
        //                Console.WriteLine(data);
        //                return pResult.success.ToLower() == "true" && pResult.result.ToLower() == "pass";
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.WriteLine(ex);
        //        return false;
        //    }
        //    finally
        //    {
        //        bitmap?.Dispose();
        //    }
        //}

        //public static async Task<bool> PredictDebug(string requestUri = "http://127.0.0.1:5000/debug/test")
        //{
        //    using (var ofd = new System.Windows.Forms.OpenFileDialog())
        //    {
        //        ofd.Filter = "Image file (*.jpg, *.png, *.tiff)|*.jpg; *.png; *.tiff|All files (*.*)|*.*";
        //        ofd.FilterIndex = 0;
        //        ofd.RestoreDirectory = true;
        //        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //        {
        //            using (var image = new Bitmap(ofd.FileName))
        //            {
        //                return await Predict(requestUri, image);
        //            }
        //        }
        //    }
        //    return false;
        //}
    }

    public class PredictResult
    {
        public string success { get; set; }
        public string result { get; set; }
    }
}
