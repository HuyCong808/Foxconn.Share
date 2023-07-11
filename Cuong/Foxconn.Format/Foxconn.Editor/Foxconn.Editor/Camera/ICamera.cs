using Foxconn.Editor.Enums;

namespace Foxconn.Editor.Camera
{
    public interface ICamera
    {
        public void DeviceListAcq();
        public bool IsConnected { get; }
        public bool IsGrabbing { get; }
        public bool IsStreaming { get; }
        public int Open(string userDefinedName = "");
        public int Close();
        public int StartGrabbing();
        public int StopGrabbing();
        public int StartStreaming();
        public int StopStreaming();
        public int GetParameter(KeyName key, ref object value);
        public int SetParameter(KeyName key, object value);
        public void ClearImageBuffer();
        public System.Drawing.Bitmap GrabFrame(int timeout = 1000);
    }

    public abstract class ICameraFactory
    {
        public abstract ICamera CreateCamera();
    }

    //public class BaslerFactory : ICameraFactory
    //{
    //    public override ICamera CreateCamera()
    //    {
    //        return new Basler();
    //    }
    //}

    //public class BaumerFactory : ICameraFactory
    //{
    //    public override ICamera CreateCamera()
    //    {
    //        return new Baumer();
    //    }
    //}

    public class HikvisionFactory : ICameraFactory
    {
        public override ICamera CreateCamera()
        {
            return new Hikvision();
        }
    }

    //public class MindvisionFactory : ICameraFactory
    //{
    //    public override ICamera CreateCamera()
    //    {
    //        return new MindVision();
    //    }
    //}

    public class WebcamFactory : ICameraFactory
    {
        public override ICamera CreateCamera()
        {
            return new Webcam();
        }
    }

    public class CameraFactory
    {
        public static ICamera GetCamera(CameraType type)
        {
            ICamera factory = null;
            switch (type)
            {
                case CameraType.Unknow:
                    break;
                //case CameraType.Basler:
                //    BaslerFactory baslerFactory = new BaslerFactory();
                //    factory = baslerFactory.CreateCamera();
                //    break;
                //case CameraType.Baumer:
                //    BaumerFactory baumerFactory = new BaumerFactory();
                //    factory = baumerFactory.CreateCamera();
                //    break;
                case CameraType.Hikvision:
                    HikvisionFactory hikvisionFactory = new HikvisionFactory();
                    factory = hikvisionFactory.CreateCamera();
                    break;
                //case CameraType.Mindvision:
                //    MindvisionFactory mindvisionFactory = new MindvisionFactory();
                //    factory = mindvisionFactory.CreateCamera();
                //    break;
                case CameraType.Webcam:
                    WebcamFactory webcamFactory = new WebcamFactory();
                    factory = webcamFactory.CreateCamera();
                    break;
                default:
                    break;
            }
            return factory;
        }
    }

    public enum KeyType
    {
        None,
        Boolean,
        Integer,
        Enumeration,
        Float,
        Command,
        String
    }

    public enum KeyName
    {
        DeviceType,
        DeviceScanType,
        DeviceVendorName,
        DeviceModelName,
        DeviceManufacturerInfo,
        DeviceVersion,
        DeviceFirmwareVersion,
        DeviceSerialNumber,
        DeviceID,
        DeviceUserID,
        DeviceUptime,
        BoardDeviceType,
        DeviceConnectionSelector,
        DeviceConnectionSpeed,
        DeviceConnectionStatus,
        DeviceLinkSelector,
        DeviceLinkSpeed,
        DeviceLinkConnectionCount,
        DeviceLinkHeartbeatMode,
        DeviceLinkHeartbeatTimeout,
        DeviceStreamChannelCount,
        DeviceStreamChannelSelector,
        DeviceStreamChannelType,
        DeviceStreamChannelLink,
        DeviceStreamChannelEndianness,
        DeviceStreamChannelPacketSize,
        DeviceEventChannelCount,
        DeviceCharacterSet,
        DeviceReset,
        DeviceTemperatureSelector,
        DeviceTemperature,
        FindMe,
        DeviceMaxThroughput,
        WidthMax,
        HeightMax,
        RegionSelector,
        RegionDestination,
        Width,
        Height,
        OffsetX,
        OffsetY,
        ReverseX,
        ReverseY,
        ReverseScanDirection,
        PixelFormat,
        PixelSize,
        ImageCompressionMode,
        ImageCompressionQuality,
        TestPatternGeneratorSelector,
        TestPattern,
        BinningSelector,
        BinningHorizontal,
        BinningVertical,
        DecimationHorizontal,
        DecimationVertical,
        Deinterlacing,
        FrameSpecInfoSelector,
        FrameSpecInfo,
        AcquisitionMode,
        AcquisitionStart,
        AcquisitionStop,
        AcquisitionBurstFrameCount,
        AcquisitionFrameRate,
        AcquisitionFrameRateEnable,
        AcquisitionLineRate,
        AcquisitionLineRateEnable,
        ResultingLineRate,
        ResultingFrameRate,
        TriggerSelector,
        TriggerMode,
        TriggerSoftware,
        TriggerSource,
        TriggerActivation,
        TriggerDelay,
        TriggerCacheEnable,
        SensorShutterMode,
        ExposureMode,
        ExposureTime,
        ExposureAuto,
        AutoExposureTimeLowerLimit,
        AutoExposureTimeUpperLimit,
        GainShutPrior,
        FrameTimeoutEnable,
        FrameTimeoutTime,
        HDREnable,
        HDRSelector,
        HDRShuter,
        HDRGain,
        LineSelector,
        LineMode,
        LineInverter,
        LineTermination,
        LineStatus,
        LineStatusAll,
        LineSource,
        StrobeEnable,
        LineDebouncerTime,
        StrobeLineDuration,
        StrobeLineDelay,
        StrobeLinePreDelay,
        CounterSelector,
        CounterEventSource,
        CounterResetSource,
        CounterReset,
        CounterValue,
        CounterCurrentValue,
        Gain,
        GainAuto,
        AutoGainLowerLimit,
        AutoGainUpperLimit,
        ADCGainEnable,
        DigitalShift,
        DigitalShiftEnable,
        Brightness,
        BlackLevel,
        BlackLevelEnable,
        BlackLevelAuto,
        BalanceWhiteAuto,
        BalanceRatioSelector,
        BalanceRatio,
        Gamma,
        GammaSelector,
        GammaEnable,
        Sharpness,
        SharpnessEnable,
        SharpnessAuto,
        Hue,
        HueEnable,
        HueAuto,
        Saturation,
        SaturationEnable,
        SaturationAuto,
        DigitalNoiseReductionMode,
        NoiseReduction,
        AirspaceNoiseReduction,
        TemporalNoiseReduction,
        AutoFunctionAOISelector,
        AutoFunctionAOIWidth,
        AutoFunctionAOIHeight,
        AutoFunctionAOIOffsetX,
        AutoFunctionAOIOffsetY,
        AutoFunctionAOIUsageIntensity,
        AutoFunctionAOIUsageWhiteBalance,
        LUTSelector,
        LUTEnable,
        LUTIndex,
        LUTValue,
        LUTValueAll,
        EncoderSelector,
        EncoderSourceA,
        EncoderSourceB,
        EncoderTriggerMode,
        EncoderCounterMode,
        EncoderCounter,
        EncoderCounterMax,
        EncoderCounterReset,
        EncoderMaxReverseCounter,
        EncoderReverseCounterReset,
        InputSource,
        SignalAlignment,
        PreDivider,
        Multiplier,
        PostDivider,
        ShadingSelector,
        ActivateShading,
        NUCEnable,
        FPNCEnable,
        PRNUCEnable,
        UserSetCurrent,
        UserSetSelector,
        UserSetLoad,
        UserSetSave,
        UserSetDefault,
        PayloadSize,
        GevVersionMajor,
        GevVersionMinor,
        GevDeviceModeIsBigEndian,
        GevDeviceModeCharacterSet,
        GevInterfaceSelector,
        GevMACAddress,
        GevSupportedOptionSelector,
        GevSupportedOption,
        GevCurrentIPConfigurationLLA,
        GevCurrentIPConfigurationDHCP,
        GevCurrentIPConfigurationPersistentIP,
        GevPAUSEFrameReception,
        GevCurrentIPAddress,
        GevCurrentSubnetMask,
        GevCurrentDefaultGateway,
        GevFirstURL,
        GevSecondURL,
        GevNumberOfInterfaces,
        GevPersistentIPAddress,
        GevPersistentSubnetMask,
        GevPersistentDefaultGateway,
        GevLinkSpeed,
        GevMessageChannelCount,
        GevStreamChannelCount,
        GevHeartbeatTimeout,
        GevGVCPHeartbeatDisable,
        GevTimestampTickFrequency,
        GevTimestampControlLatch,
        GevTimestampControlReset,
        GevTimestampControlLatchReset,
        GevTimestampValue,
        GevCCP,
        GevStreamChannelSelector,
        GevSCPInterfaceIndex,
        GevSCPHostPort,
        GevSCPDirection,
        GevSCPSFireTestPacket,
        GevSCPSDoNotFragment,
        GevSCPSBigEndian,
        PacketUnorderSupport,
        GevSCPSPacketSize,
        GevSCPD,
        GevSCDA,
        GevSCSP,
        TLParamsLocked
    }
}
