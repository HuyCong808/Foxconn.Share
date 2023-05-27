using Foxconn.App.Helper.Enums;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Foxconn.App.Controllers.Socket
{
    public class TcpServer
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
        protected string _clientHost { get; set; }
        private TcpListener _tcpListener { get; set; }
        private System.Net.Sockets.Socket _tcpClient { get; set; }
        private IPEndPoint _newClient { get; set; }
        private NetworkStream _networkStream { get; set; }
        private StreamReader _streamReader { get; set; }
        private StreamWriter _streamWriter { get; set; }
        private DateTime _dateTime { get; set; }

        public TcpServer()
        {
            _isConnected = false;
            _dataReceived = string.Empty;
            _clientHost = string.Empty;
            //_tcpListener = new TcpListener(IPAddress.Any, _port);
            _tcpClient = null;
            _newClient = null;
            _networkStream = null;
            _streamReader = null;
            _streamWriter = null;
            _dateTime = DateTime.Now;
            //Task.Factory.StartNew(() => ListenLoop(), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void StartListen()
        {
            _tcpListener = new TcpListener(IPAddress.Any, _port);
            Task.Factory.StartNew(() => ListenLoop(), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
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

        private async Task ListenLoop()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        _isConnected = false;
                        //InvokeStatus.Invoke(ConnectionStatus.Waiting);
                        Console.WriteLine($"Waiting for a connection...");
                        _tcpListener.Start();
                        _tcpClient = _tcpListener.AcceptSocket();
                        if (_tcpClient.Connected)
                        {
                            _isConnected = true;
                            _newClient = (IPEndPoint)_tcpClient.RemoteEndPoint;
                            _clientHost = _newClient.Address.ToString();
                            InvokeStatus.Invoke(ConnectionStatus.Connected);
                            Console.WriteLine($"Connected ({_clientHost}:{_port})!");
                            using (_networkStream = new NetworkStream(_tcpClient))
                            using (_streamReader = new StreamReader(_networkStream))
                            using (_streamWriter = new StreamWriter(_networkStream))
                            {
                                while (true)
                                {
                                    try
                                    {
                                        string data = _streamReader.ReadLine().Trim();
                                        if (data.Length > 0)
                                        {
                                            _dataReceived = data;
                                            InvokeDataReceived?.Invoke(data);
                                            Console.WriteLine($"Received ({_clientHost}:{_port}): {data}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _dataReceived = string.Empty;
                                        ErrorDetails(ex);
                                        InvokeStatus.Invoke(ConnectionStatus.Disconnected);
                                        Console.WriteLine($"Disconnected ({_clientHost}:{_port})!");
                                        break;
                                    }
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
                        ErrorDetails(ex);
                        InvokeStatus.Invoke(ConnectionStatus.Error);
                        Console.WriteLine($"Error listening on port {_port}!");
                        break;
                    }
                }
            }
            finally
            {
                _tcpListener.Stop();
                Console.WriteLine("Stop listening for new clients");
            }
        }

        public async Task Send(string data)
        {
            if (_isConnected)
            {
                _streamWriter.WriteLine(data);
                _streamWriter.Flush();
                await _streamWriter.FlushAsync();
                Console.WriteLine($"Sent ({_clientHost}:{_port}): {data}");
            }
            else
            {
                Console.WriteLine($"Cannot send to ({_clientHost}:{_port}): {data}");
            }
        }

        public bool HasConnected()
        {
            try
            {
                if (_tcpClient != null && _tcpClient.Connected)
                {
                    Console.WriteLine($"Has connected ({_clientHost}:{_port})");
                    return !_tcpClient.Poll(1, SelectMode.SelectRead) || _tcpClient.Available != 0;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorDetails(ex);
                return false;
            }
        }

        public void ForcedClose()
        {
            try
            {
                if (_tcpClient != null)
                {
                    try
                    {
                        _tcpClient.Shutdown(SocketShutdown.Both);
                        _tcpClient.Disconnect(false);
                    }
                    finally
                    {
                        _tcpClient.Close();
                        _tcpClient.Dispose();
                    }
                    //InvokeStatus.Invoke(ConnectionStatus.Disconnected);
                    Console.WriteLine($"Forced close ({_clientHost}:{_port})");
                }
            }
            catch (Exception ex)
            {
                ErrorDetails(ex);
            }
        }

        public void Disconnect()
        {
            _tcpClient?.Close();
            _networkStream?.Close();
            _streamReader?.Close();
            _streamWriter?.Close();
            InvokeStatus.Invoke(ConnectionStatus.Disconnected);
        }

        public void Release()
        {
            Disconnect();
            _tcpClient?.Dispose();
            _networkStream?.Dispose();
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
