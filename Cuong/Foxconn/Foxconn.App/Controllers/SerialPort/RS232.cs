using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace Foxconn.App.Controllers.SerialPort
{
    public class RS232
    {
        private enum Buffer
        {
            In,
            Out,
            All,
        }
        public delegate void ErrorEvent(Exception ex);
        public delegate void DataReceivedEvent(string data);
        private readonly object _syncObject = new object();
        public ErrorEvent InvokeError { get; set; }
        public DataReceivedEvent InvokeDataReceived { get; set; }
        public string DataReceived
        {
            get => _dataReceived;
            set => _dataReceived = value;
        }
        protected string _portName { get; set; }
        protected bool _isConnected { get; set; }
        private System.IO.Ports.SerialPort _serialPort { get; set; }
        private List<string> _portNames { get; set; }
        private string _dataReceived { get; set; }
        private DateTime _dateTime { get; set; }
        private int _readTimeout { get; set; }
        private int _writeTimeout { get; set; }

        public RS232()
        {
            //_serialPort = new System.IO.Ports.SerialPort();
            _portNames = new List<string>();
            _portNames = System.IO.Ports.SerialPort.GetPortNames().ToList();
            _portName = string.Empty;
            _isConnected = false;
            _dataReceived = string.Empty;
            _dateTime = DateTime.Now;
            _readTimeout = 500;
            _writeTimeout = 500;
        }

        public bool Connect()
        {
            try
            {
                if (_isConnected)
                {
                    return true;
                }
                else
                {
                    // Create a new SerialPort object with default settings.
                    _serialPort = new System.IO.Ports.SerialPort
                    {
                        // Allow the user to set the appropriate properties.
                        PortName = _portName,
                        BaudRate = 9600,
                        Parity = Parity.None,
                        DataBits = 8,
                        StopBits = StopBits.One,
                        Handshake = Handshake.None,
                        // Set the read/write timeouts
                        ReadTimeout = _writeTimeout,
                        WriteTimeout = _readTimeout
                    };

                    // Open
                    if (_portNames.Contains(_portName))
                    {
                        _serialPort.Open();
                        _isConnected = _serialPort.IsOpen;
                        // Event
                        if (_isConnected)
                        {
                            Console.WriteLine($"Connected {_portName}!");
                            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedEventHandler);
                        }
                        else
                        {
                            Console.WriteLine($"Cannot connect to {_portName}");
                        }
                        Discards(Buffer.All);
                    }
                    else
                    {
                        _isConnected = false;
                    }

                }
                return _isConnected;
            }
            catch (Exception ex)
            {
                ErrorDetails(ex);
                return false;
            }
        }

        public bool Connect(string portName)
        {
            try
            {
                if (_isConnected)
                {
                    return true;
                }
                else
                {
                    _portName = portName;

                    // Create a new SerialPort object with default settings.
                    _serialPort = new System.IO.Ports.SerialPort
                    {
                        // Allow the user to set the appropriate properties.
                        PortName = portName,
                        BaudRate = 9600,
                        Parity = Parity.None,
                        DataBits = 8,
                        StopBits = StopBits.One,
                        Handshake = Handshake.None,
                        // Set the read/write timeouts
                        ReadTimeout = _writeTimeout,
                        WriteTimeout = _readTimeout
                    };

                    // Open
                    if (_portNames.Contains(portName))
                    {
                        _serialPort.Open();
                        _isConnected = _serialPort.IsOpen;
                        // Event
                        if (_isConnected)
                        {
                            Console.WriteLine($"Connected {portName}!");
                            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedEventHandler);
                        }
                        else
                        {
                            Console.WriteLine($"Cannot connect to {portName}");
                        }
                        Discards(Buffer.All);
                    }
                    else
                    {
                        _isConnected = false;
                    }

                }
                return _isConnected;
            }
            catch (Exception ex)
            {
                ErrorDetails(ex);
                return false;
            }
        }

        public bool Connect(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake)
        {
            try
            {
                if (_isConnected)
                {
                    return true;
                }
                else
                {
                    _portName = portName;

                    // Create a new SerialPort object with default settings.
                    _serialPort = new System.IO.Ports.SerialPort
                    {
                        // Allow the user to set the appropriate properties.
                        PortName = portName,
                        BaudRate = baudRate,
                        Parity = parity,
                        DataBits = dataBits,
                        StopBits = stopBits,
                        Handshake = handshake,
                        // Set the read/write timeouts
                        ReadTimeout = _writeTimeout,
                        WriteTimeout = _readTimeout
                    };

                    // Open
                    if (_portNames.Contains(portName))
                    {
                        _serialPort.Open();
                        _isConnected = _serialPort.IsOpen;
                        // Event
                        if (_isConnected)
                        {
                            Console.WriteLine($"Connected {portName}!");
                            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedEventHandler);
                        }
                        else
                        {
                            Console.WriteLine($"Cannot connect to {portName}");
                        }
                        Discards(Buffer.All);
                    }
                    else
                    {
                        _isConnected = false;
                    }

                }
                return _isConnected;
            }
            catch (Exception ex)
            {
                ErrorDetails(ex);
                return false;
            }
        }

        public void Send(string data)
        {
            if (_isConnected)
            {
                Discards(Buffer.Out);
                _serialPort.Write(data + "\r\n");
                Console.WriteLine($"Sent {_portName}: {data}");
            }
        }

        private void DataReceivedEventHandler(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (System.IO.Ports.SerialPort)sender;
            var data = serialPort.ReadExisting().Trim();
            if (data.Length > 0)
            {
                _dataReceived = data;
                InvokeDataReceived?.Invoke(data);
                Discards(Buffer.In);
                Console.WriteLine($"Received {_portName}: {data}");
            }
        }

        public void Disconnect()
        {
            if (_isConnected)
            {
                _serialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedEventHandler);
                _serialPort.Close();
                Discards(Buffer.All);
                _isConnected = false;
            }
        }

        public void Release()
        {
            Disconnect();
            _serialPort?.Dispose();
        }

        private void Discards(Buffer buffer)
        {
            switch (buffer)
            {
                case Buffer.In:
                    // Discards data from the serial driver's receive buffer.
                    _serialPort.DiscardInBuffer();
                    break;
                case Buffer.Out:
                    // Discards data from the serial driver's transmit buffer.
                    _serialPort.DiscardOutBuffer();
                    break;
                case Buffer.All:
                    // Discards all data
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();
                    break;
                default:
                    break;
            }
        }

        private void ErrorDetails(Exception ex)
        {
            InvokeError?.Invoke(ex);
        }

        private void ErrorDetails(string msg)
        {
            InvokeError?.Invoke(new Exception(msg));
        }
    }
}
