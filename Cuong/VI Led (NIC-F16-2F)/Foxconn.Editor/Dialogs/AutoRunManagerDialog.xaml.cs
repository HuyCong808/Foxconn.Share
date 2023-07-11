using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.Editor.Camera;
using Foxconn.Editor.Configuration;
using Foxconn.Editor.Enums;
using Foxconn.Editor.OpenCV;
using Foxconn.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Emgu.CV.BarcodeDetector;
using SystemColors = Foxconn.Editor.Enums.SystemColors;

namespace Foxconn.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for AutoRunManager.xaml
    /// </summary>
    public partial class AutoRunManagerDialog : Window, INotifyPropertyChanged
    {
        public static AutoRunManagerDialog Current;
        private Worker _loopWorker;
        private Worker _loopMonitor;
        private Board _program => ProgramManager.Current.Program;
        private Properties.Settings _settings = Properties.Settings.Default;
        private MachineParams _param = MachineParams.Current;
        private DeviceManager _device = DeviceManager.Current;
        private List<FOVResult> _FOVResults = new List<FOVResult>();
        private List<SMDResult> _SMDResults = new List<SMDResult>();
        private Image<Bgr, byte> _image = null;
        private VideoCapture _webcamMonitor = null;
        private Mat _frame = null;

        public bool IsRunning => _loopWorker.IsRunning;
        public bool IsMonitoring => _loopMonitor.IsRunning;

        #region Binding Property
        private string _errorMessage = string.Empty;

        public string BoardName => _program.Name;

        public string Barcode => GlobalDataManager.Current.Barcode;

        public string CycleTime => GlobalDataManager.Current.CycleTime;

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                NotifyPropertyChanged();
            }
        }

        public int TotalPass
        {
            get => _settings.TotalPass;
            set
            {
                _settings.TotalPass = value;
                NotifyPropertyChanged();
            }
        }

        public int TotalFail
        {
            get => _settings.TotalFail;
            set
            {
                _settings.TotalFail = value;
                NotifyPropertyChanged();
            }
        }

        public double YeildRate
        {
            get => _settings.YeildRate;
            set
            {
                _settings.YeildRate = value;
                NotifyPropertyChanged();
            }
        }

        public string ScannerData
        {
            get => GlobalDataManager.Current.ScannerData;
            set
            {
                GlobalDataManager.Current.ScannerData = value;
                NotifyPropertyChanged();
            }
        }

        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public AutoRunManagerDialog()
        {
            InitializeComponent();
            Current = this;
            DataContext = this;
            _loopWorker = new Worker(new ThreadStart(AutoRunProcess));
            _loopMonitor = new Worker(new ThreadStart(AutoRunProcessMonitoring));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Stop();
        }

        public bool Run()
        {
            try
            {
                if (IsRunning)
                {
                    return true;
                }
                if (!Prepare() || _program == null)
                {
                    return false;
                }
                GlobalDataManager.Current.IsAutoRun = true;
                _loopWorker.Start();
                _loopMonitor.Start();
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
        }

        public void Stop()
        {
            try
            {
                if (!IsRunning || !IsMonitoring)
                {
                    return;
                }
                GlobalDataManager.Current.IsAutoRun = false;
                _webcamMonitor?.Dispose();
                _loopWorker.Stop();
                _loopMonitor.Stop();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                MainWindow.Current.Show();
            }
        }

        private void AutoRunProcess()
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Auto run thread.";
            int count = 5;
            FOVType pCheck = FOVType.Unknow;
            while (!_loopWorker.WaitStopSignal(10))
            {
                try
                {
                    UpdateStatusBar(string.Concat(Enumerable.Repeat(".", count / 5)), SystemColors.White);
                    count = (count / 5 == 5) ? 5 : ++count;

                    pCheck = FOVType.SN;
                    int nRet = CheckType(pCheck);
                    if (nRet == 1)
                    {
                        UpdateStatusBar(GlobalDataManager.Current.Barcode, SystemColors.Yellow);
                    }

                    //int fRet = 0, nRet = 0, s = 100 / MachineParams.Current.NumCheck, i = 0;
                    //string sRet = string.Empty;
                    //if (GlobalDataManager.Current.Scan)
                    //{
                    //    GlobalDataManager.Current.Scan = false;
                    //    switch (GlobalDataManager.Current.Num)
                    //    {
                    //        case 1:
                    //            {
                    //                pCheck = FOVType.LED1;
                    //                break;
                    //            }
                    //        case 2:
                    //            {
                    //                pCheck = FOVType.LED2;
                    //                break;
                    //            }
                    //        case 3:
                    //            {
                    //                pCheck = FOVType.LED3;
                    //                break;
                    //            }
                    //        case 4:
                    //            {
                    //                pCheck = FOVType.LED4;
                    //                break;
                    //            }
                    //        case 5:
                    //            {
                    //                pCheck = FOVType.LED5;
                    //                break;
                    //            }
                    //        case 6:
                    //            {
                    //                pCheck = FOVType.LED6;
                    //                break;
                    //            }
                    //        default:
                    //            break;
                    //    }
                    //    Stopwatch stopwatch = new Stopwatch();
                    //    stopwatch.Start();
                    //    GlobalDataManager.Current.IsDoingInspection = true;
                    //    GlobalDataManager.Current.BreakFlow = false;
                    //    GlobalDataManager.Current.Barcode = string.Empty;

                    //    for (int j = 0; j < MachineParams.Current.NumCheck; j++)
                    //    {
                    //        Trace.WriteLine($"[{j}]");
                    //        ++i;
                    //        UpdateStatusBar(s * i, SystemColors.Yellow);
                    //        nRet = CheckType(pCheck);
                    //        if (nRet == 1 || AnalyzeResults(pCheck))
                    //        {
                    //            SaveResults(pCheck);
                    //            fRet = 1;
                    //            break;
                    //        }
                    //        else
                    //        {
                    //            SaveResults(pCheck);
                    //        }
                    //    }
                    //    fRet = AnalyzeResults(pCheck) ? 1 : -1;
                    //    sRet = fRet == 1 ? "PASS" : "FAIL";
                    //    UpdateStatusBar(sRet, SystemColors.White);
                    //    UpdateStatusBar(100, fRet == 1 ? SystemColors.Green : SystemColors.Red);
                    //    _device.ProgramTest.SocketWriteData(sRet);
                    //    Trace.WriteLine($"Final Result: {sRet}");

                    //    UpdateStatusBar(0, SystemColors.Gray);
                    //    stopwatch.Stop();
                    //    GlobalDataManager.Current.IsDoingInspection = false;
                    //    GlobalDataManager.Current.BreakFlow = false;
                    //    GlobalDataManager.Current.Barcode = string.Empty;
                    //    GlobalDataManager.Current.CycleTime = stopwatch.Elapsed.ToString();
                    //    GlobalOptimizing.FreeMemory();
                    //}
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    MessageBox.Show(ex.Message, "AutoRun", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
        }

        private void AutoRunProcessMonitoring()
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Auto run monitoring thread.";
            int count = 0;
            while (!_loopMonitor.WaitStopSignal(100))
            {
                try
                {
                    #region Ping Devices
                    ++count;
                    if (count == 10)
                    {
                        count = 0;
                        _device.PingDevices();
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
        }

        private bool Prepare()
        {
            bool bRet = true;
            try
            {
                int nRet = _device.PingDevices();
                if (nRet != 1)
                {
                    bRet = false;
                }

                if (_param.WebcamMonitor)
                {
                    OpenWebcam();
                }

                UpdateYieldRate(0, 0);
                CvImage.Clear("", maxDayCount: 7);
            }
            catch (Exception ex)
            {
                bRet = false;
                Trace.WriteLine(ex);
            }
            return bRet;
        }

        private void OpenWebcam()
        {
            Task.Run(() =>
            {
                CvInvoke.UseOpenCL = false;
                try
                {
                    _frame = new Mat();
                    _webcamMonitor = new VideoCapture(0);
                    int fourcc = VideoWriter.Fourcc('M', 'J', 'P', 'G');
                    _webcamMonitor.Set(Emgu.CV.CvEnum.CapProp.FourCC, fourcc);
                    _webcamMonitor.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
                    _webcamMonitor.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, 1080);
                    _webcamMonitor.ImageGrabbed += ProcessFrame;
                    _webcamMonitor.Start();
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            });
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (_webcamMonitor != null && _webcamMonitor.Ptr != IntPtr.Zero)
            {
                _webcamMonitor.Retrieve(_frame, 0);
                Dispatcher.Invoke(() => imageBox.SourceFromBitmap = _frame.ToBitmap());
            }
        }

        private void mnuiExit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiWindowsConsole_Click(object sender, RoutedEventArgs e)
        {
            Extensions.SwitchConsole();
        }

        private void mnuiOpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory);
        }

        private void mnuiViewHelp_Click(object sender, RoutedEventArgs e)
        {
            string email = "quang-tiep.nguyen@mail.foxconn.com";
            string contents = "subject=[Foxconn Application] Request Support&body=Dear Team, ...";
            string link = $"mailto:{email}?{contents}";
            Process.Start(new ProcessStartInfo
            {
                FileName = link,
                UseShellExecute = true
            });
        }

        private void mnuiAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog dialog = new AboutDialog();
            dialog.ShowDialog();
        }

        private void imageBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomScale = imageBox.ZoomScale;
            if (e.Delta > 0)
            {
                zoomScale = 1.2 * zoomScale;
                if (zoomScale < imageBox.MaximumScale)
                {
                    imageBox.ZoomScale = zoomScale;
                }
            }
            else
            {
                zoomScale = 0.8 * zoomScale;
                if (zoomScale > imageBox.MinimumScale)
                {
                    imageBox.ZoomScale = zoomScale;
                }
            }
        }

        private void btnBreakFlow_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r = MessageBox.Show("Break?", "Test Flow", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            if (r == MessageBoxResult.OK)
            {
                UpdateStatusBar("Break Flow", SystemColors.White);
                Trace.WriteLine("AutoRunManager: BreakFlow = IsEnabled");
                GlobalDataManager.Current.BreakFlow = true;
            }
        }

        public void UpdateStatusBar(string text, SystemColors color)
        {
            Dispatcher.Invoke(() =>
            {
                statusBar.StateText = text;
                statusBar.StateColor = MyColors.GetColor(color);
            });
        }

        public void UpdateStatusBar(int progress, SystemColors color)
        {
            Dispatcher.Invoke(() =>
            {
                statusBar.Progress = progress;
                statusBar.CircleColor = MyColors.GetColor(color);
            });
        }

        private (ICamera, FOV) GetFOVParams(CameraMode mode, FOVType type)
        {
            DeviceManager device = DeviceManager.Current;
            FOV pFOV = _program.FOVs.FirstOrDefault(x => x.IsEnabled == true && x.CameraMode == mode && x.Type == type);
            if (pFOV != null)
            {
                ICamera camera;
                if (pFOV.CameraMode == CameraMode.Top)
                {
                    camera = device.Camera1;
                }
                else if (pFOV.CameraMode == CameraMode.Bottom)
                {
                    camera = device.Camera2;
                }
                else
                {
                    camera = null;
                }
                return (camera, pFOV);
            }
            return (null, null);
        }

        private (ICamera, FOV) GetFOVParams(FOVType type)
        {
            DeviceManager device = DeviceManager.Current;
            FOV pFOV = _program.FOVs.FirstOrDefault(x => x.IsEnabled == true && x.Type == type);
            if (pFOV != null)
            {
                ICamera camera;
                if (pFOV.CameraMode == CameraMode.Top)
                {
                    camera = device.Camera1;
                }
                else if (pFOV.CameraMode == CameraMode.Bottom)
                {
                    camera = device.Camera2;
                }
                else
                {
                    camera = null;
                }
                return (camera, pFOV);
            }
            return (null, null);
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

        private Bitmap GetFOVBitmap(ICamera pCamera, FOV pFOV)
        {
            Bitmap bitmap = null;
            int times = _param.LoopCapture > 0 ? _param.LoopCapture : 1;
            pCamera.SetParameter(KeyName.ExposureTime, pFOV.ExposureTime);
            pCamera.ClearImageBuffer();
            for (int i = 0; i < times; i++)
            {
                pCamera.SetParameter(KeyName.TriggerSoftware, 1);
                using (Bitmap bmp = pCamera.GrabFrame())
                {
                    bitmap = (Bitmap)bmp?.Clone();
                }
            }
            return bitmap;
        }

        private void ShowFOVBitmap(CameraMode mode, Bitmap bmp)
        {
            Dispatcher.Invoke(() =>
            {
                if (bmp != null)
                {
                    if (mode == CameraMode.Top)
                    {
                        imageBox.SourceFromBitmap = bmp;
                    }
                    else if (mode == CameraMode.Bottom)
                    {
                        imageBox.SourceFromBitmap = bmp;
                    }
                }
                else
                {
                    if (mode == CameraMode.Top)
                    {
                        imageBox.Source = null;
                    }
                    else if (mode == CameraMode.Bottom)
                    {
                        imageBox.Source = null;
                    }
                }
            });
        }

        private System.Windows.Point GetOffsetROI(FOV pFOV, Image<Bgr, byte> image)
        {
            System.Windows.Point offset = new System.Windows.Point();
            using (Image<Bgr, byte> src = image.Clone())
            using (Image<Bgr, byte> dst = image.Clone())
            {
                SMD pSMD = pFOV.SMDs.FirstOrDefault(x => x.IsEnabled == true && x.Type == SMDType.Mark);
                if (pSMD != null)
                {
                    src.ROI = pSMD.ROI.Rectangle;
                    dst.ROI = pSMD.ROI.Rectangle;
                    CvResult cvRet;
                    switch (pSMD.Algorithm)
                    {
                        case SMDAlgorithm.Unknow:
                            break;
                        case SMDAlgorithm.Contour_Text:
                            {
                                cvRet = pSMD.Contours.RunText(src.Copy(), dst, pSMD.ROI.Rectangle);
                                offset.X = cvRet.Center.X - (pSMD.Contours.P0.X + (pSMD.Contours.P2.X - pSMD.Contours.P0.X) / 2);
                                offset.Y = cvRet.Center.Y - (pSMD.Contours.P0.Y + (pSMD.Contours.P2.Y - pSMD.Contours.P0.Y) / 2);
                                break;
                            }
                        case SMDAlgorithm.Contour_Box:
                            {
                                cvRet = pSMD.Contours.RunBox(src.Copy(), dst, pSMD.ROI.Rectangle);
                                offset.X = cvRet.Center.X - (pSMD.Contours.P0.X + (pSMD.Contours.P2.X - pSMD.Contours.P0.X) / 2);
                                offset.Y = cvRet.Center.Y - (pSMD.Contours.P0.Y + (pSMD.Contours.P2.Y - pSMD.Contours.P0.Y) / 2);
                                break;
                            }
                        case SMDAlgorithm.Contour_TextBox:
                            {
                                cvRet = pSMD.Contours.RunTextBox(src.Copy(), dst, pSMD.ROI.Rectangle);
                                offset.X = cvRet.Center.X - (pSMD.Contours.P0.X + (pSMD.Contours.P2.X - pSMD.Contours.P0.X) / 2);
                                offset.Y = cvRet.Center.Y - (pSMD.Contours.P0.Y + (pSMD.Contours.P2.Y - pSMD.Contours.P0.Y) / 2);
                                break;
                            }
                        case SMDAlgorithm.MarkTracing:
                            {
                                cvRet = pSMD.MarkTracing.Run(src.Copy(), dst, pSMD.ROI.Rectangle);
                                offset.X = cvRet.Center.X - pSMD.MarkTracing.Center.X;
                                offset.Y = cvRet.Center.Y - pSMD.MarkTracing.Center.Y;
                                break;
                            }
                        case SMDAlgorithm.FeatureMatching:
                            {
                                cvRet = pSMD.FeatureMatching.Run(src.Copy(), dst, pSMD.ROI.Rectangle);
                                offset.X = cvRet.Center.X - pSMD.FeatureMatching.Center.X;
                                offset.Y = cvRet.Center.Y - pSMD.FeatureMatching.Center.Y;
                                break;
                            }
                        case SMDAlgorithm.TemplateMatching:
                            {
                                cvRet = pSMD.TemplateMatching.Run(src.Copy(), dst, pSMD.ROI.Rectangle);
                                offset.X = cvRet.Center.X - pSMD.TemplateMatching.Center.X;
                                offset.Y = cvRet.Center.Y - pSMD.TemplateMatching.Center.Y;
                                break;
                            }
                        case SMDAlgorithm.CodeRecognition:
                            break;
                        case SMDAlgorithm.HSVExtraction:
                            break;
                        case SMDAlgorithm.LuminanceExtraction:
                            break;
                        case SMDAlgorithm.LuminanceExtractionQty:
                            break;
                        case SMDAlgorithm.DeepLearning:
                            break;
                        default:
                            break;
                    }
                    src.ROI = new Rectangle();
                    dst.ROI = new Rectangle();
                    ShowFOVBitmap(pFOV.CameraMode, dst.ToBitmap());
                }
            }
            return offset;
        }

        private CvResult CheckSMD(SMD pSMD, Image<Bgr, byte> src, Image<Bgr, byte> dst)
        {
            CvResult cvRet = new CvResult();
            switch (pSMD.Algorithm)
            {
                case SMDAlgorithm.Unknow:
                    break;
                case SMDAlgorithm.Contour_Text:
                    {
                        cvRet = pSMD.Contours.RunText(src, dst, pSMD.ROI.Rectangle);
                        break;
                    }
                case SMDAlgorithm.Contour_Box:
                    {
                        cvRet = pSMD.Contours.RunBox(src, dst, pSMD.ROI.Rectangle);
                        break;
                    }
                case SMDAlgorithm.Contour_TextBox:
                    {
                        cvRet = pSMD.Contours.RunTextBox(src, dst, pSMD.ROI.Rectangle);
                        break;
                    }
                case SMDAlgorithm.MarkTracing:
                    {
                        cvRet = pSMD.MarkTracing.Run(src, dst, pSMD.ROI.Rectangle);
                        break;
                    }
                case SMDAlgorithm.FeatureMatching:
                    {
                        cvRet = pSMD.FeatureMatching.Run(src, dst, pSMD.ROI.Rectangle);
                        break;
                    }
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
                            GlobalDataManager.Current.Barcode = cvRet.Content;
                        }
                        Trace.WriteLine($"SN: {cvRet.Content} (Length: {cvRet.Content.Length})");
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
                case SMDAlgorithm.LuminanceExtractionQty:
                    {
                        cvRet = pSMD.LuminanceExtractionQty.Run(src, dst, pSMD.ROI.Rectangle);
                        break;
                    }
                case SMDAlgorithm.DeepLearning:
                    {
                        cvRet = pSMD.DeepLearning.Run(src, dst, pSMD.ROI.Rectangle);
                        break;
                    }
                default:
                    break;
            }
            return cvRet;
        }

        private int CheckFOV(FOV pFOV)
        {
            int fRet = 1;
            ShowFOVBitmap(pFOV.CameraMode, null);
            if (MachineParams.Current.DebugMode)
            {
                GlobalDataManager.Current.Barcode = "T0902965789";
                MessageBoxResult r = MessageBox.Show($"Return {pFOV.Type}", "Debugging", MessageBoxButton.YesNoCancel, MessageBoxImage.Information, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                if (r == MessageBoxResult.Yes)
                {
                    Trace.WriteLine($"FOV: {pFOV.Type} = 1");
                    return 1;
                }
                else if (r == MessageBoxResult.No)
                {
                    Trace.WriteLine($"FOV: {pFOV.Type} = -1");
                    return -1;
                }
                else
                {
                    Trace.WriteLine($"FOV: {pFOV.Type} = 0");
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
                            IEnumerable<SMD> pSMDs = pFOV.SMDs.Where(x => x.IsEnabled == true);
                            if (pSMDs == null)
                            {
                                fRet = -1;
                            }
                            else
                            {
                                System.Windows.Point offset = GetOffsetROI(pFOV, src);
                                foreach (SMD pSMD in pSMDs)
                                {
                                    Rectangle offsetROI = new Rectangle
                                    {
                                        X = pSMD.ROI.Rectangle.X + (int)offset.X,
                                        Y = pSMD.ROI.Rectangle.Y + (int)offset.Y,
                                        Width = pSMD.ROI.Rectangle.Width,
                                        Height = pSMD.ROI.Rectangle.Height
                                    };
                                    src.ROI = offsetROI;
                                    dst.ROI = offsetROI;
                                    CvResult cvRet = CheckSMD(pSMD, src.Copy(), dst);
                                    if (!cvRet.Result)
                                    {
                                        fRet = -1;
                                    }
                                    src.ROI = new Rectangle();
                                    dst.ROI = new Rectangle();
                                    UpdateSMDResults(pSMD, cvRet);
                                }
                            }
                            //_image = dst.Clone();
                            ShowFOVBitmap(pFOV.CameraMode, dst.ToBitmap());
                            //CvImage.Save("", "", dst.Mat, quality: 50);
                        }
                    }
                    else
                    {
                        fRet = -1;
                        string message = $"No Bitmap ({pFOV.CameraMode}, {pFOV.Type})";
                        Trace.WriteLine(message);
                        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            }
            else
            {
                fRet = -1;
                string message = $"No Data ({pFOV.CameraMode}, {pFOV.Type})";
                Trace.WriteLine(message);
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
            return fRet;
        }

        private int CheckType(FOVType type, FOVType targetType = FOVType.Unknow)
        {
            try
            {
                FOVType currentType = targetType != FOVType.Unknow ? targetType : type;
                Trace.WriteLine($"Check: {currentType}");
                int fRet = 1;
                FOV pFOV = _program.FOVs.FirstOrDefault(x => x.IsEnabled == true && x.Type == type);
                if (pFOV == null)
                {
                    fRet = -1;
                }
                else
                {
                    //_image = null;
                    //_SMDResults.Clear();
                    //GlobalDataManager.Current.Barcode = string.Empty;
                    int nRet = CheckFOV(pFOV);
                    if (nRet != 1)
                    {
                        fRet = -1;
                    }
                    //List<SMDResult> smds = new List<SMDResult>();
                    //if (_SMDResults.Count > 0)
                    //{
                    //    foreach (SMDResult smd in _SMDResults)
                    //    {
                    //        smds.Add(smd.Clone());
                    //    }
                    //}
                    //UpdateFOVResults(pFOV.Type, nRet, _image);
                }
                Trace.WriteLine($"Result: {fRet}");
                return fRet;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }

        private int CheckCamera(CameraMode mode)
        {
            try
            {
                Trace.WriteLine($"CheckCamera: {mode}");
                int fRet = 1;
                IEnumerable<FOV> searchFOVs = _program.FOVs.Where(x => x.IsEnabled == true && x.CameraMode == mode);
                if (searchFOVs == null)
                {
                    fRet = -1;
                }
                else
                {
                    int i = 0;
                    int total = searchFOVs.Count() - 1;
                    UpdateStatusBar("Waiting Image...", SystemColors.White);
                    UpdateStatusBar(0, SystemColors.Gray);
                    foreach (FOV pFOV in searchFOVs)
                    {
                        UpdateStatusBar($"Waiting Image ({i}/{total})...", SystemColors.White);
                        UpdateStatusBar((100 / total) * i, SystemColors.Yellow);
                        int nRet = CheckFOV(pFOV);
                        if (nRet != 1)
                        {
                            fRet = -1;
                        }
                        UpdateStatusBar($"Result Image ({i}/{total}): {nRet}", SystemColors.White);
                        UpdateStatusBar(100, SystemColors.Green);
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

        public void ShowErrorMessage(string message)
        {
            Dispatcher.InvokeAsync(() => { ErrorMessage = message; });
        }

        private void UpdateYieldRate(int numPass, int numFail)
        {
            DateTime now = DateTime.Now;

            if ((now - _settings.Today).Days == 0)
            {
                _settings.TotalPass += numPass;
                _settings.TotalFail += numFail;
            }
            else
            {
                _settings.Today = now;
                _settings.TotalPass = numPass;
                _settings.TotalFail = numFail;
            }

            double numTotal = _settings.TotalPass + _settings.TotalFail;
            if (numTotal == 0)
            {
                _settings.YeildRate = 0;
            }
            else
            {
                _settings.YeildRate = Math.Round(((double)_settings.TotalPass / (_settings.TotalPass + _settings.TotalFail)) * 100, 2);
            }
            _settings.Save();
            NotifyPropertyChanged();
        }

        private void UpdateFOVResults(FOVType pType, int pResult, Image<Bgr, byte> pImage)
        {
            FOVResult f = _FOVResults.FirstOrDefault(x => x.Type == pType);
            if (f != null)
            {
                f.Type = pType;
                f.Result = pResult;
                f.Image = pImage;
            }
            else
            {
                FOVResult fov = new FOVResult
                {
                    Type = pType,
                    Result = pResult,
                    Image = pImage
                };
                _FOVResults.Add(fov);
            }
        }

        private void UpdateSMDResults(SMD pSMD, CvResult pResult)
        {
            SMDResult s = _SMDResults.FirstOrDefault(x => x.SMD == pSMD);
            if (s != null)
            {
                s.SMD = pSMD;
                s.CvResult = pResult;
            }
            else
            {
                SMDResult smd = new SMDResult
                {
                    SMD = pSMD,
                    CvResult = pResult
                };
                _SMDResults.Add(smd);
            }
        }

        private bool AnalyzeResults(FOVType type)
        {
            SMDResult f = _SMDResults.FirstOrDefault(x => x.CvResult.Result == false);
            if (f != null)
            {
                return false;
            }
            return true;
        }

        private void SaveResults(FOVType type)
        {
            string localFolder = @$"{AppDomain.CurrentDomain.BaseDirectory}logs\{DateTime.Now:yyyy-MM-dd}\{GlobalDataManager.Current.SN}\";
            FileExplorer.CreateDirectory(localFolder);
            FOVResult f = _FOVResults.FirstOrDefault(x => x.Type == type);
            if (f != null)
            {
                string filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                CvImage.Save(localFolder, filename, f.Image.Mat, 50);
            }
        }
    }

    public class SMDResult
    {
        public SMD SMD { get; set; }
        public CvResult CvResult { get; set; }

        public SMDResult()
        {
            SMD = new SMD();
            CvResult = new CvResult();
        }

        public SMDResult Clone()
        {
            return new SMDResult()
            {
                SMD = SMD,
                CvResult = CvResult
            };
        }
    }

    public class FOVResult
    {
        public FOVType Type { get; set; }
        public int Result { get; set; }
        public List<SMDResult> SMDs { get; set; }
        public Image<Bgr, byte> Image { get; set; }

        public FOVResult()
        {
            Type = FOVType.Unknow;
            Result = -1;
            SMDs = new List<SMDResult>();
            Image = null;
        }
    }
}
