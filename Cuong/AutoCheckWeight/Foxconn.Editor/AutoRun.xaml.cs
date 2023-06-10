using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.Editor.Camera;
using Foxconn.Editor.Configuration;
using Foxconn.Editor.OpenCV;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Foxconn.Editor
{
    /// <summary>
    /// Interaction logic for AutoRun.xaml
    /// </summary>
    public partial class AutoRun : Window
    {
        public static AutoRun Current;
        private Worker _loopWorker;
        private Board _program
        {
            get => ProgramManager.Current.Program;
            set => ProgramManager.Current.Program = value;
        }

        private MachineParams _param = MachineParams.Current;
        private DeviceManager _device = DeviceManager.Current;
        private Image<Bgr, byte> _image { get; set; }
        private VideoCapture _capture = null;
        private Mat _frame;
        private string _SN { get; set; }
        public string CycleTime { get; set; }
        public string BoardName { get; set; }

        public int PassRate { get; set; }
        public int FailRate { get; set; }
        public float YeildRate { get; set; }

        public AutoRun()
        {
            InitializeComponent();
            DataContext = this;
            _loopWorker = new Worker(new ThreadStart(AutoRunProcess));
            BoardName = _program.Name;
            PassRate = Properties.Settings.Default.PASS;
            FailRate = Properties.Settings.Default.FAIL;
            YeildRate = Properties.Settings.Default.YeildRate;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogInfo("AutoRun ========> Start AutoRun");
            StartUp();
            UpdateStatusControl("Running...");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Stop();
        }

        public bool StartUp()
        {
            try
            {
                _loopWorker.Start();
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

            MainWindow.Current.Show();
            LogInfo("AutoRun ========>  Stop AutoRun");
        }

        public void AutoRunProcess()
        {

        }

        private void mnuiSetSpeed_Click(object sender, RoutedEventArgs e)
        {
            epdSetup.IsExpanded = true;

        }
        private void mnuiOpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory);
        }

        private void imbCamera_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (imbCamera.Image != null)
            {
                double newZoom = imbCamera.ZoomScale * (e.Delta > 0 ? 1.2 : 0.8);
                newZoom = Math.Max(0.5, Math.Min(5, newZoom)); // gioi han ti le zoom
                imbCamera.SetZoomScale(newZoom, e.Location);
            }
        }


        private void imbCamera_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                imbCamera.SetZoomScale(1, e.Location);
                imbCamera.AutoScrollOffset = new System.Drawing.Point(0, 0);
            }
        }


        private int CheckType(FOVType type, FOVType targetType = FOVType.Unknow)
        {
            try
            {
                FOVType tempType = targetType != FOVType.Unknow ? targetType : type;
                LogInfo($"CheckType: {tempType}");
                int fRet = 1;
                FOV pFOV = _program.FOVs.FirstOrDefault(x => x.IsEnable == true && x.FOVType == type);
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

        private int CheckFOV(FOV pFOV)
        {
            int fRet = 1;
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
                                        X = SMD.ROI.Rectangle.X,
                                        Y = SMD.ROI.Rectangle.Y,
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
                                    else
                                        fRet = 1;
                                    src.ROI = new Rectangle();
                                    dst.ROI = new Rectangle();
                                }
                            }
                        }
                    }
                }
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
                            LogsAutoRun("SN= " + _SN);
                        }
                        else
                        {
                            _SN = string.Empty;
                            Console.WriteLine("SN = Empty");
                            LogsAutoRun("SN = Empty");
                        }
                        break;
                    }
                case SMDAlgorithm.HSVExtraction:
                    {
                        cvRet = pSMD.HSVExtraction.Run(src, dst, pSMD.ROI.Rectangle);
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
                            default:
                                break;
                        }
                        src.ROI = new Rectangle();
                        dst.ROI = new Rectangle();
                        ShowFOVImage(pFOV.CameraMode, dst.ToBitmap());
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

        public void ShowFOVImage(CameraMode cameraMode, Bitmap bmp)
        {
            if (cameraMode != CameraMode.Unknow)
            {
                imbCamera.Image = bmp.ToImage<Bgr, byte>();
            }
            else
            {
                imbCamera.Image = null;
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

        private void mnuiTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FOV pFOV = _program.FOVs[0];
                int nRet = CheckFOV0(pFOV);
                lblResult.Content = nRet == 1 ? "PASS" : "FAIL";
                lblResult.FontWeight = FontWeights.Bold;
                lblResult.FontSize = 100;
                lblResult.Foreground = nRet == 1 ? System.Windows.Media.Brushes.LimeGreen : System.Windows.Media.Brushes.Red;
                //borderResult.Background = nRet == 1 ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
                SetRate();
                UpdateStatusControl("Complete!");
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }



        public int CheckFOV0(FOV pFOV)
        {
            int fRet = 1;
            string filepath = _program.ImageBoard.Blocks[0].Filename;
            Bitmap bmp = new Bitmap(filepath);
            if (bmp != null)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

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
                                LogsAutoRun($"Check SMD_{SMD.Id} : OK");

                            }
                            else
                            {
                                Console.WriteLine($"Check SMD_{SMD.Id} : FAIL");
                                LogsAutoRun($"Check SMD_{SMD.Id} : FAIL");
                                fRet = -1;
                            }

                            src.ROI = new Rectangle();
                            dst.ROI = new Rectangle();
                            imbCamera.Image = dst.Clone();

                        }
                        stopwatch.Stop();
                        CycleTime = stopwatch.Elapsed.ToString();
                        txbCycleTime.Text = CycleTime;
                    }

                }
            }
            return fRet;
        }

        #region logs
        public void LogInfo(string message)
        {
            Logger.Current.Info(message);
        }

        public void LogError(string message)
        {
            Logger.Current.Error(message);
        }

        #endregion

        private void btnSetSpeedRobot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string str = txtSetSpeedRobot.Text;
                if (int.TryParse(str, out int value) && str != string.Empty)
                {
                    if (value > 0 && value <= 100)
                    {
                        string data = $"Set Speed: {value}".Trim();
                        //_device.TCPClient.SocketWriteData(data);
                        MessageBox.Show(data, "Set Speed", MessageBoxButton.OK, MessageBoxImage.Information);
                        LogInfo(data);
                        LogsAutoRun($"SENT_TCPCLIENT: SPEED = {value}".Trim());
                    }
                    else
                    {
                        MessageBox.Show("Out of range value: 1-100", "Set Speed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Input error!", "Set Speed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }


        public void LogsAutoRun(string message)
        {
            string currentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            int MAX_LINES = 500;
            Dispatcher.Invoke(() =>
            {
                // Thêm dòng mới vào Textbox
                txtLogsAutoRun.AppendText(currentTime + "   " + "[INFO]" + "   " + message + Environment.NewLine);
                // Kiểm tra số lượng dòng và xóa các dòng cũ hơn nếu vượt quá giới hạn
                int lineCount = txtLogsAutoRun.LineCount;
                if (lineCount > MAX_LINES)
                {
                    int indexFirstLineToRemove = txtLogsAutoRun.GetCharacterIndexFromLineIndex(0);
                    int indexLastLineToRemove = txtLogsAutoRun.GetCharacterIndexFromLineIndex(lineCount - MAX_LINES);
                    txtLogsAutoRun.Select(indexFirstLineToRemove, indexLastLineToRemove);
                    txtLogsAutoRun.SelectedText = "";
                }
            });
        }

        public void UpdateStatusControl(string text)
        {
            txbStatusBar.Text = text;
        }


        public void SetRate()
        {
            if (lblResult.Content == "PASS")
            {
                Properties.Settings.Default.PASS++;
                txbPassRate.Text = Properties.Settings.Default.PASS.ToString();
                if (lblResult.Content == "FAIL")
                {
                    Properties.Settings.Default.FAIL++;
                    txbFailRate.Text = Properties.Settings.Default.PASS.ToString();
                }
                if (FailRate == 0)
                {
                    Properties.Settings.Default.YeildRate = 100.00f;
                    txbYieldRate.Text = Properties.Settings.Default.YeildRate.ToString();
                }
                else
                {
                    Properties.Settings.Default.YeildRate = (PassRate / FailRate) * 100;
                    txbYieldRate.Text = Properties.Settings.Default.YeildRate.ToString();
                }
            }
        }
    }
}
