using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.TestUI.Editor;
using Foxconn.TestUI.Enums;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using ZXing;
using ZXing.Common;

namespace Foxconn.TestUI.OpenCV
{
    public class CvCodeRecognition : NotifyProperty
    {
        private CodeMode _mode { get; set; }
        private CodeFormat _format { get; set; }
        private string _prefix { get; set; }
        private int _length { get; set; }

        public CodeMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                NotifyPropertyChanged(nameof(Mode));
            }
        }

        public CodeFormat Format
        {
            get => _format;
            set
            {
                _format = value;
                NotifyPropertyChanged(nameof(Format));
            }
        }

        public string Prefix
        {
            get => _prefix;
            set
            {
                _prefix = value;
                NotifyPropertyChanged(nameof(Prefix));
            }
        }

        public int Length
        {
            get => _length;
            set
            {
                _length = value;
                NotifyPropertyChanged(nameof(Length));
            }
        }

        public CvCodeRecognition()
        {
            _mode = CodeMode.ZXing;
            _format = CodeFormat.CODE_128;
            _prefix = string.Empty;
            _length = 12;
        }

        public CvResult Run(Image<Bgr, byte> src, Image<Bgr, byte> dst = null, Rectangle ROI = new Rectangle())
        {
            CvResult cvRet = new CvResult();
            try
            {
                if (_mode == CodeMode.ZXing)
                {
                    string data = Decode(src, 5);
                    cvRet.Content = data;
                    if (data != "NOT FOUND" && data.Length == _length)
                    {
                        if (_prefix != string.Empty)
                        {
                            if (data.Substring(0, _prefix.Length) == _prefix)
                            {
                                cvRet.Result = true;
                                DrawResult(ref dst, true);
                            }
                            else
                            {
                                cvRet.Result = false;
                                DrawResult(ref dst, false);
                            }
                        }
                        else
                        {
                            cvRet.Result = true;
                            DrawResult(ref dst, true);
                        }
                    }
                    else
                    {
                        DrawResult(ref dst, false);
                    }
                }
                else if (_mode == CodeMode.Halcon)
                {
                    string data = string.Empty;
                    if (_format == CodeFormat.QR_CODE)
                    {
                        data = ReadQRByHalcon(src);
                    }
                    else if (_format == CodeFormat.DATA_MATRIX)
                    {
                        data = ReadMatrixByHalcon(src);
                    }
                    else
                    {
                        data = ReadSNByHalcon(src);
                    }
                    cvRet.Content = data;
                    if (data.Length == _length)
                    {
                        if (_prefix != string.Empty)
                        {
                            if (data.Substring(0, _prefix.Length) == _prefix)
                            {
                                cvRet.Result = true;
                                DrawResult(ref dst, true);
                            }
                            else
                            {
                                cvRet.Result = false;
                                DrawResult(ref dst, false);
                            }
                        }
                        else
                        {
                            cvRet.Result = true;
                            DrawResult(ref dst, true);
                        }
                    }
                    else
                    {
                        DrawResult(ref dst, false);
                    }
                }
                else
                {
                    cvRet.Result = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                src?.Dispose();
            }
            return cvRet;
        }

        private string Decode(Image<Bgr, byte> src, int step = 2)
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
                                                var result = ReadByZXing(bitmap, _format);
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
             //   AutoRun.Current.UpdateLogError(data);
            }
            return data;
        }

        private Result ReadByZXing(Bitmap bitmap, CodeFormat format)
        {
            var formats = new List<BarcodeFormat>();
            switch (format)
            {
                case CodeFormat.AZTEC:
                    formats.Add(BarcodeFormat.AZTEC);
                    break;
                case CodeFormat.CODABAR:
                    formats.Add(BarcodeFormat.CODABAR);
                    break;
                case CodeFormat.CODE_39:
                    formats.Add(BarcodeFormat.CODE_39);
                    break;
                case CodeFormat.CODE_93:
                    formats.Add(BarcodeFormat.CODE_93);
                    break;
                case CodeFormat.CODE_128:
                    formats.Add(BarcodeFormat.CODE_128);
                    break;
                case CodeFormat.DATA_MATRIX:
                    formats.Add(BarcodeFormat.DATA_MATRIX);
                    break;
                case CodeFormat.EAN_8:
                    formats.Add(BarcodeFormat.EAN_8);
                    break;
                case CodeFormat.EAN_13:
                    formats.Add(BarcodeFormat.EAN_13);
                    break;
                case CodeFormat.ITF:
                    formats.Add(BarcodeFormat.ITF);
                    break;
                case CodeFormat.MAXICODE:
                    formats.Add(BarcodeFormat.MAXICODE);
                    break;
                case CodeFormat.PDF_417:
                    formats.Add(BarcodeFormat.PDF_417);
                    break;
                case CodeFormat.QR_CODE:
                    formats.Add(BarcodeFormat.QR_CODE);
                    break;
                case CodeFormat.RSS_14:
                    formats.Add(BarcodeFormat.RSS_14);
                    break;
                case CodeFormat.RSS_EXPANDED:
                    formats.Add(BarcodeFormat.RSS_EXPANDED);
                    break;
                case CodeFormat.UPC_A:
                    formats.Add(BarcodeFormat.UPC_A);
                    break;
                case CodeFormat.UPC_E:
                    formats.Add(BarcodeFormat.UPC_E);
                    break;
                case CodeFormat.All_1D:
                    formats.Add(BarcodeFormat.All_1D);
                    break;
                case CodeFormat.UPC_EAN_EXTENSION:
                    formats.Add(BarcodeFormat.UPC_EAN_EXTENSION);
                    break;
                case CodeFormat.MSI:
                    formats.Add(BarcodeFormat.MSI);
                    break;
                case CodeFormat.PLESSEY:
                    formats.Add(BarcodeFormat.PLESSEY);
                    break;
                case CodeFormat.IMB:
                    formats.Add(BarcodeFormat.IMB);
                    break;
                case CodeFormat.PHARMA_CODE:
                    formats.Add(BarcodeFormat.PHARMA_CODE);
                    break;
                default:
                    break;
            }

            var options = new DecodingOptions
            {
                TryHarder = true,
                ReturnCodabarStartEnd = true
            };
            options.TryHarder = true;
            options.PossibleFormats = formats;

            var reader = new BarcodeReader
            {
                AutoRotate = true,
                TryInverted = true,
                Options = options
            };
            var result = reader.Decode(bitmap);
            return result;
        }

        public string ReadSNByHalcon(Image<Bgr, byte> image)
        {
            // Local iconic variables 
            HObject ho_Image;
            HObject ho_SymbolRegions;
            // Local control variables 
            var hv_BarCodeHandle = new HTuple();
            var hv_DecodedDataStrings = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_SymbolRegions);

            //Read an image
            ho_Image.Dispose();
            // Convert bitmap to ho_Image
            ho_Image = CvHalcon.BitmapToHalconImage(image.ToBitmap());

            //Create a model for reading 1D barcode
            hv_BarCodeHandle.Dispose();
            HOperatorSet.CreateBarCodeModel(new HTuple(), new HTuple(), out hv_BarCodeHandle);

            //Read the symbol in the image
            ho_SymbolRegions.Dispose(); hv_DecodedDataStrings.Dispose();
            HOperatorSet.FindBarCode(ho_Image, out ho_SymbolRegions, hv_BarCodeHandle, "auto", out hv_DecodedDataStrings);
            var strings = hv_DecodedDataStrings.ToSArr();
            string barcodeText = strings.Length > 0 ? strings[0].ToString().Trim() : string.Empty;

            ho_Image.Dispose();
            ho_SymbolRegions.Dispose();

            hv_BarCodeHandle.Dispose();
            hv_DecodedDataStrings.Dispose();

            return barcodeText;
        }

        public string ReadQRByHalcon(Image<Bgr, byte> image)
        {
            // Local iconic variables 
            HObject ho_Image;
            HObject ho_SymbolXLDs;
            // Local control variables 
            var hv_DataCodeHandle = new HTuple();
            var hv_ResultHandles = new HTuple();
            var hv_DecodedDataStrings = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_SymbolXLDs);

            //Read an image
            ho_Image.Dispose();
            // Convert bitmap to ho_Image
            ho_Image = CvHalcon.BitmapToHalconImage(image.ToBitmap());

            //Create a model for reading QR Code
            hv_DataCodeHandle.Dispose();
            HOperatorSet.CreateDataCode2dModel("QR Code", new HTuple(), new HTuple(), out hv_DataCodeHandle);

            //Read the symbol in the image
            ho_SymbolXLDs.Dispose();
            hv_ResultHandles.Dispose();
            hv_DecodedDataStrings.Dispose();
            HOperatorSet.FindDataCode2d(ho_Image, out ho_SymbolXLDs, hv_DataCodeHandle, new HTuple(), new HTuple(), out hv_ResultHandles, out hv_DecodedDataStrings);
            var strings = hv_DecodedDataStrings.ToSArr();
            string barcodeText = strings.Length > 0 ? strings[0].ToString().Trim() : string.Empty;

            ho_Image.Dispose();
            ho_SymbolXLDs.Dispose();

            hv_DataCodeHandle.Dispose();
            hv_ResultHandles.Dispose();
            hv_DecodedDataStrings.Dispose();

            return barcodeText;
        }

        public string ReadMatrixByHalcon(Image<Bgr, byte> image)
        {
            // Local iconic variables 
            HObject ho_Image;
            HObject ho_SymbolXLDs;
            // Local control variables 
            var hv_DataCodeHandle = new HTuple();
            var hv_ResultHandles = new HTuple();
            var hv_DecodedDataStrings = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_SymbolXLDs);

            //Read an image
            ho_Image.Dispose();
            // Convert bitmap to ho_Image
            ho_Image = CvHalcon.BitmapToHalconImage(image.ToBitmap());

            //Create a model for reading Dat Matrix
            hv_DataCodeHandle.Dispose();
            HOperatorSet.CreateDataCode2dModel("Data Matrix ECC 200", new HTuple(), new HTuple(), out hv_DataCodeHandle);

            //Read the symbol in the image
            ho_SymbolXLDs.Dispose();
            hv_ResultHandles.Dispose();
            hv_DecodedDataStrings.Dispose();
            HOperatorSet.FindDataCode2d(ho_Image, out ho_SymbolXLDs, hv_DataCodeHandle, new HTuple(), new HTuple(), out hv_ResultHandles, out hv_DecodedDataStrings);
            var strings = hv_DecodedDataStrings.ToSArr();
            string barcodeText = strings.Length > 0 ? strings[0].ToString().Trim() : string.Empty;

            ho_Image.Dispose();
            ho_SymbolXLDs.Dispose();
            hv_DataCodeHandle.Dispose();
            hv_ResultHandles.Dispose();
            hv_DecodedDataStrings.Dispose();

            return barcodeText;
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
    }

    public class CvHalcon
    {
        /// <summary>
        /// Convert ByteArray to HImage
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static HImage ByteArrayToHImage(byte[] bytes, int width, int height, bool color)
        {
            var imageSize = width * height;
            var pixelFormat = color ? PixelFormat.Format32bppRgb : PixelFormat.Format8bppIndexed;

            var bmp = new Bitmap(width, height, pixelFormat);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr imagePtr = bmpData.Scan0;
            Marshal.Copy(bytes, 0, imagePtr, imageSize);
            bmp.UnlockBits(bmpData);

            return new HImage("byte", width, height, imagePtr);
        }
        /// <summary>
        /// Convert HImage to Bitmap
        /// </summary>
        /// <param name="hObject"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Bitmap HalconImageToBitmap(HObject hObject, bool color)
        {
            if (hObject == null)
            {
                throw new ArgumentNullException("HalconImage");
            }

            HTuple pointerRed = null;
            HTuple pointerGreen = null;
            HTuple pointerBlue = null;
            HTuple type;
            HTuple width;
            HTuple height;

            // Halcon
            var pixelFormat = color ? PixelFormat.Format32bppRgb : PixelFormat.Format8bppIndexed;
            if (color)
            {
                HOperatorSet.GetImagePointer3(hObject, out pointerRed, out pointerGreen, out pointerBlue, out type, out width, out height);
            }
            else
            {
                HOperatorSet.GetImagePointer1(hObject, out pointerBlue, out type, out width, out height);
            }

            var bmp = new Bitmap(width, height, pixelFormat);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];


            IntPtr ptrB = new IntPtr(pointerBlue);
            IntPtr ptrG = IntPtr.Zero;
            IntPtr ptrR = IntPtr.Zero;
            if (pointerGreen != null)
            {
                ptrG = new IntPtr(pointerGreen);
            }

            if (pointerRed != null)
            {
                ptrR = new IntPtr(pointerRed);
            }

            int channels = color ? 3 : 1;

            // Stride
            int strideTotal = Math.Abs(bmpData.Stride);
            int unmapByes = strideTotal - (int)width * channels;
            for (int i = 0, offset = 0; i < bytes; i += channels, offset++)
            {
                if ((offset + 1) % width == 0)
                {
                    i += unmapByes;
                }

                rgbValues[i] = Marshal.ReadByte(ptrB, offset);
                if (color)
                {
                    rgbValues[i + 1] = Marshal.ReadByte(ptrG, offset);
                    rgbValues[i + 2] = Marshal.ReadByte(ptrR, offset);
                }
            }

            Marshal.Copy(rgbValues, 0, bmpData.Scan0, bytes);
            bmp.UnlockBits(bmpData);

            return bmp;
        }
        /// <summary>
        /// Convert Bitmap to HImage
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static HImage BitmapToHalconImage(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException("Bitmap");
            }

            int width = bitmap.Width;
            int height = bitmap.Height;
            int ptrSize = width * height;

            // R/G/B
            IntPtr ptrR = Marshal.AllocHGlobal(ptrSize);
            IntPtr ptrG = Marshal.AllocHGlobal(ptrSize);
            IntPtr ptrB = Marshal.AllocHGlobal(ptrSize);

            // Bitmap
            int offset = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color c = bitmap.GetPixel(x, y);
                    Marshal.WriteByte(ptrR, offset, c.R);
                    Marshal.WriteByte(ptrG, offset, c.G);
                    Marshal.WriteByte(ptrB, offset, c.B);
                    offset++;
                }
            }

            // HImage
            HImage hImage = new HImage();
            hImage.GenImage3("byte", width, height, ptrR, ptrG, ptrB);

            Marshal.FreeHGlobal(ptrB);
            Marshal.FreeHGlobal(ptrG);
            Marshal.FreeHGlobal(ptrR);

            return hImage;
        }
        /// <summary>
        /// Convert Bitmap to ByteArray
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            var bytes = Array.Empty<byte>();

            if (bitmap != null)
            {
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Tiff);
                bytes = stream.ToArray();
            }

            return bytes;
        }
        /// <summary>
        /// Convert HObject to ByteArray
        /// </summary>
        /// <param name="hObject"></param>
        /// <returns></returns>
        public static byte[] HalconImageToByteArray(HObject hObject, bool color)
        {
            return BitmapToByteArray(HalconImageToBitmap(hObject, color));
        }
    }
}
