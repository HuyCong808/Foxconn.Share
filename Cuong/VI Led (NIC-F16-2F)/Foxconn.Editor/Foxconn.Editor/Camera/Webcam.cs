using DirectShowLib;
using Emgu.CV;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace Foxconn.Editor.Camera
{
    public class Webcam : ICamera
    {
        #region CAMERA VARIABLES
        private VideoCapture m_MyCamera = null;
        private Mat _frame = null;
        #endregion
        private readonly object _lockObj = new object();
        private string _userDefinedName = string.Empty;
        private bool _isConnected = false;
        private bool _isGrabbing = false;
        private bool _isStreaming = false;

        public bool IsConnected => _isConnected;

        public bool IsGrabbing => _isGrabbing;

        public bool IsStreaming => _isStreaming;

        public Webcam() { }

        public void DeviceListAcq()
        {
            GC.Collect();
            int index = 0;
            foreach (var device in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
            {
                string modelName = device.Name;
                index += 1;
            }
        }

        public int Open(string userDefinedName = "")
        {
            try
            {
                if (userDefinedName != "")
                    _userDefinedName = userDefinedName;

                if (_isConnected)
                    return 1;

                if (m_MyCamera == null)
                {
                    int.TryParse(_userDefinedName, out int camIndex);
                    m_MyCamera = new VideoCapture(camIndex, VideoCapture.API.DShow);
                    _isConnected = m_MyCamera.IsOpened;
                    int fourcc = VideoWriter.Fourcc('M', 'J', 'P', 'G');
                    m_MyCamera.Set(Emgu.CV.CvEnum.CapProp.FourCC, fourcc);
                    m_MyCamera.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
                    m_MyCamera.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, 1080);
                    return _isConnected ? 1 : -1;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            return -1;
        }

        public int Close()
        {
            try
            {
                int nRet = 0;
                if (m_MyCamera != null)
                {
                    if (_isStreaming)
                    {
                        nRet = StopStreaming();
                    }

                    if (_isGrabbing)
                    {
                        nRet = StopGrabbing();
                    }
                }
                m_MyCamera.Dispose();
                _isConnected = false;
                return nRet;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }

        public int StartGrabbing()
        {
            int nRet = -1;
            if (_isGrabbing)
            {
                return 0;
            }
            if (m_MyCamera != null && !_isGrabbing)
            {
                nRet = 1;
                //m_MyCamera.Start();
                //m_MyCamera.ImageGrabbed += ProcessFrame;
                _isGrabbing = true;
            }
            return nRet;
        }

        public int StopGrabbing()
        {
            int nRet = -1;
            if (m_MyCamera != null && _isGrabbing)
            {
                nRet = 1;
                m_MyCamera.Stop();
                _isGrabbing = false;
            }
            return nRet;
        }

        public int StartStreaming()
        {
            return -1;
        }

        public int StopStreaming()
        {
            return -1;
        }

        public int GetParameter(KeyName key, ref object value)
        {
            return 1;
        }

        public int SetParameter(KeyName key, object value)
        {
            return 1;
        }

        public void ClearImageBuffer()
        {

        }

        public Bitmap GrabFrame(int timeout = 1000)
        {
            try
            {
                lock (_lockObj)
                {
                    if (m_MyCamera != null && m_MyCamera.Ptr != IntPtr.Zero)
                    {
                        _frame = null;
                        m_MyCamera.Start();
                        int loop = timeout / 10;
                        for (int i = 0; i < loop; i++)
                        {
                            _frame = m_MyCamera.QueryFrame();
                            if (_frame != null && i >= 2)
                            {
                                break;
                            }
                            else
                            {
                                Thread.Sleep(10);
                            }
                        }
                        m_MyCamera.Stop();
                        return _frame?.ToBitmap();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            return null;
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (m_MyCamera != null && m_MyCamera.Ptr != IntPtr.Zero)
            {
                m_MyCamera.Retrieve(_frame, 0);
            }
        }
    }
}
