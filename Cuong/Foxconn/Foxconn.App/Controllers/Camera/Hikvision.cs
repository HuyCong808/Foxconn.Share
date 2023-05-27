using Foxconn.App.Helper.Enums;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Foxconn.App.Controllers.Camera
{
    public class Hikvision : ICamera
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
        private MyCamera _camera { get; set; }
        private MyCamera.MV_CC_DEVICE_INFO_LIST _deviceListSdk;
        // Buffer for getting image from driver
        private uint m_nBufSizeForDriver { get; set; }
        private byte[] m_pBufForDriver { get; set; }
        // Buffer for saving image
        private uint m_nBufSizeForSaveImage { get; set; }
        private byte[] m_pBufForSaveImage { get; set; }
        private long _frameCounter { get; set; }
        private long _frameNumber { get; set; }
        #endregion
        private bool _disposed { get; set; }

        public Hikvision() { }

        public Hikvision(CameraInformation cameraInformation)
        {
            _cameraInformation = cameraInformation;
        }

        ~Hikvision()
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
            ShowMessage("CAMERA HIKVISION", ConsoleColor.Yellow);
            _disposed = false;
            _cameraType = CameraType.Hikvision;
            _filterMethod = FilterDevice.None;
            _orderDeviceMethod = OrderListDevice.SerialNumber;
            _filterString = new List<string>();
            _deviceList = new List<CameraInformation>();
            _cameraInformation = new CameraInformation();
            _deviceListSdk = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            // Buffer for getting image from driver
            m_nBufSizeForDriver = 3072 * 2048 * 3;
            m_pBufForDriver = new byte[3072 * 2048 * 3];
            // Buffer for saving image
            m_nBufSizeForSaveImage = 3072 * 2048 * 3 * 3 + 2048;
            m_pBufForSaveImage = new byte[3072 * 2048 * 3 * 3 + 2048];
            _frameCounter = 0;
            _frameNumber = 0;
            return ScanDeviceList() > 0;
        }

        public int ScanDeviceList()
        {
            try
            {
                int nRet;
                // Create device list
                GC.Collect();
                _deviceList.Clear();
                var deviceList = new List<CameraInformation>();
                // Get list of camera devices.
                nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref _deviceListSdk);
                if (nRet != MyCamera.MV_OK)
                {
                    Console.WriteLine("Enumerate devices fail!:{0:x8}", nRet);
                    return 0;
                }
                for (int i = 0; i < _deviceListSdk.nDeviceNum; i++)
                {
                    var device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(_deviceListSdk.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                    if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                    {
                        var buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                        var stGigEDeviceInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                        deviceList.Add(new CameraInformation
                        {
                            SerialNumber = stGigEDeviceInfo.chSerialNumber,
                            ModelName = stGigEDeviceInfo.chModelName,
                            UserDefinedName = stGigEDeviceInfo.chUserDefinedName
                        });
                        if (stGigEDeviceInfo.chUserDefinedName != "")
                        {
                            Console.WriteLine("GigE: " + stGigEDeviceInfo.chUserDefinedName + " (" + stGigEDeviceInfo.chSerialNumber + ")");
                        }
                        else
                        {
                            Console.WriteLine("GigE: " + stGigEDeviceInfo.chManufacturerName + " " + stGigEDeviceInfo.chModelName + " (" + stGigEDeviceInfo.chSerialNumber + ")");
                        }
                    }
                    else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                    {
                        var buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stUsb3VInfo, 0);
                        var stUsb3DeviceInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                        deviceList.Add(new CameraInformation
                        {
                            SerialNumber = stUsb3DeviceInfo.chSerialNumber,
                            ModelName = stUsb3DeviceInfo.chModelName,
                            UserDefinedName = stUsb3DeviceInfo.chUserDefinedName
                        });
                        if (stUsb3DeviceInfo.chUserDefinedName != "")
                        {
                            Console.WriteLine("USB: " + stUsb3DeviceInfo.chUserDefinedName + " (" + stUsb3DeviceInfo.chSerialNumber + ")");
                        }
                        else
                        {
                            Console.WriteLine("USB: " + stUsb3DeviceInfo.chManufacturerName + " " + stUsb3DeviceInfo.chModelName + " (" + stUsb3DeviceInfo.chSerialNumber + ")");
                        }
                    }
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

        public MyCamera.MV_CC_DEVICE_INFO FindDevice(string serialNumber)
        {
            for (int i = 0; i < _deviceListSdk.nDeviceNum; i++)
            {
                // Get selected device information
                var device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(_deviceListSdk.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    var buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                    var stGigEDeviceInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    if (serialNumber == stGigEDeviceInfo.chSerialNumber)
                    {
                        return device;
                    }
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    var buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stUsb3VInfo, 0);
                    var stUsb3DeviceInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    if (serialNumber == stUsb3DeviceInfo.chSerialNumber)
                    {
                        return device;
                    }
                }
            }
            return new MyCamera.MV_CC_DEVICE_INFO();
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
            try
            {
                if (_isConnected)
                    return true;
                // Create a new camera object.
                _camera = new MyCamera();
                var device = FindDevice(serialNumber);
                int nRet = _camera.MV_CC_CreateDevice_NET(ref device);
                if (nRet != MyCamera.MV_OK)
                {
                    return false;
                }
                // Judge whether the specified device is accessible.
                if (MyCamera.MV_CC_IsDeviceAccessible_NET(ref device, MyCamera.MV_ACCESS_Exclusive) == false)
                {
                    Console.WriteLine("The device is unaccessible");
                    return false;
                }
                // Open device.
                nRet = _camera.MV_CC_OpenDevice_NET();
                if (nRet == MyCamera.MV_OK)
                {
                    _isConnected = true;
                }
                else
                {
                    _camera.MV_CC_DestroyDevice_NET();
                    Console.WriteLine("Device open fail!", nRet);
                    return false;
                }
                // Detection network optimal package size(It only works for the GigE camera).
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    int nPacketSize = _camera.MV_CC_GetOptimalPacketSize_NET();
                    if (nPacketSize > 0)
                    {
                        nRet = _camera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                        if (nRet != MyCamera.MV_OK)
                        {
                            Console.WriteLine("Warning: Set Packet Size failed {0:x8}", nRet);
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Warning: Get Packet Size failed {0:x8}", nPacketSize);
                        return false;
                    }
                }
                GetPayLoadSize();
                _camera.MV_CC_SetImageNodeNum_NET(1); //Get only one frame
                // Update camera information.
                _cameraInformation = (CameraInformation)_deviceList.Find(item => item.SerialNumber == serialNumber).Clone();
                return _isConnected;
            }
            catch (Exception ex)
            {
                ErrorDetails(ex.StackTrace);
                return false;
            }
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
            if (IsConnected)
            {
                return true;
            }

            try
            {
                // Create a new camera object.
                // Connect to camera.
                // Set trigger software.
                // Delay 1000ms
                // Start grabbing.

                if (_isConnected && _isGrabbing)
                    return true;
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
            int nRet = _camera.MV_CC_StartGrabbing_NET();
            if (nRet == MyCamera.MV_OK)
            {
                _isGrabbing = true;
            }
            else
            {
                _isGrabbing = false;
            }
            return true;
        }

        public bool StopGrabbing()
        {
            if (!_isConnected)
                return true;
            if (!_isGrabbing)
                return true;
            // Stop grabbing.
            int nRet = _camera.MV_CC_StopGrabbing_NET();
            if (nRet == MyCamera.MV_OK)
            {
                _isGrabbing = false;
            }
            else
            {
                _isGrabbing = true;
            }
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
                _camera.MV_CC_SetEnumValue_NET("TriggerMode", 1);
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
                        _camera.MV_CC_SetEnumValue_NET("TriggerSource", 7);
                        break;
                }
            }
            else
            {
                _camera.MV_CC_SetEnumValue_NET("TriggerMode", 0);
            }
            return true;
        }

        public void ExcuteTriggerSoftware()
        {
            try
            {
                if (!_isConnected || !_isGrabbing)
                    return; // Do nothing.
                int nRet = _camera.MV_CC_SetCommandValue_NET("TriggerSoftware");
                if (nRet != MyCamera.MV_OK)
                {
                    Console.WriteLine("Trigger Fail!", nRet);
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
                    int nRet;
                    var pData = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForDriver, 0);
                    var stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
                    // Get one frame timeout, timeout is 1 sec
                    nRet = _camera.MV_CC_GetOneFrameTimeout_NET(pData, m_nBufSizeForDriver, ref stFrameInfo, timeout);
                    if (MyCamera.MV_OK != nRet)
                    {
                        Console.WriteLine("No Data!", nRet);
                        return null;
                    }
                    else
                    {
                        Console.WriteLine($"Get One Frame: Width[{stFrameInfo.nWidth}], Height[{stFrameInfo.nHeight}], FrameNum[{stFrameInfo.nFrameNum}]");
                    }

                    MyCamera.MvGvspPixelType enDstPixelType;
                    if (IsMonoData(stFrameInfo.enPixelType))
                    {
                        enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8;
                    }
                    else if (IsColorData(stFrameInfo.enPixelType))
                    {
                        enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
                    }
                    else
                    {
                        Console.WriteLine("No such pixel type!", 0);
                        return null;
                    }

                    var pImage = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForSaveImage, 0);
                    var stSaveParam = new MyCamera.MV_SAVE_IMAGE_PARAM_EX();
                    var stConverPixelParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();
                    stConverPixelParam.nWidth = stFrameInfo.nWidth;
                    stConverPixelParam.nHeight = stFrameInfo.nHeight;
                    stConverPixelParam.pSrcData = pData;
                    stConverPixelParam.nSrcDataLen = stFrameInfo.nFrameLen;
                    stConverPixelParam.enSrcPixelType = stFrameInfo.enPixelType;
                    stConverPixelParam.enDstPixelType = enDstPixelType;
                    stConverPixelParam.pDstBuffer = pImage;
                    stConverPixelParam.nDstBufferSize = m_nBufSizeForSaveImage;
                    nRet = _camera.MV_CC_ConvertPixelType_NET(ref stConverPixelParam);
                    if (MyCamera.MV_OK != nRet)
                    {
                        return null;
                    }

                    if (enDstPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
                    {
                        //************************Mono8 Bitmap*******************************
                        var bmp = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 1, PixelFormat.Format8bppIndexed, pImage);

                        var cp = bmp.Palette;
                        // init palette
                        for (int i = 0; i < 256; i++)
                        {
                            cp.Entries[i] = Color.FromArgb(i, i, i);
                        }
                        // set palette back
                        bmp.Palette = cp;

                        //bmp.Save("image.bmp", ImageFormat.Bmp);
                        return bmp;
                    }
                    else
                    {
                        //*********************RGB8 Bitmap**************************
                        for (int i = 0; i < stFrameInfo.nHeight; i++)
                        {
                            for (int j = 0; j < stFrameInfo.nWidth; j++)
                            {
                                byte chRed = m_pBufForSaveImage[i * stFrameInfo.nWidth * 3 + j * 3];
                                m_pBufForSaveImage[i * stFrameInfo.nWidth * 3 + j * 3] = m_pBufForSaveImage[i * stFrameInfo.nWidth * 3 + j * 3 + 2];
                                m_pBufForSaveImage[i * stFrameInfo.nWidth * 3 + j * 3 + 2] = chRed;
                            }
                        }
                        try
                        {
                            var bmp = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 3, PixelFormat.Format24bppRgb, pImage);
                            //bmp.Save("image.bmp", ImageFormat.Bmp);
                            return bmp;
                        }
                        catch
                        {

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
                if (_isConnected)
                {
                    var stParam = new MyCamera.MVCC_FLOATVALUE();
                    int nRet = _camera.MV_CC_GetFloatValue_NET("ExposureTime", ref stParam);
                    if (nRet == MyCamera.MV_OK)
                    {
                        Console.WriteLine($"Get ExposureTime: {stParam.fCurValue}");
                        return stParam.fCurValue;
                    }
                    else
                    {
                        return -1;
                    }
                }
                return -1;
            }
            set
            {
                if (_isConnected)
                {
                    int nRet = _camera.MV_CC_SetFloatValue_NET("ExposureTime", (float)value);
                    if (nRet == MyCamera.MV_OK)
                    {
                        Console.WriteLine($"Set ExposureTime: {value}");
                    }
                }
            }
        }

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
                    int nRet = _camera.MV_CC_CloseDevice_NET();
                    if (nRet == MyCamera.MV_OK)
                    {
                        _isConnected = false;
                        Console.WriteLine("Close device!");
                    }
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
                    int nRet = _camera.MV_CC_DestroyDevice_NET();
                    if (nRet == MyCamera.MV_OK)
                    {
                        _camera = null;
                        Console.WriteLine("Destroy device!");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorDetails(ex.StackTrace);
                return false;
            }
        }

        public bool GetPayLoadSize()
        {
            if (_isConnected)
            {
                int nRet;
                uint nPayloadSize = 0;
                var stParam = new MyCamera.MVCC_INTVALUE();
                nRet = _camera.MV_CC_GetIntValue_NET("PayloadSize", ref stParam);
                if (nRet != MyCamera.MV_OK)
                {
                    Console.WriteLine("Get PayloadSize failed", nRet);
                    return false;
                }
                nPayloadSize = stParam.nCurValue;
                if (nPayloadSize > m_nBufSizeForDriver)
                {
                    m_nBufSizeForDriver = nPayloadSize;
                    m_pBufForDriver = new byte[m_nBufSizeForDriver];

                    // Determine the buffer size to save image
                    // BMP image size: width * height * 3 + 2048 (Reserved for BMP header)
                    m_nBufSizeForSaveImage = m_nBufSizeForDriver * 3 + 2048;
                    m_pBufForSaveImage = new byte[m_nBufSizeForSaveImage];
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsMonoData(MyCamera.MvGvspPixelType enGvspPixelType)
        {
            switch (enGvspPixelType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsColorData(MyCamera.MvGvspPixelType enGvspPixelType)
        {
            switch (enGvspPixelType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_YUYV_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YCBCR411_8_CBYYCRYY:
                    return true;
                default:
                    return false;
            }
        }

        private Bitmap ConvertRawToBitmap(IntPtr pData, MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo)
        {
            return null;
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
