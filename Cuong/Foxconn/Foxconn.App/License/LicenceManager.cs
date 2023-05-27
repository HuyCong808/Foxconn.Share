using System;
using Foxconn.App.Helper;

namespace Foxconn.App.License
{
    public class LicenceManager : IDisposable
    {
        private bool _disposed { get; set; }

        public LicenceManager()
        {

        }

        #region Disposable
        ~LicenceManager()
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
                Logger.Instance.Write(ex.StackTrace ?? "");
            }
        }
    }
}
