using Foxconn.App.Helper;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Foxconn.App.Controllers.SerialPort
{
    public class SerialPortManager : IDisposable
    {
        private readonly MainWindow Root = MainWindow.Current;
        public List<SerialPort> SerialPortList { get; set; }
        private bool _disposed { get; set; }

        public SerialPortManager()
        {
            SerialPortList = new List<SerialPort>();
        }

        #region Disposable
        ~SerialPortManager()
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
            var serialPorts = Root.AppManager.DatabaseManager.Basic.SerialPorts;
            foreach (var item in serialPorts)
            {
                if (item.Enable)
                {
                    Root.ShowMessage($"[Serial Port {item.Index}] Enable");
                    var serialPort = new SerialPort
                    {
                        Index = item.Index,
                        Alias = item.Alias,
                        PortName = item.PortName
                    };
                    serialPort.StartSerialPort();
                    SerialPortList.Add(serialPort);
                }
                else
                {
                    Root.ShowMessage($"[Serial Port {item.Index}] Disable");
                }
                Thread.Sleep(10);
            }
        }
    }
}
