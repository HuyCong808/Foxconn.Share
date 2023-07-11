using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Foxconn.Editor
{
    public class TCPClient
    {
        private Thread _threadClient = null;
        private TcpClient _tcpClient = null;
        private Stream _stream = null;
        private StreamReader _streamReader = null;
        private StreamWriter _streamWriter = null;
        private readonly ASCIIEncoding _encoding = new ASCIIEncoding();
        private string _host = string.Empty;
        private int _port = 0;
        private bool _isConnected = false;
        private string _dataReceived = string.Empty;

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

        public bool IsConnected
        {
            get => _isConnected;
            set => _isConnected = value;
        }

        public string DataReceived => _dataReceived;

        public int Ping(string host, int port, bool pingHost = false)
        {
            try
            {
                if (pingHost)
                {
                    using (Ping ping = new Ping())
                    {
                        PingReply pingReply = ping.Send(host);
                        if (pingReply.Status == IPStatus.Success)
                            return 1;
                    }
                }
                else
                {
                    using (TcpClient tcpClient = new TcpClient())
                    {
                        tcpClient.SendTimeout = 500;
                        tcpClient.ReceiveTimeout = 500;
                        if (tcpClient.ConnectAsync(host, port).Wait(500))
                            return 1;
                    }
                }
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public int Open(string host, int port)
        {
            try
            {
                _host = host;
                _port = port;
                _tcpClient = new TcpClient(host, port);
                _tcpClient.ReceiveTimeout = 500;
                _tcpClient.SendTimeout = 500;
                _isConnected = _tcpClient.Connected;
                if (_isConnected)
                {
                    _stream = _tcpClient.GetStream();
                    _streamReader = new StreamReader(_stream);
                    _streamWriter = new StreamWriter(_stream);
                    if (_threadClient == null)
                    {
                        _threadClient = new Thread(new ThreadStart(SocketDataReceived));
                        _threadClient.IsBackground = true;
                        _threadClient.Start();
                    }
                    Logger.Current.Info($"TcpClient.Open ({_host}:{_port}): Opened");
                    return 1;
                }
                else
                {
                    Logger.Current.Info($"TcpClient.Open ({_host}:{_port}): Can not opened");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error(ex.Message);
                return -1;
            }
        }

        public void Close()
        {
            try
            {
                _isConnected = false;
                _dataReceived = string.Empty;
                _tcpClient?.Dispose();
                _stream?.Dispose();
                _streamReader?.Dispose();
                _streamWriter?.Dispose();
                Logger.Current.Info($"TcpClient.Close ({_host}:{_port}): Closed");
            }
            catch (Exception ex)
            {
                Logger.Current.Error(ex.Message);
            }
        }

        public TcpClient GetSocketClient() => _tcpClient;

        private void SocketDataReceived()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        if (_isConnected)
                        {
                            byte[] buffer = new byte[1024];
                            int bytes = _stream.Read(buffer, 0, 1024);
                            if (bytes == 0)
                            {
                                Close();
                            }
                            else
                            {
                                string data = _encoding.GetString(buffer).Replace("\0", "").Trim();
                                if (data.Length > 0)
                                {
                                    _dataReceived = data;
                                    Logger.Current.Info($"TcpClient.SocketDataReceived ({_host}:{_port}): {data}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error(ex.Message);
            }
        }

        public int SocketWriteData(string data)
        {
            try
            {
                if (_isConnected)
                {
                    byte[] dataBytes = _encoding.GetBytes(data + "\r\n");
                    _stream.Write(dataBytes, 0, dataBytes.Length);
                    _stream.Flush();
                    _stream.FlushAsync();
                    Logger.Current.Info($"TcpClient.SocketWriteData ({_host}:{_port}): {data}");

                    return 1;
                }
                else
                {
                    Logger.Current.Info($"TcpClient.SocketWriteData ({_host}:{_port}): Can not opened");
                }
                return 0;
            }
            catch (Exception ex)
            {
                Logger.Current.Error(ex.Message);
                return -1;
            }
        }
    }
}
