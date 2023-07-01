using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.AOI.Editor.Camera;
using Foxconn.AOI.Editor.Configuration;
using Foxconn.AOI.Editor.Enums;
using Foxconn.AOI.Editor.OpenCV;
using Foxconn.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Foxconn.AOI.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for AutoRunManager.xaml
    /// </summary>
    public partial class AutoRunManager : Window
    {
        public static AutoRunManager Current;
        private Worker _loopWorker;
        private Worker _loopMonitor;
        private Board _program => ProgramManager.Current.Program;
        private MachineParams _param = MachineParams.Current;
        private DeviceManager _device = DeviceManager.Current;
        private GlobalDataManager _globalData = GlobalDataManager.Current;
        private List<FOVResult> _FOVResults = new List<FOVResult>();
        private List<SMDResult> _SMDResults = new List<SMDResult>();
        private VideoCapture _capture = null;
        private Mat _frame;

        public bool IsRunning => _loopWorker.IsRunning;

        public bool IsMonitoring => _loopMonitor.IsRunning;

        public string BoardName => _program.Name;

        public string Barcode => GlobalDataManager.Current.Barcode;

        public string CycleTime => GlobalDataManager.Current.CycleTime;

        public string ScannerData
        {
            get => GlobalDataManager.Current.ScannerData;
            set => GlobalDataManager.Current.ScannerData = value;
        }

        public AutoRunManager()
        {
            InitializeComponent();
            DataContext = this;
            Current = this;
            _loopWorker = new Worker(new ThreadStart(AutoRunProcess));
            _loopMonitor = new Worker(new ThreadStart(AutoRunProcessMonitoring));
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
                    return true;
                if (!Prepare() || _program == null)
                    return false;
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
                if (_capture != null)
                    _capture.Dispose();
                if (!IsRunning)
                    return;
                GlobalDataManager.Current.IsAutoRun = false;
                _loopWorker?.Stop();
                _loopMonitor?.Stop();
                MainWindow.Current.Show();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        private void AutoRunProcess()
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Auto run thread.";
            string flagLane1 = "S200";
            string flagLane2 = "S202";
            string flagPCB1 = "";
            string flagPCB2 = "";
            string flagCAP1 = "";
            string flagCAP2 = "";
            string flagSOLDER_CAP1 = "";
            string flagSOLDER_CAP2 = "";
            string flagEnd = "";
            FOVType typePCB1 = FOVType.Unknow;
            FOVType typePCB2 = FOVType.Unknow;
            FOVType typeCAP1 = FOVType.Unknow;
            FOVType typeCAP2 = FOVType.Unknow;
            FOVType typeSOLDER_CAP1 = FOVType.Unknow;
            FOVType typeSOLDER_CAP2 = FOVType.Unknow;
            while (!_loopWorker.WaitStopSignal(100))
            {
                try
                {
                    int runL1 = _device.PLC1.GetFlag(flagLane1);
                    int runL2 = _device.PLC1.GetFlag(flagLane2);
                    int step = 100 / 7;
                    if (runL1 == 1 || runL2 == 1)
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        GlobalDataManager.Current.IsDoingInspection = true;
                        UpdateStatusControl("Processing...", -1);
                        ShowFOVBitmap(CameraMode.Top, null);
                        ShowFOVBitmap(CameraMode.Bottom, null);
                        if (runL1 == 1)
                        {
                            Trace.WriteLine("AutoRunProcess =====> L1");
                            UpdateStatusControl("Processing L1...", 0);
                            flagPCB1 = "S50";
                            flagPCB2 = "S51";
                            flagCAP1 = "S10";
                            flagCAP2 = "S11";
                            flagSOLDER_CAP1 = "S100";
                            flagSOLDER_CAP2 = "S101";
                            flagEnd = "S201";
                            typePCB1 = FOVType.L1_PCB1;
                            typePCB2 = FOVType.L1_PCB2;
                            typeCAP1 = FOVType.L1_CAP1;
                            typeCAP2 = FOVType.L1_CAP2;
                            typeSOLDER_CAP1 = FOVType.L1_SOLDER_CAP1;
                            typeSOLDER_CAP2 = FOVType.L1_SOLDER_CAP2;
                            ShowCellResult(FOVType.L1_CLEAR, "", System.Windows.Media.Brushes.Transparent);
                        }
                        if (runL2 == 1)
                        {
                            Trace.WriteLine("AutoRunProcess =====> L2");
                            UpdateStatusControl("Processing L2...", 0);
                            flagPCB1 = "S50";
                            flagPCB2 = "S51";
                            flagCAP1 = "S10";
                            flagCAP2 = "S11";
                            flagSOLDER_CAP1 = "S100";
                            flagSOLDER_CAP2 = "S101";
                            flagEnd = "S203";
                            typePCB1 = FOVType.L2_PCB1;
                            typePCB2 = FOVType.L2_PCB2;
                            typeCAP1 = FOVType.L2_CAP1;
                            typeCAP2 = FOVType.L2_CAP2;
                            typeSOLDER_CAP1 = FOVType.L2_SOLDER_CAP1;
                            typeSOLDER_CAP2 = FOVType.L2_SOLDER_CAP2;
                            ShowCellResult(FOVType.L2_CLEAR, "", System.Windows.Media.Brushes.Transparent);
                        }
                        while (!_loopWorker.WaitStopSignal(100))
                        {
                            // PCB 1
                            if (_device.PLC1.GetFlag(flagPCB1) == 1)
                            {
                                Trace.WriteLine($"AutoRunProcess =====> {typePCB1}");
                                UpdateStatusControl($"{typePCB1}...", step * 1);
                                int nRet = CheckType(typePCB1);
                                string message = nRet == 1 ? $"{typePCB1}: Pass" : $"{typePCB1}: Fail";
                                Trace.WriteLine(message);
                                UpdateStatusControl(message, 0);
                                CheckFOVResults(typeCAP1, typePCB1);
                            }

                            // PCB 2
                            if (_device.PLC1.GetFlag(flagPCB2) == 1)
                            {
                                Trace.WriteLine($"AutoRunProcess =====> {typePCB2}");
                                UpdateStatusControl($"{typePCB2}...", step * 2);
                                int nRet = CheckType(typePCB2);
                                string message = nRet == 1 ? $"{typePCB2}: Pass" : $"{typePCB2}: Fail";
                                Trace.WriteLine(message);
                                UpdateStatusControl(message, 0);
                                CheckFOVResults(typeCAP2, typePCB2);
                            }

                            // CAP 1
                            if (_device.PLC1.GetFlag(flagCAP1) == 1)
                            {
                                Trace.WriteLine($"AutoRunProcess =====> {typeCAP1}");
                                UpdateStatusControl($"{typeCAP1}...", step * 3);
                                int nRet = CheckType(typeCAP1);
                                string message = nRet == 1 ? $"{typeCAP1}: Pass" : $"{typeCAP1}: Fail";
                                Trace.WriteLine(message);
                                UpdateStatusControl(message, 0);
                                CheckFOVResults(typeCAP1, typePCB1);
                            }

                            // CAP 2
                            if (_device.PLC1.GetFlag(flagCAP2) == 1)
                            {
                                Trace.WriteLine($"AutoRunProcess =====> {typeCAP2}");
                                UpdateStatusControl($"{typeCAP2}...", step * 4);
                                int nRet = CheckType(typeCAP2);
                                string message = nRet == 1 ? $"{typeCAP2}: Pass" : $"{typeCAP2}: Fail";
                                Trace.WriteLine(message);
                                UpdateStatusControl(message, 0);
                                CheckFOVResults(typeCAP2, typePCB2);
                            }

                            // SOLDER-CAP 1
                            if (_device.PLC1.GetFlag(flagSOLDER_CAP1) == 1)
                            {
                                Trace.WriteLine($"AutoRunProcess =====> {typeSOLDER_CAP1}");
                                UpdateStatusControl($"{typeSOLDER_CAP1}...", step * 5);
                                int nRet = CheckType(typeSOLDER_CAP1);
                                string message = nRet == 1 ? $"{typeSOLDER_CAP1}: Pass" : $"{typeSOLDER_CAP1}: Fail";
                                Trace.WriteLine(message);
                                UpdateStatusControl(message, 0);
                                CheckFOVResults(typeSOLDER_CAP1);
                            }

                            // SOLDER-CAP 2
                            if (_device.PLC1.GetFlag(flagSOLDER_CAP2) == 1)
                            {
                                Trace.WriteLine($"AutoRunProcess =====> {typeSOLDER_CAP2}");
                                UpdateStatusControl($"{typeSOLDER_CAP2}...", step * 6);
                                int nRet = CheckType(typeSOLDER_CAP2);
                                string message = nRet == 1 ? $"{typeSOLDER_CAP2}: Pass" : $"{typeSOLDER_CAP2}: Fail";
                                Trace.WriteLine(message);
                                UpdateStatusControl(message, 0);
                                CheckFOVResults(typeSOLDER_CAP2);
                            }

                            if (_device.PLC1.GetFlag(flagEnd) == 1)
                            {
                                Trace.WriteLine("AutoRunProcess =====> Completed!");
                                UpdateStatusControl("Completed!", 100);
                                break;
                            }
                        }
                        SaveFOVResults(typePCB1, typeCAP1, typeSOLDER_CAP1, typePCB2, typeCAP2, typeSOLDER_CAP2);
                        _SMDResults.Clear();
                        _FOVResults.Clear();
                        GlobalDataManager.Current.Barcode = string.Empty;
                        GlobalDataManager.Current.IsDoingInspection = false;
                        stopwatch.Stop();
                        GlobalDataManager.Current.CycleTime = stopwatch.Elapsed.ToString();
                        GlobalOptimizing.FreeMemory();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
        }

        private void AutoRunProcessMonitoring()
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Auto run monitoring thread.";
            int numCount = 0;
            while (!_loopMonitor.WaitStopSignal(250))
            {
                try
                {
                    if (_param.Scanner.IsEnabled)
                    {
                        if (_param.Scanner.DataLength == _globalData.ScannerData.Length && _globalData.ScannerData.Length > 0)
                        {
                            _globalData.Barcode = _globalData.ScannerData;
                            Trace.WriteLine($"Scanner Data: {_globalData.ScannerData} (Length:{_globalData.ScannerData.Length})");
                        }
                    }

                    if (numCount == 5)
                    {
                        _device.Ping();
                        numCount = 0;
                    }
                    ++numCount;
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
                int nRet = _device.Ping();
                if (nRet != 1)
                {
                    bRet = false;
                }
                OpenWebcam();
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
                    _capture = new VideoCapture();
                    int fourcc = VideoWriter.Fourcc('M', 'J', 'P', 'G');
                    _capture.Set(Emgu.CV.CvEnum.CapProp.FourCC, fourcc);
                    _capture.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
                    _capture.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, 1080);
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
                Dispatcher.Invoke(() => imageBox3.SourceFromBitmap = _frame.ToBitmap());
            }
        }

        private void imageBox1_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double zoomScale = imageBox1.ZoomScale;
            if (e.Delta > 0)
            {
                zoomScale = 1.2 * zoomScale;
                if (zoomScale < imageBox1.MaxScale)
                {
                    imageBox1.ZoomScale = zoomScale;
                }
            }
            else
            {
                zoomScale = 0.8 * zoomScale;
                if (zoomScale > imageBox1.MinScale)
                {
                    imageBox1.ZoomScale = zoomScale;
                }
            }
        }

        private void imageBox2_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double zoomScale = imageBox2.ZoomScale;
            if (e.Delta > 0)
            {
                zoomScale = 1.2 * zoomScale;
                if (zoomScale < imageBox2.MaxScale)
                {
                    imageBox2.ZoomScale = zoomScale;
                }
            }
            else
            {
                zoomScale = 0.8 * zoomScale;
                if (zoomScale > imageBox2.MinScale)
                {
                    imageBox2.ZoomScale = zoomScale;
                }
            }
        }

        private void imageBox3_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double zoomScale = imageBox3.ZoomScale;
            if (e.Delta > 0)
            {
                zoomScale = 1.2 * zoomScale;
                if (zoomScale < imageBox3.MaxScale)
                {
                    imageBox3.ZoomScale = zoomScale;
                }
            }
            else
            {
                zoomScale = 0.8 * zoomScale;
                if (zoomScale > imageBox3.MinScale)
                {
                    imageBox3.ZoomScale = zoomScale;
                }
            }
        }

        private void UpdateStatusControl(string text, int progress)
        {
            // #FFFFFF00 Yellow
            // #FF00FF00 Green
            // #FFFF0000 Red
            Dispatcher.Invoke(() =>
            {
                statusControl.StateText = text;
                if (progress > 0)
                {
                    if (progress == 100)
                    {
                        statusControl.Progress = 100;
                        statusControl.StateColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF00FF00");
                    }
                    else
                    {
                        statusControl.Progress = progress;
                        statusControl.StateColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF00");
                    }
                }
                if (progress == -1)
                {
                    statusControl.Progress = 0;
                    statusControl.StateColor = System.Windows.Media.Brushes.Gray;
                }
            });
        }

        private (ICamera, FOV) GetFOVParams(CameraMode mode, FOVType type)
        {
            DeviceManager device = DeviceManager.Current;
            FOV pFOV = _program.FOVs.FirstOrDefault(x => x.IsEnabled == true && x.CameraMode == mode && x.Type == type);
            if (pFOV == null)
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
            if (pFOV == null)
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

        private void ShowFOVBitmap(CameraMode mode, Bitmap bmp)
        {
            Dispatcher.Invoke(() =>
            {
                if (bmp != null)
                {
                    if (mode == CameraMode.Top)
                    {
                        imageBox1.SourceFromBitmap = bmp;
                    }
                    else if (mode == CameraMode.Bottom)
                    {
                        imageBox2.SourceFromBitmap = bmp;
                    }
                }
                else
                {
                    if (mode == CameraMode.Top)
                    {
                        imageBox1.ClearSource();
                    }
                    else if (mode == CameraMode.Bottom)
                    {
                        imageBox2.ClearSource();
                    }
                }
            });
        }

        private System.Windows.Point GetOffsetROI(FOV pFOV, Image<Bgr, byte> image)
        {
            try
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
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return new System.Windows.Point();
            }
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
                        else
                        {
                            GlobalDataManager.Current.Barcode = string.Empty;
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
                MessageBoxResult r = MessageBox.Show($"Checking {pFOV.Type}?", "Debug", MessageBoxButton.YesNoCancel, MessageBoxImage.Information, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);
                if (r == MessageBoxResult.Yes)
                {
                    Trace.WriteLine($"CheckFOV.DebugMode: {pFOV.Type} = 1");
                    return 1;
                }
                else if (r == MessageBoxResult.No)
                {
                    Trace.WriteLine($"CheckFOV.DebugMode: {pFOV.Type} = -1");
                    return -1;
                }
                else
                {
                    Trace.WriteLine($"CheckFOV.DebugMode: {pFOV.Type} = 0");
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
                                }
                            }
                            UpdateFOVResults(pFOV.Type, fRet, GlobalDataManager.Current.Barcode, dst.Clone());
                            ShowFOVBitmap(pFOV.CameraMode, dst.ToBitmap());
                            //CvImage.Save("", "", dst.Mat, quality: 50);

                            string path;
                            path = @$"{AppDomain.CurrentDomain.BaseDirectory}logs\L11\";
                            if (pFOV.Type == FOVType.L1_CAP1 && Directory.Exists(path))
                            {
                                CvImage.Save(path, "", src.Mat, 100);
                            }

                            path = @$"{AppDomain.CurrentDomain.BaseDirectory}logs\L12\";
                            if (pFOV.Type == FOVType.L1_CAP2 && Directory.Exists(path))
                            {
                                CvImage.Save(path, "", src.Mat, 100);
                            }

                            path = @$"{AppDomain.CurrentDomain.BaseDirectory}logs\L21\";
                            if (pFOV.Type == FOVType.L2_CAP1 && Directory.Exists(path))
                            {
                                CvImage.Save(path, "", src.Mat, 100);
                            }

                            path = @$"{AppDomain.CurrentDomain.BaseDirectory}logs\L22\";
                            if (pFOV.Type == FOVType.L2_CAP2 && Directory.Exists(path))
                            {
                                CvImage.Save(path, "", src.Mat, 100);
                            }
                        }
                    }
                    else
                    {
                        fRet = -1;
                        string message = $"CheckFOV: No Bitmap ({pFOV.CameraMode}, {pFOV.Type})";
                        Trace.WriteLine(message);
                        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            }
            else
            {
                fRet = -1;
                string message = $"CheckFOV: No Data ({pFOV.CameraMode}, {pFOV.Type})";
                Trace.WriteLine(message);
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
            return fRet;
        }

        private int CheckType(FOVType type, FOVType targetType = FOVType.Unknow)
        {
            try
            {
                FOVType tempType = targetType != FOVType.Unknow ? targetType : type;
                Trace.WriteLine($"CheckType: {tempType}");
                int fRet = 1;
                FOV pFOV = _program.FOVs.FirstOrDefault(x => x.IsEnabled == true && x.Type == type);
                if (pFOV == null)
                {
                    fRet = -1;
                }
                else
                {
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
                    foreach (FOV pFOV in searchFOVs)
                    {
                        int nRet = CheckFOV(pFOV);
                        if (nRet != 1)
                        {
                            fRet = -1;
                        }
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

        private void UpdateFOVResults(FOVType type, int result, string SN, Image<Bgr, byte> image)
        {
            FOVResult item = _FOVResults.Find(x => x.Type == type);
            if (item != null)
            {
                item.Type = type;
                item.Result = result;
                item.SN = SN;
                item.Image = image;
            }
            else
            {
                FOVResult temp = new FOVResult
                {
                    Type = type,
                    Result = result,
                    SN = SN,
                    Image = image
                };
                _FOVResults.Add(temp);
            }
        }

        private void CheckFOVResults(FOVType pCAP, FOVType pPCB)
        {
            string pass = "S1";
            string fail = "S2";
            string stepC1 = "S8";
            string stepC2 = "S9";
            FOVResult fovCAP = _FOVResults.Find(x => x.Type == pCAP);
            FOVResult fovPCB = _FOVResults.Find(x => x.Type == pPCB);
            if (fovCAP != null && fovPCB != null)
            {
                if (fovCAP.Result == 1 && fovPCB.Result == 1)
                {
                    // PASS PCB & CAP
                    if (_param.Terminal.IsEnabled)
                    {
                        string SN = fovPCB.SN;
                        if (fovPCB.SN == string.Empty)
                        {
                            _device.PLC1.SetDevice(fail, 1);
                            ShowCellResult(pPCB, "FAIL-SN", System.Windows.Media.Brushes.Red);
                            return;
                        }
                        string data = SN.PadRight(25) + "CHECK";
                        _device.Terminal.SerialWriteData(data);
                        bool isTimeout = false;
                        int timeout = 10000;
                        for (int i = 0; i < timeout / 400; i++)
                        {
                            string responseData = _device.Terminal.DataReceived;
                            if (responseData.Contains("ROUTING") && responseData.Contains("PASS"))
                            {
                                isTimeout = true;
                                _device.PLC1.SetDevice(pass, 1);
                                ShowCellResult(pPCB, "ROUTING", System.Windows.Media.Brushes.White);
                                return;
                            }
                            else if (responseData.Contains("SOLDER_CAP") && responseData.Contains("PASS"))
                            {
                                isTimeout = true;
                                // Sending S8, S9 before send S1
                                if (pCAP == FOVType.L1_CAP1 || pCAP == FOVType.L2_CAP1)
                                {
                                    _device.PLC1.SetDevice(stepC1, 1);
                                }
                                else if (pCAP == FOVType.L1_CAP2 || pCAP == FOVType.L2_CAP2)
                                {
                                    _device.PLC1.SetDevice(stepC2, 1);
                                }
                                _device.PLC1.SetDevice(pass, 1);
                                ShowCellResult(pPCB, "SOLDER_CAP", System.Windows.Media.Brushes.White);
                                return;
                            }
                            else if (responseData.Contains("ERRO"))
                            {
                                isTimeout = true;
                                _device.PLC1.SetDevice(fail, 1);
                                ShowCellResult(pPCB, "ERRO", System.Windows.Media.Brushes.Red);
                                return;
                            }
                            Thread.Sleep(25);
                        }
                        if (!isTimeout)
                        {
                            _device.PLC1.SetDevice(fail, 1);
                            ShowCellResult(pPCB, "FAIL-TIMEOUT", System.Windows.Media.Brushes.Red);
                        }
                    }
                    else
                    {
                        _device.PLC1.SetDevice(pass, 1);
                        ShowCellResult(pPCB, "PASS", System.Windows.Media.Brushes.Green);
                    }
                }
                else
                {
                    // FAIL PCB & CAP
                    if (_param.Terminal.IsEnabled)
                    {
                        string SN = fovPCB.SN;
                        if (SN == string.Empty)
                        {
                            _device.PLC1.SetDevice(fail, 1);
                            ShowCellResult(pPCB, "FAIL-SN", System.Windows.Media.Brushes.Red);
                            return;
                        }
                        //string errorCode = "VI0010";
                        //string data = SN.PadRight(25) + errorCode.PadRight(10) + "NG";
                        //_device.Terminal.SerialWriteData(data);
                        //int timeout = 10000;
                        //for (int i = 0; i < timeout / 400; i++)
                        //{
                        //    string responseData = _device.Terminal.DataReceived;
                        //    if (responseData.Contains(data + "PASS") || responseData.Contains(data + "ERRO"))
                        //        break;
                        //    Thread.Sleep(25);
                        //}
                        _device.PLC1.SetDevice(fail, 1);
                        ShowCellResult(pPCB, "FAIL", System.Windows.Media.Brushes.Red);
                    }
                    else
                    {
                        _device.PLC1.SetDevice(fail, 1);
                        ShowCellResult(pPCB, "FAIL", System.Windows.Media.Brushes.Red);
                    }
                }
            }
            else
            {
                if (fovCAP != null)
                {
                    _device.PLC1.SetDevice(fovCAP.Result == 1 ? pass : fail, 1);
                }
                if (fovPCB != null)
                {
                    _device.PLC1.SetDevice(fovPCB.Result == 1 ? pass : fail, 1);
                }
            }
        }

        private void CheckFOVResults(FOVType pSOLDER_CAP)
        {
            string SN = string.Empty;
            FOVType type;
            if (pSOLDER_CAP == FOVType.L1_SOLDER_CAP1)
            {
                type = FOVType.L1_PCB1;
            }
            else if (pSOLDER_CAP == FOVType.L1_SOLDER_CAP2)
            {
                type = FOVType.L1_PCB2;
            }
            else if (pSOLDER_CAP == FOVType.L2_SOLDER_CAP1)
            {
                type = FOVType.L2_PCB1;
            }
            else if (pSOLDER_CAP == FOVType.L2_SOLDER_CAP2)
            {
                type = FOVType.L2_PCB2;
            }
            else
            {
                type = FOVType.Unknow;
            }
            FOVResult temp = _FOVResults.Find(x => x.Type == type);
            if (temp != null)
            {
                SN = temp.SN;
            }
            string pass = "S1";
            string fail = "S2";
            if (SN == string.Empty)
            {
                _device.PLC1.SetDevice(fail, 1);
                ShowCellResult(pSOLDER_CAP, "FAIL-SN", System.Windows.Media.Brushes.Red);
                return;
            }
            FOVResult r = _FOVResults.Find(x => x.Type == pSOLDER_CAP);
            if (r != null)
            {
                if (r.Result == 1)
                {
                    // PASS SOLDER_CAP
                    if (_param.Terminal.IsEnabled)
                    {
                        string data = SN.PadRight(25) + "OK";
                        _device.Terminal.SerialWriteData(data);
                        bool isTimeout = false;
                        int timeout = 10000;
                        for (int i = 0; i < timeout / 400; i++)
                        {
                            string responseData = _device.Terminal.DataReceived;
                            if (responseData == data + "PASS")
                            {
                                isTimeout = true;
                                _device.PLC1.SetDevice(pass, 1);
                                ShowCellResult(pSOLDER_CAP, "PASS", System.Windows.Media.Brushes.Green);
                                return;
                            }
                            else if (responseData == data + "ERRO")
                            {
                                isTimeout = true;
                                _device.PLC1.SetDevice(fail, 1);
                                ShowCellResult(pSOLDER_CAP, "ERRO", System.Windows.Media.Brushes.Red);
                                return;
                            }
                            Thread.Sleep(25);
                        }
                        if (!isTimeout)
                        {
                            _device.PLC1.SetDevice(fail, 1);
                            ShowCellResult(pSOLDER_CAP, "FAIL-TIMEOUT", System.Windows.Media.Brushes.Red);
                        }
                    }
                    else
                    {
                        _device.PLC1.SetDevice(pass, 1);
                        ShowCellResult(pSOLDER_CAP, "PASS", System.Windows.Media.Brushes.Green);
                    }
                }
                else
                {
                    // FAIL SOLDER_CAP
                    if (_param.WorkerConfirm2)
                    {
                        string msg = "Đây có phải lỗi ảo không?\r\nNhấn 'OK' nếu lỗi ảo\r\rNhấn 'Cancel' nếu lỗi thật";
                        MessageBoxResult mbr = MessageBox.Show(msg, "Xác nhận", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                        if (mbr == MessageBoxResult.OK)
                        {
                            r.WorkerConfirm = "WPASS";
                            _device.PLC1.SetDevice(pass, 1);
                            ShowCellResult(pSOLDER_CAP, "PASS-WORKER", System.Windows.Media.Brushes.Green);
                            return;
                        }
                        else
                        {
                            r.WorkerConfirm = "WFAIL";
                            _device.PLC1.SetDevice(fail, 1);
                            ShowCellResult(pSOLDER_CAP, "FAIL-WORKER", System.Windows.Media.Brushes.Red);
                            if (_param.Terminal.IsEnabled)
                            {
                                string errorCode = "PCB001";
                                string data = SN.PadRight(25) + errorCode.PadRight(10) + "NG";
                                _device.Terminal.SerialWriteData(data);
                                int timeout = 10000;
                                for (int i = 0; i < timeout / 400; i++)
                                {
                                    string responseData = _device.Terminal.DataReceived;
                                    if (responseData == data + "PASS" || responseData == data + "ERRO")
                                        break;
                                    Thread.Sleep(25);
                                }
                            }
                        }
                    }
                    else
                    {
                        _device.PLC1.SetDevice(fail, 1);
                        ShowCellResult(pSOLDER_CAP, "FAIL", System.Windows.Media.Brushes.Red);
                    }
                }
            }
        }

        private void ShowCellResult(FOVType pSOLDER_CAP, string text, SolidColorBrush color)
        {
            Dispatcher.Invoke(() =>
            {
                switch (pSOLDER_CAP)
                {
                    case FOVType.Unknow:
                        break;
                    case FOVType.SN:
                        break;
                    case FOVType.L1_CAP1:
                    case FOVType.L1_PCB1:
                    case FOVType.L1_SOLDER_CAP1:
                        {
                            txtL1_C1.Text = text;
                            txtL1_C1.Foreground = color;
                            txtL1_C1.FontWeight = FontWeights.Bold;
                            break;
                        }
                    case FOVType.L1_CAP2:
                    case FOVType.L1_PCB2:
                    case FOVType.L1_SOLDER_CAP2:
                        {
                            txtL1_C2.Text = text;
                            txtL1_C2.Foreground = color;
                            txtL1_C2.FontWeight = FontWeights.Bold;
                            break;
                        }
                    case FOVType.L2_CAP1:
                    case FOVType.L2_PCB1:
                    case FOVType.L2_SOLDER_CAP1:
                        {
                            txtL2_C1.Text = text;
                            txtL2_C1.Foreground = color;
                            txtL2_C1.FontWeight = FontWeights.Bold;
                            break;
                        }
                    case FOVType.L2_CAP2:
                    case FOVType.L2_PCB2:
                    case FOVType.L2_SOLDER_CAP2:
                        {
                            txtL2_C2.Text = text;
                            txtL2_C2.Foreground = color;
                            txtL2_C2.FontWeight = FontWeights.Bold;
                            break;
                        }
                    case FOVType.L1_CLEAR:
                        {
                            txtL1_C1.Text = text;
                            txtL1_C1.Foreground = color;
                            txtL1_C1.FontWeight = FontWeights.Bold;
                            txtL1_C2.Text = text;
                            txtL1_C2.Foreground = color;
                            txtL1_C2.FontWeight = FontWeights.Bold;
                            break;
                        }
                    case FOVType.L2_CLEAR:
                        {
                            txtL2_C1.Text = text;
                            txtL2_C1.Foreground = color;
                            txtL2_C1.FontWeight = FontWeights.Bold;
                            txtL2_C2.Text = text;
                            txtL2_C2.Foreground = color;
                            txtL2_C2.FontWeight = FontWeights.Bold;
                            break;
                        }
                    default:
                        break;
                }
            });
        }

        private void SaveFOVResults(FOVType pPCB1, FOVType pCAP1, FOVType pSOLDER_CAP1, FOVType pPCB2, FOVType pCAP2, FOVType pSOLDER_CAP2)
        {
            FOVResult fResultPCB1 = _FOVResults.Find(x => x.Type == pPCB1);
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
                    CvImage.Save(localFolder, filename, fResultPCB1.Image.Mat, 50);

                    FOVResult fResultCAP1 = _FOVResults.Find(x => x.Type == pCAP1);
                    if (fResultCAP1 != null)
                    {
                        if (fResultCAP1.WorkerConfirm != string.Empty)
                        {
                            filename = $"Image_{fResultCAP1.WorkerConfirm}_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        else
                        {
                            filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        CvImage.Save(localFolder, filename, fResultCAP1.Image.Mat, 50);
                    }

                    FOVResult fResultSOLDER_CAP1 = _FOVResults.Find(x => x.Type == pSOLDER_CAP1);
                    if (fResultSOLDER_CAP1 != null)
                    {
                        if (fResultSOLDER_CAP1.WorkerConfirm != string.Empty)
                        {
                            filename = $"Image_{fResultSOLDER_CAP1.WorkerConfirm}_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        else
                        {
                            filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        CvImage.Save(localFolder, filename, fResultSOLDER_CAP1.Image.Mat, 50);
                    }
                }
            }

            FOVResult fResultPCB2 = _FOVResults.Find(x => x.Type == pPCB2);
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
                    CvImage.Save(localFolder, filename, fResultPCB2.Image.Mat, 50);

                    FOVResult fResultCAP2 = _FOVResults.Find(x => x.Type == pCAP2);
                    if (fResultCAP2 != null)
                    {
                        if (fResultCAP2.WorkerConfirm != string.Empty)
                        {
                            filename = $"Image_{fResultCAP2.WorkerConfirm}_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        else
                        {
                            filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        CvImage.Save(localFolder, filename, fResultCAP2.Image.Mat, 50);
                    }

                    FOVResult fResultSOLDER_CAP2 = _FOVResults.Find(x => x.Type == pSOLDER_CAP2);
                    if (fResultSOLDER_CAP2 != null)
                    {
                        if (fResultSOLDER_CAP2.WorkerConfirm != string.Empty)
                        {
                            filename = $"Image_{fResultSOLDER_CAP2.WorkerConfirm}_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        else
                        {
                            filename = $"Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                        }
                        CvImage.Save(localFolder, filename, fResultSOLDER_CAP2.Image.Mat, 50);
                    }
                }
            }
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

        public SMDResult Clone()
        {
            return new SMDResult()
            {
                SMD = SMD,
                Result = Result
            };
        }
    }

    public class FOVResult
    {
        public FOVType Type { get; set; }
        public int Result { get; set; }
        public string SN { get; set; }
        public Image<Bgr, byte> Image { get; set; }
        public string WorkerConfirm { get; set; }

        public FOVResult()
        {
            Type = FOVType.Unknow;
            Result = -1;
            SN = string.Empty;
            Image = null;
            WorkerConfirm = string.Empty;
        }
    }
}
