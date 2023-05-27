using Foxconn.App.Controllers.Socket;
using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using Foxconn.App.ViewModels;
using System;
using System.Threading.Tasks;
using static Foxconn.App.Models.RuntimeConfiguration.Position;

namespace Foxconn.App.Controllers.Server
{
    public class Server : TcpServer
    {
        private readonly MainWindow Root = MainWindow.Current;
        public TabHome TabHome { get; set; }
        public PlcConfiguration Plc { get; set; }
        public RobotConfiguration Robot { get; set; }
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
        public ModelName ModelName
        {
            get => _modelName;
            set => _modelName = value;
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
        private ModelName _modelName { get; set; }
        private ConnectionStatus _status { get; set; }
        private TestResult _testResult { get; set; }
        private DateTime _dateTime { get; set; }
        private int _counting { get; set; }
        private System.Timers.Timer _timer { get; set; }

        public Server() : base()
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

        public bool StartServer()
        {
            StartListen();
            Root.ShowMessage($"[Server {_index}] Waiting (localhost:{_port})");
            return true;
        }

        private void ErrorConnectionStatusEventHandler(ConnectionStatus status)
        {
            _status = status;
            string _message = "None";
            string _text = "None";
            AppColor _color = AppColor.None;
            switch (status)
            {
                case ConnectionStatus.None:
                    break;
                case ConnectionStatus.Waiting:
                    {
                        _message = $"[Server {_index}] Waiting for a connection (localhost:{_port})";
                        _text = "Waiting...";
                        _color = AppColor.Yellow;
                        Root.ShowMessage(_message, _color);
                        break;
                    }
                case ConnectionStatus.Connected:
                    {
                        _message = $"[Server {_index}] Connected (localhost:{_port})";
                        _text = "Connected";
                        _color = AppColor.Green;
                        Root.ShowMessage(_message, AppColor.None);
                        break;
                    }
                case ConnectionStatus.Disconnected:
                    {
                        // Test result is none when client disconnected
                        _testResult = TestResult.None;
                        _message = $"[Server {_index}] Disconnected (localhost:{_port})";
                        _text = "Disconnected";
                        _color = AppColor.Red;
                        Root.ShowMessage(_message, _color);
                        break;
                    }
                case ConnectionStatus.Error:
                    {
                        _message = $"[Server {_index}] Error (localhost:{_port})";
                        _text = "Error";
                        _color = AppColor.Red;
                        Root.ShowMessage(_message, _color);
                        break;
                    }
                default:
                    break;
            }
            //Root.ShowMessage(_message, _color);
            AppUi.ShowLabel(Root, TabHome.LabelHostPort, $"{_clientHost}:{_port}", AppColor.None, AppColor.Black);
            AppUi.ShowLabel(Root, TabHome.LabelStatus, _text, AppColor.None, _color);
        }

        private void ErrorEventHandler(Exception ex)
        {
            Logger.Instance.Write(ex.StackTrace);
        }

        private async void DataReceivedEventHandler(string data)
        {
            Root.ShowMessage($"[Server {_index}] Received ({_clientHost}:{_port}): {data}");
            switch (data)
            {
                case "INIT":
                    {
                        await Send($"{data}OK");
                        _counting = 0;
                        _dateTime = DateTime.Now;
                        _testResult = TestResult.Init;
                        AppUi.ShowLabel(Root, TabHome.LabelStatus, data, AppColor.None, AppColor.Blue);
                        break;
                    }
                case "PASS":
                    {
                        await Send($"{data}OK");
                        _counting = 0;
                        _dateTime = DateTime.Now;
                        _testResult = TestResult.Pass;
                        AppUi.ShowLabel(Root, TabHome.LabelStatus, data, AppColor.None, AppColor.Green);
                        break;
                    }
                case "FAIL":
                    {
                        await Send($"{data}OK");
                        _counting = 0;
                        _dateTime = DateTime.Now;
                        _testResult = TestResult.Fail;
                        AppUi.ShowLabel(Root, TabHome.LabelStatus, data, AppColor.None, AppColor.Red);
                        break;
                    }
                case "REPA":
                    {
                        await Send($"{data}OK");
                        _counting = 0;
                        _dateTime = DateTime.Now;
                        _testResult = TestResult.Repair;
                        AppUi.ShowLabel(Root, TabHome.LabelStatus, data, AppColor.None, AppColor.Orange);
                        break;
                    }
                case "1INIT2INIT":
                    {
                        await Send($"{data}OK");
                        _counting = 0;
                        _dateTime = DateTime.Now;
                        _testResult = TestResult.InitInit;
                        AppUi.ShowLabel(Root, TabHome.LabelStatus, data, AppColor.None, AppColor.Blue);
                        break;
                    }
                case "1PASS2PASS":
                    {
                        await Send($"{data}OK");
                        _counting = 0;
                        _dateTime = DateTime.Now;
                        _testResult = TestResult.PassPass;
                        AppUi.ShowLabel(Root, TabHome.LabelStatus, data, AppColor.None, AppColor.Green);
                        break;
                    }
                case "1FAIL2FAIL":
                    {
                        await Send($"{data}OK");
                        _counting = 0;
                        _dateTime = DateTime.Now;
                        _testResult = TestResult.FailFail;
                        AppUi.ShowLabel(Root, TabHome.LabelStatus, data, AppColor.None, AppColor.Red);
                        break;
                    }
                case "1PASS2FAIL":
                    {
                        await Send($"{data}OK");
                        _counting = 0;
                        _dateTime = DateTime.Now;
                        _testResult = TestResult.PassFail;
                        AppUi.ShowLabel(Root, TabHome.LabelStatus, data, AppColor.None, AppColor.Mint);
                        break;
                    }
                case "1FAIL2PASS":
                    {
                        await Send($"{data}OK");
                        _counting = 0;
                        _dateTime = DateTime.Now;
                        _testResult = TestResult.FailPass;
                        AppUi.ShowLabel(Root, TabHome.LabelStatus, data, AppColor.None, AppColor.Mint);
                        break;
                    }
                case "LOCKED":
                    {
                        await Send($"{data}OK");
                        _counting = 0;
                        _dateTime = DateTime.Now;
                        _testResult = TestResult.Locked;
                        AppUi.ShowLabel(Root, TabHome.LabelStatus, data, AppColor.None, AppColor.Red);
                        break;
                    }
                case "OPEN_FIXTUREOK":
                    break;
                case "OPEN_FIXTURENG":
                    break;
                case "CLOSE_FIXTUREOK":
                    break;
                case "CLOSE_FIXTURENG":
                    break;
                default:
                    {
                        if (!(data.Contains("RUN") && (data.Contains("OK") || data.Contains("NG"))))
                        {
                            await Send($"WRONG_FORMAT");
                        }
                        break;
                    }
            }
            AppUi.ShowLabel(Root, TabHome.LabelTime, $"Waiting Time: {_counting} (s)", AppColor.None, AppColor.Black);
        }

        public new async Task<bool> Send(string data)
        {
            if (_isConnected)
            {
                await base.Send(data);
                Root.ShowMessage($"[Server {_index}] Sent ({_clientHost}:{_port}): {data}");
                return true;
            }
            else
            {
                Root.ShowMessage($"[Server {_index}] Cannot send to ({_clientHost}:{_port}): {data}", AppColor.Red);
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
            AppUi.ClearImageBox(Root, Root.imbCamera0);
        }
    }
}
