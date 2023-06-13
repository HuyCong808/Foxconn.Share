using System;
using System.IO.Ports;
using System.Windows;

namespace Foxconn.Editor
{
    public class SerialPortClient
    {
        private SerialPort _serialPort { get; set; }
        private string _portName { get;set; }
        private bool _isConnected { get; set; }
        private string _dataReceived { get; set; }

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

        public string DataReceive
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
                    Logger.Current.Info($"SerialClient.Open ({_portName}): Opened");
                    return 1;
                }
                else
                {
                    _isConnected = false;
                    Logger.Current.Info($"SerialClient.Open ({_portName}): Can not opened");
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
                _serialPort.DataReceived -= new SerialDataReceivedEventHandler(SerialDataReceived);
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                _serialPort.Close();
                _serialPort.Dispose();
                Logger.Current.Info($"SerialClient.Close ({_portName}): Closed");
            }
            catch (Exception ex)
            {
                Logger.Current.Error(ex.Message);
            }
        }

        public int SerialWriteData(string data)
        {
            try
            {
                _dataReceived = string.Empty;
                _serialPort.DiscardOutBuffer();
                _serialPort.WriteLine(data + "\r\n");
                 Logger.Current.Info($"SerialClient.SerialWriteData ({_portName}): {data}");
                return 1;
            }
            catch (Exception ex)
            {
                Logger.Current.Error(ex.Message);
                return -1;
            }
        }

        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = _serialPort.ReadExisting().Trim();
                if (data.Length > 0)
                {
                    _dataReceived = data;
                    _serialPort.DiscardInBuffer();
                    Logger.Current.Info($"SerialClient.SerialDataReceived ({_portName}): {data}");
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error(ex.Message);
            }
        }



    }
}
