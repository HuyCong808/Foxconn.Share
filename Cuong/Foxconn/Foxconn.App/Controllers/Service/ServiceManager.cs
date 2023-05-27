using Foxconn.App.Helper;
using System;

namespace Foxconn.App.Controllers.Service
{
    public class ServiceManager : IDisposable
    {
        private readonly MainWindow Root = MainWindow.Current;
        public Flask Flask { get; set; }
        private bool _disposed { get; set; }

        public ServiceManager()
        {

        }

        #region Disposable
        ~ServiceManager()
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
            var services = Root.AppManager.DatabaseManager.Basic.Services;
            foreach (var item in services)
            {
                if (item.Enable)
                {
                    Root.ShowMessage($"[Service {item.Index}] Enable");
                    if (item.Alias == "Flask")
                    {
                        Flask = new Flask()
                        {
                            Index = item.Index,
                            Alias = item.Alias,
                            UserId = item.UserId,
                            Password = item.Password,
                            Host = item.Host,
                            Port = item.Port,
                            ServiceName = item.ServiceName,
                        };
                        Flask.StartFlask();
                    }
                }
                else
                {
                    Root.ShowMessage($"[Service {item.Index}] Disable");
                }
            }
        }
    }
}
