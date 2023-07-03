using Foxconn.Editor.Controls;
using Foxconn.Editor.Enums;
using System;
using System.Windows;
using SystemColors = Foxconn.Editor.Enums.SystemColors;

namespace Foxconn.Editor
{
    public class SocketTest : SocketServer
    {
        private int _id = -1;
        private TestModel _model = TestModel.Unknow;
        private TestResult _result = TestResult.Unknow;
        private GoldenSampleType _goldenSample = GoldenSampleType.Unknow;
        private DateTime _time = DateTime.Now;
        private System.Timers.Timer _timerCount = null;
        private int _numCount = 0;
        private SocketTestUI _socketUI = null;

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public TestModel Model
        {
            get => _model;
            set => _model = value;
        }

        public TestResult Result => _result;

        public GoldenSampleType GoldenSample => _goldenSample;

        public DateTime Time => _time;

        public SocketTestUI SocketUI
        {
            get => _socketUI;
            set => _socketUI = value;
        }

        public SocketTest() : base()
        {
            _id = -1;
            _model = TestModel.Unknow;
            _result = TestResult.Unknow;
            _goldenSample = GoldenSampleType.Unknow;
            _time = DateTime.Now;
            _timerCount = new System.Timers.Timer(1000);
            _timerCount.Elapsed += OntimedEvent;
            _timerCount.Enabled = true;
            InvokeInfo += InfoEventHandler;
            InvokeDataReceived += DataReceivedEventHandler;
        }

        public void Dispose()
        {
            _timerCount?.Dispose();
            Close();
        }

        private void InfoEventHandler(SocketStatus status, string remoteAddress)
        {
            switch (status)
            {
                case SocketStatus.Unknow:
                    {
                        ShowSocketInfo($"{remoteAddress}:{Port}", SystemColors.White);
                        break;
                    }
                case SocketStatus.Waiting:
                    {
                        ShowSocketInfo($"{remoteAddress}:{Port}", SystemColors.White);
                        break;
                    }
                case SocketStatus.Connected:
                    {
                        _numCount = 0;
                        _result = TestResult.Unknow;
                        _time = DateTime.Now;
                        ShowSocketInfo($"{remoteAddress}:{Port}", SystemColors.Green);
                        ShowSocketStatus("-----", SystemColors.White);
                        ClearGoldenSample();
                        break;
                    }
                case SocketStatus.Disconnected:
                    {
                        _numCount = 0;
                        _result = TestResult.Unknow;
                        _time = DateTime.Now;
                        ShowSocketInfo($"{remoteAddress}:{Port}", SystemColors.Red);
                        ShowSocketStatus("-----", SystemColors.White);
                        ClearGoldenSample();
                        break;
                    }
                default:
                    break;
            }
        }

        private void DataReceivedEventHandler(string data)
        {
            switch (data)
            {
                case "INIT":
                    {
                        if (SocketWriteData(data + "OK") == 1)
                        {
                            _numCount = 0;
                            _time = DateTime.Now;
                            _result = TestResult.S_1INIT;
                            ShowSocketStatus("INIT", SystemColors.Cyan);
                        }
                        break;
                    }
                case "PASS":
                    {
                        if (SocketWriteData(data + "OK") == 1)
                        {
                            _numCount = 0;
                            _time = DateTime.Now;
                            _result = TestResult.S_1PASS;
                            ShowSocketStatus("PASS", SystemColors.Green);
                        }
                        break;
                    }
                case "FAIL":
                    {
                        if (SocketWriteData(data + "OK") == 1)
                        {
                            _numCount = 0;
                            _time = DateTime.Now;
                            _result = TestResult.S_1FAIL;
                            ShowSocketStatus("FAIL", SystemColors.Red);
                        }
                        break;
                    }
                case "REPA":
                    {
                        if (SocketWriteData(data + "OK") == 1)
                        {
                            _numCount = 0;
                            _time = DateTime.Now;
                            _result = TestResult.S_1REPA;
                            ShowSocketStatus("REPA", SystemColors.Orange);
                        }
                        break;
                    }
                case "1INIT2INIT":
                    {
                        if (SocketWriteData(data + "OK") == 1)
                        {
                            _numCount = 0;
                            _time = DateTime.Now;
                            _result = TestResult.D_1INIT2INIT;
                            ShowSocketStatus("1INIT2INIT", SystemColors.Cyan);
                        }
                        break;
                    }
                case "1PASS2PASS":
                    {
                        if (SocketWriteData(data + "OK") == 1)
                        {
                            _numCount = 0;
                            _time = DateTime.Now;
                            _result = TestResult.D_1PASS2PASS;
                            ShowSocketStatus("1PASS2PASS", SystemColors.Green);
                        }
                        break;
                    }
                case "1PASS2FAIL":
                    {
                        if (SocketWriteData(data + "OK") == 1)
                        {
                            _numCount = 0;
                            _time = DateTime.Now;
                            _result = TestResult.D_1PASS2FAIL;
                            ShowSocketStatus("1PASS2FAIL", SystemColors.Pink);
                        }
                        break;
                    }
                case "1FAIL2PASS":
                    {
                        if (SocketWriteData(data + "OK") == 1)
                        {
                            _numCount = 0;
                            _time = DateTime.Now;
                            _result = TestResult.D_1FAIL2PASS;
                            ShowSocketStatus("1FAIL2PASS", SystemColors.Pink);
                        }
                        break;
                    }
                case "1FAIL2FAIL":
                    {
                        if (SocketWriteData(data + "OK") == 1)
                        {
                            _numCount = 0;
                            _time = DateTime.Now;
                            _result = TestResult.D_1FAIL2FAIL;
                            ShowSocketStatus("1FAIL2FAIL", SystemColors.Red);
                        }
                        break;
                    }
                case "GOLDEN_SAMPLE_PASS":
                    {
                        if (SocketWriteData(data + "OK") == 1)
                        {
                            _numCount = 0;
                            _time = DateTime.Now;
                            _result = TestResult.GOLDEN_SAMPLE_PASS;
                            ShowSocketStatus("GSP", SystemColors.Green);
                        }
                        break;
                    }
                case "GOLDEN_SAMPLE_FAIL":
                    {
                        if (SocketWriteData(data + "OK") == 1)
                        {
                            _numCount = 0;
                            _time = DateTime.Now;
                            _result = TestResult.GOLDEN_SAMPLE_FAIL;
                            ShowSocketStatus("GSF", SystemColors.Red);
                        }
                        break;
                    }
                case "TEST_GOLDEN_SAMPLE_PASS":
                    {
                        if (SocketWriteData(data + "OK") == 1)
                        {
                            _goldenSample = GoldenSampleType.Pass;
                            ShowSocketGoldenSample("P", GoldenSampleType.Pass);
                            ShowSocketGoldenSample("_", GoldenSampleType.Fail);
                        }
                        break;
                    }
                case "TEST_GOLDEN_SAMPLE_FAIL":
                    {
                        if (SocketWriteData(data + "OK") == 1)
                        {
                            _goldenSample = GoldenSampleType.Fail;
                            ShowSocketGoldenSample("_", GoldenSampleType.Pass);
                            ShowSocketGoldenSample("F", GoldenSampleType.Fail);
                        }
                        break;
                    }
                case "LOCK":
                    {
                        if (SocketWriteData(data + "OK") == 1)
                        {
                            _numCount = 0;
                            _time = DateTime.Now;
                            _result = TestResult.LOCK;
                            ShowSocketStatus("LOCKED", SystemColors.Brown);
                        }
                        break;
                    }
                case "OPEN_FIXTUREOK":
                    {
                        //GlobalDataManager.Current.OpenFixtureOK = true;
                        break;
                    }
                case "OPEN_FIXTURENG":
                    {
                        //GlobalDataManager.Current.OpenFixtureNG = true;
                        break;
                    }
                case "CLOSE_FIXTUREOK":
                    {
                        //GlobalDataManager.Current.CloseFixtureOK = true;
                        break;
                    }
                case "CLOSE_FIXTURENG":
                    {
                        //GlobalDataManager.Current.CloseFixtureNG = true;
                        break;
                    }
                case "ERROR":
                    {
                        if (SocketWriteData(data + "OK") == 1)
                        {
                            _numCount = 0;
                            _time = DateTime.Now;
                            _result = TestResult.Unknow;
                            _goldenSample = GoldenSampleType.Unknow;
                            SocketForceClose();
                            ShowSocketStatus("ERROR", SystemColors.Red);
                        }
                        break;
                    }
                default:
                    {
                        if (data.Length >= 3 && data.Substring(0, 3) != "RUN")
                        {
                            SocketWriteData("FORMAT_ERROR");
                        }
                    }
                    break;
            }
        }

        private void OntimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            string text = $"Time: {_numCount} (s)";
            if (_numCount >= 600 && _numCount % 2 == 1)
            {
                ShowSocketTime(text, SystemColors.Red);
            }
            else
            {
                ShowSocketTime(text, SystemColors.White);
            }
            ++_numCount;
        }

        private void ShowSocketInfo(string text, SystemColors color)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (_socketUI != null)
                {
                    _socketUI.Info.Content = text;
                    _socketUI.Info.Foreground = MyColors.GetColor(color);
                }
            });
        }

        private void ShowSocketStatus(string text, SystemColors color)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (_socketUI != null)
                {
                    _socketUI.Status.Content = text;
                    _socketUI.Status.Foreground = MyColors.GetColor(color);
                }
            });
        }

        private void ShowSocketTime(string text, SystemColors color)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (_socketUI != null)
                {
                    _socketUI.Time.Content = text;
                    _socketUI.Time.Foreground = MyColors.GetColor(color);
                }
            });
        }

        public void ShowSocketGoldenSample(string text, GoldenSampleType type, SystemColors color = SystemColors.White)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (_socketUI != null)
                {
                    if (type == GoldenSampleType.Pass)
                    {
                        _socketUI.GoldenSamplePass.Content = text;
                        _socketUI.GoldenSamplePass.Foreground = MyColors.GetColor(color);
                    }
                    if (type == GoldenSampleType.Fail)
                    {
                        _socketUI.GoldenSampleFail.Content = text;
                        _socketUI.GoldenSampleFail.Foreground = MyColors.GetColor(color);
                    }
                }
            });
        }

        public void Testing(string status = "TESTING")
        {
            _numCount = 0;
            _result = TestResult.TESTING;
            _time = DateTime.Now;
            ShowSocketStatus(status, SystemColors.Yellow);
        }

        public void TestError(string status = "ERROR")
        {
            _numCount = 0;
            _result = TestResult.Unknow;
            _time = DateTime.Now;
            ShowSocketStatus(status, SystemColors.Red);
        }

        public int ClearFlow()
        {
            int nRet = SocketForceClose();
            _numCount = 0;
            _result = TestResult.Unknow;
            _time = DateTime.Now;
            ShowSocketStatus("-----", SystemColors.White);
            return nRet;
        }

        public void ClearGoldenSample()
        {
            _numCount = 0;
            _goldenSample = GoldenSampleType.Unknow;
            ShowSocketGoldenSample("_", GoldenSampleType.Pass);
            ShowSocketGoldenSample("_", GoldenSampleType.Fail);
        }

        public void ClearFixture()
        {
            _numCount = 0;
            _result = TestResult.Unknow;
            _time = DateTime.Now;
            ShowSocketStatus("CLEAR", SystemColors.Yellow);
        }
    }
}
