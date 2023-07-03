using MvCamCtrl.NET;
using MVSDK;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CameraHandle = System.Int32;
using MvApi = MVSDK.MvApi;

namespace Foxconn.Editor.Camera
{
    public class MindVision : ICamera
    {
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
                        Console.WriteLine($"Driver Version: {Encoding.UTF8.GetString(devInfo.acDriverVersion)}");
                        Console.WriteLine($"Friendly Name: {Encoding.UTF8.GetString(devInfo.acFriendlyName)}");
                        Console.WriteLine($"Link Name: {Encoding.UTF8.GetString(devInfo.acLinkName)}");
                        Console.WriteLine($"Port Type: {Encoding.UTF8.GetString(devInfo.acPortType)}");
                        Console.WriteLine($"Product Name: {Encoding.UTF8.GetString(devInfo.acProductName)}");
                        Console.WriteLine($"Product Series: {Encoding.UTF8.GetString(devInfo.acProductSeries)}");
                        Console.WriteLine($"Sensor Type: {Encoding.UTF8.GetString(devInfo.acSensorType)}");
                        Console.WriteLine($"SN: {Encoding.UTF8.GetString(devInfo.acSn)}");
                        string userDefinedName = Encoding.UTF8.GetString(devInfo.acFriendlyName, 0, 7);
                        string modelName = Encoding.UTF8.GetString(devInfo.acProductName, 0, 9);
                        string serialNumber = Encoding.UTF8.GetString(devInfo.acSn, 0, 12);
                    }
                }
            }
            else
            {
                Console.WriteLine("Did not find the camera, if you have connected to the camera may not be sufficient authority, try to run the program with administrator privileges.");
                //Environment.Exit(0);
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

                // Create a new camera object.
                m_hCamera = FindDevice(userDefinedName);
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
                    // Two ways to get preview image, set callback function or use timer or independent thread mode,
                    // take the initiative to call CameraGetImageBuffer interface to capture.
                    // This example demonstrates only two ways, note that the two ways can also be used at the same time, but in the callback function,
                    // Do not use CameraGetImageBuffer, otherwise it will cause deadlock.
#if USE_CALL_BACK
                    m_CaptureCallback = new CAMERA_SNAP_PROC(ImageCaptureCallback);
                    MvApi.CameraSetCallbackFunction(m_hCamera, m_CaptureCallback, m_iCaptureCallbackCtx, ref pCaptureCallOld);
#else // If you need to use multi-threaded, use the following way
                    m_bExitCaptureThread = false;
                    m_tCaptureThread = new Thread(new ThreadStart(CaptureThreadProc));
                    m_tCaptureThread.Start();
#endif
                    //MvApi.CameraReadSN and MvApi.CameraWriteSN used to read and write from the camera user-defined serial number or other data, 32 bytes
                    //MvApi.CameraSaveUserData and MvApi.CameraLoadUserData are used to read custom data from the camera, 512 bytes


                    //m_DlgSettings.m_hCamera = m_hCamera;
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
                if (m_hCamera > 0)
                {
                    if (_isStreaming)
                    {
                        nRet = StopStreaming();
                        if (nRet != MyCamera.MV_OK)
                        {
                            Trace.WriteLine("Stop Streaming Fail!");
                            return nRet;
                        }
                    }

                    if (_isGrabbing)
                    {
                        nRet = StopGrabbing();
                        if (nRet != MyCamera.MV_OK)
                        {
                            Trace.WriteLine("Stop Grabbing Fail!");
                            return nRet;
                        }
                    }

                    CameraSdkStatus status = MvApi.CameraUnInit(m_hCamera);
                    if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                    {
                        Console.WriteLine("Close device!");
                        _isConnected = false;
                    }

                    //MvApi.CameraUnInit(m_hCamera);
                    Marshal.FreeHGlobal(m_ImageBuffer);
                    Marshal.FreeHGlobal(m_ImageBufferSnapshot);
                    m_hCamera = 0;
                    Console.WriteLine("Destroy device!");
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

            if (m_hCamera > 0 && !_isGrabbing)
            {
                CameraSdkStatus status = MvApi.CameraPlay(m_hCamera);
                if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
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
            if (m_hCamera > 0 && _isGrabbing)
            {
                CameraSdkStatus status = MvApi.CameraPlay(m_hCamera);
                if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    _isGrabbing = true;
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
                    double pfExposureTime = 0.0;
                    MvApi.CameraGetExposureTime(m_hCamera, ref pfExposureTime);
                    return (int)pfExposureTime;
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
                    MvApi.CameraSetExposureTime(m_hCamera, (int)value);
                }
                if (key == KeyName.TriggerMode)
                {
                    MvApi.CameraSetTriggerMode(m_hCamera, 1);
                }
                if (key == KeyName.TriggerSource)
                {
                    MvApi.CameraSoftTrigger(m_hCamera);
                }
                if (key == KeyName.TriggerSoftware)
                {
                    MvApi.CameraSoftTriggerEx(m_hCamera, 1);
                }
            }
            return 1;
        }

        public void ClearImageBuffer()
        {
            if (_isConnected)
            {
                MvApi.CameraClearBuffer(m_hCamera);
            }
        }

        public Bitmap GrabFrame(int timeout = 1000)
        {
            try
            {
                lock (_lockObj)
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
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            return null;
        }

        private CameraHandle FindDevice(string userDefinedName)
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
                        if (Encoding.UTF8.GetString(devInfo.acFriendlyName).Contains(userDefinedName))
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

        private void CaptureThreadProc()
        {
            CameraSdkStatus eStatus;
            tSdkFrameHead FrameHead;
            IntPtr uRawBuffer; // rawbuffer is applied internally by the SDK. Application layer do not call delete like release function

            while (m_bExitCaptureThread == false)
            {
                // 500 milliseconds timeout, the image is not captured before the thread will be suspended, release the CPU, so no need to call this thread sleep
                eStatus = MvApi.CameraGetImageBuffer(m_hCamera, out FrameHead, out uRawBuffer, 500);

                if (eStatus == CameraSdkStatus.CAMERA_STATUS_SUCCESS) // If the trigger mode, it may timeout
                {
                    // Image processing, the original output is converted to RGB format bitmap data, while overlay white balance, saturation, LUT ISP processing.
                    MvApi.CameraImageProcess(m_hCamera, uRawBuffer, m_ImageBuffer, ref FrameHead);
                    // Overlay reticle, auto exposure window, white balance window information (only superimposed set to visible).
                    MvApi.CameraImageOverlay(m_hCamera, m_ImageBuffer, ref FrameHead);
                    // call the SDK encapsulated interface, showing the preview image
                    MvApi.CameraDisplayRGB24(m_hCamera, m_ImageBuffer, ref FrameHead);
                    // After successful call CameraGetImageBuffer must be released, the next time you can continue to call CameraGetImageBuffer to capture the image.
                    MvApi.CameraReleaseImageBuffer(m_hCamera, uRawBuffer);

                    if (FrameHead.iWidth != m_tFrameHead.iWidth || FrameHead.iHeight != m_tFrameHead.iHeight)
                    {
                        m_bEraseBk = true;
                        m_tFrameHead = FrameHead;
                    }

                    m_iDisplayedFrames++;

                    if (m_bSaveImage)
                    {
                        MvApi.CameraSaveImage(m_hCamera, "c:\\test.bmp", m_ImageBuffer, ref FrameHead, emSdkFileType.FILE_BMP, 100);
                        m_bSaveImage = false;
                    }
                }
            }
        }
    }
}
