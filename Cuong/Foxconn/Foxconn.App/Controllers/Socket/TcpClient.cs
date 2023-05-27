using Foxconn.App.Helper.Enums;
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Foxconn.App.Controllers.Socket
{
    public class TcpClient
    {
        public delegate void ConnectionStatusEvent(ConnectionStatus status);
        public delegate void ErrorEvent(Exception ex);
        public delegate void DataReceivedEvent(string data);
        private readonly object _syncObject = new object();
        private readonly ASCIIEncoding _encoding = new ASCIIEncoding();
        private const int _writeTimeout = 500;
        private const int _readTimeout = 500;
        private const int _bufferSize = 1024;
        private const int _delay = 10;
        public ConnectionStatusEvent InvokeStatus { get; set; }
        public ErrorEvent InvokeError { get; set; }
        public DataReceivedEvent InvokeDataReceived { get; set; }
        public string DataReceived
        {
            get => _dataReceived;
            set => _dataReceived = value;
        }
        protected string _host { get; set; }
        protected int _port { get; set; }
        protected bool _isConnected { get; set; }
        protected string _dataReceived { get; set; }
        private System.Net.Sockets.TcpClient _tcpClient { get; set; }
        private Stream _stream { get; set; }
        //private NetworkStream _networkStream { get; set; }
        private StreamReader _streamReader { get; set; }
        private StreamWriter _streamWriter { get; set; }
        private DateTime _dateTime { get; set; }

        public TcpClient()
        {
            _isConnected = false;
            _dataReceived = string.Empty;
            _tcpClient = null;
            _stream = null;
            //_networkStream = null;
            _streamReader = null;
            _streamWriter = null;
            _dateTime = DateTime.Now;
            Task.Factory.StartNew(() => ReceiveLoop(), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public bool Connect()
        {
            _tcpClient = new System.Net.Sockets.TcpClient(_host, _port);
            _isConnected = _tcpClient.Connected;
            if (_isConnected)
            {
                _stream = _tcpClient.GetStream();
                _streamReader = new StreamReader(_stream);
                _streamWriter = new StreamWriter(_stream);
                InvokeStatus.Invoke(ConnectionStatus.Connected);
                Console.WriteLine($"Connected ({_host}:{_port})!");
            }
            else
            {
                Console.WriteLine($"Cannot connect to ({_host}:{_port})");
            }
            return _isConnected;
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
                    client.SendTimeout = _writeTimeout;
                    client.ReceiveTimeout = _readTimeout;
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

        private async Task ReceiveLoop()
        {
            while (true)
            {
                try
                {
                    if (_isConnected)
                    {
                        byte[] buff = new byte[_bufferSize];
                        int bytes = _stream.Read(buff, 0, _bufferSize);
                        if (bytes == 0)
                        {
                            _isConnected = false;
                            _dataReceived = string.Empty;
                            Disconnect();
                            Console.WriteLine($"Disconnected ({_host}:{_port})!");
                        }
                        else
                        {
                            //string data = _encoding.GetString(buff).Trim(new char[] { '\0' });
                            string data = _encoding.GetString(buff).Replace("\0", "").Trim();
                            if (data.Length > 0)
                            {
                                _dataReceived = data;
                                InvokeDataReceived?.Invoke(data);
                                Console.WriteLine($"Received ({_host}:{_port}): {data}");
                            }
                        }
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        public async Task Send(string data)
        {
            if (_isConnected)
            {
                //byte[] dataBytes = _encoding.GetBytes(data);
                byte[] dataBytes = _encoding.GetBytes(data + "\r\n");
                _stream.Write(dataBytes, 0, dataBytes.Length);
                _stream.Flush();
                await _stream.FlushAsync();
                Console.WriteLine($"Sent ({_host}:{_port}): {data}");
            }
            else
            {
                Console.WriteLine($"Cannot send to ({_host}:{_port}): {data}");
            }
        }

        public void Disconnect()
        {
            _tcpClient?.Close();
            _stream?.Close();
            _streamReader?.Close();
            _streamWriter?.Close();
            InvokeStatus.Invoke(ConnectionStatus.Disconnected);
        }

        public void Release()
        {
            Disconnect();
            _tcpClient?.Dispose();
            _stream?.Dispose();
            _streamReader?.Dispose();
            _streamWriter?.Dispose();
        }

        private void ErrorDetails(Exception ex)
        {
            InvokeError?.Invoke(ex);
        }

        private void ErrorDetails(string msg)
        {
            InvokeError?.Invoke(new Exception(msg));
        }
    }
}
