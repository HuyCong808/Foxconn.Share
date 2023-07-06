using MvCamCtrl.NET;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace Foxconn.Editor.Camera
{
    public class Hikvision : ICamera
    {
        private readonly object _lockObj = new object();
        private MyCamera m_MyCamera = null;
        private MyCamera.MV_CC_DEVICE_INFO_LIST m_stDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
        private uint m_nBufSizeForDriver = 3072 * 2048 * 3;
        private byte[] m_pBufForDriver = new byte[3072 * 2048 * 3];
        private uint m_nBufSizeForSaveImage = 3072 * 2048 * 3 * 3 + 2048;
        private byte[] m_pBufForSaveImage = new byte[3072 * 2048 * 3 * 3 + 2048];
        private int m_nImageOnBuffer = 1;
        private string _userDefinedName = string.Empty;
        private bool _isConnected = false;
        private bool _isGrabbing = false;
        private bool _isStreaming = false;

        public bool IsConnected => _isConnected;

        public bool IsGrabbing => _isGrabbing;

        public bool IsStreaming => _isStreaming;

        public void DeviceListAcq()
        {
            try
            {
                // Create Device List
                GC.Collect();
                m_stDeviceList.nDeviceNum = 0;
                int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_stDeviceList);
                if (0 != nRet)
                {
                    Console.WriteLine("Enumerate devices fail!");
                    return;
                }

                // Display device name in the form list
                for (int i = 0; i < m_stDeviceList.nDeviceNum; i++)
                {
                    MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                    if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                    {
                        MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));

                        if (gigeInfo.chUserDefinedName != "")
                        {
                            Console.WriteLine("GEV: " + gigeInfo.chUserDefinedName + " (" + gigeInfo.chSerialNumber + ")");
                        }
                        else
                        {
                            Console.WriteLine("GEV: " + gigeInfo.chManufacturerName + " " + gigeInfo.chModelName + " (" + gigeInfo.chSerialNumber + ")");
                        }
                    }
                    else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                    {
                        MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                        if (usbInfo.chUserDefinedName != "")
                        {
                            Console.WriteLine("U3V: " + usbInfo.chUserDefinedName + " (" + usbInfo.chSerialNumber + ")");
                        }
                        else
                        {
                            Console.WriteLine("U3V: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")");
                        }
                    }
                }

                // Select the first item
                if (m_stDeviceList.nDeviceNum != 0)
                {

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
                    m_MyCamera = new MyCamera();
                    return CreateDevice(_userDefinedName, m_nImageOnBuffer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
                        if (nRet != MyCamera.MV_OK)
                        {
                            Console.WriteLine("Stop Streaming Fail!");
                            return nRet;
                        }
                    }

                    if (_isGrabbing)
                    {
                        nRet = StopGrabbing();
                        if (nRet != MyCamera.MV_OK)
                        {
                            Console.WriteLine("Stop Grabbing Fail!");
                            return nRet;
                        }
                    }

                    nRet = m_MyCamera.MV_CC_CloseDevice_NET();
                    if (nRet != MyCamera.MV_OK)
                    {
                        Console.WriteLine("Close Device Fail!");
                        return nRet;
                    }

                    nRet = m_MyCamera.MV_CC_DestroyDevice_NET();
                    if (nRet != MyCamera.MV_OK)
                    {
                        Console.WriteLine("Destroy Device Fail!");
                        return nRet;
                    }
                }
                _isConnected = false;
                return nRet;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
                nRet = m_MyCamera.MV_CC_StartGrabbing_NET();
                if (nRet == MyCamera.MV_OK)
                {
                    _isGrabbing = true;
                }
                else
                {
                    Console.WriteLine("Start Grabbing Fail!");
                }
            }
            return nRet;
        }

        public int StopGrabbing()
        {
            int nRet = -1;
            if (m_MyCamera != null && _isGrabbing)
            {
                nRet = m_MyCamera.MV_CC_StopGrabbing_NET();
                if (nRet == MyCamera.MV_OK)
                {
                    _isGrabbing = false;
                }
                else
                {
                    Console.WriteLine("Stop Grabbing Fail!");
                }
            }
            return nRet;
        }

        public int StartStreaming()
        {
            return MyCamera.MV_OK;
        }

        public int StopStreaming()
        {
            return MyCamera.MV_OK;
        }

        public int GetParameter(KeyName key, ref object value)
        {
            string keyString = GetStringKey(key);
            KeyType keyType = GetKeyType(keyString);
            return GetValue(keyString, ref value, keyType);
        }

        public int SetParameter(KeyName key, object value)
        {
            string keyString = GetStringKey(key);
            KeyType keyType = GetKeyType(keyString);
            return SetValue(keyString, value, keyType);
        }

        public void ClearImageBuffer()
        {
            if (m_MyCamera != null)
            {
                if (_isConnected)
                {
                    m_MyCamera.MV_CC_ClearImageBuffer_NET();
                }
            }
        }

        public Bitmap GrabFrame(int timeout = 1000)
        {
            try
            {
                lock (_lockObj)
                {
                    int nRet;
                    uint nPayloadSize = 0;
                    MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
                    nRet = m_MyCamera.MV_CC_GetIntValue_NET("PayloadSize", ref stParam);
                    if (nRet != MyCamera.MV_OK)
                    {
                        return null;
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

                    IntPtr pData = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForDriver, 0);
                    MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
                    // Get one frame timeout, timeout is 1 sec
                    nRet = m_MyCamera.MV_CC_GetOneFrameTimeout_NET(pData, m_nBufSizeForDriver, ref stFrameInfo, timeout);
                    if (nRet != MyCamera.MV_OK)
                    {
                        return null;
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
                        return null;
                    }

                    IntPtr pImage = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForSaveImage, 0);
                    MyCamera.MV_PIXEL_CONVERT_PARAM stConverPixelParam = new MyCamera.MV_PIXEL_CONVERT_PARAM
                    {
                        nWidth = stFrameInfo.nWidth,
                        nHeight = stFrameInfo.nHeight,
                        pSrcData = pData,
                        nSrcDataLen = stFrameInfo.nFrameLen,
                        enSrcPixelType = stFrameInfo.enPixelType,
                        enDstPixelType = enDstPixelType,
                        pDstBuffer = pImage,
                        nDstBufferSize = m_nBufSizeForSaveImage
                    };
                    nRet = m_MyCamera.MV_CC_ConvertPixelType_NET(ref stConverPixelParam);
                    if (nRet != MyCamera.MV_OK)
                    {
                        return null;
                    }

                    if (enDstPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
                    {
                        //************************Mono8 Bitmap*******************************
                        Bitmap bmp = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 1, PixelFormat.Format8bppIndexed, pImage);

                        ColorPalette cp = bmp.Palette;
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
                            Bitmap bmp = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 3, PixelFormat.Format24bppRgb, pImage);
                            return bmp;
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }

        private int CreateDevice(string userDefinedName, int m_nImageOnBuffer)
        {
            (bool bRet, MyCamera.MV_CC_DEVICE_INFO stDevInfo) = FindDevice(userDefinedName);
            if (!bRet)
            {
                Console.WriteLine("Device find fail!");
                return -1;
            }

            int nRet = m_MyCamera.MV_CC_CreateDevice_NET(ref stDevInfo);
            if (nRet != MyCamera.MV_OK)
            {
                Console.WriteLine("Device create fail!");
                return nRet;
            }

            bool bAsscessMode = MyCamera.MV_CC_IsDeviceAccessible_NET(ref stDevInfo, MyCamera.MV_ACCESS_Exclusive);
            if (bAsscessMode == false)
            {
                nRet = m_MyCamera.MV_CC_DestroyDevice_NET();
                return -1;
            }

            nRet = m_MyCamera.MV_CC_OpenDevice_NET(MyCamera.MV_ACCESS_Exclusive, 0);
            if (nRet != MyCamera.MV_OK)
            {
                nRet = m_MyCamera.MV_CC_DestroyDevice_NET();
                Console.WriteLine("Device open fail!");
                return nRet;
            }

            // Detection network optimal package size(It only works for the GigE camera).
            if (stDevInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                int nPacketSize = m_MyCamera.MV_CC_GetOptimalPacketSize_NET();
                if (nPacketSize > 0)
                {
                    nRet = m_MyCamera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                    if (nRet != MyCamera.MV_OK)
                    {
                        Console.WriteLine("Warning: Set Packet Size failed!");
                        return nRet;
                    }
                }
                else
                {
                    Console.WriteLine("Warning: Get Packet Size failed!");
                    return -1;
                }
            }

            m_MyCamera.MV_CC_SetImageNodeNum_NET((uint)m_nImageOnBuffer);
            _isConnected = true;
            return 1;
        }

        public (bool, MyCamera.MV_CC_DEVICE_INFO) FindDevice(string userDefinedName)
        {
            for (int i = 0; i < m_stDeviceList.nDeviceNum; i++)
            {
                // Get selected device information
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                    MyCamera.MV_GIGE_DEVICE_INFO stGigEDeviceInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    if (userDefinedName == stGigEDeviceInfo.chUserDefinedName)
                    {
                        return (true, device);
                    }
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stUsb3VInfo, 0);
                    MyCamera.MV_USB3_DEVICE_INFO stUsb3DeviceInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    if (userDefinedName == stUsb3DeviceInfo.chUserDefinedName)
                    {
                        return (true, device);
                    }
                }
            }
            return (true, new MyCamera.MV_CC_DEVICE_INFO());
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
                    return true;

                default:
                    return false;
            }
        }

        private string GetStringKey(KeyName key)
        {
            return Enum.GetName(typeof(KeyName), key).Trim();
        }

        private KeyType GetKeyType(string keyName)
        {
            string[] IEnumeration = new string[] {"DeviceType","DeviceScanType","DeviceConnectionStatus","DeviceLinkHeartbeatMode",
                "DeviceStreamChannelType","DeviceStreamChannelEndianness","DeviceCharacterSet","DeviceTemperatureSelector",
                "RegionSelector","RegionDestination","PixelFormat","PixelSize","ImageCompressionMode","TestPatternGeneratorSelector",
                "TestPattern","BinningSelector","BinningHorizontal","BinningVertical","DecimationHorizontal","DecimationVertical",
                "Deinterlacing","FrameSpecInfoSelector","AcquisitionMode","TriggerSelector","TriggerMode","TriggerSource",
                "TriggerActivation","SensorShutterMode","ExposureMode","ExposureAuto","GainShutPrior","LineSelector","LineMode",
                "LineSource","CounterSelector","CounterEventSource","CounterResetSource","GainAuto","BlackLevelAuto","BalanceWhiteAuto",
                "BalanceRatioSelector","GammaSelector","SharpnessAuto","HueAuto","SaturationAuto","DigitalNoiseReductionMode",
                "AutoFunctionAOISelector","LUTSelector","EncoderSelector","EncoderSourceA","EncoderSourceB","EncoderTriggerMode",
                "EncoderCounterMode","InputSource","SignalAlignment","ShadingSelector","UserSetSelector","UserSetDefault",
                "GevDeviceModeCharacterSet","GevSupportedOptionSelector","GevCCP"};

            string[] IBoolean = new string[] {"ReverseX","ReverseY","ReverseScanDirection","FrameSpecInfo","AcquisitionFrameRateEnable",
                "AcquisitionLineRateEnable","TriggerCacheEnable","FrameTimeoutEnable","HDREnable","LineInverter","LineTermination",
                "LineStatus","StrobeEnable","ADCGainEnable","DigitalShiftEnable","BlackLevelEnable","GammaEnable","SharpnessEnable",
                "HueEnable","SaturationEnable","AutoFunctionAOIUsageIntensity","AutoFunctionAOIUsageWhiteBalance","LUTEnable",
                "NUCEnable","FPNCEnable","PRNUCEnable","GevDeviceModeIsBigEndian","GevSupportedOption","GevCurrentIPConfigurationLLA",
                "GevCurrentIPConfigurationDHCP","GevCurrentIPConfigurationPersistentIP","GevPAUSEFrameReception","GevGVCPHeartbeatDisable",
                "GevSCPSFireTestPacket","GevSCPSDoNotFragment","GevSCPSBigEndian","PacketUnorderSupport"};

            string[] ICommand = new string[] {"DeviceReset","FindMe","AcquisitionStart","AcquisitionStop","TriggerSoftware","CounterReset",
                "EncoderCounterReset","EncoderReverseCounterReset","ActivateShading","UserSetLoad","UserSetSave","GevTimestampControlLatch",
                "GevTimestampControlReset","GevTimestampControlLatchReset"};

            string[] IFloat = new string[] {"DeviceTemperature","AcquisitionFrameRate","ResultingFrameRate","TriggerDelay","ExposureTime",
                "HDRGain","Gain","AutoGainLowerLimit","AutoGainUpperLimit","DigitalShift","Gamma"};

            string[] IString = new string[] {
                "DeviceVendorName","DeviceModelName","DeviceManufacturerInfo","DeviceVersion","DeviceFirmwareVersion","DeviceSerialNumber","DeviceID",
                "DeviceUserID"
                };
            string[] IInteger = new string[] {"DeviceUptime","BoardDeviceType","DeviceConnectionSelector","DeviceConnectionSpeed",
                "DeviceLinkSelector","DeviceLinkSpeed","DeviceLinkConnectionCount","DeviceLinkHeartbeatTimeout","DeviceStreamChannelCount",
                "DeviceStreamChannelSelector","DeviceStreamChannelLink","DeviceStreamChannelPacketSize","DeviceEventChannelCount",
                "DeviceMaxThroughput","WidthMax","HeightMax","Width","Height","OffsetX","OffsetY","ImageCompressionQuality",
                "AcquisitionBurstFrameCount","AcquisitionLineRate","ResultingLineRate","AutoExposureTimeLowerLimit",
                "AutoExposureTimeUpperLimit","FrameTimeoutTime","HDRSelector","HDRShuter","LineStatusAll","LineDebouncerTime",
                "StrobeLineDuration","StrobeLineDelay","StrobeLinePreDelay","CounterValue","CounterCurrentValue","Brightness",
                "BlackLevel","BalanceRatio","Sharpness","Hue","Saturation","NoiseReduction","AirspaceNoiseReduction",
                "TemporalNoiseReduction","AutoFunctionAOIWidth","AutoFunctionAOIHeight","AutoFunctionAOIOffsetX","AutoFunctionAOIOffsetY",
                "LUTIndex","LUTValue","EncoderCounter","EncoderCounterMax","EncoderMaxReverseCounter","PreDivider","Multiplier",
                "PostDivider","UserSetCurrent","PayloadSize","GevVersionMajor","GevVersionMinor","GevInterfaceSelector","GevMACAddress",
                "GevCurrentIPAddress","GevCurrentSubnetMask","GevCurrentDefaultGateway","GevNumberOfInterfaces","GevPersistentIPAddress",
                "GevPersistentSubnetMask","GevPersistentDefaultGateway","GevLinkSpeed","GevMessageChannelCount","GevStreamChannelCount",
                "GevHeartbeatTimeout","GevTimestampTickFrequency","GevTimestampValue","GevStreamChannelSelector","GevSCPInterfaceIndex",
                "GevSCPHostPort","GevSCPDirection","GevSCPSPacketSize","GevSCPD","GevSCDA","GevSCSP","TLParamsLocked"};

            if (IEnumeration.Contains(keyName))
            {
                return KeyType.Enumeration;
            }
            else if (IBoolean.Contains(keyName))
            {
                return KeyType.Boolean;
            }
            else if (ICommand.Contains(keyName))
            {
                return KeyType.Command;
            }
            else if (IFloat.Contains(keyName))
            {
                return KeyType.Float;
            }
            else if (IString.Contains(keyName))
            {
                return KeyType.String;
            }
            else if (IInteger.Contains(keyName))
            {
                return KeyType.Integer;
            }
            else
            {
                return KeyType.None;
            }
        }

        private int GetValue<H>(string key, ref H value, KeyType dataType)
        {
            int nRet = MyCamera.MV_OK;
            try
            {
                switch (dataType)
                {
                    case KeyType.Boolean:
                        {
                            bool t = true;
                            nRet = m_MyCamera.MV_CC_GetBoolValue_NET(key, ref t);
                            value = (H)Convert.ChangeType(t, typeof(H));
                            break;
                        }
                    case KeyType.Integer:
                        {
                            MyCamera.MVCC_INTVALUE_EX t = new MyCamera.MVCC_INTVALUE_EX();
                            nRet = m_MyCamera.MV_CC_GetIntValueEx_NET(key, ref t);
                            value = (H)Convert.ChangeType(t.nCurValue, typeof(H));
                            break;
                        }
                    case KeyType.Enumeration:
                        {
                            MyCamera.MVCC_ENUMVALUE t = new MyCamera.MVCC_ENUMVALUE();
                            nRet = m_MyCamera.MV_CC_GetEnumValue_NET(key, ref t);
                            value = (H)Convert.ChangeType(t.nCurValue, typeof(H));
                            break;
                        }
                    case KeyType.Float:
                        {
                            MyCamera.MVCC_FLOATVALUE t = new MyCamera.MVCC_FLOATVALUE();
                            nRet = m_MyCamera.MV_CC_GetFloatValue_NET(key, ref t);
                            value = (H)Convert.ChangeType(t.fCurValue, typeof(H));
                            break;
                        }
                    case KeyType.String:
                        {
                            MyCamera.MVCC_STRINGVALUE t = new MyCamera.MVCC_STRINGVALUE();
                            nRet = m_MyCamera.MV_CC_GetStringValue_NET(key, ref t);
                            value = (H)Convert.ChangeType(t.chCurValue, typeof(H));
                            break;
                        }
                }
                return nRet;
            }
            catch
            {
                return -1000;
            }

        }

        private int SetValue(string key, object value, KeyType dataType)
        {
            int nRet = MyCamera.MV_OK;
            try
            {
                switch (dataType)
                {
                    case KeyType.Boolean:
                        {
                            bool v = (Convert.ToInt16(value) == 1) ? true : false;
                            nRet = m_MyCamera.MV_CC_SetBoolValue_NET(key, v);
                            break;
                        }
                    case KeyType.Integer:
                        {
                            nRet = m_MyCamera.MV_CC_SetIntValueEx_NET(key, Convert.ToInt64(value));
                            break;
                        }
                    case KeyType.Enumeration:
                        {
                            nRet = m_MyCamera.MV_CC_SetEnumValue_NET(key, Convert.ToUInt32(value));
                            break;
                        }
                    case KeyType.Float:
                        {
                            float v = (float)Convert.ToDecimal(value);
                            nRet = m_MyCamera.MV_CC_SetFloatValue_NET(key, v);
                            break;
                        }
                    case KeyType.Command:
                        {
                            nRet = m_MyCamera.MV_CC_SetCommandValue_NET(key);
                            break;
                        }
                    case KeyType.String:
                        {
                            nRet = m_MyCamera.MV_CC_SetStringValue_NET(key, Convert.ToString(value));
                            break;
                        }
                }
                return nRet;
            }
            catch
            {
                return -56;
            }
        }
    }
}
