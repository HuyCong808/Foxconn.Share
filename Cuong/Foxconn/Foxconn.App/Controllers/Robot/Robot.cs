using Foxconn.App.Controllers.Socket;
using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using System;
using System.Threading.Tasks;

namespace Foxconn.App.Controllers.Robot
{
    public class Robot : TcpClient
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
        public ConnectionStatus Status
        {
            get => _status;
            set => _status = value;
        }
        public TestResult TestResult => _testResult;
        public DateTime DateTime => _dateTime;
        private int _index { get; set; }
        private string _alias { get; set; }
        private ConnectionStatus _status { get; set; }
        private TestResult _testResult { get; set; }
        private DateTime _dateTime { get; set; }

        public Robot()
        {
            _status = ConnectionStatus.None;
            _testResult = TestResult.None;
            _dateTime = DateTime.Now;
            InvokeStatus += ErrorConnectionStatusEventHandler;
            InvokeError += ErrorEventHandler;
            InvokeDataReceived += DataReceivedEventHandler;
        }

        public bool StartRobot()
        {
            if (IsPingSuccessAsync())
            {
                if (Connect())
                {
                    _status = ConnectionStatus.Connected;
                    //Root.ShowMessage($"[Robot {_index}] Connected ({_host}:{_port})");
                }
            }
            else
            {
                _status = ConnectionStatus.Disconnected;
                Root.ShowMessage($"[Robot {_index}] Cannot ping to ({_host}:{_port})", AppColor.Red);
            }
            return _status == ConnectionStatus.Connected;
        }

        public void CheckConnection()
        {
            if (IsPingSuccessAsync())
            {
                if (_status == ConnectionStatus.Disconnected)
                {
                    Root.ShowMessage($"[Robot {_index}] Connected ({_host}:{_port})");
                }
                _status = ConnectionStatus.Connected;
            }
            else
            {
                _status = ConnectionStatus.Disconnected;
                Root.ShowMessage($"[Robot {_index}] Cannot ping to ({_host}:{_port})", AppColor.Red);
            }
        }

        private void ErrorConnectionStatusEventHandler(ConnectionStatus status)
        {
            _status = status;
            var _message = "None";
            var _text = "None";
            var _color = AppColor.None;
            switch (status)
            {
                case ConnectionStatus.None:
                    break;
                case ConnectionStatus.Waiting:
                    {
                        _message = $"[Robot {_index}] Waiting ({_host}:{_port})";
                        _text = "Waiting...";
                        _color = AppColor.Yellow;
                        break;
                    }
                case ConnectionStatus.Connected:
                    {
                        _message = $"[Robot {_index}] Connected ({_host}:{_port})";
                        _text = "Connected";
                        _color = AppColor.None;
                        break;
                    }
                case ConnectionStatus.Disconnected:
                    {
                        _message = $"[Robot {_index}] Disconnected ({_host}:{_port})";
                        _text = "Disconnected";
                        _color = AppColor.Red;
                        break;
                    }
                case ConnectionStatus.Error:
                    {
                        _message = $"[Robot {_index}] Error ({_host}:{_port})";
                        _text = "Error";
                        _color = AppColor.Red;
                        break;
                    }
                default:
                    break;
            }
            Root.ShowMessage(_message, _color);
        }

        private void ErrorEventHandler(Exception ex)
        {
            Logger.Instance.Write(ex.StackTrace);
        }

        private void DataReceivedEventHandler(string data)
        {
            _dateTime = DateTime.Now;
            if (data.Contains("ERROR:"))
            {
                Root.ShowMessage($"[Robot {_index}] Received ({_host}:{_port}): {data}", AppColor.Red);
            }
            else
            {
                Root.ShowMessage($"[Robot {_index}] Received ({_host}:{_port}): {data}");
            }
        }

        public new async Task<bool> Send(string data)
        {
            if (_isConnected)
            {
                await base.Send(data);
                Root.ShowMessage($"[Robot {_index}] Sent ({_host}:{_port}): {data}");
                return true;
            }
            else
            {
                Root.ShowMessage($"[Robot {_index}] Cannot send to ({_host}:{_port}): {data}", AppColor.Red);
                return false;
            }
        }
    }
}
