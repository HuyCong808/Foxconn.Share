using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Diagnostics;
using System.Drawing;

namespace Foxconn.Editor.OpenCV
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
                Trace.WriteLine(ex);
            }
            finally
            {
                src?.Dispose();
            }
            return cvRet;
        }
    }
}
