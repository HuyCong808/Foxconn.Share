using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using System;
using System.Collections.Generic;

namespace Foxconn.App.Controllers.Image
{
    public class ImageManager
    {
        private readonly MainWindow Root = MainWindow.Current;
        public List<Image> ImageList { get; set; }
        public Image Setup { get; set; }
        public Image Setup0 { get; set; }
        public Image Setup1 { get; set; }
        public Image Setup2 { get; set; }
        public Image Process0 { get; set; }
        public Image Process1 { get; set; }
        public Image Process2 { get; set; }
        private bool _disposed { get; set; }

        public ImageManager()
        {
            ImageList = new List<Image>();
            Setup0 = new Image { Index = 0 };
            Setup1 = new Image { Index = 1 };
            Setup2 = new Image { Index = 2 };
            Process0 = new Image { Index = 0 };
            Process1 = new Image { Index = 1 };
            Process2 = new Image { Index = 2 };
            Setup = Setup0;
        }

        #region Disposable
        ~ImageManager()
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
                Setup?.Stop();
                Setup0?.Stop();
                Setup1?.Stop();
                Setup2?.Stop();
                Process0?.Stop();
                Process1?.Stop();
                Process2?.Stop();
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
            var cameras = Root.AppManager.DatabaseManager.Basic.Camera.Devices;
            foreach (var item in cameras)
            {
                if (item.Enable)
                {
                    Root.ShowMessage($"[Image {item.Index}] Enable");
                    if (item.Index == 0)
                    {
                        var camera = Root.AppManager.CameraManager.CameraList.Find(x => x.Index == 0);
                        if (camera != null)
                        {
                            Process0.Camera = camera.CameraInstance;
                            if (Process0.Start())
                            {
                                Root.ShowMessage("[Image 0] Started");
                            }
                            else
                            {
                                Root.ShowMessage("[Image 0] Cannot start", AppColor.Red);
                            }
                        }
                    }
                    else if (item.Index == 1)
                    {
                        var camera = Root.AppManager.CameraManager.CameraList.Find(x => x.Index == 1);
                        if (camera != null)
                        {
                            Process1.Camera = camera.CameraInstance;
                            if (Process1.Start())
                            {
                                Root.ShowMessage("[Image 1] Started");
                            }
                            else
                            {
                                Root.ShowMessage("[Image 1] Cannot start", AppColor.Red);
                            }
                        }
                    }
                    else if (item.Index == 2)
                    {
                        var camera = Root.AppManager.CameraManager.CameraList.Find(x => x.Index == 2);
                        if (camera != null)
                        {
                            Process2.Camera = camera.CameraInstance;
                            if (Process2.Start())
                            {
                                Root.ShowMessage("[Image 2] Started");
                            }
                            else
                            {
                                Root.ShowMessage("[Image 2] Cannot start", AppColor.Red);
                            }
                        }
                    }
                }
                else
                {
                    Root.ShowMessage($"[Image {item.Index}] Disable");
                }
            }
            GetProcessConfiguration();
        }

        private void GetProcessConfiguration()
        {
            Process0.Init(Root.AppManager.DatabaseManager.Runtime.StepsForCamera0, false);
            Process1.Init(Root.AppManager.DatabaseManager.Runtime.StepsForCamera1, false);
            Process2.Init(Root.AppManager.DatabaseManager.Runtime.StepsForCamera2, false);
        }

        public void GetSetupConfiguration()
        {
            Setup0.Init(Root.AppManager.DatabaseManager.Runtime.StepsForCamera0, true);
            Setup1.Init(Root.AppManager.DatabaseManager.Runtime.StepsForCamera1, true);
            Setup2.Init(Root.AppManager.DatabaseManager.Runtime.StepsForCamera2, true);
        }
    }
}
