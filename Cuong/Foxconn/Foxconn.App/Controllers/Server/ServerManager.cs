using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Foxconn.App.Controllers.Server
{
    public class ServerManager : IDisposable
    {
        private readonly MainWindow Root = MainWindow.Current;
        public List<Server> ServerList { get; set; }
        private bool _disposed { get; set; }

        public ServerManager()
        {
            ServerList = new List<Server>();
        }

        #region Disposable
        ~ServerManager()
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
                if (ServerList.Count > 0)
                {
                    foreach (var item in ServerList)
                    {
                        item.Release();
                        item.TerminateTimer();
                    }
                }
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
            var servers = Root.AppManager.DatabaseManager.Runtime.Positions.FindAll(x => x.IsServer == true);
            if (servers.Count > 0)
            {
                foreach (var item in servers)
                {
                    if (item.Enable)
                    {
                        Root.ShowMessage($"[Server {item.Index}] Enable");
                        var server = new Server
                        {
                            Index = item.Index,
                            Alias = item.Alias,
                            Host = item.Server.Host,
                            Port = item.Server.Port,
                            ModelName = item.ModelName,
                            TabHome = Root.AppManager.TabHome[item.Index],
                            Plc = item.Plc,
                            Robot = item.Robot,
                        };
                        server.StartServer();
                        ServerList.Add(server);
                        Root.Dispatcher.Invoke(() => { Root.AppManager.TabHome[item.Index].Border.Visibility = Visibility.Visible; });
                    }
                    else
                    {
                        Root.ShowMessage($"[Server {item.Index}] Disable");
                        Root.Dispatcher.Invoke(() => { Root.AppManager.TabHome[item.Index].Border.Visibility = Visibility.Hidden; });
                    }
                    Thread.Sleep(10);
                }
            }
            else
            {
                Root.ShowMessage($"No servers found");
            }
        }

        public void ForceDisconnect(int index)
        {
            var server = ServerList.Find(x => x.Index == index);
            if (server != null)
            {
                server.ForcedClose();
            }
        }

        public async Task Send(string data, int index)
        {
            var server = ServerList.Find(x => x.Index == index);
            if (server != null && server.Status == ConnectionStatus.Connected)
            {
                await server.Send(data);
            }
        }
    }
}
