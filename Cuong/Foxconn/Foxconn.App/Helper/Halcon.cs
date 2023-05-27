using HalconDotNet;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Foxconn.App.Helper
{
    public class Halcon
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
