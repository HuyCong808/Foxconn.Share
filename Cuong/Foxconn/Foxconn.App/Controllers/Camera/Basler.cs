using Basler.Pylon;
using Foxconn.App.Helper.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;

namespace Foxconn.App.Controllers.Camera
{
    public class Basler : ICamera
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
        private global::Basler.Pylon.Camera _camera { get; set; }
        private PixelDataConverter _converter { get; set; }
        #endregion
        private bool _disposed { get; set; }

        public Basler()
        {
            _converter = new PixelDataConverter();
        }

        public Basler(CameraInformation cameraInformation)
        {
            _cameraInformation = cameraInformation;
            _converter = new PixelDataConverter();
        }

        ~Basler()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
            ShowMessage("CAMERA BASLER", ConsoleColor.Yellow);
            _disposed = false;
            _cameraType = CameraType.Basler;
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
                GC.Collect();
                _deviceList.Clear();
                var deviceList = new List<CameraInformation>();
                // Get list of camera devices.
                var allCameras = CameraFinder.Enumerate();
                foreach (var cameraInfo in allCameras)
                {
                    deviceList.Add(new CameraInformation
                    {
                        UserDefinedName = cameraInfo[CameraInfoKey.UserDefinedName],
                        ModelName = cameraInfo[CameraInfoKey.ModelName],
                        SerialNumber = cameraInfo[CameraInfoKey.SerialNumber],
                    });
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
            _camera = new global::Basler.Pylon.Camera(serialNumber);
            _camera.Open();
            _isConnected = _camera.IsConnected;
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
                Thread.Sleep(1000);
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
            _camera.StreamGrabber.Start();
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
            _camera.StreamGrabber.Stop();
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
                _camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
                if (grabLastest)
                {
                    try
                    {
                        if (_camera.StreamGrabber.IsGrabbing)
                        {
                            _camera.Parameters[PLCameraInstance.OutputQueueSize].SetValue(1L);
                        }
                        else
                        {
                            _camera.Parameters[PLCameraInstance.OutputQueueSize].SetValue(1L);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                switch (mode)
                {
                    case TriggerSource.Line1:
                        _camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Line1);
                        break;
                    case TriggerSource.Software:
                        _camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Software);
                        break;
                }
            }
            else
            {
                _camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.Off);
            }
            return true;
        }

        public void ExcuteTriggerSoftware()
        {
            try
            {
                if (!_isConnected || !_isGrabbing)
                    return; // Do nothing.
                if (_camera.CanWaitForFrameTriggerReady)
                {
                    _camera.WaitForFrameTriggerReady(1000, TimeoutHandling.ThrowException);
                    _camera.ExecuteSoftwareTrigger();
                }
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
                    var grabResult = _camera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.ThrowException);
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
                        }
                    }
                }
                return null;
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

        }

        public double ExposureTime
        {
            get
            {
                return _camera.Parameters[PLCamera.ExposureTimeAbs].GetValue();
            }
            set
            {
                _camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(value);
            }
        }

        public double Gain
        {
            get
            {
                return _camera.Parameters[PLCamera.Gain].GetValue();
            }
            set
            {
                _camera.Parameters[PLCamera.GainAbs].SetValue(value);
            }
        }

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
                    _camera.Close();
                    _camera.Dispose();
                    _camera = null;
                    Console.WriteLine("Destroy device!");
                }
                if (_converter != null)
                {
                    _converter.Dispose();
                    _converter = null;
                    Console.WriteLine("Destroy converter!");
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorDetails(ex.StackTrace);
                return false;
            }
        }

        private void ErrorDetails(Exception ex)
        {
            InvokeError?.Invoke(ex);
        }

        private void ErrorDetails(string msg)
        {
            InvokeError?.Invoke(new Exception(msg));
        }

        private void ShowMessage(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
