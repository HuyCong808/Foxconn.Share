using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace Foxconn.AutoWeight.FoxconnEdit
{
    public class TCPServer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Thread _threadServer = null;
        private TcpListener _tcpListener = null;
        private Socket _tcpClient = null;
        private IPEndPoint _remoteEP = null;
        private NetworkStream _networkStream = null;
        private StreamReader _streamReader = null;
        private StreamWriter _streamWriter = null;
        private string _host = null;
        private int _port = 0;
        private bool _isConnected = false;
        private string _dataRecieve = string.Empty;

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
        public string DataRecieve
        {
            get => _dataRecieve;
            set => _dataRecieve = value;
        }


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
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                logger.Error(ex);
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
                        _threadServer = new Thread(new ThreadStart(SocketServerRecieve));
                        _threadServer.IsBackground = true;
                        _threadServer.Start();
                    }
                    return 1;
                }
                else
                {
                    Console.WriteLine($"SocketServer.Open({ _host}:{ _port}): Can not opened");
                    logger.Info($"SocketServer.Open({ _host}:{ _port}): Can not opened");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                logger.Error(ex);
                return -1;
            }
        }

        public void Close()
        {
            try
            {
                _isConnected = false;
                _dataRecieve = String.Empty;
                _tcpListener.Stop();
                _tcpClient.Dispose();
                _streamReader.Dispose();
                _streamWriter.Dispose();
                _networkStream.Dispose();

                logger.Info($"SocketServer.Close ({_host}:{_port}): Closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger.Error(ex);
            }
        }

        public TcpListener GetSocketServer() => _tcpListener;
        public Socket GetSocketClient() => _tcpClient;

        public void SocketServerRecieve()
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
                        logger.Info($"SocketServer.Open ({_remoteEP.Address}:{_port}): Opened");
                        using (_networkStream = new NetworkStream(_tcpClient)) ;
                        using (_streamReader = new StreamReader(_networkStream)) ;
                        using (_streamWriter = new StreamWriter(_networkStream)) ;
                        {
                            while (true)
                            {
                                try
                                {
                                    string data = _streamReader.ReadLine().Trim();
                                    if (data.Length > 0)
                                    {
                                        _dataRecieve = data;
                                        logger.Info($"SocketServer.SocketDataRecieve ({_remoteEP.Address} : {_port} )");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Trace.WriteLine(ex);
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.Info($"SocketServer.Open: ({_host} : {_port}): Can not open");
                    }

                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
        }

        public int SocketSendData(string data)
        {
            try
            {
                if (_isConnected)
                {
                    _streamWriter.WriteLine(data);
                    _streamWriter.Flush();
                    _streamWriter.FlushAsync();
                    logger.Info($"SocketServer.SocketWriteData ({_remoteEP.Address}:{_port}): {data}");
                    return 1;
                }
                else
                {
                    logger.Info($"SocketServer.SocketWriteData ({_remoteEP.Address}:{_port}): Can not opened");
                }
                return 0;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
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
                    _tcpClient.Dispose();
                    logger.Info(($"SocketServer.SocketForceClose ({_remoteEP.Address}:{_port}): Closed"));
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
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
