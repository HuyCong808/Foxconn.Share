using Foxconn.App.Controllers.Socket;
using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using Foxconn.App.ViewModels;
using System;
using System.Threading.Tasks;

namespace Foxconn.App.Controllers.Client
{
    public class Client : TcpClient
    {
        private readonly MainWindow Root = MainWindow.Current;
        public TabHome TabHome { get; set; }
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
        private int _counting { get; set; }
        private System.Timers.Timer _timer { get; set; }

        public Client() : base()
        {
            _status = ConnectionStatus.None;
            _testResult = TestResult.None;
            _dateTime = DateTime.Now;
            _counting = 0;
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OntimedEvent;
            _timer.Enabled = true;
            //_timer?.Stop();
            //_timer?.Dispose();
            InvokeStatus += ErrorConnectionStatusEventHandler;
            InvokeError += ErrorEventHandler;
            InvokeDataReceived += DataReceivedEventHandler;
        }

        public bool StartClient()
        {
            if (IsPingSuccessAsync())
            {
                if (Connect())
                {
                    _status = ConnectionStatus.Connected;
                    //Root.ShowMessage($"[Client {_index}] Connected ({_host}:{_port})", AppColor.Green);
                }
            }
            else
            {
                _status = ConnectionStatus.Disconnected;
                Root.ShowMessage($"[Client {_index}] Cannot ping to ({_host}:{_port})", AppColor.Red);
            }
            return _status == ConnectionStatus.Connected;
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
                        _message = $"[Client {_index}] Waiting ({_host}:{_port})";
                        _text = "Waiting...";
                        _color = AppColor.Yellow;
                        Root.ShowMessage(_message, _color);
                        break;
                    }
                case ConnectionStatus.Connected:
                    {
                        _message = $"[Client {_index}] Connected ({_host}:{_port})";
                        _text = "Connected";
                        _color = AppColor.Green;
                        Root.ShowMessage(_message, AppColor.None);
                        break;
                    }
                case ConnectionStatus.Disconnected:
                    {
                        _message = $"[Client {_index}] Disconnected ({_host}:{_port})";
                        _text = "Disconnected";
                        _color = AppColor.Red;
                        Root.ShowMessage(_message, _color);
                        break;
                    }
                case ConnectionStatus.Error:
                    {
                        _message = $"[Client {_index}] Error ({_host}:{_port})";
                        _text = "Error";
                        _color = AppColor.Red;
                        Root.ShowMessage(_message, _color);
                        break;
                    }
                default:
                    break;
            }
            //Root.ShowMessage(_message, _color);
            AppUi.ShowLabel(Root, TabHome.LabelHostPort, $"{_host}:{_port}", AppColor.None, AppColor.Black);
            AppUi.ShowLabel(Root, TabHome.LabelStatus, _text, AppColor.None, _color);
        }

        private void ErrorEventHandler(Exception ex)
        {
            Logger.Instance.Write(ex.StackTrace);
        }

        private void DataReceivedEventHandler(string data)
        {
            Root.ShowMessage($"[Client {_index}] Received ({_host}:{_port}): {data}");
        }

        public new async Task<bool> Send(string data)
        {
            if (_isConnected)
            {
                await base.Send(data);
                Root.ShowMessage($"[Client {_index}] Sent ({_host}:{_port}): {data}");
                return true;
            }
            else
            {
                Root.ShowMessage($"[Client {_index}] Cannot send to ({_host}:{_port}): {data}", AppColor.Red);
                return false;
            }
        }

        private void OntimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            _counting++;
            string message;
            if (_testResult == TestResult.Testing)
            {
                message = $"Testing Time: {_counting} (s)";
            }
            else
            {
                message = $"Waiting Time: {_counting} (s)";
            }
            AppColor color;
            if (_counting >= 600 && _counting % 2 == 1)
            {
                color = AppColor.Red;
            }
            else
            {
                color = AppColor.Black;
            }
            AppUi.ShowLabel(Root, TabHome.LabelTime, message, AppColor.None, color);
        }

        public void TerminateTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }

        public void StartTesting()
        {
            _counting = 0;
            _testResult = TestResult.Testing;
            AppUi.ShowLabel(Root, TabHome.LabelStatus, "TESTING", AppColor.None, AppColor.Yellow);
            AppUi.ShowLabel(Root, TabHome.LabelTime, $"Testing Time: {_counting} (s)", AppColor.None, AppColor.Black);
            AppUi.ShowLabel(Root, Root.lblFlow, "-----", AppColor.None, AppColor.Black);
        }
    }
}
