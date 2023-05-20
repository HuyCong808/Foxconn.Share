using System;
using System.IO.Ports;
using System.Windows;

namespace Foxconn.AutoWeight.FoxconnEdit
{
    public class SerialPortClient
    {
        private SerialPort _serialPort { get; set; }
        private bool _isConnected { get; set; }
        public string DataReceive { get; set; }

        public SerialPortClient()
        {
            _serialPort = new SerialPort();
            _isConnected = false;
            DataReceive = string.Empty;
        }
        public bool IsConnected
        {
            get => _isConnected;
            set => _isConnected = value;
        }

        public int Open(string portName)
        {
            try
            {
                _serialPort.PortName = portName;
                _serialPort.BaudRate = 9600;
                _serialPort.Parity = Parity.None;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialDataReceived);
                _serialPort.Open();
                if (_serialPort.IsOpen)
                {
                    _isConnected = true;
                    return 1;
                }
                else
                {
                    _isConnected = false;
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.ToString());
                return -1;
            }
        }

        public void Close()
        {
            try
            {
                _isConnected = false;
                DataReceive = string.Empty;
                _serialPort.DataReceived -= new SerialDataReceivedEventHandler(SerialDataReceived);
                _serialPort.Close();
                _serialPort.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.ToString());
            }
        }

        public int SendData(string data)
        {
            try
            {
                _serialPort.WriteLine(data);
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.ToString());
                return 0;
            }
        }

        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = _serialPort.ReadExisting().Trim();
                if (data.Length > 0)
                {
                    DataReceive = data;
                    Console.WriteLine("Recieve from SerialPort: " + data);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
