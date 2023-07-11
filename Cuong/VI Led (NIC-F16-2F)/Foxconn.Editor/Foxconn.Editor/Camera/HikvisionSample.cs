//using MvCamCtrl.NET;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.Linq;
//using System.Runtime.InteropServices;

//namespace Foxconn.Editor.Camera
//{
//    public class Hikvision_ : ICamera
//    {
//        private static object BufForDriverLock = new object();
//        private CCamera m_MyCamera = null;
//        private List<CCameraInfo> m_ltDeviceList = new List<CCameraInfo>();
//        private CImage m_pcImgForDriver;
//        private CFrameSpecInfo m_pcImgSpecInfo;
//        private Bitmap m_pcBitmap = null;
//        private PixelFormat m_enBitmapPixelFormat = PixelFormat.DontCare;
//        private string _userDefinedName = string.Empty;
//        private bool _isConnected = false;
//        private bool _isGrabbing = false;
//        private bool _isStreaming = false;

//        public bool IsConnected => _isConnected;

//        public bool IsGrabbing => _isGrabbing;

//        public bool IsStreaming => _isStreaming;

//        public void DeviceListAcq()
//        {
//            // Create Device List
//            GC.Collect();
//            m_ltDeviceList.Clear();
//            int nRet = CSystem.EnumDevices(CSystem.MV_GIGE_DEVICE | CSystem.MV_USB_DEVICE, ref m_ltDeviceList);
//            if (0 != nRet)
//            {
//                Trace.WriteLine("Enumerate devices fail!");
//                return;
//            }

//            // Display device name in the form list
//            for (int i = 0; i < m_ltDeviceList.Count; i++)
//            {
//                if (m_ltDeviceList[i].nTLayerType == CSystem.MV_GIGE_DEVICE)
//                {
//                    CGigECameraInfo gigeInfo = (CGigECameraInfo)m_ltDeviceList[i];
//                    if (gigeInfo.UserDefinedName != "")
//                    {
//                        Trace.WriteLine("GEV: " + gigeInfo.UserDefinedName + " (" + gigeInfo.chSerialNumber + ")");
//                    }
//                    else
//                    {
//                        Trace.WriteLine("GEV: " + gigeInfo.chManufacturerName + " " + gigeInfo.chModelName + " (" + gigeInfo.chSerialNumber + ")");
//                    }
//                }
//                else if (m_ltDeviceList[i].nTLayerType == CSystem.MV_USB_DEVICE)
//                {
//                    CUSBCameraInfo usbInfo = (CUSBCameraInfo)m_ltDeviceList[i];
//                    if (usbInfo.UserDefinedName != "")
//                    {
//                        Trace.WriteLine("U3V: " + usbInfo.UserDefinedName + " (" + usbInfo.chSerialNumber + ")");
//                    }
//                    else
//                    {
//                        Trace.WriteLine("U3V: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")");
//                    }
//                }
//            }

//            // Select the first item
//            if (m_ltDeviceList.Count != 0)
//            {

//            }
//        }

//        public int Open(string userDefinedName = "")
//        {
//            try
//            {
//                if (userDefinedName != "")
//                    _userDefinedName = userDefinedName;

//                if (_isConnected)
//                    return 1;

//                // Open device
//                if (null == m_MyCamera)
//                {
//                    m_MyCamera = new CCamera();
//                    return CreateDevice(_userDefinedName);
//                }
//            }
//            catch (Exception ex)
//            {
//                Trace.WriteLine($"Hikvision.Open: Exception = {ex}");
//            }
//            return -1;
//        }

//        public int Close()
//        {
//            try
//            {
//                int nRet = 0;
//                if (m_MyCamera != null)
//                {
//                    if (_isStreaming)
//                    {
//                        nRet = StopStreaming();
//                        if (CErrorDefine.MV_OK != nRet)
//                        {
//                            Trace.WriteLine("Stop Streaming Fail!");
//                            return nRet;
//                        }
//                    }

//                    if (_isGrabbing)
//                    {
//                        nRet = m_MyCamera.StopGrabbing();
//                        if (CErrorDefine.MV_OK != nRet)
//                        {
//                            Trace.WriteLine("Stop Grabbing Fail!");
//                            return nRet;
//                        }
//                    }

//                    nRet = m_MyCamera.CloseDevice();
//                    if (CErrorDefine.MV_OK != nRet)
//                    {
//                        Trace.WriteLine("Close Device Fail!");
//                        return nRet;
//                    }

//                    nRet = m_MyCamera.DestroyHandle();
//                    if (CErrorDefine.MV_OK != nRet)
//                    {
//                        Trace.WriteLine("Destroy Handle Fail!");
//                        return nRet;
//                    }
//                }
//                _isConnected = false;
//                return nRet;
//            }
//            catch (Exception ex)
//            {
//                Trace.WriteLine(ex);
//                return -1;
//            }
//        }

//        public int StartGrabbing()
//        {
//            int nRet = -1;
//            if (_isGrabbing)
//            {
//                return 0;
//            }
//            if (m_MyCamera != null && !_isGrabbing)
//            {
//                nRet = m_MyCamera.StartGrabbing();
//                if (nRet != CErrorDefine.MV_OK)
//                {
//                    Trace.WriteLine("Start Grabbing Fail!");
//                }
//                else
//                {
//                    _isGrabbing = true;
//                }
//            }
//            return nRet;
//        }

//        public int StopGrabbing()
//        {
//            int nRet = -1;
//            if (m_MyCamera != null && _isGrabbing)
//            {
//                nRet = m_MyCamera.StopGrabbing();
//                if (nRet != CErrorDefine.MV_OK)
//                {
//                    Trace.WriteLine("Stop Grabbing Fail!");
//                }
//                else
//                {
//                    _isGrabbing = false;
//                }
//            }
//            return nRet;
//        }

//        public int StartStreaming()
//        {
//            return -1;
//        }

//        public int StopStreaming()
//        {
//            return -1;
//        }

//        public int GetParameter(KeyName key, ref object value)
//        {
//            string keyString = GetStringKey(key);
//            KeyType keyType = GetKeyType(keyString);
//            return GetValue(keyString, ref value, keyType);
//        }

//        public int SetParameter(KeyName key, object value)
//        {
//            string keyString = GetStringKey(key);
//            KeyType keyType = GetKeyType(keyString);
//            return SetValue(keyString, value, keyType);
//        }

//        public void ClearImageBuffer()
//        {
//            if (m_MyCamera != null)
//            {
//                if (_isConnected)
//                    m_MyCamera.ClearImageBuffer();
//            }
//        }

//        public Bitmap GrabFrame(int timeout = 1000)
//        {
//            try
//            {
//                CFrameout pcFrameInfo = new CFrameout();
//                CDisplayFrameInfo pcDisplayInfo = new CDisplayFrameInfo();
//                CPixelConvertParam pcConvertParam = new CPixelConvertParam();
//                int nRet = CErrorDefine.MV_OK;
//                nRet = m_MyCamera.GetImageBuffer(ref pcFrameInfo, 1000);
//                if (nRet == CErrorDefine.MV_OK)
//                {
//                    lock (BufForDriverLock)
//                    {
//                        m_pcImgForDriver = pcFrameInfo.Image.Clone() as CImage;
//                        m_pcImgSpecInfo = pcFrameInfo.FrameSpec;

//                        pcConvertParam.InImage = pcFrameInfo.Image;
//                        if (PixelFormat.Format8bppIndexed == m_pcBitmap.PixelFormat)
//                        {
//                            pcConvertParam.OutImage.PixelType = MvGvspPixelType.PixelType_Gvsp_Mono8;
//                            m_MyCamera.ConvertPixelType(ref pcConvertParam);
//                        }
//                        else
//                        {
//                            pcConvertParam.OutImage.PixelType = MvGvspPixelType.PixelType_Gvsp_BGR8_Packed;
//                            m_MyCamera.ConvertPixelType(ref pcConvertParam);
//                        }

//                        // Save Bitmap Data
//                        BitmapData m_pcBitmapData = m_pcBitmap.LockBits(new Rectangle(0, 0, pcConvertParam.InImage.Width, pcConvertParam.InImage.Height), ImageLockMode.ReadWrite, m_pcBitmap.PixelFormat);
//                        Marshal.Copy(pcConvertParam.OutImage.ImageData, 0, m_pcBitmapData.Scan0, (int)pcConvertParam.OutImage.ImageData.Length);
//                        m_pcBitmap.UnlockBits(m_pcBitmapData);
//                    }
//                    m_MyCamera.FreeImageBuffer(ref pcFrameInfo);
//                    return (Bitmap)m_pcBitmap.Clone();
//                }
//            }
//            catch (Exception ex)
//            {
//                Trace.WriteLine(ex);
//            }
//            return null;
//        }

//        private int CreateDevice(string userDefinedName)
//        {
//            // Get selected device information
//            (bool bRet, CCameraInfo device) = FindDevice(userDefinedName);
//            if (!bRet)
//            {
//                Trace.WriteLine("Device find fail!");
//                return -1;
//            }

//            int nRet = m_MyCamera.CreateHandle(ref device);
//            if (CErrorDefine.MV_OK != nRet)
//            {
//                return -1;
//            }

//            // Open device
//            nRet = m_MyCamera.OpenDevice();
//            if (CErrorDefine.MV_OK != nRet)
//            {
//                m_MyCamera.DestroyHandle();
//                Trace.WriteLine("Device open fail!");
//                return -1;
//            }

//            // Detection network optimal package size(It only works for the GigE camera)
//            if (device.nTLayerType == CSystem.MV_GIGE_DEVICE)
//            {
//                int nPacketSize = m_MyCamera.GIGE_GetOptimalPacketSize();
//                if (nPacketSize > 0)
//                {
//                    nRet = m_MyCamera.SetIntValue("GevSCPSPacketSize", (uint)nPacketSize);
//                    if (CErrorDefine.MV_OK != nRet)
//                    {
//                        Trace.WriteLine("Set Packet Size failed!");
//                    }
//                }
//                else
//                {
//                    Trace.WriteLine("Get Packet Size failed!");
//                }

//                nRet = NecessaryOperBeforeGrab();
//                if (CErrorDefine.MV_OK != nRet)
//                {
//                    return -1;
//                }
//                _isConnected = true;
//                return 1;
//            }
//            else
//            {
//                return -1;
//            }
//        }

//        public (bool, CCameraInfo) FindDevice(string userDefinedName)
//        {
//            for (int i = 0; i < m_ltDeviceList.Count; i++)
//            {
//                if (m_ltDeviceList[i].nTLayerType == CSystem.MV_GIGE_DEVICE)
//                {
//                    CGigECameraInfo gigeInfo = (CGigECameraInfo)m_ltDeviceList[i];
//                    if (gigeInfo.UserDefinedName == userDefinedName)
//                    {
//                        return (true, m_ltDeviceList[i]);
//                    }
//                }
//                else if (m_ltDeviceList[i].nTLayerType == CSystem.MV_USB_DEVICE)
//                {
//                    CUSBCameraInfo usbInfo = (CUSBCameraInfo)m_ltDeviceList[i];
//                    if (usbInfo.UserDefinedName == userDefinedName)
//                    {
//                        return (true, m_ltDeviceList[i]);
//                    }
//                }
//            }
//            return (false, new CCameraInfo());
//        }

//        private bool IsMonoData(MvGvspPixelType enGvspPixelType)
//        {
//            switch (enGvspPixelType)
//            {
//                case MvGvspPixelType.PixelType_Gvsp_Mono8:
//                case MvGvspPixelType.PixelType_Gvsp_Mono10:
//                case MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
//                case MvGvspPixelType.PixelType_Gvsp_Mono12:
//                case MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
//                    return true;

//                default:
//                    return false;
//            }
//        }

//        private bool IsColorData(MvGvspPixelType enGvspPixelType)
//        {
//            switch (enGvspPixelType)
//            {
//                case MvGvspPixelType.PixelType_Gvsp_BayerGR8:
//                case MvGvspPixelType.PixelType_Gvsp_BayerRG8:
//                case MvGvspPixelType.PixelType_Gvsp_BayerGB8:
//                case MvGvspPixelType.PixelType_Gvsp_BayerBG8:
//                case MvGvspPixelType.PixelType_Gvsp_BayerGR10:
//                case MvGvspPixelType.PixelType_Gvsp_BayerRG10:
//                case MvGvspPixelType.PixelType_Gvsp_BayerGB10:
//                case MvGvspPixelType.PixelType_Gvsp_BayerBG10:
//                case MvGvspPixelType.PixelType_Gvsp_BayerGR12:
//                case MvGvspPixelType.PixelType_Gvsp_BayerRG12:
//                case MvGvspPixelType.PixelType_Gvsp_BayerGB12:
//                case MvGvspPixelType.PixelType_Gvsp_BayerBG12:
//                case MvGvspPixelType.PixelType_Gvsp_BayerGR10_Packed:
//                case MvGvspPixelType.PixelType_Gvsp_BayerRG10_Packed:
//                case MvGvspPixelType.PixelType_Gvsp_BayerGB10_Packed:
//                case MvGvspPixelType.PixelType_Gvsp_BayerBG10_Packed:
//                case MvGvspPixelType.PixelType_Gvsp_BayerGR12_Packed:
//                case MvGvspPixelType.PixelType_Gvsp_BayerRG12_Packed:
//                case MvGvspPixelType.PixelType_Gvsp_BayerGB12_Packed:
//                case MvGvspPixelType.PixelType_Gvsp_BayerBG12_Packed:
//                case MvGvspPixelType.PixelType_Gvsp_RGB8_Packed:
//                case MvGvspPixelType.PixelType_Gvsp_YUV422_Packed:
//                case MvGvspPixelType.PixelType_Gvsp_YUV422_YUYV_Packed:
//                    return true;

//                default:
//                    return false;
//            }
//        }

//        private bool IsMono(MvGvspPixelType enPixelType)
//        {
//            switch (enPixelType)
//            {
//                case MvGvspPixelType.PixelType_Gvsp_Mono1p:
//                case MvGvspPixelType.PixelType_Gvsp_Mono2p:
//                case MvGvspPixelType.PixelType_Gvsp_Mono4p:
//                case MvGvspPixelType.PixelType_Gvsp_Mono8:
//                case MvGvspPixelType.PixelType_Gvsp_Mono8_Signed:
//                case MvGvspPixelType.PixelType_Gvsp_Mono10:
//                case MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
//                case MvGvspPixelType.PixelType_Gvsp_Mono12:
//                case MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
//                case MvGvspPixelType.PixelType_Gvsp_Mono14:
//                case MvGvspPixelType.PixelType_Gvsp_Mono16:
//                    return true;
//                default:
//                    return false;
//            }
//        }

//        private int NecessaryOperBeforeGrab()
//        {
//            // Set Image Node Num
//            m_MyCamera.SetImageNodeNum(1);

//            // Get Image Width
//            CIntValue pcWidth = new CIntValue();
//            int nRet = m_MyCamera.GetIntValue("Width", ref pcWidth);
//            if (CErrorDefine.MV_OK != nRet)
//            {
//                Trace.WriteLine("Get Width Info Fail!");
//                return nRet;
//            }

//            // Get Image Height
//            CIntValue pcHeight = new CIntValue();
//            nRet = m_MyCamera.GetIntValue("Height", ref pcHeight);
//            if (CErrorDefine.MV_OK != nRet)
//            {
//                Trace.WriteLine("Get Height Info Fail!");
//                return nRet;
//            }

//            // Get Pixel Format
//            CEnumValue pcPixelFormat = new CEnumValue();
//            nRet = m_MyCamera.GetEnumValue("PixelFormat", ref pcPixelFormat);
//            if (CErrorDefine.MV_OK != nRet)
//            {
//                Trace.WriteLine("Get Pixel Format Fail!");
//                return nRet;
//            }

//            // Bitmap
//            if ((int)MvGvspPixelType.PixelType_Gvsp_Undefined == (int)pcPixelFormat.CurValue)
//            {
//                Trace.WriteLine("Unknown Pixel Format!");
//                return CErrorDefine.MV_E_UNKNOW;
//            }
//            else if (IsMono((MvGvspPixelType)pcPixelFormat.CurValue))
//            {
//                m_enBitmapPixelFormat = PixelFormat.Format8bppIndexed;
//            }
//            else
//            {
//                m_enBitmapPixelFormat = PixelFormat.Format24bppRgb;
//            }

//            if (null != m_pcBitmap)
//            {
//                m_pcBitmap.Dispose();
//                m_pcBitmap = null;
//            }
//            m_pcBitmap = new Bitmap((int)pcWidth.CurValue, (int)pcHeight.CurValue, m_enBitmapPixelFormat);

//            // Set Standard Palette in Mono8 Format
//            if (PixelFormat.Format8bppIndexed == m_enBitmapPixelFormat)
//            {
//                ColorPalette palette = m_pcBitmap.Palette;
//                for (int i = 0; i < palette.Entries.Length; i++)
//                {
//                    palette.Entries[i] = Color.FromArgb(i, i, i);
//                }
//                m_pcBitmap.Palette = palette;
//            }

//            return CErrorDefine.MV_OK;
//        }

//        private string GetStringKey(KeyName key)
//        {
//            return Enum.GetName(typeof(KeyName), key).Trim();
//        }

//        private KeyType GetKeyType(string keyName)
//        {
//            string[] IEnumeration = new string[] {"DeviceType","DeviceScanType","DeviceConnectionStatus","DeviceLinkHeartbeatMode",
//                "DeviceStreamChannelType","DeviceStreamChannelEndianness","DeviceCharacterSet","DeviceTemperatureSelector",
//                "RegionSelector","RegionDestination","PixelFormat","PixelSize","ImageCompressionMode","TestPatternGeneratorSelector",
//                "TestPattern","BinningSelector","BinningHorizontal","BinningVertical","DecimationHorizontal","DecimationVertical",
//                "Deinterlacing","FrameSpecInfoSelector","AcquisitionMode","TriggerSelector","TriggerMode","TriggerSource",
//                "TriggerActivation","SensorShutterMode","ExposureMode","ExposureAuto","GainShutPrior","LineSelector","LineMode",
//                "LineSource","CounterSelector","CounterEventSource","CounterResetSource","GainAuto","BlackLevelAuto","BalanceWhiteAuto",
//                "BalanceRatioSelector","GammaSelector","SharpnessAuto","HueAuto","SaturationAuto","DigitalNoiseReductionMode",
//                "AutoFunctionAOISelector","LUTSelector","EncoderSelector","EncoderSourceA","EncoderSourceB","EncoderTriggerMode",
//                "EncoderCounterMode","InputSource","SignalAlignment","ShadingSelector","UserSetSelector","UserSetDefault",
//                "GevDeviceModeCharacterSet","GevSupportedOptionSelector","GevCCP"};

//            string[] IBoolean = new string[] {"ReverseX","ReverseY","ReverseScanDirection","FrameSpecInfo","AcquisitionFrameRateEnable",
//                "AcquisitionLineRateEnable","TriggerCacheEnable","FrameTimeoutEnable","HDREnable","LineInverter","LineTermination",
//                "LineStatus","StrobeEnable","ADCGainEnable","DigitalShiftEnable","BlackLevelEnable","GammaEnable","SharpnessEnable",
//                "HueEnable","SaturationEnable","AutoFunctionAOIUsageIntensity","AutoFunctionAOIUsageWhiteBalance","LUTEnable",
//                "NUCEnable","FPNCEnable","PRNUCEnable","GevDeviceModeIsBigEndian","GevSupportedOption","GevCurrentIPConfigurationLLA",
//                "GevCurrentIPConfigurationDHCP","GevCurrentIPConfigurationPersistentIP","GevPAUSEFrameReception","GevGVCPHeartbeatDisable",
//                "GevSCPSFireTestPacket","GevSCPSDoNotFragment","GevSCPSBigEndian","PacketUnorderSupport"};

//            string[] ICommand = new string[] {"DeviceReset","FindMe","AcquisitionStart","AcquisitionStop","TriggerSoftware","CounterReset",
//                "EncoderCounterReset","EncoderReverseCounterReset","ActivateShading","UserSetLoad","UserSetSave","GevTimestampControlLatch",
//                "GevTimestampControlReset","GevTimestampControlLatchReset"};

//            string[] IFloat = new string[] {"DeviceTemperature","AcquisitionFrameRate","ResultingFrameRate","TriggerDelay","ExposureTime",
//                "HDRGain","Gain","AutoGainLowerLimit","AutoGainUpperLimit","DigitalShift","Gamma"};

//            string[] IString = new string[] {
//                "DeviceVendorName","DeviceModelName","DeviceManufacturerInfo","DeviceVersion","DeviceFirmwareVersion","DeviceSerialNumber","DeviceID",
//                "DeviceUserID"
//                };
//            string[] IInteger = new string[] {"DeviceUptime","BoardDeviceType","DeviceConnectionSelector","DeviceConnectionSpeed",
//                "DeviceLinkSelector","DeviceLinkSpeed","DeviceLinkConnectionCount","DeviceLinkHeartbeatTimeout","DeviceStreamChannelCount",
//                "DeviceStreamChannelSelector","DeviceStreamChannelLink","DeviceStreamChannelPacketSize","DeviceEventChannelCount",
//                "DeviceMaxThroughput","WidthMax","HeightMax","Width","Height","OffsetX","OffsetY","ImageCompressionQuality",
//                "AcquisitionBurstFrameCount","AcquisitionLineRate","ResultingLineRate","AutoExposureTimeLowerLimit",
//                "AutoExposureTimeUpperLimit","FrameTimeoutTime","HDRSelector","HDRShuter","LineStatusAll","LineDebouncerTime",
//                "StrobeLineDuration","StrobeLineDelay","StrobeLinePreDelay","CounterValue","CounterCurrentValue","Brightness",
//                "BlackLevel","BalanceRatio","Sharpness","Hue","Saturation","NoiseReduction","AirspaceNoiseReduction",
//                "TemporalNoiseReduction","AutoFunctionAOIWidth","AutoFunctionAOIHeight","AutoFunctionAOIOffsetX","AutoFunctionAOIOffsetY",
//                "LUTIndex","LUTValue","EncoderCounter","EncoderCounterMax","EncoderMaxReverseCounter","PreDivider","Multiplier",
//                "PostDivider","UserSetCurrent","PayloadSize","GevVersionMajor","GevVersionMinor","GevInterfaceSelector","GevMACAddress",
//                "GevCurrentIPAddress","GevCurrentSubnetMask","GevCurrentDefaultGateway","GevNumberOfInterfaces","GevPersistentIPAddress",
//                "GevPersistentSubnetMask","GevPersistentDefaultGateway","GevLinkSpeed","GevMessageChannelCount","GevStreamChannelCount",
//                "GevHeartbeatTimeout","GevTimestampTickFrequency","GevTimestampValue","GevStreamChannelSelector","GevSCPInterfaceIndex",
//                "GevSCPHostPort","GevSCPDirection","GevSCPSPacketSize","GevSCPD","GevSCDA","GevSCSP","TLParamsLocked"};

//            if (IEnumeration.Contains(keyName))
//            {
//                return KeyType.Enumeration;
//            }
//            else if (IBoolean.Contains(keyName))
//            {
//                return KeyType.Boolean;
//            }
//            else if (ICommand.Contains(keyName))
//            {
//                return KeyType.Command;
//            }
//            else if (IFloat.Contains(keyName))
//            {
//                return KeyType.Float;
//            }
//            else if (IString.Contains(keyName))
//            {
//                return KeyType.String;
//            }
//            else if (IInteger.Contains(keyName))
//            {
//                return KeyType.Integer;
//            }
//            else
//            {
//                return KeyType.None;
//            }
//        }

//        private int GetValue<H>(string key, ref H value, KeyType dataType)
//        {
//            int nRet = CErrorDefine.MV_OK;
//            try
//            {
//                switch (dataType)
//                {
//                    case KeyType.Boolean:
//                        {
//                            bool t = true;
//                            nRet = m_MyCamera.GetBoolValue(key, ref t);
//                            value = (H)Convert.ChangeType(t, typeof(H));
//                            break;
//                        }
//                    case KeyType.Integer:
//                        {
//                            CIntValue t = new CIntValue();
//                            nRet = m_MyCamera.GetIntValue(key, ref t);
//                            value = (H)Convert.ChangeType(t.CurValue, typeof(H));
//                            break;
//                        }
//                    case KeyType.Enumeration:
//                        {
//                            CEnumValue t = new CEnumValue();
//                            nRet = m_MyCamera.GetEnumValue(key, ref t);
//                            value = (H)Convert.ChangeType(t.CurValue, typeof(H));
//                            break;
//                        }
//                    case KeyType.Float:
//                        {
//                            CFloatValue t = new CFloatValue();
//                            nRet = m_MyCamera.GetFloatValue(key, ref t);
//                            value = (H)Convert.ChangeType(t.CurValue, typeof(H));
//                            break;
//                        }
//                    case KeyType.String:
//                        {
//                            CStringValue t = new CStringValue();
//                            nRet = m_MyCamera.GetStringValue(key, ref t);
//                            value = (H)Convert.ChangeType(t.CurValue, typeof(H));
//                            break;
//                        }
//                }
//                return nRet;
//            }
//            catch
//            {
//                return -1000;
//            }

//        }

//        private int SetValue(string key, object value, KeyType dataType)
//        {
//            int nRet = CErrorDefine.MV_OK;
//            try
//            {
//                switch (dataType)
//                {
//                    case KeyType.Boolean:
//                        {
//                            bool v = (Convert.ToInt16(value) == 1) ? true : false;
//                            nRet = m_MyCamera.SetBoolValue(key, v);
//                            break;
//                        }
//                    case KeyType.Integer:
//                        {
//                            nRet = m_MyCamera.SetIntValue(key, Convert.ToInt64(value));
//                            break;
//                        }
//                    case KeyType.Enumeration:
//                        {
//                            nRet = m_MyCamera.SetEnumValue(key, Convert.ToUInt32(value));
//                            break;
//                        }
//                    case KeyType.Float:
//                        {
//                            float v = (float)Convert.ToDecimal(value);
//                            nRet = m_MyCamera.SetFloatValue(key, v);
//                            break;
//                        }
//                    case KeyType.Command:
//                        {
//                            nRet = m_MyCamera.SetCommandValue(key);
//                            break;
//                        }
//                    case KeyType.String:
//                        {
//                            nRet = m_MyCamera.SetStringValue(key, Convert.ToString(value));
//                            break;
//                        }
//                }
//                return nRet;
//            }
//            catch
//            {
//                return -56;
//            }
//        }
//    }
//}
