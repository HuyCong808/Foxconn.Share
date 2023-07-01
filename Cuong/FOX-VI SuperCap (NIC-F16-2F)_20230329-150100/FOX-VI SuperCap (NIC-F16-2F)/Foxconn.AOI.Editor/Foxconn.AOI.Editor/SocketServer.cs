using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Foxconn.AOI.Editor
{
    public class SocketServer
    {
        private Thread _threadServer = null;
        private TcpListener _tcpListener = null;
        private Socket _tcpClient = null;
        private IPEndPoint _remoteEP = null;
        private NetworkStream _networkStream = null;
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

        public int Open(string ip, int port)
        {

            try
            {
                _host = ip;
                _port = port;
                if (Ping(ip, port) != 1)
                {
                    _tcpListener = new TcpListener(IPAddress.Any, port);
                    if (_threadServer == null)
                    {
                        _threadServer = new Thread(new ThreadStart(SocketDataReceived));
                        _threadServer.IsBackground = true;
                        _threadServer.Start();
                    }
                    return 1;
                }
                else
                {
                    Trace.WriteLine($"SocketServer.Open ({_host}:{_port}): Can not opened");
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
                _tcpListener?.Stop();
                _tcpClient?.Dispose();
                _networkStream?.Dispose();
                _streamReader?.Dispose();
                _streamWriter?.Dispose();
                Trace.WriteLine($"SocketServer.Close ({_host}:{_port}): Closed");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public TcpListener GetSocketServer() => _tcpListener;

        public Socket GetSocketClient() => _tcpClient;

        private void SocketDataReceived()
        {
            while (true)
            {
                try
                {
                    _isConnected = false;
                    _tcpListener.Start();
                    _tcpClient = _tcpListener.AcceptSocket();
                    if (_tcpClient.Connected)
                    {
                        _isConnected = _tcpClient.Connected;
                        
                        _remoteEP = (IPEndPoint)_tcpClient.RemoteEndPoint;
                        Trace.WriteLine($"SocketServer.Open ({_remoteEP.Address}:{_port}): Opened");
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
                                        Trace.WriteLine($"SocketServer.SocketDataReceived ({_remoteEP.Address}:{_port}): {data}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _isConnected = false;
                                    _dataReceived = string.Empty;
                                    Trace.WriteLine(ex);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Trace.WriteLine($"SocketServer.Open ({_host}:{_port}): Can not opened");
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
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
                    Trace.WriteLine($"SocketServer.SocketWriteData ({_remoteEP.Address}:{_port}): {data}");
                    return 1;
                }
                else
                {
                    Trace.WriteLine($"SocketServer.SocketWriteData ({_remoteEP.Address}:{_port}): Can not opened");
                }
                return 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }

        public int SocketForceClose()
        {
            try
            {
                if (_tcpClient != null)
                {
                    _tcpClient.Shutdown(SocketShutdown.Both);
                    _tcpClient.Disconnect(false);
                    _tcpClient?.Dispose();
                    Trace.WriteLine($"SocketServer.SocketForceClose ({_remoteEP.Address}:{_port}): Closed");
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }

        public int SocketIsAlive()
        {
            try
            {
                if (_tcpClient != null && _isConnected)
                {
                    if (_tcpClient.Available != 0 || !_tcpClient.Poll(1, SelectMode.SelectRead))
                    {
                        Trace.WriteLine($"SocketServer.SocketIsAlive ({_remoteEP.Address}:{_port}): Available");
                        return 1;
                    }
                }
                Trace.WriteLine($"SocketServer.SocketIsAlive ({_remoteEP.Address}:{_port}): Unavailable");
                return 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }
    }
}
