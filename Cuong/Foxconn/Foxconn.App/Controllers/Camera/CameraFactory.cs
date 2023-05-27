using Foxconn.App.Helper.Enums;
using System;
using System.Collections.Generic;

namespace Foxconn.App.Controllers.Camera
{
    public enum FilterDevice
    {
        None,
        SerialNumber,
        ModelName,
        UserDefinedName,
    }
    public enum OrderListDevice
    {
        None,
        SerialNumber,
        ModelName,
        UserDefinedName,
    }
    public enum TriggerMode
    {
        Enable,
        Disable,
    }
    public enum TriggerSource
    {
        Line0,
        Line1,
        Line2,
        Line3,
        Counter0,
        Software,
        FrequencyConverter,
    }
    public delegate void StreamingEvent(System.Drawing.Bitmap bitmap);
    public delegate void ErrorEvent(Exception ex);

    public interface ICamera
    {
        public CameraType CameraType { get; }
        public ErrorEvent InvokeError { get; set; }
        public StreamingEvent InvokeStreaming { get; set; }
        public List<CameraInformation> DeviceList { get; }
        public CameraInformation CameraInformation { get; set; }
        public bool IsConnected { get; }
        public bool IsGrabbing { get; }
        public bool IsStreaming { get; }
        public double ExposureTime { get; set; }
        public double Gain { get; set; }
        public bool Initialize();
        public int ScanDeviceList();
        public string FindDeviceBySerialNumber(string serialNumber);
        public bool Connect(CameraInformation cameraInformation = null);
        public bool Connect(int index);
        public bool Connect(string serialNumber);
        public bool OpenStrategies(bool grabLastest, CameraInformation cameraInformation = null);
        public bool OpenWithStrategies(string serialNumber, bool grabLastest);
        public bool OpenSettings(bool show = true);
        public bool StartGrabbing();
        public bool StopGrabbing();
        public bool StartStreaming();
        public bool StopStreaming();
        public bool SetTriggerMode(TriggerMode state, TriggerSource mode = TriggerSource.Software, bool grabLastest = false);
        public void ExcuteTriggerSoftware();
        public System.Drawing.Bitmap GrabFrame(int timeout = 1000);
        public void GetParameter();
        public void SetParameter();
        public bool Disconnect();
        public bool Release();
    }

    public abstract class ICameraFactory
    {
        public abstract ICamera CreateCamera();
    }

    public class BaslerFactory : ICameraFactory
    {
        public override ICamera CreateCamera()
        {
            return new Basler();
        }
    }

    public class BaumerFactory : ICameraFactory
    {
        public override ICamera CreateCamera()
        {
            return new Baumer();
        }
    }

    public class HikvisionFactory : ICameraFactory
    {
        public override ICamera CreateCamera()
        {
            return new Hikvision();
        }
    }

    public class MindvisionFactory : ICameraFactory
    {
        public override ICamera CreateCamera()
        {
            return new Mindvision();
        }
    }

    public class WebcamFactory : ICameraFactory
    {
        public override ICamera CreateCamera()
        {
            return new Webcam();
        }
    }

    public class CameraFactory
    {
        public static ICamera GetCamera(CameraType cameraType)
        {
            ICamera factory = null;
            switch (cameraType)
            {
                case CameraType.None:
                    break;
                case CameraType.Basler:
                    var baslerFactory = new BaslerFactory();
                    factory = baslerFactory.CreateCamera();
                    break;
                case CameraType.Baumer:
                    var baumerFactory = new BaumerFactory();
                    factory = baumerFactory.CreateCamera();
                    break;
                case CameraType.Hikvision:
                    var hikvisionFactory = new HikvisionFactory();
                    factory = hikvisionFactory.CreateCamera();
                    break;
                case CameraType.Mindvision:
                    var mindvisionFactory = new MindvisionFactory();
                    factory = mindvisionFactory.CreateCamera();
                    break;
                case CameraType.Webcam:
                    var webcamFactory = new WebcamFactory();
                    factory = webcamFactory.CreateCamera();
                    break;
                default:
                    break;
            }
            return factory;
        }
    }
}
