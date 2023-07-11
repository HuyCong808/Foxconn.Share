using Basler.Pylon;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace Foxconn.Editor.Camera
{
    public class Basler : ICamera
    {
        #region CAMERA VARIABLES
        private global::Basler.Pylon.Camera _camera { get; set; }
        private PixelDataConverter _converter { get; set; }
        #endregion
        private readonly object _lockObj = new object();
        private string _userDefinedName = string.Empty;
        private bool _isConnected = false;
        private bool _isGrabbing = false;
        private bool _isStreaming = false;

        public bool IsConnected => _isConnected;

        public bool IsGrabbing => _isGrabbing;

        public bool IsStreaming => _isStreaming;

        public void DeviceListAcq()
        {
            GC.Collect();
            int nRet;
            _converter = new PixelDataConverter();
            var allCameras = CameraFinder.Enumerate();
            foreach (var cameraInfo in allCameras)
            {
                string userDefinedName = cameraInfo[CameraInfoKey.UserDefinedName];
                string modelName = cameraInfo[CameraInfoKey.ModelName];
                string serialNumber = cameraInfo[CameraInfoKey.SerialNumber];
                Console.WriteLine($"User Defined Name: {userDefinedName}");
                Console.WriteLine($"Model Name: {modelName}");
                Console.WriteLine($"Serial Number: {serialNumber}");
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

                if (_camera == null)
                {
                    string serialNumber = GetSerialNumber(userDefinedName);
                    _camera = new global::Basler.Pylon.Camera(serialNumber);
                    _camera.Open();
                    _isConnected = _camera.IsConnected;
                }

                return _isConnected ? 1 : 0;
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
                if (_camera != null)
                {
                    if (_isStreaming)
                    {
                        nRet = StopStreaming();
                        if (nRet != 1)
                        {
                            Trace.WriteLine("Stop Streaming Fail!");
                            return nRet;
                        }
                    }

                    if (_isGrabbing)
                    {
                        nRet = StopGrabbing();
                        if (nRet != 1)
                        {
                            Trace.WriteLine("Stop Grabbing Fail!");
                            return nRet;
                        }
                    }

                    if (_camera != null)
                    {
                        _camera.Close();
                        _camera.Dispose();
                        Trace.WriteLine("Close Device Fail!");
                    }

                    if (_converter != null)
                    {
                        _converter.Dispose();
                        _converter = null;
                        Trace.WriteLine("Destroy Device Fail!");
                    }
                }
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
            if (_camera != null && !_isGrabbing)
            {
                _camera.StreamGrabber.Start();
                if (_camera.StreamGrabber.IsGrabbing)
                {
                    _isGrabbing = true;
                }
                else
                {
                    Trace.WriteLine("Start Grabbing Fail!");
                }
            }
            nRet = _isGrabbing ? 1 : -1;
            return nRet;
        }

        public int StopGrabbing()
        {
            int nRet = -1;
            if (_camera != null && _isGrabbing)
            {
                _camera.StreamGrabber.Stop();
                if (!_camera.StreamGrabber.IsGrabbing)
                {
                    _isGrabbing = false;
                }
                else
                {
                    Trace.WriteLine("Stop Grabbing Fail!");
                }
            }
            nRet = _isGrabbing ? -1 : 1;
            return nRet;
        }

        public int StartStreaming()
        {
            return 1;
        }

        public int StopStreaming()
        {
            return 1;
        }

        public int GetParameter(KeyName key, ref object value)
        {
            if (_isConnected)
            {
                if (key == KeyName.ExposureTime)
                {
                    return (int)_camera.Parameters[PLCamera.ExposureTimeAbs].GetValue();
                }
            }
            return 1;
        }

        public int SetParameter(KeyName key, object value)
        {
            if (_isConnected)
            {
                if (key == KeyName.ExposureTime)
                {
                    _camera.Parameters[PLCamera.ExposureTimeAbs].SetValue((double)value);
                }
                if (key == KeyName.TriggerMode)
                {
                    _camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
                }
                if (key == KeyName.TriggerSource)
                {
                    _camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Software);
                }
                if (key == KeyName.TriggerSoftware)
                {
                    if (_camera.WaitForFrameTriggerReady(1000, TimeoutHandling.ThrowException))
                    {
                        _camera.ExecuteSoftwareTrigger();
                    }
                }
            }
            return 1;
        }

        public void ClearImageBuffer()
        {
            if (_isConnected)
            {
                _camera.Parameters[PLCameraInstance.ClearBufferModeEnable].SetValue(true);
            }
        }

        public Bitmap GrabFrame(int timeout = 1000)
        {
            try
            {
                lock (_lockObj)
                {
                    var grabResult = _camera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.Return);
                    if (grabResult != null)
                    {
                        using (grabResult)
                        {
                            if (grabResult.GrabSucceeded)
                            {
                                var bitmap = new Bitmap(grabResult.Width, grabResult.Height, PixelFormat.Format32bppRgb);
                                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                                _converter.OutputPixelFormat = PixelType.BGRA8packed;
                                _converter.Convert(bitmapData.Scan0, bitmapData.Stride * bitmap.Height, grabResult);
                                bitmap.UnlockBits(bitmapData);
                                return bitmap;
                            }
                            else
                            {
                                Console.WriteLine("Error: {0} {1}", grabResult.ErrorCode, grabResult.ErrorDescription);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            return null;
        }

        private string GetSerialNumber(string userDefinedName)
        {
            var allCameras = CameraFinder.Enumerate();
            foreach (var cameraInfo in allCameras)
            {
                if (userDefinedName == cameraInfo[CameraInfoKey.UserDefinedName])
                {
                    return cameraInfo[CameraInfoKey.SerialNumber];
                }
            }
            return string.Empty;
        }
    }
}
