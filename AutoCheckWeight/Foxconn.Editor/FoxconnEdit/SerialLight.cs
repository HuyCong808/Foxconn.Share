using NLog;
using System;
using System.IO.Ports;
using System.Threading;

namespace Foxconn.Editor.FoxconnEdit
{
    public class SerialLight
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly object _lockObj = new object();
        private SerialPort _serialPort { get; set; }
        private string _portName { get; set; }
        private bool _isConnected { get; set; }
        private string _dataReceive { get; set; }

        public SerialLight()
        {
            _serialPort = new SerialPort();
            _portName = string.Empty;
            _isConnected = false;
            _dataReceive = string.Empty;
        }

        public string PortName => _portName;
        public bool IsConnected => _isConnected;
        public string DataReceive => _dataReceive;

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
                    logger.Info($"SerialLightt.Open ({_portName}): Opened ");
                    return 1;
                }
                else
                {
                    logger.Info(($"SerialLight.Open ({_portName}): Can not Open"));
                }
                return 0;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return -1;
            }
        }

        public void Close()
        {
            try
            {
                _isConnected = false;
                _dataReceive = String.Empty;
                _serialPort.DataReceived -= new SerialDataReceivedEventHandler(SerialDataReceived);
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                _serialPort.Dispose();
                _serialPort.Close();
                logger.Info($"SerialLight.Close ({_portName}): Closed");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public SerialPort GetSerialPort() => _serialPort;

        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = _serialPort.ReadExisting().Trim();
                if (data.Length > 0) ;
                {
                    _dataReceive = data;
                    _serialPort.DiscardInBuffer();
                    logger.Info($"SerialLight.SerialDataReceived ({_portName}): {data}");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public int SendData(string data)
        {
            try
            {
                _serialPort.DiscardOutBuffer();
                _serialPort.WriteLine(data);
                logger.Info($"SerialLight.SendData ({_portName}) : {data}");
                return 1;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return -1;
            }
        }

        public int SendCommand(byte[] CommandBytes, int timeOut = 2000)
        {
            lock (_lockObj)
            {
                int loop = timeOut / 50;
                int result = -1;
                try
                {
                    _serialPort.Write(CommandBytes, 0, CommandBytes.Length);
                    for (int i = 0; i < loop; i++)
                    {
                        Thread.Sleep(50);
                        string temp = _serialPort.ReadExisting();
                        if (temp.Contains("ok"))
                        {
                            result = 1;
                            break;
                        }
                    }
                }
                catch
                {
                    result = -1;
                }
                return result;
            }
        }

        public int SetBrightness(int channel, int value)
        {
            byte[] commandBytes = { 0xab, 0xba, 0x03, 0x31, (byte)channel, (byte)value };
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
