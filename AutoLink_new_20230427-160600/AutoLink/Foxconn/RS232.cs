using System;
using System.IO.Ports;

namespace Foxconn
{
    public class RS232
    {
        public bool IsConnected { get; set; }
        public string DataReceived { get; set; }
        private SerialPort mySerialPort { get; set; }

        public RS232()
        {
            IsConnected = false;
            DataReceived = string.Empty;
            mySerialPort = new SerialPort();
        }

        public bool Open(string portName)
        {
            try
            {
                mySerialPort.PortName = portName;
                mySerialPort.BaudRate = 9600;
                mySerialPort.Parity = Parity.None;
                mySerialPort.StopBits = StopBits.One;
                mySerialPort.DataBits = 8;
                mySerialPort.Handshake = Handshake.None;
                mySerialPort.RtsEnable = true;
                mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                mySerialPort.Open();
                if (mySerialPort.IsOpen)
                {
                    IsConnected = true;
                    return true;
                }
                else
                {
                    IsConnected = false;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool Send(string data)
        {
            if (IsConnected)
            {
                mySerialPort.WriteLine(data);
            }
            return true;
        }

        public bool Close()
        {
            mySerialPort.Close();
            return true;
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            Console.WriteLine($"Data Received: {indata}");
            if (indata.Length > 0)
            {
                DataReceived = indata.Trim();
            }
        }
    }
}
