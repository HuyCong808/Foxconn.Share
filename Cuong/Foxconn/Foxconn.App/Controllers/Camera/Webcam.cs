using DirectShowLib;
using Emgu.CV;
using Foxconn.App.Helper.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Foxconn.App.Controllers.Camera
{
    public class Webcam : ICamera
    {
        private readonly object _syncObject = new object();
        public CameraType CameraType => _cameraType;
        public ErrorEvent InvokeError { get; set; }
        public StreamingEvent InvokeStreaming { get; set; }
        public List<CameraInformation> DeviceList => _deviceList;
        public CameraInformation CameraInformation
        {
            get => _cameraInformation;
            set => _cameraInformation = value;
        }
        public bool IsConnected => _isConnected;
        public bool IsGrabbing => _isGrabbing;
        public bool IsStreaming => _isStreaming;
        private CameraType _cameraType { get; set; }
        private static FilterDevice _filterMethod { get; set; }
        private static OrderListDevice _orderDeviceMethod { get; set; }
        private static List<string> _filterString { get; set; }
        private List<CameraInformation> _deviceList { get; set; }
        private bool _isConnected { get; set; }
        private bool _isGrabbing { get; set; }
        private bool _isStreaming { get; set; }
        private CameraInformation _cameraInformation { get; set; }
        //private CameraObject _camera { get; set; }
        #region CAMERA VARIABLES
        private VideoCapture _camera { get; set; }
        private Mat _matFrame { get; set; }
        private Bitmap _bitmapFrame { get; set; }
        #endregion
        private bool _disposed { get; set; }

        public Webcam() { }

        public Webcam(CameraInformation cameraInformation)
        {
            _cameraInformation = cameraInformation;
        }

        ~Webcam()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        //     Gets the maximum number of gen
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Release();
            }

            _disposed = true;
        }

        public bool Initialize()
        {
            ShowMessage("CAMERA WEBCAM", ConsoleColor.Yellow);
            _disposed = false;
            _cameraType = CameraType.Webcam;
            _filterMethod = FilterDevice.None;
            _orderDeviceMethod = OrderListDevice.SerialNumber;
            _filterString = new List<string>();
            _deviceList = new List<CameraInformation>();
            _cameraInformation = new CameraInformation();
            return ScanDeviceList() > 0;
        }

        public int ScanDeviceList()
        {
            try
            {
                // Create device list
                GC.Collect();
                _deviceList.Clear();
                var deviceList = new List<CameraInformation>();
                // Get list of camera devices.
                int index = 0;
                foreach (var device in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
                {
                    var modelName = device.Name;
                    deviceList.Add(new CameraInformation
                    {
                        UserDefinedName = $"Camera{index}",
                        ModelName = modelName,
                        SerialNumber = index.ToString(),
                    });
                    Console.WriteLine($"User Defined Name: Camera{index}");
                    Console.WriteLine($"Model Name: {modelName}");
                    Console.WriteLine($"Serial Number: {index}");
                    index += 1;
                }
                if (deviceList.Count > 0)
                {
                    var orderList = new List<CameraInformation>();
                    switch (_orderDeviceMethod)
                    {
                        case OrderListDevice.None:
                            orderList = deviceList;
                            break;
                        case OrderListDevice.UserDefinedName:
                            orderList = deviceList.OrderBy(item => item.UserDefinedName).ToList();
                            break;
                        case OrderListDevice.ModelName:
                            orderList = deviceList.OrderBy(item => item.ModelName).ToList();
                            break;
                        case OrderListDevice.SerialNumber:
                            orderList = deviceList.OrderBy(item => item.SerialNumber).ToList();
                            break;
                        default:
                            break;
                    }
                    foreach (var item in orderList)
                    {
                        switch (_filterMethod)
                        {
                            case FilterDevice.None:
                                {
                                    _deviceList.Add(item);
                                    break;
                                }
                            case FilterDevice.UserDefinedName:
                                {
                                    if (_filterString.IndexOf(item.UserDefinedName) > -1)
                                    {
                                        _deviceList.Add(item);
                                    }
                                }
                                break;
                            case FilterDevice.ModelName:
                                {
                                    if (_filterString.IndexOf(item.ModelName) > -1)
                                    {
                                        _deviceList.Add(item);
                                    }
                                }
                                break;
                            case FilterDevice.SerialNumber:
                                {
                                    if (_filterString.IndexOf(item.SerialNumber) > -1)
                                    {
                                        _deviceList.Add(item);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                _deviceList = deviceList;
                return _deviceList.Count;
            }
            catch (Exception ex)
            {
                ErrorDetails(ex);
                return 0;
            }
        }

        public string FindDeviceBySerialNumber(string serialNumber)
        {
            return string.Empty;
        }

        public bool Connect(CameraInformation cameraInformation = null)
        {
            if (cameraInformation == null)
            {
                return _cameraInformation != null && Connect(_cameraInformation.SerialNumber);
            }
            else
            {
                return Connect(cameraInformation.SerialNumber);
            }
        }

        public bool Connect(int index)
        {
            return index < _deviceList.Count && Connect(_deviceList[index].SerialNumber);
        }

        public bool Connect(string serialNumber)
        {
            if (_isConnected)
                return true;
            // Create a new camera object.
            CvInvoke.UseOpenCL = false;
            _camera = new VideoCapture(Convert.ToInt32(serialNumber), VideoCapture.API.DShow);
            _isConnected = _camera.IsOpened;
            SetDefaultParameter();
            // Update camera information.
            _cameraInformation = (CameraInformation)_deviceList.Find(item => item.SerialNumber == serialNumber).Clone();
            return _isConnected;
        }

        public bool OpenStrategies(bool grabLastest, CameraInformation cameraInformation = null)
        {
            if (cameraInformation == null)
            {
                return _cameraInformation != null && OpenWithStrategies(_cameraInformation.SerialNumber, grabLastest);
            }
            else
            {
                return Connect(cameraInformation.SerialNumber);
            }
        }

        public bool OpenWithStrategies(string serialNumber, bool grabLastest)
        {
            try
            {
                // Create a new camera object.
                // Connect to camera.
                // Set trigger software.
                // Delay 1000ms
                // Start grabbing.

                if (_isConnected && _isGrabbing)
                    return true;
                Connect(serialNumber);
                SetTriggerMode(TriggerMode.Enable, TriggerSource.Software);
                //Thread.Sleep(1000);
                StartGrabbing();
                return true;
            }
            catch (Exception ex)
            {
                ErrorDetails(ex);
                return false;
            }
        }

        public bool OpenSettings(bool show = true)
        {
            return true;
        }

        public bool StartGrabbing()
        {
            if (!_isConnected)
                return false;
            if (_isGrabbing)
                return true;
            // Start grabbing.
            //camDevice.Start();
            _isGrabbing = true;
            return true;
        }

        public bool StopGrabbing()
        {
            if (!_isConnected)
                return true;
            if (!_isGrabbing)
                return true;
            // Stop grabbing.
            _camera.Stop();
            _isGrabbing = false;
            return true;
        }

        public bool StartStreaming()
        {
            if (!_isConnected)
                return false;
            if (_isStreaming)
                return true;
            // Start streaming.
            // ...
            _isStreaming = true;
            return true;
        }

        public bool StopStreaming()
        {
            if (!_isConnected)
                return true;
            if (!_isStreaming)
                return true;
            // Stop streaming.
            // ...
            _isStreaming = false;
            return true;
        }

        public bool SetTriggerMode(TriggerMode state, TriggerSource mode = TriggerSource.Software, bool grabLastest = false)
        {
            if (!_isConnected)
                return false; // Do nothing.
            if (state == TriggerMode.Enable)
            {
                //_camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
                if (grabLastest)
                {
                    try
                    {
                        //if (_isGrabbing)
                        //{
                        //    _camera.Parameters[PLCameraInstance.OutputQueueSize].SetValue(1L);
                        //}
                        //else
                        //{
                        //    _camera.Parameters[PLCameraInstance.OutputQueueSize].SetValue(1L);
                        //}
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                switch (mode)
                {
                    case TriggerSource.Line1:
                        //_camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Line1);
                        break;
                    case TriggerSource.Software:
                        //_camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Software);
                        break;
                }
            }
            else
            {
                //_camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.Off);
            }
            return true;
        }

        public void ExcuteTriggerSoftware()
        {
            try
            {
                if (!_isConnected || !_isGrabbing)
                    return; // Do nothing.
                _matFrame = null;
                _bitmapFrame = null;
                _camera.Start();
                System.Threading.Thread.Sleep(250);
                _camera.Stop();
            }
            catch (Exception ex)
            {
                ErrorDetails(ex);
            }
        }

        public Bitmap GrabFrame(int timeout = 1000)
        {
            try
            {
                if (!_isConnected || !_isGrabbing)
                    return null; // Do nothing.
                lock (_syncObject)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (_camera != null && _camera.Ptr != IntPtr.Zero)
                        {
                            _matFrame = _camera.QueryFrame();
                        }
                    }
                    return _matFrame?.ToBitmap();
                    //return _bitmapFrame ?? null;
                }
                //return null;
            }
            catch (Exception ex)
            {
                ErrorDetails(ex);
                return null;
            }
        }

        public void GetParameter()
        {



        }

        public void SetParameter()
        {
            if (_camera != null)
            {
                // It use for show properties of webcam, note it only use for VideoCapture.API.DShow
                _camera.Set(Emgu.CV.CvEnum.CapProp.Settings, 1);
            }
        }

        private void SetDefaultParameter()
        {
            if (_camera != null)
            {
                // Codec
                var fourcc = VideoWriter.Fourcc('M', 'J', 'P', 'G');
                _camera.Set(Emgu.CV.CvEnum.CapProp.FourCC, fourcc);

                // Resolution
                _camera.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
                _camera.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, 1080);

                // Fps
                _camera.Set(Emgu.CV.CvEnum.CapProp.Fps, 30);

                // Frame count
                _camera.Set(Emgu.CV.CvEnum.CapProp.FrameCount, 30);

                //// Brightness
                //_camera.Set(Emgu.CV.CvEnum.CapProp.Brightness, 0);

                //// Contrast
                //_camera.Set(Emgu.CV.CvEnum.CapProp.Contrast, 0);

                //// Hue
                //_camera.Set(Emgu.CV.CvEnum.CapProp.Hue, 0);

                //// Saturation
                //_camera.Set(Emgu.CV.CvEnum.CapProp.Saturation, 58);

                //// Sharpness
                //_camera.Set(Emgu.CV.CvEnum.CapProp.Sharpness, 3);

                //// Gamma
                //_camera.Set(Emgu.CV.CvEnum.CapProp.Gamma, 165);

                //// Whiteblance
                //_camera.Set(Emgu.CV.CvEnum.CapProp.AutoWb, 44);
                //_camera.Set(Emgu.CV.CvEnum.CapProp.WhiteBalanceRedV, 26);
                //_camera.Set(Emgu.CV.CvEnum.CapProp.WhiteBalanceBlueU, 17);

                //// Exposure
                //_camera.Set(Emgu.CV.CvEnum.CapProp.AutoExposure, 21);
                //_camera.Set(Emgu.CV.CvEnum.CapProp.Exposure, -6);

                //// Focus
                //_camera.Set(Emgu.CV.CvEnum.CapProp.Autofocus, 39);
                //_camera.Set(Emgu.CV.CvEnum.CapProp.Focus, 28);

                // Buffer size
                _camera.Set(Emgu.CV.CvEnum.CapProp.Buffersize, 38);

                //// It use only when the camera supports this mode
                //_camera.Set(Emgu.CV.CvEnum.CapProp.Trigger, 24);
                //_camera.Set(Emgu.CV.CvEnum.CapProp.TriggerDelay, 25);

                //// It use for show properties of webcam, note it only use for VideoCapture.API.DShow
                //_camera.Set(Emgu.CV.CvEnum.CapProp.Settings, 1);
            }
        }

        public double ExposureTime { get; set; }

        public double Gain { get; set; }

        public bool Disconnect()
        {
            try
            {
                if (_camera != null)
                {
                    if (_isGrabbing)
                        StopGrabbing();
                    if (_isStreaming)
                        StopStreaming();
                    _isConnected = false;
                    Console.WriteLine("Close device!");
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorDetails(ex.StackTrace);
                return false;
            }
        }

        public bool Release()
        {
            try
            {
                if (_camera != null)
                {
                    _camera.Dispose();
                    _camera = null;
                    Console.WriteLine("Destroy device!");
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorDetails(ex.StackTrace);
                return false;
            }
        }

        private void ShowMessage(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private void ErrorDetails(Exception ex)
        {
            InvokeError?.Invoke(ex);
        }

        private void ErrorDetails(string msg)
        {
            InvokeError?.Invoke(new Exception(msg));
        }
    }
}
