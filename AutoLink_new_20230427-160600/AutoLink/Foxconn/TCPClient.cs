using NLog;
using System;
using System.Net.Sockets;

namespace Foxconn
{
    class TCPClient
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private TcpClient _client;
        private string _dataReceived = string.Empty;
        

        public string DataReceived
        {
            get => _dataReceived;
            set => _dataReceived = value;
        }

        public TCPClient()
        {
            _client = new TcpClient();
        }

        public bool Connect(string ip, int port)
        {
            try
            {
                _client.Connect(ip, port);
                if (_client.Connected)
                {
                    // Thread
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

        }

        public void Send(string message)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
            NetworkStream stream = _client.GetStream();
            stream.Write(data, 0, data.Length);
            logger.Info("Sent: {0}", message);
            Console.WriteLine("Sent: {0}", message);
        }

        public void Receive1()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        if (_client.Connected)
                        {
                            NetworkStream stream = _client.GetStream();
                            byte[] data = new byte[256];
                            int bytes = stream.Read(data, 0, data.Length);
                            if (bytes == 0)
                            {
                                Close();
                            }
                            else
                            {
                                string response = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                                if (response.Length > 0)
                                {
                                    _dataReceived = response;
                                    logger.Info("Received: {0}", response);
                                    Console.WriteLine("Received: {0}", response);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        public string Receive()
        {
            NetworkStream stream = _client.GetStream();
            byte[] data = new byte[256];
            int bytes = stream.Read(data, 0, data.Length);
            string response = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Console.WriteLine("Received:{0}", response);
            return response;
        }

        public void Disconnect()
        {
            _client.Close();
        }

        public void Close()
        {
            _client.Close();
        }
    }
}
