using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.Editor.Camera;
using Foxconn.Editor.Configuration;
using Foxconn.Editor.Enums;
using Foxconn.Editor.OpenCV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        private Worker _loopWorkerMonitor;

        private Board _program
        {
            get => ProgramManager.Current.Program;
            set => ProgramManager.Current.Program = value;
        }
        private DataBase _database
        {
            get => ProgramManager.Current.Database;
            set => ProgramManager.Current.Database = value;
        }
        private MachineParams _param = MachineParams.Current;
        private DeviceManager _device = DeviceManager.Current;
        private Statistics _statistic = Statistics.Current;
        private List<FOVResult> _FOVResults = new List<FOVResult>();
        private List<SMDResult> _SMDResults = new List<SMDResult>();
        FOVResult fovResult = new FOVResult();
        private Properties.Settings _settings = Properties.Settings.Default;
        private Image<Bgr, byte> _image = null;
        private VideoCapture _capture = null;
        private Mat _frame;
        public bool IsOpenResultDialog = false;
        public bool IsSetSpeedRobot = false;
        private FOVType _FOVTypeImb1_L1 = FOVType.Unknow;
        private FOVType _FOVTypeImb2_L1 = FOVType.Unknow;
        private FOVType _FOVTypeImb1_L2 = FOVType.Unknow;
        private FOVType _FOVTypeImb2_L2 = FOVType.Unknow;
        private string _SN { get; set; }
        public string CycleTime { get; set; }
        public string BoardName { get; set; }


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
        public DateTime TimeUpdateRate
        {
            get => _settings.TimeUpdateRate;
            set => _settings.TimeUpdateRate = value;
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
            _loopWorkerMonitor = new Worker(new ThreadStart(AutoRunProcessMonitor));

            BoardName = _program.Name;
            TotalPass = Properties.Settings.Default.TotalPass;
            TotalFail = Properties.Settings.Default.TotalFail;
            TotalChecked = Properties.Settings.Default.TotalChecked;
            YeildRate = Properties.Settings.Default.YeildRate;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogInfo("AutoRun ========> Start AutoRun");
            StartUp();
            UpdateStatusControl("Running...", 0);
            StatusSFIS();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _database.Save();
            Stop();
            LogInfo("AutoRun ========> Stop AutoRun");
        }

        public bool StartUp()
        {
            try
            {
                _loopWorker.Start();
                _loopWorkerMonitor.Start();
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
                CvImage.Clear("", maxDayCount: 7);
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
            _loopWorkerMonitor.Stop();
            MainWindow.Current.Show();
        }

        private void mnuiOpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory);
        }

        private void mnuiSetSpeedRobot_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSetSpeedRobot)
            {
                SetSpeedRobotDiaLog setspeed = new SetSpeedRobotDiaLog();
                setspeed.Show();
                IsSetSpeedRobot = true;
            }
        }

        private void mnuiResetRate_Click(object sender, RoutedEventArgs e)
        {
            switch (IsAdmin())
            {
                case 1:
                    _statistic.ResetStatisticButton();
                    break;
                case 2:
                    _statistic.ResetFailRate();
                    break;

            }
            Properties.Settings.Default.Save();
            NotifyPropertyChanged();
        }

        public void AutoRunProcess()
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Auto run thread.";
            FOVType pCheck = FOVType.Unknow;
            //string flagLane1 = "S200";
            //string flagLane2 = "S202";
            string flagL1_LABEL1 = "S50";
            string flagL1_LABEL2 = "S51";
            string flagL1_COLOR1 = "S52";
            string flagL1_COLOR2 = "S53";
            string flagL1_SOLDER_CAP1 = "S100";
            string flagL1_SOLDER_CAP2 = "S101";

            string flagL2_LABEL1 = "S250";
            string flagL2_LABEL2 = "S251";
            string flagL2_COLOR1 = "S252";
            string flagL2_COLOR2 = "S253";
            string flagL2_SOLDER_CAP1 = "S300";
            string flagL2_SOLDER_CAP2 = "S301";

            //string flagL1_End = "S201";
            //string flagL2_End = "S203";

            while (_loopWorker.WaitStopSignal(100))
            {
                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    UpdateStatusControl("Processing...", -1);
                    ShowFOVImage(CameraMode.Top, null, FOVType.Unknow);
                    ShowCellResult(pCheck, "", System.Windows.Media.Brushes.Transparent);
                    int step = 100 / 12;
                    while (_loopWorker.WaitStopSignal(100))
                    {
                        pCheck = FOVType.L1_LABEL1;
                        if (GetSignal(flagL1_LABEL1))
                        {
                            LogInfo($"AutoRunProcess =====> {FOVType.L1_LABEL1}");
                            UpdateStatusControl($"{FOVType.L1_LABEL1}", step * 1);
                            int nRet = CheckType(FOVType.L1_LABEL1);
                            string message = nRet == 1 ? $"{FOVType.L1_LABEL1}: Pass" : $"{FOVType.L1_LABEL1}: Fail";
                            // 
                            CheckFOVResults(pCheck);
                            UpdateStatusControl(message, 0);
                        }

                        pCheck = FOVType.L1_LABEL2;
                        if (GetSignal(flagL1_LABEL2))
                        {
                            LogInfo($"AutoRunProcess =====> {FOVType.L1_LABEL2}");
                            UpdateStatusControl($"{FOVType.L1_LABEL2}", step * 2);
                            int nRet = CheckType(FOVType.L1_LABEL2);
                            string message = nRet == 1 ? $"{FOVType.L1_LABEL2}: Pass" : $"{FOVType.L1_LABEL2}: Fail";
                            CheckFOVResults(pCheck);
                            UpdateStatusControl(message, 0);
                        }

                        pCheck = FOVType.L1_COLOR1;
                        if (GetSignal(flagL1_COLOR1))
                        {
                            LogInfo($"AutoRunProcess =====> {FOVType.L1_COLOR1}");
                            UpdateStatusControl($"{FOVType.L1_COLOR1}", step * 3);
                            int nRet = CheckType(FOVType.L1_COLOR1);
                            string message = nRet == 1 ? $"{FOVType.L1_COLOR1}: Pass" : $"{FOVType.L1_COLOR1}: Fail";
                            CheckFOVResults(pCheck);
                            UpdateStatusControl(message, 0);
                        }

                        pCheck = FOVType.L1_COLOR2;
                        if (GetSignal(flagL1_COLOR2))
                        {
                            LogInfo($"AutoRunProcess =====> {FOVType.L1_COLOR2}");
                            UpdateStatusControl($"{FOVType.L1_COLOR2}", step * 4);
                            int nRet = CheckType(FOVType.L1_COLOR2);
                            string message = nRet == 1 ? $"{FOVType.L1_COLOR2}: Pass" : $"{FOVType.L1_COLOR2}: Fail";
                            CheckFOVResults(pCheck);
                            UpdateStatusControl(message, 0);
                        }

                        pCheck = FOVType.L1_SOLDER_CAP1;
                        if (GetSignal(flagL1_SOLDER_CAP1))
                        {
                            LogInfo($"AutoRunProcess =====> {FOVType.L1_SOLDER_CAP1}");
                            UpdateStatusControl($"{FOVType.L1_SOLDER_CAP1}", step * 5);
                            int nRet = CheckType(FOVType.L1_SOLDER_CAP1);
                            string message = nRet == 1 ? $"{FOVType.L1_SOLDER_CAP1}: Pass" : $"{FOVType.L1_SOLDER_CAP1}: Fail";
                            CheckFOVResults(pCheck);
                            UpdateStatusControl(message, 0);
                        }

                        pCheck = FOVType.L1_SOLDER_CAP2;
                        if (GetSignal(flagL1_SOLDER_CAP2))
                        {
                            LogInfo($"AutoRunProcess =====> {FOVType.L1_SOLDER_CAP2}");
                            UpdateStatusControl($"{FOVType.L1_SOLDER_CAP2}", step * 6);
                            int nRet = CheckType(FOVType.L1_SOLDER_CAP2);
                            string message = nRet == 1 ? $"{FOVType.L1_SOLDER_CAP2}: Pass" : $"{FOVType.L1_SOLDER_CAP2}: Fail";
                            CheckFOVResults(pCheck);
                            UpdateStatusControl(message, 0);
                        }

                        pCheck = FOVType.L2_LABEL1;
                        if (GetSignal(flagL2_LABEL1))
                        {
                            LogInfo($"AutoRunProcess =====> {FOVType.L2_LABEL1}");
                            UpdateStatusControl($"{FOVType.L2_LABEL1}", step * 7);
                            int nRet = CheckType(FOVType.L2_LABEL1);
                            string message = nRet == 1 ? $"{FOVType.L2_LABEL1}: Pass" : $"{FOVType.L2_LABEL1}: Fail";
                            CheckFOVResults(pCheck);
                            UpdateStatusControl(message, 0);
                        }

                        pCheck = FOVType.L2_LABEL2;
                        if (GetSignal(flagL2_LABEL2))
                        {
                            LogInfo($"AutoRunProcess =====> {FOVType.L2_LABEL2}");
                            UpdateStatusControl($"{FOVType.L2_LABEL2}", step * 8);
                            int nRet = CheckType(FOVType.L2_LABEL2);
                            string message = nRet == 1 ? $"{FOVType.L2_LABEL2}: Pass" : $"{FOVType.L2_LABEL2}: Fail";
                            CheckFOVResults(pCheck);
                            UpdateStatusControl(message, 0);
                        }

                        pCheck = FOVType.L2_COLOR1;
                        if (GetSignal(flagL2_COLOR1))
                        {
                            LogInfo($"AutoRunProcess =====> {FOVType.L2_COLOR1}");
                            UpdateStatusControl($"{FOVType.L2_COLOR1}", step * 9);
                            int nRet = CheckType(FOVType.L2_COLOR1);
                            string message = nRet == 1 ? $"{FOVType.L2_COLOR1}: Pass" : $"{FOVType.L2_COLOR1}: Fail";
                            CheckFOVResults(pCheck);
                            UpdateStatusControl(message, 0);
                        }

                        pCheck = FOVType.L2_COLOR2;
                        if (GetSignal(flagL2_COLOR2))
                        {
                            LogInfo($"AutoRunProcess =====> {FOVType.L2_COLOR2}");
                            UpdateStatusControl($"{FOVType.L2_COLOR2}", step * 10);
                            int nRet = CheckType(FOVType.L2_COLOR2);
                            string message = nRet == 1 ? $"{FOVType.L2_COLOR2}: Pass" : $"{FOVType.L2_COLOR2}: Fail";
                            CheckFOVResults(pCheck);
                            UpdateStatusControl(message, 0);
                        }

                        pCheck = FOVType.L2_SOLDER_CAP1;
                        if (GetSignal(flagL2_SOLDER_CAP1))
                        {
                            LogInfo($"AutoRunProcess =====> {FOVType.L2_SOLDER_CAP1}");
                            UpdateStatusControl($"{FOVType.L2_SOLDER_CAP1}", step * 11);
                            int nRet = CheckType(FOVType.L2_SOLDER_CAP1);
                            string message = nRet == 1 ? $"{FOVType.L2_SOLDER_CAP1}: Pass" : $"{FOVType.L2_SOLDER_CAP1}: Fail";
                            CheckFOVResults(pCheck);
                            UpdateStatusControl(message, 0);
                        }

                        pCheck = FOVType.L2_SOLDER_CAP2;
                        if (GetSignal(flagL2_SOLDER_CAP2))
                        {
                            LogInfo($"AutoRunProcess =====> {FOVType.L2_SOLDER_CAP2}");
                            UpdateStatusControl($"{FOVType.L2_SOLDER_CAP2}", step * 12);
                            int nRet = CheckType(FOVType.L2_SOLDER_CAP2);
                            string message = nRet == 1 ? $"{FOVType.L2_SOLDER_CAP2}: Pass" : $"{FOVType.L2_SOLDER_CAP2}: Fail";
                            CheckFOVResults(pCheck);
                            UpdateStatusControl(message, 0);
                        }
                    }
                    SaveFOVResults(FOVType.L1_LABEL1, FOVType.L1_COLOR1, FOVType.L1_SOLDER_CAP1, FOVType.L2_LABEL1, FOVType.L2_COLOR1, FOVType.L2_SOLDER_CAP1, FOVType.L1_LABEL2, FOVType.L1_COLOR2, FOVType.L1_SOLDER_CAP2, FOVType.L2_LABEL2, FOVType.L2_COLOR2, FOVType.L2_SOLDER_CAP2);
                    _FOVResults.Clear();
                    _SN = string.Empty;
                    stopwatch.Stop();
                    TimeSpan elapsedTime = stopwatch.Elapsed;
                    CycleTime = string.Format("{0}:{1:00} {2}", (int)elapsedTime.TotalSeconds, elapsedTime.Milliseconds / 10.0, "(s)");
                }
                catch (Exception ex)
                {
                    LogError(ex.Message);
                }
                finally
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void AutoRunProcessMonitor()
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Auto run monitoring thread.";
            while (!_loopWorkerMonitor.WaitStopSignal(1000))
            {
                try
                {
                    //    StatusMachine();
                    //    StatusDoor();
                    //UpdateNewStatistic();
                }
                catch (Exception ex)
                {
                    LogError(ex.Message);
                }
                Thread.Sleep(100);
            }
        }

        private bool GetSignal(string device)
        {
            if (_device.PLC1.GetFlag(device) == 1)
            {
                _device.PLC1.SetBitDevice(device, 0);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetResultToPLC(FOVType pCheck, bool pResult) // set pass fail voi PLC
        {
            string pass = "S3";
            string fail = "S4";
            string r = string.Empty;
            switch (pCheck)
            {
                case FOVType.Unknow:
                    break;
                case FOVType.L1_LABEL1:
                case FOVType.L1_COLOR1:
                case FOVType.L1_SOLDER_CAP1:
                case FOVType.L1_LABEL2:
                case FOVType.L1_COLOR2:
                case FOVType.L1_SOLDER_CAP2:
                case FOVType.L2_LABEL1:
                case FOVType.L2_COLOR1:
                case FOVType.L2_SOLDER_CAP1:
                case FOVType.L2_LABEL2:
                case FOVType.L2_COLOR2:
                case FOVType.L2_SOLDER_CAP2:
                    {
                        _device.PLC1.SetDevice(r = pResult ? pass : fail, 1);
                        break;
                    }
                default:
                    break;
            }
        }

        public void SetResultToPLC(FOVType pCheck)
        {
            string overcell_1 = "S8";
            string overcell_2 = "S9";
            switch (pCheck)
            {
                case FOVType.Unknow:
                    break;
                case FOVType.L1_LABEL1:
                case FOVType.L1_COLOR1:
                case FOVType.L1_SOLDER_CAP1:
                case FOVType.L2_LABEL1:
                case FOVType.L2_COLOR1:
                case FOVType.L2_SOLDER_CAP1:
                    {
                        _device.PLC1.SetDevice(overcell_1, 1);
                        break;
                    }
                case FOVType.L1_LABEL2:
                case FOVType.L1_COLOR2:
                case FOVType.L1_SOLDER_CAP2:
                case FOVType.L2_LABEL2:
                case FOVType.L2_COLOR2:
                case FOVType.L2_SOLDER_CAP2:
                    {
                        _device.PLC1.SetDevice(overcell_2, 1);
                        break;
                    }
                default:
                    break;
            }
        }


        private void CheckFOVResults(FOVType pCheck)
        {
            switch (pCheck)
            {
                case FOVType.Unknow:
                    break;
                case FOVType.L1_LABEL1:
                case FOVType.L1_LABEL2:
                    {
                        var f = _FOVResults.FirstOrDefault(x => x.FOVType == pCheck);
                        var temp = _database.Data.Find(x => x.SN == f.SN);
                        if (f != null)
                        {
                            if (f.Result == 1)
                            {
                                int nRet = CheckTerminal(f.SN);
                                switch (nRet)
                                {
                                    case 1: // ROUTING
                                        {
                                            if (temp != null) // SN đã có trong database    
                                            {
                                                if (temp.IsStep2 == true) // đã qua step2 nhưng gửi IT lỗi
                                                {
                                                    SetResultToPLC(pCheck, false);
                                                    ShowCellResult(pCheck, "OVERED STEP2", System.Windows.Media.Brushes.Red);
                                                }
                                                else if (temp.IsStep1 == true)
                                                {
                                                    SetResultToPLC(pCheck); // bỏ qua bước hàn
                                                    SetResultToPLC(pCheck, true);
                                                    ShowCellResult(pCheck, "OVERED STEP1", System.Windows.Media.Brushes.Red);
                                                }
                                            }
                                            else
                                            {
                                                SetResultToPLC(pCheck, true);
                                                _database.AddData(f.SN);
                                                ShowCellResult(pCheck, "ROUTING", System.Windows.Media.Brushes.White);
                                            }
                                            break;
                                        }
                                    case 2: // SOLDER_CAP
                                        {
                                            if (temp != null && temp.IsStep1 == true) // đã qua làn 1 check hàn fail, gửi IT fail cell
                                            {
                                                SetResultToPLC(pCheck);
                                                SetResultToPLC(pCheck, true);
                                                ShowCellResult(pCheck, "SOLDER_CAP", System.Windows.Media.Brushes.Red);
                                            }
                                            else if (temp == null)
                                            {
                                                SetResultToPLC(pCheck, false);
                                                ShowCellResult(pCheck, "SOLDER_CAP", System.Windows.Media.Brushes.Red);
                                            }
                                            break;
                                        }
                                    case 0: // Check Terminal Error 
                                        {
                                            SetResultToPLC(pCheck, false);
                                            ShowCellResult(pCheck, "ERROR", System.Windows.Media.Brushes.Red);
                                            break;
                                        }
                                    case -1: // TIME_OUT
                                        {
                                            SetResultToPLC(pCheck, false);
                                            ShowCellResult(pCheck, "TIME-OUT", System.Windows.Media.Brushes.Red);
                                            break;
                                        }
                                    case -2:
                                        {
                                            SetResultToPLC(pCheck, true);
                                            ShowCellResult(pCheck, "PASS-SN", System.Windows.Media.Brushes.Green);

                                            break;
                                        }

                                }
                            }
                            else // CHECK SN FAIL
                            {
                                SetResultToPLC(pCheck, false);
                                ShowCellResult(pCheck, "FAIL-SN", System.Windows.Media.Brushes.Red);
                            }
                        }
                        else
                        {
                            SetResultToPLC(pCheck, false);
                            ShowCellResult(pCheck, "NOT RESULT", System.Windows.Media.Brushes.Red);
                        }
                        break;
                    }
                case FOVType.L1_COLOR1:
                case FOVType.L1_COLOR2:
                case FOVType.L2_COLOR1:
                case FOVType.L2_COLOR2:
                    {
                        var f = _FOVResults.FirstOrDefault(x => x.FOVType == pCheck);

                        if (f != null)
                        {
                            if (f.Result == 1)
                            {
                                SetResultToPLC(pCheck, true);
                                ShowCellResult(pCheck, "PASS-COLOR", System.Windows.Media.Brushes.Green);
                            }
                            else
                            {
                                SetResultToPLC(pCheck, false);
                                ShowCellResult(pCheck, "FAIL-COLOR", System.Windows.Media.Brushes.Red);
                            }
                        }

                        break;
                    }
                case FOVType.L1_SOLDER_CAP1:
                case FOVType.L1_SOLDER_CAP2:
                    {
                        var f = _FOVResults.FirstOrDefault(x => x.FOVType == pCheck);
                        var temp = _database.Data.Find(x => x.SN == f.SN);
                        if (f != null)
                        {
                            if (temp != null)
                            {
                                if (f.Result == 1)
                                {
                                    SetResultToPLC(pCheck, true);
                                    temp.IsStep1 = true;
                                    ShowCellResult(pCheck, "PASS", System.Windows.Media.Brushes.Green);
                                }
                                else
                                {
                                    if (WorkerConfirm(pCheck) == 1) // WPASS - LANE1 chưa gửi IT
                                    {
                                        SetResultToPLC(pCheck, true);
                                        temp.IsStep1 = true;
                                        ShowCellResult(pCheck, "W_PASS", System.Windows.Media.Brushes.Green);
                                    }
                                    else if (WorkerConfirm(pCheck) == 0) // WFAIL
                                    {
                                        if (CheckTerminal(f.SN, false) == 1)
                                        {
                                            SetResultToPLC(pCheck, false);
                                            temp.IsStep1 = true;
                                            ShowCellResult(pCheck, "W_FAIL (SFIS)", System.Windows.Media.Brushes.Red);
                                        }
                                        else if (CheckTerminal(f.SN, false) == 0)
                                        {
                                            SetResultToPLC(pCheck, false);
                                            temp.IsStep1 = true;
                                            ShowCellResult(pCheck, "ERROR", System.Windows.Media.Brushes.Red);
                                        }
                                        else
                                        {
                                            SetResultToPLC(pCheck, false);
                                            temp.IsStep1 = true;
                                            ShowCellResult(pCheck, "W_FAIL", System.Windows.Media.Brushes.Red);
                                        }
                                    }
                                    else
                                    {
                                        int nRet = CheckTerminal(f.SN, false);
                                        if (nRet == 1)
                                        {
                                            SetResultToPLC(pCheck, false);
                                            temp.IsStep1 = true;
                                            _database.Data.Remove(temp);
                                            ShowCellResult(pCheck, "FAIL (SFIS)", System.Windows.Media.Brushes.Red);
                                        }
                                        else if (nRet == 0)
                                        {
                                            SetResultToPLC(pCheck, false);
                                            temp.IsStep1 = true;
                                            ShowCellResult(pCheck, "FAIL (ERRO SFIS)", System.Windows.Media.Brushes.Red);
                                        }
                                        else
                                        {
                                            SetResultToPLC(pCheck, false);
                                            temp.IsStep1 = true;
                                            ShowCellResult(pCheck, "FAIL", System.Windows.Media.Brushes.Red);
                                        }

                                    }
                                }
                            }
                            else
                            {
                                SetResultToPLC(pCheck, false);
                                ShowCellResult(pCheck, "NO DATA", System.Windows.Media.Brushes.Red);
                            }
                        }
                        else
                        {
                            SetResultToPLC(pCheck, false);
                            ShowCellResult(pCheck, "NO RESULT", System.Windows.Media.Brushes.Red);
                        }
                        break;
                    }

                case FOVType.L2_LABEL1:
                case FOVType.L2_LABEL2:
                    {
                        var f = _FOVResults.FirstOrDefault(x => x.FOVType == pCheck);
                        var temp = _database.Data.Find(x => x.SN == f.SN);
                        if (f != null)
                        {
                            if (f.Result == 1)
                            {
                                int nRet = CheckTerminal(f.SN);
                                switch (nRet)
                                {
                                    case 0: // IT trả về ERRO
                                        {
                                            SetResultToPLC(pCheck, false);
                                            ShowCellResult(pCheck, "ERROR", System.Windows.Media.Brushes.Red);
                                            break;
                                        }
                                    case 1: // ROUNTING
                                        {
                                            if (temp != null) //đã có trong database
                                            {
                                                if (temp.IsStep1 == true)
                                                {
                                                    SetResultToPLC(pCheck, true);
                                                    ShowCellResult(pCheck, "ROUNTING", System.Windows.Media.Brushes.White);
                                                }
                                                else if (temp.IsStep2 == true)  // đã hàn xong làn 2  lỗi không gửi được IT
                                                {
                                                    SetResultToPLC(pCheck);
                                                    SetResultToPLC(pCheck, true);
                                                    ShowCellResult(pCheck, "ROUNTING", System.Windows.Media.Brushes.White);
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                SetResultToPLC(pCheck, false);
                                                ShowCellResult(pCheck, "UNDO STEP1", System.Windows.Media.Brushes.Red);
                                            }
                                            break;
                                        }
                                    case 2: //SOLDER_CAP
                                        {
                                            if (temp != null && temp.IsStep1 == true) // Fail step1 (đã gửi IT fail) 
                                            {
                                                SetResultToPLC(pCheck, true);
                                                ShowCellResult(pCheck, "SOLDER_CAP", System.Windows.Media.Brushes.White);
                                            }
                                            else if (temp == null)
                                            {
                                                SetResultToPLC(pCheck);
                                                SetResultToPLC(pCheck, true);
                                                ShowCellResult(pCheck, "SOLDER_CAP", System.Windows.Media.Brushes.White);
                                            }
                                            break;
                                        }
                                    case -1: // TIME_OUT
                                        {
                                            SetResultToPLC(pCheck, false);
                                            ShowCellResult(pCheck, "TIME-OUT", System.Windows.Media.Brushes.Red);
                                            break;
                                        }
                                    case -2:
                                        {
                                            SetResultToPLC(pCheck, true);
                                            ShowCellResult(pCheck, "PASS-SN", System.Windows.Media.Brushes.Green);

                                            break;
                                        }
                                }
                            }
                            else
                            {
                                SetResultToPLC(pCheck, false);
                                ShowCellResult(pCheck, "FAIL-SN", System.Windows.Media.Brushes.Red);
                            }
                        }
                        else
                        {
                            SetResultToPLC(pCheck, false);
                            ShowCellResult(pCheck, "NO RESULT", System.Windows.Media.Brushes.Red);
                        }
                        break;
                    }

                case FOVType.L2_SOLDER_CAP1:
                case FOVType.L2_SOLDER_CAP2:
                    {
                        var f = _FOVResults.FirstOrDefault(x => x.FOVType == pCheck);
                        var temp = _database.Data.Find(x => x.SN == f.SN);

                        if (f != null)
                        {
                            if (temp != null)
                            {
                                if (f.Result == 1)
                                {
                                    int nRet = CheckTerminal(f.SN, true);
                                    if (nRet == 1)
                                    {
                                        SetResultToPLC(pCheck, true);
                                        temp.IsStep2 = true;
                                        _database.Data.Remove(temp);
                                        ShowCellResult(pCheck, "PASS (SFIS)", System.Windows.Media.Brushes.Green);
                                    }
                                    else if (nRet == 0)
                                    {
                                        SetResultToPLC(pCheck, true);
                                        temp.IsStep2 = true;
                                        ShowCellResult(pCheck, "PASS (ERRO SFIS)", System.Windows.Media.Brushes.Green);
                                    }
                                    else
                                    {
                                        SetResultToPLC(pCheck, true);
                                        temp.IsStep2 = true;
                                        ShowCellResult(pCheck, "PASS", System.Windows.Media.Brushes.Green);
                                    }
                                }
                                else
                                {
                                    if (WorkerConfirm(pCheck) == 1)
                                    {
                                        int nRet = CheckTerminal(f.SN, true);
                                        if (nRet == 1)
                                        {
                                            SetResultToPLC(pCheck, true);
                                            temp.IsStep2 = true;
                                            _database.Data.Remove(temp);
                                            ShowCellResult(pCheck, "W_PASS (SFIS)", System.Windows.Media.Brushes.Green);
                                        }
                                        else if (nRet == 0)
                                        {
                                            SetResultToPLC(pCheck, true);
                                            temp.IsStep2 = true;
                                            ShowCellResult(pCheck, "W_PASS (ERRO SFIS)", System.Windows.Media.Brushes.Green);
                                        }
                                        else
                                        {
                                            SetResultToPLC(pCheck, true);
                                            temp.IsStep2 = true;
                                            ShowCellResult(pCheck, "W_PASS", System.Windows.Media.Brushes.Green);
                                        }
                                    }
                                    else if (WorkerConfirm(pCheck) == 0)
                                    {
                                        int nRet = CheckTerminal(f.SN, false);
                                        if (nRet == 1)
                                        {
                                            SetResultToPLC(pCheck, false);
                                            temp.IsStep2 = true;
                                            _database.Data.Remove(temp);
                                            ShowCellResult(pCheck, "W_FAIL (SFIS)", System.Windows.Media.Brushes.Red);
                                        }
                                        else if (nRet == 0)
                                        {
                                            SetResultToPLC(pCheck, false);
                                            temp.IsStep2 = true;
                                            ShowCellResult(pCheck, "W_FAIL (ERRO SFIS)", System.Windows.Media.Brushes.Red);
                                        }
                                        else
                                        {
                                            SetResultToPLC(pCheck, false);
                                            temp.IsStep2 = true;
                                            ShowCellResult(pCheck, "W_FAIL", System.Windows.Media.Brushes.Red);
                                        }
                                    }
                                    else
                                    {
                                        int nRet = CheckTerminal(f.SN, false);
                                        if (nRet == 1)
                                        {
                                            SetResultToPLC(pCheck, false);
                                            temp.IsStep2 = true;
                                            _database.Data.Remove(temp);
                                            ShowCellResult(pCheck, "FAIL (SFIS)", System.Windows.Media.Brushes.Red);
                                        }
                                        else if (nRet == 0)
                                        {
                                            SetResultToPLC(pCheck, false);
                                            temp.IsStep2 = true;
                                            ShowCellResult(pCheck, "FAIL (ERRO SFIS)", System.Windows.Media.Brushes.Red);
                                        }
                                        else
                                        {
                                            SetResultToPLC(pCheck, false);
                                            temp.IsStep2 = true;
                                            ShowCellResult(pCheck, "FAIL", System.Windows.Media.Brushes.Red);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (f.Result == 1)
                                {
                                    SetResultToPLC(pCheck, true);
                                    ShowCellResult(pCheck, "PASS", System.Windows.Media.Brushes.Green);
                                }
                                else
                                {
                                    SetResultToPLC(pCheck, false);
                                    ShowCellResult(pCheck, "FAIL", System.Windows.Media.Brushes.Red);
                                }
                            }
                            //   SetResultToPLC(pCheck, f.Result);
                        }
                        break;
                    }
            }
        }

        public void ShowCellResult(FOVType pSOLDER_CAP, string text, SolidColorBrush color)
        {
            Dispatcher.Invoke(() =>
            {
                switch (pSOLDER_CAP)
                {
                    case FOVType.Unknow:
                        break;
                    case FOVType.L1_LABEL1:
                    case FOVType.L1_COLOR1:
                    case FOVType.L1_SOLDER_CAP1:
                        {
                            txtL1_Cell1.Text = text;
                            txtL1_Cell1.Foreground = color;
                            txtL1_Cell1.FontWeight = FontWeights.Bold;
                            break;
                        }

                    case FOVType.L1_LABEL2:
                    case FOVType.L1_COLOR2:
                    case FOVType.L1_SOLDER_CAP2:
                        {
                            txtL1_Cell2.Text = text;
                            txtL1_Cell2.Foreground = color;
                            txtL1_Cell2.FontWeight = FontWeights.Bold;
                            break;
                        }

                    case FOVType.L2_LABEL1:
                    case FOVType.L2_COLOR1:
                    case FOVType.L2_SOLDER_CAP1:
                        {
                            txtL2_Cell1.Text = text;
                            txtL2_Cell1.Foreground = color;
                            txtL2_Cell1.FontWeight = FontWeights.Bold;
                            break;
                        }

                    case FOVType.L2_LABEL2:
                    case FOVType.L2_COLOR2:
                    case FOVType.L2_SOLDER_CAP2:
                        {
                            txtL2_Cell2.Text = text;
                            txtL2_Cell2.Foreground = color;
                            txtL2_Cell2.FontWeight = FontWeights.Bold;
                            break;
                        }
                    case FOVType.L1_CLEAR:
                        {
                            txtL1_Cell1.Text = text;
                            txtL1_Cell1.Foreground = color;
                            txtL1_Cell1.FontWeight = FontWeights.Bold;
                            txtL1_Cell2.Text = text;
                            txtL1_Cell2.Foreground = color;
                            txtL1_Cell2.FontWeight = FontWeights.Bold;
                            break;
                        }
                    case FOVType.L2_CLEAR:
                        {
                            txtL2_Cell2.Text = text;
                            txtL2_Cell2.Foreground = color;
                            txtL2_Cell2.FontWeight = FontWeights.Bold;
                            txtL2_Cell2.Text = text;
                            txtL2_Cell2.Foreground = color;
                            txtL2_Cell2.FontWeight = FontWeights.Bold;
                            break;
                        }
                    default:
                        break;
                }
            });
        }
        public int WorkerConfirm(FOVType pSOLDER_CAP)
        {
            FOVResult result = _FOVResults.Find(x => x.FOVType == pSOLDER_CAP);
            if (_param.WorkerConfirm)
            {
                string msg = "Đây có phải lỗi ảo không?\r\nNhấn 'OK' nếu lỗi ảo\r\rNhấn 'Cancel' nếu lỗi thật";

                MessageBoxResult mbr = MessageBox.Show(msg, "Xác nhận", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                if (mbr == MessageBoxResult.OK)
                {
                    result.WorkerConfirm = "WPASS";
                    //PASS-WORKER
                    return 1;
                }
                else
                {
                    result.WorkerConfirm = "WFAIL";
                    //FAIL-WORKER
                    return 0;
                }
            }
            else
            {
                return -1;
            }
        }

        #region Mouse Event

        private void imbImageBox1_L1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (imbImageBox1_L1.Image != null)
                {
                    ShowResultDataGrid(_FOVTypeImb1_L1, imbImageBox1_L1);
                }
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                imbImageBox1_L1.SetZoomScale(1, e.Location);
                imbImageBox1_L1.AutoScrollOffset = new System.Drawing.Point(0, 0);
            }
        }

        private void imbImageBox2_L1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (imbImageBox2_L1.Image != null)
                {
                    ShowResultDataGrid(_FOVTypeImb2_L1, imbImageBox2_L1);
                }
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                imbImageBox2_L1.SetZoomScale(1, e.Location);
                imbImageBox2_L1.AutoScrollOffset = new System.Drawing.Point(0, 0);
            }
        }

        private void imbImageBox1_L2_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (imbImageBox1_L2.Image != null)
                {
                    ShowResultDataGrid(_FOVTypeImb1_L2, imbImageBox1_L2);
                }
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                imbImageBox1_L2.SetZoomScale(1, e.Location);
                imbImageBox1_L2.AutoScrollOffset = new System.Drawing.Point(0, 0);
            }
        }

        private void imbImageBox2_L2_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (imbImageBox2_L2.Image != null)
                {

                    ShowResultDataGrid(_FOVTypeImb2_L2, imbImageBox2_L2);
                }
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                imbImageBox2_L2.SetZoomScale(1, e.Location);
                imbImageBox2_L2.AutoScrollOffset = new System.Drawing.Point(0, 0);
            }

        }


        public void ShowResultDialog(FOVType fovtype, Emgu.CV.UI.ImageBox imagebox)
        {
            if (!IsOpenResultDialog)
            {
                Image<Bgr, byte> sourceImage = imagebox.Image as Image<Bgr, byte>;
                ImageResultDialog imageResult = new ImageResultDialog();

                imageResult.imbImageResult.Image = sourceImage;

                List<string> list = new List<string>();

                var fResult = _FOVResults.Find(x => x.FOVType == fovtype);
                if (fResult != null)
                {
                    imageResult.txbFOVType.Text = $"FOVType: {fovtype}";

                    foreach (var rsmd in fResult.SMDs)
                    {
                        string message = $"\r\n Name: {rsmd.SMD.Name} \r Score: {Math.Round(rsmd.Result.Score, 2)}\r\n ==> Result: {rsmd.Result.Result}";
                        list.Add(message);
                    }
                    imageResult.lblInfomation.Content = string.Join(Environment.NewLine, list);
                }
                imageResult.Show();

                IsOpenResultDialog = true;
            }
        }


        public void ShowResultDataGrid(FOVType fovtype, Emgu.CV.UI.ImageBox imagebox)
        {
            if (!IsOpenResultDialog)
            {
                Image<Bgr, byte> sourceImage = imagebox.Image as Image<Bgr, byte>;
                ImageResultDialog imageResult = new ImageResultDialog();

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

        public int CheckTerminal(string SN, int timeout = 10000)
        {
            string data = SN.PadRight(25) + "CHECK";
            if (_param.Terminal.IsEnabled)
            {
                _device.Terminal.SerialWriteData(data);
                bool isTimeout = false;

                for (int i = 0; i < timeout / 400; i++)
                {
                    string responseData = _device.Terminal.DataReceived;
                    if (responseData.Contains("ROUTING") && responseData.Contains("PASS"))
                    {
                        isTimeout = true;
                        return 1;
                    }
                    else if (responseData.Contains("SOLDER_CAP") && responseData.Contains("PASS"))
                    {
                        isTimeout = true;
                        return 2;
                    }
                    else if (responseData.Contains("ERRO"))
                    {
                        isTimeout = true;
                        return 0;
                    }
                    Thread.Sleep(25);
                }
                if (!isTimeout)
                {
                    return -1;
                }
            }
            else
            {
                return -2;
            }
            return -2;
        }

        public int CheckTerminal(string SN, bool fRet)
        {
            string pResult = fRet ? "PASS" : "FAIL";
            string pErrorCode = fRet ? "" : "PCB001";
            if (_param.Terminal.IsEnabled)
            {
                string data = SN.PadRight(25) + pResult;
                _device.Terminal.SerialWriteData(data);
                int timeout = 10000;
                for (int i = 0; i < timeout / 400; i++)
                {
                    string response = _device.Terminal.DataReceived;
                    if (response == data + pErrorCode.PadRight(10) + "PASS")
                    {
                        return 1;
                    }
                    else if (response == data + pErrorCode.PadRight(10) + "ERRO")
                    {
                        // FAIL
                        return 0;
                    }
                    Thread.Sleep(25);
                }
            }
            else
            {
                return -2;
            }
            return -2;
        }

        private int CheckType(FOVType type, FOVType targetType = FOVType.Unknow)
        {
            try
            {
                FOVType tempType = targetType != FOVType.Unknow ? targetType : type;
                LogInfo($"CheckType: {tempType}");
                int fRet = 1;
                FOV pFOV = _program.FOVs.FirstOrDefault(x => x.IsEnabled == true && x.FOVType == type);
                if (pFOV == null)
                {
                    fRet = -1;
                }
                else
                {
                    _SMDResults.Clear();
                    int nRet = CheckFOV(pFOV);
                    if (nRet != 1)
                    {
                        fRet = -1;
                    }
                }
                return fRet;
            }
            catch (Exception ex)
            {
                LogError("CheckType :" + ex.Message);
                return -1;
            }
        }

        private int CheckFOV(FOV pFOV)
        {
            int fRet = 1;
            ShowFOVImage(pFOV.CameraMode, null, pFOV.FOVType);
            if(_param.DebugMode)
            {
                MessageBoxResult result = MessageShow.YesNoCancel($"Checking... {pFOV.FOVType}", "DebugMode");
                if(result == MessageBoxResult.Yes)
                {
                    LogInfo($"CheckFOV.DebugMode : {pFOV.FOVType} = 1");
                    return 1;
                }
                else if(result == MessageBoxResult.No)
                {
                    LogInfo($"CheckFOV.DebugMode : {pFOV.FOVType} = 0" );
                    return 0;
                }
                else
                {
                    LogInfo($"CheckFOV.DebugMode : {pFOV.FOVType} = -1");
                }
            }


            ICamera pCamera = GetFOVParams(pFOV.CameraMode);
            if (pCamera != null)
            {
                using (Bitmap bmp = GetFOVBitmap(pCamera, pFOV))
                {
                    if (bmp != null)
                    {
                        using (Image<Bgr, byte> src = bmp.ToImage<Bgr, byte>())
                        using (Image<Bgr, byte> dst = bmp.ToImage<Bgr, byte>())
                        {
                            IEnumerable<SMD> pSMDs = pFOV.SMDs.Where(x => x.IsEnabled = true);
                            if (pSMDs == null)
                            {
                                fRet = -1;
                            }
                            else
                            {
                                System.Windows.Point offset = GetOffsetROI(pFOV, src);
                                foreach (var SMD in pSMDs)
                                {
                                    Rectangle offsetROI = new Rectangle
                                    {
                                        X = SMD.ROI.Rectangle.X + (int)offset.X,
                                        Y = SMD.ROI.Rectangle.Y + (int)offset.Y,
                                        Width = SMD.ROI.Rectangle.Width,
                                        Height = SMD.ROI.Rectangle.Height
                                    };

                                    src.ROI = offsetROI;
                                    dst.ROI = offsetROI;
                                    CvResult cvRet = CheckSMD(SMD, src.Copy(), dst);
                                    if (!cvRet.Result)
                                    {
                                        fRet = -1;
                                    }
                                    _SMDResults.Add(new SMDResult { SMD = SMD, Result = cvRet });
                                    src.ROI = new Rectangle();
                                    dst.ROI = new Rectangle();
                                    _image = dst.Clone();

                                    _FOVResults.Add(new FOVResult { SMDs = _SMDResults });

                                    List<SMDResult> temp = new List<SMDResult>();
                                    if (_FOVResults.Count > 0)
                                    {
                                        foreach (var item in _SMDResults)
                                        {
                                            temp.Add(item);
                                        }
                                    }
                                    UpdateFOVResult(pFOV.FOVType, fRet, _SN, _image, temp);
                                }
                            }

                            //UpdateFOVResult(pFOV.FOVType, fRet, _SN, dst.Clone(), _SMDResults);
                            ShowFOVImage(pFOV.CameraMode, dst.Clone(), pFOV.FOVType);
                        }
                    }
                    else
                    {
                        fRet = -1;
                        string message = $"CheckFOV: No bitmap ({pFOV.CameraMode},{pFOV.FOVType}";
                        MessageShow.Error(message, "Error");
                        LogError(message);
                    }
                }
            }
            else
            {
                fRet = -1;
                string message = $"CheckFOV: No Camera ({pFOV.CameraMode}, {pFOV.FOVType})";
                LogError(message);

                MessageShow.Error(message, "Error");
            }
            return fRet;
        }

        private CvResult CheckSMD(SMD pSMD, Image<Bgr, byte> src, Image<Bgr, byte> dst)
        {
            CvResult cvRet = new CvResult();
            switch (pSMD.Algorithm)
            {
                case SMDAlgorithm.Unknow:
                    break;

                case SMDAlgorithm.TemplateMatching:
                    {
                        cvRet = pSMD.TemplateMatching.Run(src, dst, pSMD.ROI.Rectangle);
                        break;
                    }
                case SMDAlgorithm.CodeRecognition:
                    {
                        cvRet = pSMD.CodeRecognition.Run(src, dst, pSMD.ROI.Rectangle);
                        if (cvRet.Result)
                        {
                            _SN = cvRet.Content;
                            Console.WriteLine("SN= " + _SN);
                        }
                        else
                        {
                            _SN = string.Empty;
                            Console.WriteLine("SN = Empty");
                        }
                        break;
                    }
                case SMDAlgorithm.HSVExtraction:
                    {
                        cvRet = pSMD.HSVExtraction.Run(src, dst, pSMD.ROI.Rectangle);
                        break;
                    }
                case SMDAlgorithm.LuminanceExtraction:
                    {
                        cvRet = pSMD.LuminanceExtraction.Run(src, dst, pSMD.ROI.Rectangle);
                        break;
                    }
                default:
                    break;
            }
            return cvRet;
        }

        private Bitmap GetFOVBitmap(ICamera pCamera, FOV pFOV)
        {
            pCamera.SetParameter(KeyName.ExposureTime, pFOV.ExposureTime);
            pCamera.ClearImageBuffer();
            pCamera.SetParameter(KeyName.TriggerSoftware, 1);
            using (Bitmap bmp = pCamera.GrabFrame())
            {
                if (bmp != null)
                {
                    return (Bitmap)bmp.Clone();
                }
            }
            return null;
        }

        private System.Windows.Point GetOffsetROI(FOV pFOV, Image<Bgr, byte> image)
        {
            try
            {
                System.Windows.Point offset = new System.Windows.Point();
                using (Image<Bgr, byte> src = image.Clone())
                using (Image<Bgr, byte> dst = image.Clone())
                {
                    SMD pSMD = pFOV.SMDs.FirstOrDefault(x => x.IsEnabled == true && x.SMDType == SMDType.Mark);
                    if (pSMD != null)
                    {
                        src.ROI = pSMD.ROI.Rectangle;
                        dst.ROI = pSMD.ROI.Rectangle;
                        CvResult cvRet;
                        switch (pSMD.Algorithm)
                        {
                            case SMDAlgorithm.Unknow:
                                break;
                            case SMDAlgorithm.TemplateMatching:
                                {
                                    cvRet = pSMD.TemplateMatching.Run(src.Copy(), dst, pSMD.ROI.Rectangle);
                                    offset.X = (int)cvRet.Center.X - (int)pSMD.TemplateMatching.Center.X;
                                    offset.Y = (int)cvRet.Center.Y - (int)pSMD.TemplateMatching.Center.Y;
                                    break;
                                }
                            case SMDAlgorithm.CodeRecognition:
                                break;
                            case SMDAlgorithm.HSVExtraction:
                                break;
                            case SMDAlgorithm.LuminanceExtraction:
                                break;
                            default:
                                break;
                        }
                        src.ROI = new Rectangle();
                        dst.ROI = new Rectangle();
                        ShowFOVImage(pFOV.CameraMode, dst.Clone(), pFOV.FOVType);
                    }
                }
                return offset;
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                return new System.Windows.Point();
            }
        }

        public void ShowFOVImage(CameraMode cameraMode, Image<Bgr, byte> image, FOVType fovtype)
        {
            if (image != null)
            {
                if (cameraMode != CameraMode.Unknow)
                {
                    if (fovtype == FOVType.L1_LABEL1 || fovtype == FOVType.L1_COLOR1 || fovtype == FOVType.L1_SOLDER_CAP1)
                    {
                        imbImageBox1_L1.Image = image;

                        switch (fovtype)
                        {
                            case FOVType.L1_LABEL1:
                                {
                                    _FOVTypeImb1_L1 = FOVType.L1_LABEL1;
                                    break;
                                }

                            case FOVType.L1_COLOR1:
                                {
                                    _FOVTypeImb1_L1 = FOVType.L1_COLOR1;
                                    break;
                                }

                            case FOVType.L1_SOLDER_CAP1:
                                {
                                    _FOVTypeImb1_L1 = FOVType.L1_SOLDER_CAP1;
                                    break;
                                }
                        }
                    }
                    else if (fovtype == FOVType.L1_LABEL2 || fovtype == FOVType.L1_COLOR2 || fovtype == FOVType.L1_SOLDER_CAP2)
                    {
                        imbImageBox2_L1.Image = image;

                        switch (fovtype)
                        {
                            case FOVType.L1_LABEL2:
                                {
                                    _FOVTypeImb2_L1 = FOVType.L1_LABEL2;
                                    break;
                                }

                            case FOVType.L1_COLOR2:
                                {
                                    _FOVTypeImb2_L1 = FOVType.L1_COLOR2;
                                    break;
                                }

                            case FOVType.L1_SOLDER_CAP2:
                                {
                                    _FOVTypeImb2_L1 = FOVType.L1_SOLDER_CAP2;
                                    break;
                                }
                        }
                    }

                    else if (fovtype == FOVType.L2_LABEL1 || fovtype == FOVType.L2_COLOR1 || fovtype == FOVType.L2_SOLDER_CAP1)
                    {
                        imbImageBox1_L2.Image = image;

                        switch (fovtype)
                        {
                            case FOVType.L2_LABEL1:
                                {
                                    _FOVTypeImb1_L2 = FOVType.L2_LABEL1;
                                    break;
                                }

                            case FOVType.L2_COLOR1:
                                {
                                    _FOVTypeImb1_L2 = FOVType.L2_COLOR1;
                                    break;
                                }

                            case FOVType.L2_SOLDER_CAP1:
                                {
                                    _FOVTypeImb1_L2 = FOVType.L2_SOLDER_CAP1;
                                    break;
                                }
                        }
                    }
                    else
                    {
                        imbImageBox2_L2.Image = image;
                        switch (fovtype)
                        {
                            case FOVType.L2_LABEL2:
                                {
                                    _FOVTypeImb2_L2 = FOVType.L2_LABEL2;
                                    break;
                                }

                            case FOVType.L2_COLOR2:
                                {
                                    _FOVTypeImb2_L2 = FOVType.L2_COLOR2;
                                    break;
                                }

                            case FOVType.L2_SOLDER_CAP2:
                                {
                                    _FOVTypeImb2_L2 = FOVType.L2_SOLDER_CAP2;
                                    break;
                                }
                        }
                    }
                }
            }
            else
            {
                if (fovtype == FOVType.L1_LABEL1 || fovtype == FOVType.L1_COLOR1 || fovtype == FOVType.L1_SOLDER_CAP1)
                {
                    imbImageBox1_L1.Image = null;
                }
                else if (fovtype == FOVType.L1_LABEL2 || fovtype == FOVType.L1_COLOR2 || fovtype == FOVType.L1_SOLDER_CAP2)
                {
                    imbImageBox2_L1.Image = null;
                }
                else if (fovtype == FOVType.L1_LABEL1 || fovtype == FOVType.L1_COLOR1 || fovtype == FOVType.L1_SOLDER_CAP1)
                {
                    imbImageBox1_L2.Image = null;
                }
                else
                {
                    imbImageBox2_L2.Image = null;
                }
            }
        }

        private ICamera GetFOVParams(CameraMode mode)
        {
            DeviceManager device = DeviceManager.Current;
            ICamera camera;
            if (mode == CameraMode.Top)
            {
                camera = device.Camera1;
            }
            else if (mode == CameraMode.Bottom)
            {
                camera = device.Camera2;
            }
            else
            {
                camera = null;
            }
            return camera;
        }

        #region Update Status Operation

        public void StatusMachine()
        {
            string running = "";
            string resetting = "";
            string reseted = "";
            string stop = "";
            //#FFBDAD01 : yellow
            int value = -1;

            if (_device.PLC1.GetDevice(running, ref value, false) == 1)
            {
                brdStatusMachine.Background = System.Windows.Media.Brushes.Green;
                lblStatusMachine.Content = "RUNNING";
            }
            else if (_device.PLC1.GetDevice(resetting, ref value, false) == 1)
            {
                brdStatusMachine.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFBDAD01");
                lblStatusMachine.Content = "RESETTING";
            }
            else if (_device.PLC1.GetDevice(reseted, ref value, false) == 1)
            {
                brdStatusMachine.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFBDAD01");
                lblStatusMachine.Content = "RESETED";
            }

            else if (_device.PLC1.GetDevice(stop, ref value, false) == 1)
            {
                brdStatusMachine.Background = System.Windows.Media.Brushes.Red;
                lblStatusMachine.Content = "STOPPING";
            }
        }

        public void StatusDoor()
        {
            string close = "";
            string open = "";
            int value = -1;
            if (_device.PLC1.GetDevice(close, ref value, false) == 1)
            {
                brdDoor.Background = System.Windows.Media.Brushes.Green;
                lblDoor.Content = "CLOSE";
            }
            else if (_device.PLC1.GetDevice(open, ref value, false) == 1)
            {
                brdDoor.Background = System.Windows.Media.Brushes.Red;
                lblDoor.Content = "OPEN";
            }
        }

        public void StatusSFIS()
        {
            if (_param.Terminal.IsEnabled)
            {
                if (_device.Terminal.IsConnected)
                {
                    lblSFIS.Content = "OK";
                    brdSFIS.Background = System.Windows.Media.Brushes.Green;
                }
                else if (!_device.Terminal.IsConnected)
                {
                    lblSFIS.Content = "Wait...";
                    brdSFIS.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFBDAD01");
                }
            }
        }
        #endregion

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
                Dispatcher.Invoke(() => imbCamera.Image = _frame);
            }
        }


        #endregion
        //=============================================================================================================================
        #region TEST
        private void mnuiTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int numPass = 0;
                int numFail = 0;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                FOV pFOV0 = _program.FOVs[0];
                int nRet0 = CheckType0(pFOV0.FOVType);
                //int nRet = CheckFOV0(pFOV);
                FOVType pCheck0 = pFOV0.FOVType;
                string text0 = nRet0 == 1 ? "PASS" : "FAIL";

                numPass = nRet0 == 1 ? 1 : 0;
                numFail = nRet0 != 1 ? 1 : 0;


                SolidColorBrush color0 = nRet0 == 1 ? System.Windows.Media.Brushes.LimeGreen : System.Windows.Media.Brushes.Red;
                ShowCellResult(pCheck0, text0, color0);
                _statistic.UpdateRate(numPass, numFail);
                UpdateStatusControl("Complete!", 100);

                Thread.Sleep(1000);


                FOV pFOV1 = _program.FOVs[1];
                int nRet1 = CheckType1(pFOV1.FOVType);
                FOVType pCheck1 = pFOV1.FOVType;

                string text1 = nRet1 == 1 ? "PASS" : "FAIL";
                SolidColorBrush color1 = nRet1 == 1 ? System.Windows.Media.Brushes.LimeGreen : System.Windows.Media.Brushes.Red;

                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                CycleTime = string.Format("{0}:{1:00} {2}", (int)elapsedTime.TotalSeconds, elapsedTime.Milliseconds / 10.0, "(s)");

                ShowCellResult(pCheck1, text1, color1);
                //UpdateRate();
                UpdateStatusControl("Complete!", 100);


                NotifyPropertyChanged();

            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        private int CheckType0(FOVType type, FOVType targetType = FOVType.Unknow)
        {
            try
            {
                FOVType tempType = targetType != FOVType.Unknow ? targetType : type;
                LogInfo($"CheckType: {tempType}");
                int fRet = 1;
                FOV pFOV = _program.FOVs.FirstOrDefault(x => x.IsEnabled == true && x.FOVType == type);
                if (pFOV == null)
                {
                    fRet = -1;
                }
                else
                {
                    _SMDResults.Clear();
                    int nRet = CheckFOV0(pFOV);
                    if (nRet != 1)
                    {
                        fRet = -1;
                    }
                    //_FOVResults.Add(new FOVResult { SMDs = _SMDResults });

                    //List<SMDResult> temp = new List<SMDResult>();
                    //if (_FOVResults.Count > 0)
                    //{
                    //    foreach (var item in _SMDResults)
                    //    {
                    //        temp.Add(item);
                    //    }
                    //}
                    //UpdateFOVResult(pFOV.FOVType, fRet, _SN, _image, temp);
                }
                return fRet;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }

        public int CheckFOV0(FOV pFOV)
        {
            int fRet = 1;
            string filepath = _program.ImageBoard.Blocks[0].Filename;
            Bitmap bmp = new Bitmap(filepath);
            if (bmp != null)
            {
                using (Image<Bgr, byte> src = bmp.ToImage<Bgr, byte>())
                using (Image<Bgr, byte> dst = bmp.ToImage<Bgr, byte>())
                {
                    IEnumerable<SMD> pSMDs = pFOV.SMDs.Where(x => x.IsEnabled == true);
                    if (pSMDs == null)
                    {
                        fRet = -1;
                    }
                    else
                    {
                        System.Windows.Point offset = GetOffsetROI(pFOV, src);
                        foreach (var SMD in pSMDs)
                        {
                            Rectangle offsetROI = new Rectangle
                            {
                                X = (int)(SMD.ROI.Rectangle.X + offset.X),
                                Y = (int)(SMD.ROI.Rectangle.Y + offset.Y),
                                Width = SMD.ROI.Rectangle.Width,
                                Height = SMD.ROI.Rectangle.Height
                            };

                            src.ROI = offsetROI;
                            dst.ROI = offsetROI;
                            CvResult cvRet = CheckSMD(SMD, src.Copy(), dst);
                            if (cvRet.Result)
                            {
                                Console.WriteLine($"Check SMD_{SMD.Id} : OK");
                            }
                            else
                            {
                                Console.WriteLine($"Check SMD_{SMD.Id} : FAIL");
                                fRet = -1;
                            }
                            _SMDResults.Add(new SMDResult { SMD = SMD, Result = cvRet });
                            src.ROI = new Rectangle();
                            dst.ROI = new Rectangle();
                            _image = dst.Clone();

                            _FOVResults.Add(new FOVResult { SMDs = _SMDResults });

                            List<SMDResult> temp = new List<SMDResult>();
                            if (_FOVResults.Count > 0)
                            {
                                foreach (var item in _SMDResults)
                                {
                                    temp.Add(item);
                                }
                            }
                            UpdateFOVResult(pFOV.FOVType, fRet, _SN, _image, temp);
                        }
                    }
                    ShowFOVImage(pFOV.CameraMode, dst.Clone(), pFOV.FOVType);
                    _database.AddData(_SN);
                    // _database.Data.Remove(_database.Data.Find(x => x.SN == _SN));
                    NotifyPropertyChanged();
                }
            }
            return fRet;
        }

        private int CheckType1(FOVType type, FOVType targetType = FOVType.Unknow)
        {
            try
            {
                FOVType tempType = targetType != FOVType.Unknow ? targetType : type;
                LogInfo($"CheckType: {tempType}");
                int fRet = 1;
                FOV pFOV = _program.FOVs.FirstOrDefault(x => x.IsEnabled == true && x.FOVType == type);
                if (pFOV == null)
                {
                    fRet = -1;
                }
                else
                {
                    _SMDResults.Clear();
                    //  int nRet = CheckFOV(pFOV);
                    int nRet = CheckFOV1(pFOV);
                    if (nRet != 1)
                    {
                        fRet = -1;
                    }
                }
                return fRet;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }


        public int CheckFOV1(FOV pFOV)
        {
            int fRet = 1;
            string filepath = _program.ImageBoard.Blocks[1].Filename;
            Bitmap bmp = new Bitmap(filepath);
            if (bmp != null)
            {


                using (Image<Bgr, byte> src = bmp.ToImage<Bgr, byte>())
                using (Image<Bgr, byte> dst = bmp.ToImage<Bgr, byte>())
                {
                    IEnumerable<SMD> pSMDs = pFOV.SMDs.Where(x => x.IsEnabled == true);
                    if (pSMDs == null)
                    {
                        fRet = -1;
                    }
                    else
                    {
                        System.Windows.Point offset = GetOffsetROI(pFOV, src);
                        foreach (var SMD in pSMDs)
                        {
                            Rectangle offsetROI = new Rectangle
                            {
                                X = (int)(SMD.ROI.Rectangle.X + offset.X),
                                Y = (int)(SMD.ROI.Rectangle.Y + offset.Y),
                                Width = SMD.ROI.Rectangle.Width,
                                Height = SMD.ROI.Rectangle.Height
                            };

                            src.ROI = offsetROI;
                            dst.ROI = offsetROI;
                            CvResult cvRet = CheckSMD(SMD, src.Copy(), dst);
                            if (cvRet.Result)
                            {
                                Console.WriteLine($"Check SMD_{SMD.Id} : OK");
                            }
                            else
                            {
                                Console.WriteLine($"Check SMD_{SMD.Id} : FAIL");
                                fRet = -1;
                            }
                            _SMDResults.Add(new SMDResult { SMD = SMD, Result = cvRet });


                            src.ROI = new Rectangle();
                            dst.ROI = new Rectangle();
                            _image = dst.Clone();

                            _FOVResults.Add(new FOVResult { SMDs = _SMDResults });

                            List<SMDResult> temp = new List<SMDResult>();
                            if (_FOVResults.Count > 0)
                            {
                                foreach (var item in _SMDResults)
                                {
                                    temp.Add(item);
                                }
                            }
                            UpdateFOVResult(pFOV.FOVType, fRet, _SN, _image, temp);
                        }
                    }
                    ShowFOVImage(pFOV.CameraMode, dst.Clone(), pFOV.FOVType);
                    _database.AddData(_SN);
                    // _database.Data.Remove(_database.Data.Find(x => x.SN == _SN));
                    NotifyPropertyChanged();
                }
            }
            return fRet;
        }


        // ====================================================================================================================================

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


        public void Result(FOVType fovtype, int result, string SN, Image<Bgr, byte> image)
        {
            fovResult.Result = result;
            fovResult.SN = SN;
            fovResult.Image = image;
            fovResult.FOVType = fovtype;
        }

        public void UpdateStatusControl(string text, int progress)
        {
            Dispatcher.Invoke(() =>
            {
                txbStatusBar.Text = text;
                prbStatus.Value = progress;
            });
        }


        public int IsAdmin()
        {
            LoginDialog loginDialog = new LoginDialog();
            loginDialog.ShowDialog();
            if (loginDialog.txtUsername.Text.ToLower() == "admin" && loginDialog.Password.Password.ToLower() == "admin".ToLower() && !loginDialog.Cancel())
            {
                return 1;
            }
            else if (loginDialog.txtUsername.Text.ToLower() == "at" && loginDialog.Password.Password.ToLower() == "foxconnat" && !loginDialog.Cancel())
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

        private void SaveFOVResults(FOVType pL1_LABLE1, FOVType pL1_COLOR1, FOVType pL1_SOLDER_CAP1, FOVType pL2_LABLE1, FOVType pL2_COLOR1, FOVType pL2_SOLDER_CAP1, FOVType pL1_LABLE2, FOVType pL1_COLOR2, FOVType pL1_SOLDER_CAP2, FOVType pL2_LABLE2, FOVType pL2_COLOR2, FOVType pL2_SOLDER_CAP2)
        {
            FOVResult fResultPCB1 = _FOVResults.Find(x => x.FOVType == pL1_LABLE1 || x.FOVType == pL2_LABLE1);
            if (fResultPCB1 != null)
            {
                string SN = fResultPCB1.SN;
                if (SN != string.Empty)
                {
                    string localFolder = @$"{AppDomain.CurrentDomain.BaseDirectory}logs\{DateTime.Now:yyyy-MM-dd}\{DateTime.Now:HH-mm-ss}_{SN}\";
                    FileExplorer.CreateDirectory(localFolder);
                    string filename;
                    if (fResultPCB1.WorkerConfirm != string.Empty)
                    {
                        filename = $"Image_{fResultPCB1.WorkerConfirm}_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                    }
                    else
                    {
                        filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                    }
                    if (!File.Exists(filename))
                    {
                        CvImage.Save(localFolder, filename, fResultPCB1.Image.Mat, 50);
                    }


                    FOVResult fResultL1_COLOR1 = _FOVResults.Find(x => x.FOVType == pL1_COLOR1);
                    if (fResultL1_COLOR1 != null)
                    {
                        if (fResultL1_COLOR1.WorkerConfirm != string.Empty)
                        {
                            filename = $"Image_{fResultL1_COLOR1.WorkerConfirm}_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        else
                        {
                            filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        if (!File.Exists(filename))
                        {
                            CvImage.Save(localFolder, filename, fResultL1_COLOR1.Image.Mat, 50);
                        }
                    }

                    FOVResult fResultL1_SOLDER_CAP1 = _FOVResults.Find(x => x.FOVType == pL1_SOLDER_CAP1);
                    if (fResultL1_SOLDER_CAP1 != null)
                    {
                        if (fResultL1_SOLDER_CAP1.WorkerConfirm != string.Empty)
                        {
                            filename = $"Image_{fResultL1_SOLDER_CAP1.WorkerConfirm}_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        else
                        {
                            filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        if (!File.Exists(filename))
                        {
                            CvImage.Save(localFolder, filename, fResultL1_SOLDER_CAP1.Image.Mat, 50);
                        }
                    }

                    FOVResult fResultL2_COLOR1 = _FOVResults.Find(x => x.FOVType == pL2_COLOR1);
                    if (fResultL2_COLOR1 != null)
                    {
                        if (fResultL2_COLOR1.WorkerConfirm != string.Empty)
                        {
                            filename = $"Image_{fResultL2_COLOR1.WorkerConfirm}_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        else
                        {
                            filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        if (!File.Exists(filename))
                        {
                            CvImage.Save(localFolder, filename, fResultL2_COLOR1.Image.Mat, 50);
                        }
                    }

                    FOVResult fResultL2_SOLDER_CAP1 = _FOVResults.Find(x => x.FOVType == pL2_SOLDER_CAP1);
                    if (fResultL2_SOLDER_CAP1 != null)
                    {
                        if (fResultL2_SOLDER_CAP1.WorkerConfirm != string.Empty)
                        {
                            filename = $"Image_{fResultL2_SOLDER_CAP1.WorkerConfirm}_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        else
                        {
                            filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        if (!File.Exists(filename))
                        {
                            CvImage.Save(localFolder, filename, fResultL2_SOLDER_CAP1.Image.Mat, 50);
                        }
                    }
                }
            }

            FOVResult fResultPCB2 = _FOVResults.Find(x => x.FOVType == pL1_LABLE2 || x.FOVType == pL2_LABLE2);
            if (fResultPCB2 != null)
            {
                string SN = fResultPCB2.SN;
                if (SN != string.Empty)
                {
                    string localFolder = @$"{AppDomain.CurrentDomain.BaseDirectory}logs\{DateTime.Now:yyyy-MM-dd}\{DateTime.Now:HH-mm-ss}_{SN}\";
                    FileExplorer.CreateDirectory(localFolder);
                    string filename;
                    if (fResultPCB2.WorkerConfirm != string.Empty)
                    {
                        filename = $"Image_{fResultPCB2.WorkerConfirm}_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                    }
                    else
                    {
                        filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                    }
                    if (!File.Exists(filename))
                    {
                        CvImage.Save(localFolder, filename, fResultPCB2.Image.Mat, 50);
                    }


                    FOVResult fResultL1_COLOR2 = _FOVResults.Find(x => x.FOVType == pL1_COLOR2);
                    if (fResultL1_COLOR2 != null)
                    {
                        if (fResultL1_COLOR2.WorkerConfirm != string.Empty)
                        {
                            filename = $"Image_{fResultL1_COLOR2.WorkerConfirm}_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        else
                        {
                            filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        if (!File.Exists(filename))
                        {
                            CvImage.Save(localFolder, filename, fResultL1_COLOR2.Image.Mat, 50);
                        }
                    }

                    FOVResult fResultL1_SOLDER_CAP2 = _FOVResults.Find(x => x.FOVType == pL1_SOLDER_CAP2);
                    if (fResultL1_SOLDER_CAP2 != null)
                    {
                        if (fResultL1_SOLDER_CAP2.WorkerConfirm != string.Empty)
                        {
                            filename = $"Image_{fResultL1_SOLDER_CAP2.WorkerConfirm}_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        else
                        {
                            filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        if (!File.Exists(filename))
                        {
                            CvImage.Save(localFolder, filename, fResultL1_SOLDER_CAP2.Image.Mat, 50);
                        }

                    }

                    FOVResult fResultL2_COLOR2 = _FOVResults.Find(x => x.FOVType == pL2_COLOR2);
                    if (fResultL2_COLOR2 != null)
                    {
                        if (fResultL2_COLOR2.WorkerConfirm != string.Empty)
                        {
                            filename = $"Image_{fResultL2_COLOR2.WorkerConfirm}_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        else
                        {
                            filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        if (!File.Exists(filename))
                        {
                            CvImage.Save(localFolder, filename, fResultL2_COLOR2.Image.Mat, 50);
                        }
                    }

                    FOVResult fResultL2_SOLDER_CAP2 = _FOVResults.Find(x => x.FOVType == pL2_SOLDER_CAP2);
                    if (fResultL2_SOLDER_CAP2 != null)
                    {
                        if (fResultL2_SOLDER_CAP2.WorkerConfirm != string.Empty)
                        {
                            filename = $"Image_{fResultL2_SOLDER_CAP2.WorkerConfirm}_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        else
                        {
                            filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        if (!File.Exists(filename))
                        {
                            CvImage.Save(localFolder, filename, fResultL2_SOLDER_CAP2.Image.Mat, 50);
                        }
                    }
                }
            }
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

}



