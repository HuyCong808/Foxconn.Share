using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using System;

namespace Foxconn.App.Controllers.SerialPort
{
    public class SerialPort : RS232
    {
        private readonly MainWindow Root = MainWindow.Current;
        public int Index
        {
            get => _index;
            set => _index = value;
        }
        public string Alias
        {
            get => _alias;
            set => _alias = value;
        }
        public string PortName
        {
            get => _portName;
            set => _portName = value;
        }
        public ConnectionStatus Status
        {
            get => _status;
            set => _status = value;
        }
        private int _index { get; set; }
        private string _alias { get; set; }
        private ConnectionStatus _status { get; set; }

        public SerialPort() : base()
        {
            _status = ConnectionStatus.None;
            InvokeError += ErrorEventHandler;
            InvokeDataReceived += DataReceivedEventHandler;
        }

        public bool StartSerialPort()
        {
            if (Connect())
            {
                _status = ConnectionStatus.Connected;
                Root.ShowMessage($"[Serial Port {_index}] Connected ({_portName})");
            }
            else
            {
                _status = ConnectionStatus.Disconnected;
                Root.ShowMessage($"[Serial Port {_index}] Cannot connect ({_portName})", AppColor.Red);
            }
            return _status == ConnectionStatus.Connected;
        }

        private void ErrorEventHandler(Exception ex)
        {
            Logger.Instance.Write(ex.StackTrace);
        }

        private void DataReceivedEventHandler(string data)
        {
            Root.ShowMessage($"[Serial Port {_index}] Received ({_portName}): {data}");
        }

        public new void Send(string data)
        {
            if (_isConnected)
            {
                base.Send(data);
                Root.ShowMessage($"[Serial Port {_index}] Sent ({_portName}): {data}");
            }
            else
            {
                Root.ShowMessage($"[Serial Port {_index}] Cannot send to (({_portName})): {data}.", AppColor.Red);
            }
        }
    }
}
