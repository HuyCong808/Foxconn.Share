using Foxconn.App.Helper;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace Foxconn.App.Controllers.Client
{
    public class ClientManager : IDisposable
    {
        private readonly MainWindow Root = MainWindow.Current;
        public List<Client> ClientList { get; set; }
        private bool _disposed { get; set; }

        public ClientManager()
        {
            ClientList = new List<Client>();
        }

        #region Disposable
        ~ClientManager()
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
                if (ClientList.Count > 0)
                {
                    foreach (var item in ClientList)
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
            var clients = Root.AppManager.DatabaseManager.Runtime.Positions.FindAll(x => x.IsClient == true);
            if (clients.Count > 0)
            {
                foreach (var item in clients)
                {
                    if (item.Enable)
                    {
                        Root.ShowMessage($"[Client {item.Index}] Enable");
                        var client = new Client
                        {
                            Index = item.Index,
                            Alias = item.Alias,
                            Host = item.Client.Host,
                            Port = item.Client.Port,
                            TabHome = Root.AppManager.TabHome[item.Index],
                        };
                        client.StartClient();
                        ClientList.Add(client);
                        Root.Dispatcher.Invoke(() => { Root.AppManager.TabHome[item.Index].Border.Visibility = Visibility.Visible; });
                    }
                    else
                    {
                        Root.ShowMessage($"[Client {item.Index}] Disable");
                        Root.Dispatcher.Invoke(() => { Root.AppManager.TabHome[item.Index].Border.Visibility = Visibility.Hidden; });
                    }
                    Thread.Sleep(10);
                }
            }
            else
            {
                Root.ShowMessage($"No clients found");
            }
        }
    }
}
