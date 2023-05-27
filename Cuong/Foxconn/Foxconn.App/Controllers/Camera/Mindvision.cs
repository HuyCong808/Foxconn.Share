using Foxconn.App.Helper.Enums;
using MVSDK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CameraHandle = System.Int32;
using MvApi = MVSDK.MvApi;

namespace Foxconn.App.Controllers.Camera
{
    public class Mindvision : ICamera
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
        protected static CameraHandle m_hCamera = 0;   // Handle
        protected static IntPtr m_ImageBuffer;         // Preview channel RGB image cache
        protected static IntPtr m_ImageBufferSnapshot; // Capture channel RGB image cache
        protected static tSdkCameraCapbility tCameraCapability; // Camera characterization
        protected int m_iDisplayedFrames = 0; // The total number of frames already displayed
        protected CAMERA_SNAP_PROC m_CaptureCallback;
        protected IntPtr m_iCaptureCallbackCtx; // Contextual parameter of image callback function
        protected Thread m_tCaptureThread; // Image capture thread
        protected bool m_bExitCaptureThread = false; // Use threads to collect, let the thread exit the mark
        protected IntPtr m_iSettingPageMsgCallbackCtx; // Camera configuration interface message callback function context parameters
        protected tSdkFrameHead m_tFrameHead;
        //protected SnapshotDlg m_DlgSnapshot = new SnapshotDlg(); // Display window for capturing images
        //protected Settings m_DlgSettings = new Settings();
        protected bool m_bEraseBk = false;
        protected bool m_bSaveImage = false;
        #endregion
        private bool _disposed { get; set; }

        public Mindvision() { }

        public Mindvision(CameraInformation cameraInformation)
        {
            _cameraInformation = cameraInformation;
        }

        ~Mindvision()
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
            ShowMessage("CAMERA MINDVISION", ConsoleColor.Yellow);
            _disposed = false;
            _cameraType = CameraType.Mindvision;
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
                CameraSdkStatus status;
                tSdkCameraDevInfo[] tCameraDevInfoList;
                IntPtr ptr;
                int i;
                status = MvApi.CameraEnumerateDevice(out tCameraDevInfoList);
                if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    if (tCameraDevInfoList != null)
                    {
                        foreach (var devInfo in tCameraDevInfoList)
                        {
                            deviceList.Add(new CameraInformation
                            {
                                UserDefinedName = Encoding.UTF8.GetString(devInfo.acFriendlyName, 0, 7),
                                ModelName = Encoding.UTF8.GetString(devInfo.acProductName, 0, 9),
                                SerialNumber = Encoding.UTF8.GetString(devInfo.acSn, 0, 12),
                            });
                            Console.WriteLine($"Driver Version: {Encoding.UTF8.GetString(devInfo.acDriverVersion)}");
                            Console.WriteLine($"Friendly Name: {Encoding.UTF8.GetString(devInfo.acFriendlyName)}");
                            Console.WriteLine($"Link Name: {Encoding.UTF8.GetString(devInfo.acLinkName)}");
                            Console.WriteLine($"Port Type: {Encoding.UTF8.GetString(devInfo.acPortType)}");
                            Console.WriteLine($"Product Name: {Encoding.UTF8.GetString(devInfo.acProductName)}");
                            Console.WriteLine($"Product Series: {Encoding.UTF8.GetString(devInfo.acProductSeries)}");
                            Console.WriteLine($"Sensor Type: {Encoding.UTF8.GetString(devInfo.acSensorType)}");
                            Console.WriteLine($"SN: {Encoding.UTF8.GetString(devInfo.acSn)}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Did not find the camera, if you have connected to the camera may not be sufficient authority, try to run the program with administrator privileges.");
                    //Environment.Exit(0);
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

        public CameraHandle FindDevice(string serialNumber)
        {
            CameraSdkStatus status;
            tSdkCameraDevInfo[] tCameraDevInfoList;
            IntPtr ptr;
            int i;

            status = MvApi.CameraEnumerateDevice(out tCameraDevInfoList);
            if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            {
                if (tCameraDevInfoList != null)
                {
                    int index = 0;
                    foreach (var devInfo in tCameraDevInfoList)
                    {
                        if (Encoding.UTF8.GetString(devInfo.acSn).Contains(serialNumber))
                        {
                            //status = MvApi.CameraInit(ref tCameraDevInfoList[index], -1, -1, ref m_hCamera);
                            status = MvApi.CameraInit(ref tCameraDevInfoList[index], (int)emSdkParameterMode.PARAM_MODE_BY_SN, (int)emSdkParameterTeam.PARAMETER_TEAM_A, ref m_hCamera);
                            if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                            {
                                return m_hCamera;
                            }
                            else
                            {
                                m_hCamera = 0;
                                Console.WriteLine("Camera init error");
                                string errstr = string.Format("Camera initialization error, error code {0}, error reason", status);
                                string errstring = MvApi.CameraGetErrorString(status);
                                // string str1
                                Console.WriteLine(errstr + errstring, "ERROR");
                                //Environment.Exit(0);
                            }
                        }
                        index += 1;
                    }
                }
            }
            else
            {
                Console.WriteLine("Did not find the camera, if you have connected to the camera may not be sufficient authority, try to run the program with administrator privileges.");
                //Environment.Exit(0);
            }
            return 0;
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
            m_hCamera = FindDevice(serialNumber);
            if (m_hCamera > 0)
            {
                //Get the camera characterization.
                MvApi.CameraGetCapability(m_hCamera, out tCameraCapability);

                m_ImageBuffer = Marshal.AllocHGlobal(tCameraCapability.sResolutionRange.iWidthMax * tCameraCapability.sResolutionRange.iHeightMax * 3 + 1024);
                m_ImageBufferSnapshot = Marshal.AllocHGlobal(tCameraCapability.sResolutionRange.iWidthMax * tCameraCapability.sResolutionRange.iHeightMax * 3 + 1024);

                if (tCameraCapability.sIspCapacity.bMonoSensor != 0)
                {
                    MvApi.CameraSetIspOutFormat(m_hCamera, (uint)emImageFormat.CAMERA_MEDIA_TYPE_MONO8);
                }

                #region Initialize the display module, using the SDK's internal display interface
                ////Initialize the display module, using the SDK's internal display interface
                //MvApi.CameraDisplayInit(m_hCamera, PreviewBox.Handle);
                //MvApi.CameraSetDisplaySize(m_hCamera, PreviewBox.Width, PreviewBox.Height);
                #endregion

                #region Set the capture channel resolution.
                //Set the capture channel resolution.
                tSdkImageResolution tResolution;
                tResolution.uSkipMode = 0;
                tResolution.uBinAverageMode = 0;
                tResolution.uBinSumMode = 0;
                tResolution.uResampleMask = 0;
                tResolution.iVOffsetFOV = 0;
                tResolution.iHOffsetFOV = 0;
                tResolution.iWidthFOV = tCameraCapability.sResolutionRange.iWidthMax;
                tResolution.iHeightFOV = tCameraCapability.sResolutionRange.iHeightMax;
                tResolution.iWidth = tResolution.iWidthFOV;
                tResolution.iHeight = tResolution.iHeightFOV;
                //tResolution.iIndex = 0xff; represents the custom resolution if tResolution.iWidth and tResolution.iHeight
                //Defined as 0, then follow the preview channel to capture the resolution. Snapshot channel resolution can be dynamically changed.
                //In this example, the capture resolution is fixed to the maximum resolution.
                tResolution.iIndex = 0xff;
                tResolution.acDescription = new byte[32];//Descriptive information may not be set
                tResolution.iWidthZoomHd = 0;
                tResolution.iHeightZoomHd = 0;
                tResolution.iWidthZoomSw = 0;
                tResolution.iHeightZoomSw = 0;
                MvApi.CameraSetResolutionForSnap(m_hCamera, ref tResolution);
                #endregion

                #region Have the SDK dynamically create the camera's configuration window based on the camera's model.
                ////Have the SDK dynamically create the camera's configuration window based on the camera's model.
                //MvApi.CameraCreateSettingPage(m_hCamera, this.Handle, tCameraDevInfoList[numCamera].acFriendlyName,/*SettingPageMsgCalBack*/null,/*m_iSettingPageMsgCallbackCtx*/(IntPtr)null, 0);
                #endregion

                #region Add
                //                        // Two ways to get preview image, set callback function or use timer or independent thread mode,
                //                        // take the initiative to call CameraGetImageBuffer interface to capture.
                //                        // This example demonstrates only two ways, note that the two ways can also be used at the same time, but in the callback function,
                //                        // Do not use CameraGetImageBuffer, otherwise it will cause deadlock.
                //#if USE_CALL_BACK
                //                        m_CaptureCallback = new CAMERA_SNAP_PROC(ImageCaptureCallback);
                //                        MvApi.CameraSetCallbackFunction(m_hCamera, m_CaptureCallback, m_iCaptureCallbackCtx, ref pCaptureCallOld);
                //#else // If you need to use multi-threaded, use the following way
                //                        m_bExitCaptureThread = false;
                //                        m_tCaptureThread = new Thread(new ThreadStart(CaptureThreadProc));
                //                        m_tCaptureThread.Start();

                //#endif
                //                        //MvApi.CameraReadSN and MvApi.CameraWriteSN used to read and write from the camera user-defined serial number or other data, 32 bytes
                //                        //MvApi.CameraSaveUserData and MvApi.CameraLoadUserData are used to read custom data from the camera, 512 bytes


                //                        m_DlgSettings.m_hCamera = m_hCamera;
                #endregion

                #region Graphic transformation
                // Horizontal Flip
                if (true)
                {
                    MvApi.CameraSetMirror(m_hCamera, 0, 1);
                }
                else
                {
                    MvApi.CameraSetMirror(m_hCamera, 0, 0);
                }

                // Flip Vertically
                if (false)
                {
                    MvApi.CameraSetMirror(m_hCamera, 1, 1);
                }
                else
                {
                    MvApi.CameraSetMirror(m_hCamera, 1, 0);
                }

                // Rotate
                MvApi.CameraSetRotate(m_hCamera, 0);
                #endregion

                _isConnected = true;
            }
            else
            {
                _isConnected = false;
            }
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
            var status = MvApi.CameraPlay(m_hCamera);
            if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
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
            var status = MvApi.CameraPause(m_hCamera);
            if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
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
                MvApi.CameraSetTriggerMode(m_hCamera, 1);
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
                        MvApi.CameraSetTriggerMode(m_hCamera, 2);
                        break;
                    case TriggerSource.Software:
                        MvApi.CameraSetTriggerMode(m_hCamera, 1);
                        break;
                }
            }
            else
            {
                MvApi.CameraSetTriggerMode(m_hCamera, 0);
            }
            return true;
        }

        public void ExcuteTriggerSoftware()
        {
            try
            {
                if (!_isConnected || !_isGrabbing)
                    return; // Do nothing.
                // When the soft trigger is executed, the camera's internal buffer will be emptied, and the exposure will be resumed for an image.
                MvApi.CameraSoftTriggerEx(m_hCamera, 1);
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
                    tSdkFrameHead tFrameHead;
                    IntPtr uRawBuffer;//The SDK allocates memory for RAW data and releases it

                    if (m_hCamera <= 0)
                    {
                        return null;//The camera has not been initialized yet, the handle is invalid
                    }

                    // CameraSnapToBuffer will switch the resolution to take pictures, slower. Do real-time processing, it is recommended to use CameraGetImageBuffer function to take a picture or callback function.
                    if (MvApi.CameraSnapToBuffer(m_hCamera, out tFrameHead, out uRawBuffer, 500) == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                    {
                        // Convert the raw data from the camera to RGB format into memory m_ImageBufferSnapshot
                        MvApi.CameraImageProcess(m_hCamera, uRawBuffer, m_ImageBufferSnapshot, ref tFrameHead);

                        // CameraSnapToBuffer must be successfully released CameraReleaseImageBuffer release SDK allocated RAW data buffer
                        // Otherwise, a deadlock will occur, the preview and snap channels will be blocked until CameraReleaseImageBuffer is called and unlocked.
                        MvApi.CameraReleaseImageBuffer(m_hCamera, uRawBuffer);

                        // Convert buffer to bitmap
                        MvApi.CSharpImageFromFrame(m_ImageBufferSnapshot, ref tFrameHead);
                        int w = tFrameHead.iWidth;
                        int h = tFrameHead.iHeight;
                        bool gray = tFrameHead.uiMediaType == (uint)emImageFormat.CAMERA_MEDIA_TYPE_MONO8;
                        var bmp = new Bitmap(w, h, gray ? w : w * 3, gray ? PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb, m_ImageBufferSnapshot);
                        return bmp;
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
                double pfExposureTime = 0.0;
                if (m_hCamera > 0)
                {
                    MvApi.CameraGetExposureTime(m_hCamera, ref pfExposureTime);
                }
                return pfExposureTime;
            }
            set
            {
                if (m_hCamera > 0)
                {
                    MvApi.CameraSetExposureTime(m_hCamera, (double)value);
                }
            }
        }

        public double Gain { get; set; }

        public bool Disconnect()
        {
            try
            {
                if (m_hCamera > 0)
                {
                    if (_isGrabbing)
                        StopGrabbing();
                    if (_isStreaming)
                        StopStreaming();
                    var status = MvApi.CameraUnInit(m_hCamera);
                    if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                    {
                        Console.WriteLine("Close device!");
                        _isConnected = false;
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
                if (m_hCamera > 0)
                {
                    //MvApi.CameraUnInit(m_hCamera);
                    Marshal.FreeHGlobal(m_ImageBuffer);
                    Marshal.FreeHGlobal(m_ImageBufferSnapshot);
                    m_hCamera = 0;
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
