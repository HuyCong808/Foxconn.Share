using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;

namespace Foxconn.AOI.Editor.OpenCV
{
    public class CvHoughLine : NotifyProperty
    {
        public CvHoughLine()
        {
        }

        public CvResult Run(Image<Bgr, byte> src, Image<Bgr, byte> dst = null, Rectangle ROI = new Rectangle())
        {
            CvResult cvRet = new CvResult();
            try
            {

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
    }
}
