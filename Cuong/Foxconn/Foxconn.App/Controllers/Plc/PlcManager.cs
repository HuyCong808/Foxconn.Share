using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Foxconn.App.Controllers.Plc
{
    public class PlcManager : IDisposable
    {
        private readonly MainWindow Root = MainWindow.Current;
        public List<Plc> PlcList { get; set; }
        private bool _disposed { get; set; }

        public PlcManager()
        {
            PlcList = new List<Plc>();
        }

        #region Disposable
        ~PlcManager()
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
            var plcs = Root.AppManager.DatabaseManager.Basic.Plc.Devices;
            foreach (var item in plcs)
            {
                if (item.Enable)
                {
                    Root.ShowMessage($"[PLC {item.Index}] Enable");
                    var plc = new Plc
                    {
                        Index = item.Index,
                        Alias = item.Alias,
                        Host = item.Host,
                        Port = item.Port
                    };
                    plc.StartPlc();
                    PlcList.Add(plc);
                }
                else
                {
                    Root.ShowMessage($"[PLC {item.Index}] Disable");
                }
                Thread.Sleep(10);
            }
        }

        public void CheckConnection(int index = 0)
        {
            var plc = PlcList.Find(x => x.Index == index);
            if (plc != null)
            {
                plc.CheckConnection();
            }
        }

        public int GetValue(string label, bool showLog = false, int index = 0)
        {
            var plc = PlcList.Find(x => x.Index == index);
            if (plc != null)
            {
                if (plc.Status != ConnectionStatus.Connected)
                    return -1;
                var value = -1;
                var result = plc.GetDevice(label, ref value);
                if (result && showLog)
                {
                    Root.ShowMessage($"[PLC {index}] Get {label}:{value}");
                }
                return value;
            }
            return -1;
        }

        public bool GetDevice(string label, bool show = false, int index = 0)
        {
            var plc = PlcList.Find(x => x.Index == index);
            if (plc != null)
            {
                if (plc.Status != ConnectionStatus.Connected)
                    return false;
                var value = -1;
                var result = plc.GetDevice(label, ref value);
                if (result && value > 0 && show)
                {
                    Root.ShowMessage($"[PLC {index}] Get {label}:{value}");
                }
                return value > 0;
            }
            return false;
        }

        public bool SetDevice(string label, int value = 0, bool show = true, int index = 0)
        {
            var plc = PlcList.Find(x => x.Index == index);
            if (plc != null)
            {
                if (plc.Status != ConnectionStatus.Connected)
                    return false;
                var result = plc.SetDevice(label, value);
                if (result && show)
                {
                    Root.ShowMessage($"[PLC {index}] Set {label}:{value}");
                }
                if (!result)
                {
                    Root.ShowMessage($"[PLC {index}] Cannot set {label}:{value}", AppColor.Red);
                }
                return result;
            }
            return false;
        }

        public bool GetSignal(string label, bool show = true, int index = 0)
        {
            var plc = PlcList.Find(x => x.Index == index);
            if (plc != null)
            {
                if (plc.Status != ConnectionStatus.Connected)
                    return false;
                var value = -1;
                var result1 = plc.GetDevice(label, ref value);
                if (result1 && value > 0 && show)
                {
                    Root.ShowMessage($"[PLC {index}] Get {label}:{value}");
                }
                if (result1 && value > 0)
                {
                    var result2 = plc.SetDevice(label, 0);
                    if (result2 && show)
                    {
                        Root.ShowMessage($"[PLC {index}] Set {label}:{0}");
                    }
                    if (!result2)
                    {
                        Root.ShowMessage($"[PLC {index}] Cannot set {label}:{0}", AppColor.Red);
                    }
                    return result2;
                }
            }
            return false;
        }
    }
}
