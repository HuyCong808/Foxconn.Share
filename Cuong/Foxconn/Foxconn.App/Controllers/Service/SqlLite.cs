using Foxconn.App.Helper.Enums;
using System;
using System.Net.NetworkInformation;

namespace Foxconn.App.Controllers.Service
{
    public class SqlLite
    {
        private readonly MainWindow Root = MainWindow.Current;
        public int Index
        {
            get => _index;
            set => _index = value;
        }
        public string Alias
        {
            get => _alias;
            set => _alias = value;
        }
        public string UserId
        {
            get => _userId;
            set => _userId = value;
        }
        public string Password
        {
            get => _password;
            set => _password = value;
        }
        public string Host
        {
            get => _host;
            set => _host = value;
        }
        public int Port
        {
            get => _port;
            set => _port = value;
        }
        public string ServiceName
        {
            get => _serviceName;
            set => _serviceName = value;
        }
        public ConnectionStatus Status
        {
            get => _status;
            set => _status = value;
        }
        private int _index { get; set; }
        private string _alias { get; set; }
        private string _userId { get; set; }
        private string _password { get; set; }
        private string _host { get; set; }
        private int _port { get; set; }
        private string _serviceName { get; set; }
        private ConnectionStatus _status { get; set; }

        public SqlLite()
        {
            _status = ConnectionStatus.None;
        }

        public bool StartFlask()
        {
            _status = ConnectionStatus.Disconnected;
            if (IsPingSuccessAsync())
            {
                _status = ConnectionStatus.Connected;
                Root.ShowMessage($"[Flask {_index}] Connected ({_host}:{_port})", AppColor.Green);
            }
            else
            {
                Root.ShowMessage($"[Flask] Cannot ping to ({_host}:{_port})", AppColor.Red);
            }
            return _status == ConnectionStatus.Connected;
        }

        /// <summary>
        /// Ping to host & port
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool IsPingSuccessAsync(string host = null, int port = 0)
        {
            try
            {
                using (var client = new System.Net.Sockets.TcpClient())
                {
                    client.SendTimeout = 500;
                    client.ReceiveTimeout = 500;
                    return client.ConnectAsync(host ?? _host, port != 0 ? port : _port).Wait(500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Ping to host
        /// </summary>
        /// <param name="_ip"></param>
        /// <returns></returns>
        public bool IsPingSuccess(string host = null)
        {
            try
            {
                var ping = new Ping();
                var pingReply = ping.Send(host ?? _host);
                return pingReply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
