using System;
using System.IO.Ports;
using System.Threading;

namespace Foxconn.Editor
{
    public class SerialPortClient
    {
        private SerialPort _serialPort = null;
        private string _portName = string.Empty;
        private bool _isConnected = false;
        private string _dataReceived = string.Empty;

        public string PortName
        {
            get => _portName;
            set => _portName = value;
        }
        public bool IsConnected
        {
            get => _isConnected;
            set => _isConnected = value;
        }

        public string DataReceived
        {
            get => _dataReceived;
            set => _dataReceived = value;
        }
        public SerialPortClient()
        {
            _serialPort = new SerialPort();
            _isConnected = false;
            _dataReceived = string.Empty;
        }

        public int Open(string portName)
        {
            try
            {
                _portName = portName;
                _serialPort = new SerialPort(portName);
                _serialPort.BaudRate = 9600;
                _serialPort.Parity = Parity.None;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;
                _serialPort.ReadTimeout = 500;
                _serialPort.WriteTimeout = 500;
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialDataReceived);
                _serialPort.Open();
                if (_serialPort.IsOpen)
                {
                    _isConnected = true;
                    LogInfo($"SerialClient.Open ({_portName}): Opened");
                    return 1;
                }
                else
                {
                    _isConnected = false;
                    LogInfo($"SerialClient.Open ({_portName}): Can not opened");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                return -1;
            }
        }

        public void Close()
        {
            try
            {
                _isConnected = false;
                _dataReceived = string.Empty;
                _serialPort.DataReceived -= new SerialDataReceivedEventHandler(SerialDataReceived);
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                _serialPort.Close();
                _serialPort.Dispose();
                LogInfo($"SerialClient.Close ({_portName}): Closed");
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        public SerialPort GetSerialPort() => _serialPort;

        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // string data = _serialPort.ReadExisting();
                //string data = _serialPort.ReadLine();
                string data = _serialPort.ReadExisting().Trim();
                if (data.Length > 0)
                {
                    _dataReceived = data;
                    _serialPort.DiscardInBuffer();
                    LogInfo($"SerialClient.SerialDataReceived ({_portName}): {data}");
                }
            }
            catch (Exception ex)
            {
                LogError("SerialClient.SerialDataReceived: " + ex.Message);
            }
        }

        public int SerialWriteData(string data)
        {
            try
            {
                _dataReceived = string.Empty;
                _serialPort.DiscardOutBuffer();
                _serialPort.WriteLine(data);
                LogInfo($"SerialClient.SerialWriteData ({_portName}): {data}");
                return 1;
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                return -1;
            }
        }

        public int SerialWriteData(string data, string responseData = "", int timeout = 10000)
        {
            try
            {
                _dataReceived = string.Empty;
                _serialPort.DiscardOutBuffer();
                _serialPort.WriteLine(data + "\r\n");
                LogInfo($"SerialClient.SerialWriteData ({_portName}): {data}");
                if (responseData != "")
                {
                    for (int i = 0; i < timeout / 400; i++)
                    {
                        if (responseData == _dataReceived)
                        {
                            return 1;
                        }
                        Thread.Sleep(25);
                    }
                    return -1;
                }
                return 1;
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                return -1;
            }
        }

        public void LogInfo(string message)
        {
            Logger.Current.Info(message);
            //Console.WriteLine(message);
        }

        public void LogError(string message)
        {
            Logger.Current.Info(message);
            //Console.WriteLine(message);
        }

    }
}
