using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Foxconn.App.Controllers.Camera
{
    public class CameraManager : IDisposable
    {
        private readonly MainWindow Root = MainWindow.Current;
        public List<CameraData> CameraList { get; set; }
        public List<CameraData> CameraScan { get; set; }
        private bool _disposed { get; set; }

        public CameraManager()
        {
            CameraList = new List<CameraData>();
            CameraScan = new List<CameraData>();
        }

        #region Disposable
        ~CameraManager()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        // Public implementation of Dispose pattern callable by consumers.
        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.
            _disposed = true;
        }
        #endregion

        public void Init()
        {
            try
            {
                _disposed = false;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        public void Start()
        {
            int index = 0;
            foreach (var item in Enum.GetValues(typeof(CameraType)))
            {
                if ((CameraType)item == CameraType.None)
                    continue;

                // Create an camera instance
                var camera = CameraFactory.GetCamera((CameraType)item);
                camera.InvokeError += ErrorEventHandler;
                camera.InvokeStreaming += StreamingEventHandler;
                camera.Initialize();

                // Baumer camera must contain EGX characters in model name.
                var baumer1 = (CameraType)item == CameraType.Baumer;
                var baumer2 = camera.DeviceList.Find(x => x.ModelName.Contains("EXG"));
                if (baumer1 && baumer2 == null)
                {
                    Root.ShowMessage($"No devices found ({Enum.GetName(typeof(CameraType), (int)camera.CameraType)})");
                    camera.InvokeError -= ErrorEventHandler;
                    camera.InvokeStreaming -= StreamingEventHandler;
                    camera.Disconnect();
                    camera.Release();
                    camera = null;
                    continue;
                }

                // Hikvision camera must contain MV characters in model name.
                var hikvision1 = (CameraType)item == CameraType.Hikvision;
                var hikvision2 = camera.DeviceList.Find(x => x.ModelName.Contains("MV"));
                if (hikvision1 && hikvision2 == null)
                {
                    Root.ShowMessage($"No devices found ({Enum.GetName(typeof(CameraType), (int)camera.CameraType)})");
                    camera.InvokeError -= ErrorEventHandler;
                    camera.InvokeStreaming -= StreamingEventHandler;
                    camera.Disconnect();
                    camera.Release();
                    camera = null;
                    continue;
                }

                // Devices
                if (camera.DeviceList.Count > 0)
                {
                    Root.ShowMessage($"Devices found ({Enum.GetName(typeof(CameraType), (int)camera.CameraType)})");
                    for (int i = 0; i < camera.DeviceList.Count; i++)
                    {
                        CameraScan.Add(new CameraData
                        {
                            Index = index,
                            CameraInstance = camera,
                            CameraInformation = camera.DeviceList[i]
                        });
                        ++index;
                        Root.ShowMessage($"[Device {i}] User Defined Name: {camera.DeviceList[i].UserDefinedName}");
                        Root.ShowMessage($"[Device {i}] Model Name: {camera.DeviceList[i].ModelName}");
                        Root.ShowMessage($"[Device {i}] Serial Number: {camera.DeviceList[i].SerialNumber}");
                    }

                    foreach (var device in Root.AppManager.DatabaseManager.Basic.Camera.Devices)
                    {
                        var searchDevice = camera.DeviceList.Find(x => x.UserDefinedName == device.UserDefinedName && x.ModelName == device.ModelName && x.SerialNumber == device.SerialNumber);
                        if (searchDevice != null && device.CameraType == (CameraType)item)
                        {
                            camera.CameraInformation = searchDevice;
                            CameraList.Add(new CameraData
                            {
                                Index = device.Index,
                                CameraInstance = camera,
                                CameraInformation = searchDevice
                            });
                        }
                    }
                }
                else
                {
                    Root.ShowMessage($"No devices found ({Enum.GetName(typeof(CameraType), (int)camera.CameraType)})");
                    camera.InvokeError -= ErrorEventHandler;
                    camera.InvokeStreaming -= StreamingEventHandler;
                    camera.Disconnect();
                    camera.Release();
                    camera = null;
                }
            }
        }

        private void ErrorEventHandler(Exception ex)
        {
            Logger.Instance.Write(ex.StackTrace);
        }

        void StreamingEventHandler(Bitmap bitmap)
        {

        }
    }

    public class CameraData
    {
        public int Index { get; set; }
        public ICamera CameraInstance { get; set; }
        public CameraInformation CameraInformation { get; set; }
    }
}

//ICamera camera = null;
//camera = CameraFactory.GetCamera(CameraType.Basler);
//camera.InvokeError += ErrorEventHandler;
//camera.InvokeStreaming += StreamingEventHandler;
//camera.Initialize();

//camera = CameraFactory.GetCamera(CameraType.Baumer);
//camera.InvokeError += ErrorEventHandler;
//camera.InvokeStreaming += StreamingEventHandler;
//camera.Initialize();

//camera = CameraFactory.GetCamera(CameraType.Hikvision);
//camera.InvokeError += ErrorEventHandler;
//camera.InvokeStreaming += StreamingEventHandler;
//camera.Initialize();

//camera = CameraFactory.GetCamera(CameraType.Mindvision);
//camera.InvokeError += ErrorEventHandler;
//camera.InvokeStreaming += StreamingEventHandler;
//camera.Initialize();

//camera = CameraFactory.GetCamera(CameraType.Webcam);
//camera.InvokeError += ErrorEventHandler;
//camera.InvokeStreaming += StreamingEventHandler;
//camera.Initialize();

// Debug
//camera.OpenStrategies(true, null);
//camera.ExcuteTriggerSoftware();
//var bitmap = camera.GrabFrame();
//if (bitmap != null)
//{
//    AppUI.ShowImageBox(Root, Root.imbCamera0, bitmap);
//}