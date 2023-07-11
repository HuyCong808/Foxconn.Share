using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Foxconn.Editor
{
    public class SocketClient
    {
        private readonly object _lockObj = new object();
        private Thread _threadClient = null;
        private TcpClient _tcpClient = null;
        private NetworkStream _networkStream = null;
        private StreamReader _streamReader = null;
        private StreamWriter _streamWriter = null;
        private readonly ASCIIEncoding _encoding = new ASCIIEncoding();
        private string _host = string.Empty;
        private int _port = 0;
        private bool _isConnected = false;
        private string _dataReceived = string.Empty;

        public delegate void DataReceivedEvent(string data);
        public DataReceivedEvent InvokeDataReceived = null;

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
                        {
                            return 1;
                        }
                    }
                }
                else
                {
                    using (TcpClient tcpClient = new TcpClient())
                    {
                        tcpClient.SendTimeout = 500;
                        tcpClient.ReceiveTimeout = 500;
                        if (tcpClient.ConnectAsync(host, port).Wait(500))
                        {
                            return 1;
                        }
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
                _isConnected = _tcpClient.Connected;
                if (_isConnected)
                {
                    _networkStream = new NetworkStream(_tcpClient.Client);
                    _streamReader = new StreamReader(_networkStream);
                    _streamWriter = new StreamWriter(_networkStream);
                    if (_threadClient == null)
                    {
                        _threadClient = new Thread(new ThreadStart(SocketDataReceived));
                        _threadClient.IsBackground = true;
                        _threadClient.Start();
                    }
                    Trace.WriteLine($"SocketClient.Open ({_host}:{_port}): Opened");
                    return 1;
                }
                else
                {
                    Trace.WriteLine($"SocketClient.Open ({_host}:{_port}): Cannot open");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
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
                _networkStream?.Dispose();
                _streamReader?.Dispose();
                _streamWriter?.Dispose();
                Trace.WriteLine($"SocketClient.Close ({_host}:{_port}): Closed");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public TcpClient GetSocketClient() => _tcpClient;

        private void SocketDataReceived()
        {
            try
            {
                while (true)
                {
                    if (_isConnected)
                    {
                        try
                        {
                            string data = _streamReader.ReadLine();
                            if (data.Length > 0)
                            {
                                _dataReceived = data;
                                Trace.WriteLine($"SocketClient.SocketDataReceived ({_host}:{_port}): {data}");
                                InvokeDataReceived?.Invoke(data);
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex);
                            Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public int SocketWriteData(string data)
        {
            try
            {
                if (_isConnected)
                {
                    _streamWriter.WriteLine(data);
                    _streamWriter.Flush();
                    _streamWriter.FlushAsync();
                    Trace.WriteLine($"SocketClient.SocketWriteData ({_host}:{_port}): {data}");
                    return 1;
                }
                else
                {
                    Trace.WriteLine($"SocketClient.SocketWriteData ({_host}:{_port}): Cannot open");
                }
                return 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }

        public void ClearDataReceived()
        {
            _dataReceived = string.Empty;
        }
    }
}
