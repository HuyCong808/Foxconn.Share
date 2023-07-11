using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Foxconn.Editor
{
    internal class SocketLight
    {
        private readonly object _lockObj = new object();
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

        private int SendCommand(byte[] commandBytes)
        {
            try
            {
                int nRet = -1;
                lock (_lockObj)
                {
                    using (TcpClient tcpClient = new TcpClient(_host, _port))
                    {
                        tcpClient.SendTimeout = 1000;
                        tcpClient.ReceiveTimeout = 1000;
                        using (NetworkStream stream = tcpClient.GetStream())
                        {
                            stream.Write(commandBytes, 0, commandBytes.Length);
                            byte[] buff = new byte[1024];
                            stream.Read(buff, 0, 1024);
                            string buffer = Encoding.UTF8.GetString(buff);
                            string result = buffer.Trim(new char[] { '\0' });
                            if (result.Contains("ok"))
                            {
                                nRet = 1;
                            }
                        }
                    }
                    return nRet;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public int ActiveOne(int ch, int m)
        {
            byte[] commandBytes = new byte[] { 0xab, 0xba, 0x03, 0x32, 0x00, 0x00 };
            commandBytes[4] = Convert.ToByte(ch - 1);
            commandBytes[5] = Convert.ToByte(m);
            return SendCommand(commandBytes);
        }

        public int ActiveFour(int m1, int m2, int m3, int m4)
        {
            byte[] commandBytes = new byte[] { 0xab, 0xba, 0x05, 0x34, 0x00, 0x00, 0x00, 0x00 };
            commandBytes[4] = Convert.ToByte(m1);
            commandBytes[5] = Convert.ToByte(m2);
            commandBytes[6] = Convert.ToByte(m3);
            commandBytes[7] = Convert.ToByte(m4);
            return SendCommand(commandBytes);
        }
        public int SetOne(int ch, int val)
        {
            byte[] commandBytes = new byte[] { 0xab, 0xba, 0x03, 0x31, 0x00, 0x00 };
            commandBytes[4] = Convert.ToByte(ch - 1);
            commandBytes[5] = Convert.ToByte(val);
            return SendCommand(commandBytes);
        }
        public int SetFour(int val1, int val2, int val3, int val4)
        {
            byte[] commandBytes = new byte[] { 0xab, 0xba, 0x05, 0x33, 0x00, 0x00, 0x00, 0x00 };
            commandBytes[4] = Convert.ToByte(val1);
            commandBytes[5] = Convert.ToByte(val2);
            commandBytes[6] = Convert.ToByte(val3);
            commandBytes[7] = Convert.ToByte(val4);
            return SendCommand(commandBytes);
        }
    }
}
