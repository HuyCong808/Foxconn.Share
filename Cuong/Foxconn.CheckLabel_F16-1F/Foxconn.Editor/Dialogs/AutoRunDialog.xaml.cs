using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.Editor.Configuration;
using Foxconn.Editor.Enums;
using Foxconn.Editor.OpenCV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Foxconn.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for AutoRun.xaml
    /// </summary>
    public partial class AutoRunDialog : Window, INotifyPropertyChanged
    {
        public static AutoRunDialog Current;
        private Worker _loopWorker;
        private Worker _loopMonitor;
        private Board _program
        {
            get => ProgramManager.Current.Program;
            set => ProgramManager.Current.Program = value;
        }
        //private DataBase _database
        //{
        //    get => ProgramManager.Current.Database;
        //    set => ProgramManager.Current.Database = value;
        //}
        private MachineParams _param = MachineParams.Current;
        private DeviceManager _device = DeviceManager.Current;
        private List<FOVResult> _FOVResults = new List<FOVResult>();
        private List<SMDResult> _SMDResults = new List<SMDResult>();
        private Properties.Settings _settings = Properties.Settings.Default;
        private VideoCapture _capture = null;
        private Mat _frame;

        public bool IsOpenResultDialog = false;
        public bool IsRunning => _loopWorker.IsRunning;
        public bool IsMonitoring => _loopMonitor.IsRunning;

        public string adapterLabel = string.Empty;
        public string cableLabel = string.Empty;
        public string macLabel = string.Empty;
        public string boxSN = string.Empty;
        public string dataScanner1 = string.Empty;

        private bool _start = false;
        private bool _stop = false;
        public string CycleTime { get; set; }
        public string BoardName
        {
            get => _program.Name;
            set
            {
                _program.Name = value;
                NotifyPropertyChanged(nameof(BoardName));
            }
        }
        public int TotalPass
        {
            get => _settings.TotalPass;
            set
            {
                _settings.TotalPass = value;
                NotifyPropertyChanged(nameof(TotalPass));
            }
        }

        public int TotalFail
        {
            get => _settings.TotalFail;
            set
            {
                _settings.TotalFail = value;
                NotifyPropertyChanged(nameof(TotalFail));
            }
        }

        public int TotalChecked
        {
            get => _settings.TotalChecked;
            set
            {
                _settings.TotalChecked = value;
                NotifyPropertyChanged(nameof(TotalChecked));
            }
        }

        public float YeildRate
        {
            get => _settings.YeildRate;
            set
            {
                _settings.YeildRate = value;
                NotifyPropertyChanged(nameof(YeildRate));
            }
        }

        #region Binding Property
        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public virtual void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public AutoRunDialog()
        {
            InitializeComponent();
            DataContext = this;
            Current = this;
            _loopWorker = new Worker(new ThreadStart(AutoRunProcess));
            _loopMonitor = new Worker(new ThreadStart(AutoRunProcessMonitor));


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogInfo("AutoRun ========> Start AutoRun");
            // StartUp();
            UpdateStatusControl("Running...", 0);
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            Stop();
            LogInfo("AutoRun ========> Stop AutoRun");
        }

        public bool StartUp()
        {
            try
            {
                if (IsRunning)
                    return true;
                _loopWorker.Start();
                _loopMonitor.Start();
                if (!Prepare())
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                return false;
            }
        }

        private bool Prepare()
        {
            bool bRet = true;
            try
            {
                int nRet = _device.Ping();
                if (nRet != 1)
                {
                    bRet = false;
                }
                // OpenWebcam();
                // CvImage.Clear("", maxDayCount: 7);
            }
            catch (Exception ex)
            {
                bRet = false;
                LogError(ex.Message);
            }
            return bRet;
        }

        public void Stop()
        {
            if (_capture != null)
            {
                _capture.Dispose();
            }
            _loopWorker.Stop();
            MainWindow.Current.Show();
        }

        private void mnuiOpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory);
        }

        private void mnuiSetSpeedRobot_Click(object sender, RoutedEventArgs e)
        {
            SetSpeedRobotDiaLog setspeed = new SetSpeedRobotDiaLog();
            setspeed.Show();
        }

        private void mnuiResetRate_Click(object sender, RoutedEventArgs e)
        {
            switch (IsAdmin())
            {
                case 1:
                    ResetStatistic();
                    break;
                case 2:
                    ResetFailRate();
                    break;

            }
            NotifyPropertyChanged();
        }


        public void AutoRunProcess()
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Auto run thread.";

            bool isSendDone = false;
            string sendData = string.Empty;
            Console.WriteLine("AutoRunProcess ======> Start");
            while (!_loopWorker.WaitStopSignal(25))
            {
                try
                {
                    if (_device.Scanner1 == null || _device.Scanner2 == null || _device.TerminalIT == null || _device.TerminalBoard == null)
                    {
                        continue;
                    }

                    string data = _device.Scanner1.DataReceived;
                    if (data.Length > 0)
                    {
                        _device.Scanner1.DataReceived = string.Empty;
                        CheckScanner1(data);
                    }

                    if (CheckBoxSN(_device.Scanner2.DataReceived))
                    {
                        LogRecord($"Scanner2: Box SN = {boxSN}", null);
                        Dispatcher.Invoke(() => txtBoxSN.Text = boxSN);
                        _device.Scanner2.DataReceived = string.Empty;
                    }

                    if (boxSN.Length > 0 && dataScanner1.Length > 0)
                    {
                        sendData = boxSN + "#" + dataScanner1;
                        boxSN = string.Empty;
                        dataScanner1 = string.Empty;
                    }


                    if (_start)
                    {
                        ShowResult("Pls scan...", Brushes.Yellow);
                        isSendDone = false;
                        if (sendData.Length > 0)
                        {
                            LogRecord($"Sending Terminal: {sendData}", null);
                            Logger.Current.Info($"Sending Terminal: {sendData}");
                            
                            int nRet = CheckTerminal(sendData);
                            if (nRet == 1)
                            {
                                ShowResult("PASS", Brushes.LimeGreen);
                                LogRecord("PASS", Brushes.Green);
                                Logger.Current.Info($"PASS");
                                UpdateRate(1, 0);
                            }
                            else if (nRet == 0)
                            {
                                ShowResult("FAIL-TIMEOUT", Brushes.Red, 50);
                                LogRecord("ERROR-TIMEOUT", Brushes.Red);
                                Logger.Current.Info($"FAIL-TIMEOUT");
                                UpdateRate(0, 1);
                            }
                            else
                            {
                                ShowResult("FAIL-IT", Brushes.Red);
                                LogRecord("ERROR-IT", Brushes.Red);
                                Logger.Current.Info("FAIL-IT");
                                UpdateRate(0, 1);
                            }
                            isSendDone = true;
                            sendData = string.Empty;
                            Dispatcher.Invoke(() => dgLogRecords.Items.Clear());
                            _start = false;
                            //cableLabel = string.Empty;
                            //adapterLabel = string.Empty;
                            //macLabel = string.Empty;
                            //boxSN = string.Empty;
                        }
                    }

                    if (_stop)
                    {
                        _stop = false;
                        if (!isSendDone)
                        {
                            ShowResult("FAIL", Brushes.Red);
                            LogRecord("FAIL - Not enough 3 barcodes", Brushes.Red);
                            Logger.Current.Info("FAIL - Not enough 3 barcodes");
                            UpdateRate(0, 1);
                            adapterLabel = string.Empty;
                            cableLabel = string.Empty;
                            macLabel = string.Empty;
                            boxSN = string.Empty;
                            dataScanner1 = string.Empty;

                        }
                        Dispatcher.Invoke(() =>
                        {
                            txtBoxSN.Text = string.Empty;
                            txtLabelCable.Text = string.Empty;
                            txtLabelAdapter.Text = string.Empty;
                        });
                    }

                }
                catch (Exception ex)
                {
                    LogError("AutoRunProcess: " + ex.Message);
                }
            }
        }


        public void AutoRunProcessMonitor()
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Auto run monitoring thread.";
            Console.WriteLine("AutoRunProcessMonitor ======> Start");
            while (!_loopMonitor.WaitStopSignal(25))
            {
                try
                {
                    if (_device.TerminalBoard.DataReceived.Contains("I"))
                    {
                        LogRecord("TerminalBoard.DataReceived: " + _device.TerminalBoard.DataReceived, null);
                        _device.TerminalBoard.DataReceived = string.Empty;
                        _start = true;
                        _stop = false;
                    }
                    if (_device.TerminalBoard.DataReceived.Contains("O"))
                    {
                        LogRecord("TerminalBoard.DataReceived: " + _device.TerminalBoard.DataReceived, null);
                        _device.TerminalBoard.DataReceived = string.Empty;
                        _start = false;
                        _stop = true;
                    }
                    UpdateNewStatistic();
                }

                catch (Exception ex)
                {
                    LogError("AutoRunProcess Monitor: " + ex.Message);
                }
            }
        }


        public bool CheckScanner1(string data)
        {
            bool fRet = false;

            if (_param.StationCable.IsEnabled)
            {
                int startIndex = _param.StationCable.StartIndex;
                int length = _param.StationCable.Length;
                if (data.Length > startIndex)
                {
                    if (_param.LabelCable.Contains(data.Substring(startIndex)))
                    {
                        if (_param.StationCable.IsSubstring)
                        {
                            cableLabel = data.Substring(startIndex, length);
                        }
                        else
                        {
                            cableLabel = data;
                        }
                        LogRecord($"Scanner1: Cable Label = {cableLabel}", null);
                        txtLabelCable.Dispatcher.Invoke(() => txtLabelCable.Text = cableLabel);
                    }
                }
            }

            if (_param.StationAdapter.IsEnabled)
            {
                int startIndex = _param.StationAdapter.StartIndex;
                int length = _param.StationAdapter.Length;
                if (data.Length > startIndex)
                {
                    if (_param.LabelAdapter.Contains(data.Substring(startIndex, length)))
                    {
                        if (_param.StationAdapter.IsSubstring)
                        {
                            adapterLabel = data.Substring(startIndex, length);
                        }
                        else
                        {
                            adapterLabel = data;
                        }
                        LogRecord($"Scanner1: Adapter Label = {adapterLabel}", null);
                        txtLabelAdapter.Dispatcher.Invoke(() => txtLabelAdapter.Text = adapterLabel);
                    }
                }
            }

            if (cableLabel.Length > 0 && adapterLabel.Length > 0)
            {
                dataScanner1 = cableLabel + "#" + adapterLabel;
                cableLabel = string.Empty;
                adapterLabel = string.Empty;
                fRet = true;
            }
            return fRet;
        }


        public bool CheckStationMAC(string data)
        {
            bool fRet = false;
            if (_param.StationMAC.IsEnabled)
            {
                Dispatcher.Invoke(() => lblLabelStation.Content = "MAC Label");
                if (data.Length == _param.StationMAC.Length) // MAC(12 Characters)
                {
                    if (_param.StationMAC.IsSubstring)
                    {
                        macLabel = data.Substring(_param.StationMAC.StartIndex);
                    }
                    else
                    {
                        macLabel = data;
                    }
                    fRet = true;
                }
            }
            return fRet;
        }

        public bool CheckBoxSN(string data)
        {
            bool fRet = false;
            if (data.Length == _param.BoxSN.Length) // MAC(13 Characters)
            {
                if (_param.BoxSN.IsSubstring)
                {
                    boxSN = data.Substring(_param.BoxSN.StartIndex);
                }
                else
                {
                    boxSN = data;
                }
                fRet = true;
            }
            return fRet;
        }

        public void LogRecord(string text, SolidColorBrush color)
        {
            dgLogRecords.Dispatcher.Invoke(() =>
            {
                dgLogRecords.Items.Add(new LogRecord() { DateTime = DateTime.Now, Information = text, BrushForBackGroundColor = color });
                dgLogRecords.Items.Refresh();
                if (dgLogRecords.Items.Count > 500)
                {
                    dgLogRecords.Items.Clear();
                }
                if (dgLogRecords.Items.Count > 0)
                {
                    var border = VisualTreeHelper.GetChild(dgLogRecords, 0) as Decorator;
                    if (border != null)
                    {
                        var scroll = border.Child as ScrollViewer;
                        if (scroll != null)
                        {
                            scroll.ScrollToEnd();
                        }
                    }
                }
            });
        }

        public void ShowResult(string text, SolidColorBrush color, int size = 70)
        {
            Dispatcher.Invoke(() =>
            {
                lblResult.Foreground = color;
                lblResult.Content = text;
                lblResult.FontSize = size;
                lblResult.FontWeight = FontWeights.Bold;
            });
        }

        #region Mouse Event


        public void ShowResultDataGrid(FOVType fovtype, Emgu.CV.UI.ImageBox imagebox)
        {
            if (!IsOpenResultDialog)
            {
                Image<Bgr, byte> sourceImage = imagebox.Image as Image<Bgr, byte>;
                ImageResultDialogs imageResult = new ImageResultDialogs();

                imageResult.imbImageResult.Image = sourceImage;

                var fResult = _FOVResults.Find(x => x.FOVType == fovtype);
                if (fResult != null)
                {
                    imageResult.txbFOVType.Text = $"{fovtype}";

                    string text = fResult.Result == 1 ? "PASS" : "FAIL";
                    imageResult.txbFOVResult.Foreground = fResult.Result == 1 ? System.Windows.Media.Brushes.LimeGreen : System.Windows.Media.Brushes.Red;
                    imageResult.txbFOVResult.FontWeight = FontWeights.Bold;
                    imageResult.txbFOVResult.Text = $"{text}";

                    foreach (var rsmd in fResult.SMDs)
                    {
                        SolidColorBrush color = new SolidColorBrush();
                        if (rsmd.Result.Result == false)
                        {
                            color = System.Windows.Media.Brushes.Red;
                        }
                        imageResult.dgLogRecords.Items.Add(new ResultSMDDialog() { Name = rsmd.SMD.Name, Score = Math.Round(rsmd.Result.Score, 2), Result = rsmd.Result.Result, BrushForBackGroundColor = color });

                    }
                    imageResult.dgLogRecords.Items.Refresh();
                    imageResult.dgLogRecords.ScrollIntoView(imageResult.dgLogRecords.Items.GetItemAt(imageResult.dgLogRecords.Items.Count - 1));

                }
                imageResult.Show();
                IsOpenResultDialog = true;
            }
        }

        #endregion

        private int CheckTerminal(string data, int timeout = 10000)
        {
            LogInfo("AutoRunManager =====> Check Terminal");

            if (!_param.TerminalIT.IsEnabled)
                return 0;

            _device.TerminalIT.DataReceived = string.Empty;
            _device.TerminalIT.SerialWriteData(data + "\r\n");
            for (int i = 0; i < timeout / 25; i++)
            {
                string dataReceived = _device.TerminalIT.DataReceived;
                if (_start)
                {
                    return 0;
                }
                else if (dataReceived.Contains("PASS"))
                {
                    return 1;
                }
                else if (dataReceived.Contains("ERRO"))
                {
                    return -1;
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
            return 0;
        }

        #region StreamCamera
        private void OpenWebcam()
        {
            Task.Run(() =>
            {
                CvInvoke.UseOpenCL = false;
                try
                {
                    _frame = new Mat();
                    _capture = new VideoCapture();
                    //int fourcc = VideoWriter.Fourcc('M', 'J', 'P', 'G');
                    //_capture.Set(Emgu.CV.CvEnum.CapProp.FourCC, fourcc);
                    //_capture.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
                    //_capture.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, 1080);
                    _capture.ImageGrabbed += ProcessFrame;
                    _capture.Start();
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            });
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {

            if (_capture != null && _capture.Ptr != IntPtr.Zero)
            {
                _capture.Retrieve(_frame, 0);
                // Dispatcher.Invoke(() => imbCamera.Image = _frame);
            }
        }


        #endregion

        #region logs
        public void LogInfo(string message)
        {
            Logger.Current.Info(message);
        }

        public void LogError(string message)
        {
            Logger.Current.Error(message);
        }

        //public void LogsAutoRun(string message)
        //{
        //    string currentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        //    int MAX_LINES = 500;
        //    Dispatcher.Invoke(() =>
        //    {
        //        // Thêm dòng mới vào Textbox
        //        txtLogsAutoRun.AppendText(currentTime + "   " + "[INFO]" + "   " + message + Environment.NewLine);
        //        // Kiểm tra số lượng dòng và xóa các dòng cũ hơn nếu vượt quá giới hạn
        //        int lineCount = txtLogsAutoRun.LineCount;
        //        if (lineCount > MAX_LINES)
        //        {
        //            int indexFirstLineToRemove = txtLogsAutoRun.GetCharacterIndexFromLineIndex(0);
        //            int indexLastLineToRemove = txtLogsAutoRun.GetCharacterIndexFromLineIndex(lineCount - MAX_LINES);
        //            txtLogsAutoRun.Select(indexFirstLineToRemove, indexLastLineToRemove);
        //            txtLogsAutoRun.SelectedText = "";

        //        }
        //        txtLogsAutoRun.ScrollToEnd();
        //    });
        //}
        #endregion


        public void UpdateFOVResult(FOVType fovtype, int result, string SN, Image<Bgr, byte> image, List<SMDResult> rsmd)
        {
            FOVResult item = _FOVResults.Find(x => x.FOVType == fovtype);
            if (item != null)
            {
                item.FOVType = fovtype;
                item.Result = result;
                item.SN = SN;
                item.Image = image;
                item.SMDs = rsmd;
            }
            else
            {
                FOVResult temp = new FOVResult
                {
                    FOVType = fovtype,
                    Result = result,
                    SN = SN,
                    Image = image,
                    SMDs = rsmd,
                };
                _FOVResults.Add(temp);
            }
        }

        public void UpdateStatusControl(string text, int progress)
        {
            txbStatusBar.Text = text;
            prbStatus.Value = progress;
        }


        public bool IsNewDay()
        {
            var now = DateTime.Now;
            var dateCreated = _settings.DateCreated;
            if ((now - dateCreated).Days > 0)
            {
                dateCreated = DateTime.Now.Date;
                _settings.DateCreated = dateCreated;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateNewStatistic()
        {
            if (IsNewDay())
            {
                _settings.TotalPass = 0;
                _settings.TotalFail = 0;
                _settings.TotalChecked = 0;
                _settings.YeildRate = 0;
                _settings.Save();
                NotifyPropertyChanged();
            }
        }

        public void ResetStatistic()
        {
            _settings.Reset();
            _settings.Save();

            MessageShow.Info("Reseted Statistic", "Reset");
        }

        public void ResetFailRate()
        {
            _settings.TotalPass += _settings.TotalFail;
            _settings.TotalFail = 0;
            _settings.TotalChecked = _settings.TotalPass + _settings.TotalFail;
            _settings.YeildRate = (float)Math.Round((float)_settings.TotalPass / _settings.TotalChecked * 100, 2);
            _settings.Save();
            MessageShow.Info("Reseted FailRate", "Reset");
        }


        public void UpdateRate(int numPass, int numFail)
        {
            _settings.TotalPass += numPass;
            _settings.TotalFail += numFail;
            _settings.TotalChecked = _settings.TotalPass + _settings.TotalFail;
            if (_settings.TotalChecked == 0)
            {
                _settings.YeildRate = 0;
            }
            else
            {
                _settings.YeildRate = (float)Math.Round((float)_settings.TotalPass / _settings.TotalChecked * 100, 2);
            }

            _settings.Save();
            NotifyPropertyChanged();

        }

        public int IsAdmin()
        {
            LoginDialog loginDialog = new LoginDialog();
            loginDialog.ShowDialog();
            if (loginDialog.txtUsername.Text == "admin" && loginDialog.Password.Password == "admin" && !loginDialog.Cancel())
            {
                return 1;
            }
            else if (loginDialog.txtUsername.Text == "at" && loginDialog.Password.Password == "foxconnat" && !loginDialog.Cancel())
            {
                return 2;
            }
            else if (loginDialog.Cancel())
            {
                loginDialog.Close();
                return 0;
            }
            else
                return -1;
        }

        private void mnuiTest_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class FOVResult
    {
        public FOVType FOVType { get; set; }
        public int Result { get; set; }
        public string SN { get; set; }
        public Image<Bgr, byte> Image { get; set; }
        public string WorkerConfirm { get; set; }
        public List<SMDResult> SMDs { get; set; }


        public FOVResult()
        {
            FOVType = FOVType.Unknow;
            Result = -1;
            SN = string.Empty;
            Image = null;
            WorkerConfirm = string.Empty;
            SMDs = new List<SMDResult>();
        }
    }

    public class SMDResult
    {
        public SMD SMD { get; set; }
        public CvResult Result { get; set; }

        public SMDResult()
        {
            SMD = new SMD();
            Result = new CvResult();
        }

        public SMDResult SMDResultClone()
        {
            return new SMDResult()
            {
                SMD = SMD,
                Result = Result
            };
        }
    }

    public class ResultSMDDialog
    {
        public string Name { get; set; }
        public double Score { get; set; }
        public bool Result { get; set; }
        public SolidColorBrush BrushForBackGroundColor { get; set; }
    }

    public class LogRecord
    {
        public DateTime DateTime { get; set; }
        public string Information { get; set; }
        public SolidColorBrush BrushForBackGroundColor { get; set; }
    }

}



