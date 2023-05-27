using Foxconn.App.Helper;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace Foxconn.App.Controllers.Vnc
{
    public class VncManager : IDisposable
    {
        private readonly MainWindow Root = MainWindow.Current;
        public List<Vnc> VncList { get; set; }
        private bool _disposed { get; set; }

        public VncManager()
        {
            VncList = new List<Vnc>();
        }

        #region Disposable
        ~VncManager()
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
                HideAll();
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
                var registryKey = Registry.CurrentUser.CreateSubKey("Software\\RealVNC\\vncviewer");
                if (registryKey != null)
                {
                    registryKey.SetValue("EulaAccepted", "495139c2b98f2ccd1c353c891250ec12be4f91e4");
                    registryKey.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        public void Start()
        {
            var filePath = IntPtr.Size == 4 ?
                @$"{AppDomain.CurrentDomain.BaseDirectory}DeveloperTools\VNC-Viewer-6.0.0-Windows-32bit.exe" :
                @$"{AppDomain.CurrentDomain.BaseDirectory}DeveloperTools\VNC-Viewer-6.0.0-Windows-64bit.exe";
            var vncs = Root.AppManager.DatabaseManager.Basic.Vncs;
            foreach (var item in vncs)
            {
                if (item.Enable)
                {
                    Root.ShowMessage($"[VNC {item.Index}] Enable");
                    var configFilePath = @$"{AppDomain.CurrentDomain.BaseDirectory}DeveloperTools\VNC{item.Index}.ini";
                    var vnc = new Vnc()
                    {
                        Index = item.Index,
                        Alias = item.Alias,
                        Host = item.Host,
                        Password = item.Password,
                        FilePath = filePath,
                        ConfigFilePath = configFilePath,
                        Width = SystemInformation.WorkingArea.Size.Width / 4,
                        Height = SystemInformation.WorkingArea.Size.Height / 3,
                    };
                    vnc.CreatConfigurationFile(configFilePath, item.Host, item.Password);
                    VncList.Add(vnc);
                }
                else
                {
                    Root.ShowMessage($"[VNC {item.Index}] Disable");
                }
                Thread.Sleep(10);
            }
        }

        public void Show(int index)
        {
            var vnc = VncList.Find(x => x.Index == index);
            if (vnc != null)
            {
                _ = vnc.Start();
            }
        }

        public void Hide(int index)
        {
            var vnc = VncList.Find(x => x.Index == index);
            if (vnc != null)
            {
                _ = vnc.Stop();
            }
        }

        public void ShowAll()
        {
            foreach (var item in VncList)
            {
                _ = item.Start();
            }
        }

        public void HideAll()
        {
            foreach (var item in VncList)
            {
                _ = item.Stop();
            }
        }
    }
}
