using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace Foxconn.Editor
{
    public class SerialLight
    {
        private readonly object _lockObj = new object();
        private SerialPort _serialPort = null;
        private string _portName = string.Empty;
        private bool _isConnected = false;
        private string _dataReceived = string.Empty;

        public string PortName => _portName;

        public bool IsConnected => _isConnected;

        public string DataReceived => _dataReceived;

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
                    Trace.WriteLine($"SerialClient.Open ({_portName}): Opened");
                    return 1;
                }
                else
                {
                    Trace.WriteLine($"SerialClient.Open ({_portName}): Cannot open");
                }
                return 0;
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
                _serialPort.DataReceived -= new SerialDataReceivedEventHandler(SerialDataReceived);
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                _serialPort.Close();
                _serialPort.Dispose();
                Trace.WriteLine($"SerialClient.Close ({_portName}): Closed");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public SerialPort GetSerialPort() => _serialPort;


        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = _serialPort.ReadExisting().Trim();
                if (data.Length > 0)
                {
                    _dataReceived = data;
                    _serialPort.DiscardInBuffer();
                    Trace.WriteLine($"SerialClient.SerialDataReceived ({_portName}): {data}");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public int SerialWriteData(string data)
        {
            try
            {
                _serialPort.DiscardOutBuffer();
                _serialPort.WriteLine(data);
                Trace.WriteLine($"SerialClient.SerialWriteData ({_portName}): {data}");
                return 1;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }

        private int SendCommand(byte[] commandBytes, int timeout = 2000)
        {
            try
            {
                int nRet = -1;
                lock (_lockObj)
                {
                    _serialPort.Write(commandBytes, 0, commandBytes.Length);
                    int loop = timeout / 25;
                    for (int i = 0; i < loop; i++)
                    {
                        string temp = _serialPort.ReadExisting();
                        if (temp.Contains("ok"))
                        {
                            return 1;
                        }
                        Thread.Sleep(25);
                    }
                    return nRet;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public int SetBrightness(int ch, int val)
        {
            byte[] commandBytes = { 0xab, 0xba, 0x03, 0x31, (byte)ch, (byte)val };
            return SendCommand(commandBytes);
        }

        public int TurnOnLight()
        {
            byte[] commandBytes = { 0xab, 0xba, 0x05, 0x33, 0xff, 0xff, 0xff, 0xff };
            return SendCommand(commandBytes);
        }

        public int TurnOffLight()
        {
            byte[] commandBytes = { 0xab, 0xba, 0x05, 0x33, 0x00, 0x00, 0x00, 0x00 };
            return SendCommand(commandBytes);
        }

        public int SetBrightnessAll(int ch1, int ch2, int ch3, int ch4)
        {
            byte[] commandBytes = { 0xab, 0xba, 0x05, 0x33, (byte)ch1, (byte)ch2, (byte)ch3, (byte)ch4 };
            return SendCommand(commandBytes);
        }

        public int SetBrightnessAll(int value)
        {
            byte[] commandBytes = { 0xab, 0xba, 0x05, 0x33, (byte)value, (byte)value, (byte)value, (byte)value };
            return SendCommand(commandBytes);
        }
    }
}
