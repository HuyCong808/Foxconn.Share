using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.TestUI.Camera;
using Foxconn.TestUI.Config;
using Foxconn.TestUI.Editor;
using Foxconn.TestUI.Enums;
using Foxconn.TestUI.OpenCV;
using Foxconn.TestUI.View;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Rectangle = System.Drawing.Rectangle;

namespace Foxconn.TestUI
{
    /// <summary>
    /// Interaction logic for AutoRunDialog.xaml
    /// </summary>
    public partial class AutoRunDialog : Window
    {
        public static AutoRunDialog Current;
        private Image<Bgr, byte> _image { get; set; }
        private Worker _loopWorker;
        private Board _program
        {
            get => ProgramManager.Current.Program;
            set => ProgramManager.Current.Program = value;
        }
        private MachineParams _param = MachineParams.Current;
        private DeviceManager _device = DeviceManager.Current;
        private VideoCapture _capture = null;
        private string _data { get; set; }
        private Mat _frame;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string _SN { get; set; }
        public string CycleTime { get; set; }
        public string BoardName { get; set; }
        public AutoRunDialog()
        {
            InitializeComponent();
            Current = this;
            _loopWorker = new Worker(new ThreadStart(AutoRunProcess));
            BoardName = _program.Name;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartUp();
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
                UpdateLogError(ex.Message);
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
                UpdateLogError(ex.Message);
            }
            return bRet;
        }

        private bool GetSignal(string device)
        {
            //if (_device.PLC1.GetFlag(device) == 1)
            //{
            //    _device.PLC1.SetBitDevice(device, 0);
            //    // Neu get M1 = 1 thi reset M1 = 0
            //    return true;
            //}
            //else
            //{
            return false;
            // }
        }

        private void SetResultToPLC(FOVType pCheck, int pResult) // set pass fail voi PLC
        {
            switch (pCheck)
            {
                case FOVType.Unknow:
                    break;
                case FOVType.L1_PCB1:
                    {
                        // neu pass ->


                        // neu fail ->


                    }
                    break;
                case FOVType.L1_PCB2:
                    {
                        // neu pass ->


                        // neu fail ->


                    }
                    break;
                default:
                    break;
            }
        }

        private void CheckResults(FOVType pCheck)
        {
            switch (pCheck)
            {
                case FOVType.Unknow:
                    break;
                case FOVType.L1_PCB1:
                    {
                        var f = _FOVResults.FirstOrDefault(x => x.Type == pCheck);
                        if (f != null)
                        {
                            //  SetResultToPLC(pCheck, f.Result);
                        }
                        break;
                    }
                case FOVType.L1_PCB2:
                    {
                        var f = _FOVResults.FirstOrDefault(x => x.Type == pCheck);
                        if (f != null)
                        {
                            //   SetResultToPLC(pCheck, f.Result);
                        }
                        break;
                    }
            }
        }

        private List<FOVResult> _FOVResults = new List<FOVResult>();

        private void AutoRunProcess()
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Auto run thread.";
            string flagLane1 = "S200"; // chay lan1
            string flagLane2 = "S201"; // chay lan2
            string flaglb1 = ""; // label 1
            string flaglb2 = "";//label 2
            string flagPCB1 = ""; // check color 1
            string flagPCB2 = "";//check color 2
            string flagSOLDER1 = ""; // solder 1
            string flagSOLDER2 = ""; // soulder 2
            string flagEnd = "";
            FOVType typelb1 = FOVType.Unknow;
            FOVType typelb2 = FOVType.Unknow;
            FOVType typePCB1 = FOVType.Unknow;
            FOVType typePCB2 = FOVType.Unknow;
            FOVType typeSOLDER1 = FOVType.Unknow;
            FOVType typeSOLDER2 = FOVType.Unknow;
            while (true)
            {
                try
                {
                    int runL1 = _device.PLC1.GetFlag(flagLane1);
                    int runL2 = _device.PLC1.GetFlag(flagLane2);
                    if (runL1 == 1 || runL2 == 1)
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        if (runL1 == 1)
                        {
                            Console.WriteLine("AutoRun ---------> Lane 1");
                            flaglb1 = "S50";
                            flaglb2 = "S52";
                            flagPCB1 = "S51";
                            flagPCB2 = "S53";
                            flagSOLDER1 = "S100";
                            flagSOLDER2 = "S101";
                            typelb1 = FOVType.L1_lb1;
                            typelb2 = FOVType.L1_lb2;
                            typePCB1 = FOVType.L1_PCB1;
                            typePCB2 = FOVType.L1_PCB2;
                            typeSOLDER1 = FOVType.L1_SOLDER1;
                            typeSOLDER2 = FOVType.L1_SOLDER2;
                        }
                        if(runL2==1)
                        {
                            Console.WriteLine("AutoRun ---------> Lane 2");
                            flaglb1 = "S250";
                            flaglb2 = "S252";
                            flagPCB1 = "S251";
                            flagPCB2 = "S253";
                            flagSOLDER1 = "S300";
                            flagSOLDER2 = "S301";
                            typelb1 = FOVType.L1_lb1;
                            typelb2 = FOVType.L1_lb2;
                            typePCB1 = FOVType.L1_PCB1;
                            typePCB2 = FOVType.L1_PCB2;
                            typeSOLDER1 = FOVType.L1_SOLDER1;
                            typeSOLDER2 = FOVType.L1_SOLDER2;
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }


        private int CheckType(FOVType type)
        {
            try
            {
                FOVType tempType = type;
                Trace.WriteLine($"CheckType: {tempType}");
                int fRet = 1;
                FOV pFOV = _program.FOVs.FirstOrDefault(x => x.FOVType == type);
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
                    // Update FOV Results
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
            ICamera pCamera = GetFOVParams(pFOV.CameraMode);  // lay camera
            if (pCamera != null)
            {
                using (Bitmap bmp = GetFOVBitmap(pCamera, pFOV)) // lay anh bitmap theo camera
                {
                    if (bmp != null)
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        using (Image<Bgr, byte> src = bmp.ToImage<Bgr, byte>())
                        using (Image<Bgr, byte> dst = bmp.ToImage<Bgr, byte>())
                        {
                            IEnumerable<SMD> pSMDs = pFOV.SMDs.Where(x => x.IsEnabled == true);
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
                                src.ROI = new System.Drawing.Rectangle();
                                dst.ROI = new System.Drawing.Rectangle();
                                imbAutoRun.Image = dst.Clone();
                            }
                            stopwatch.Stop();
                            lblCircleTime.Content = "Circle Time: " + stopwatch.Elapsed;
                        }
                    }
                }
            }
            else
            {
                fRet = -1;
            }

            return fRet;
        }


        private CvResult CheckSMD(SMD pSMD, Image<Bgr, byte> src, Image<Bgr, byte> dst)
        {
            CvResult cvRet = new CvResult();
            switch (pSMD.Algorithm)
            {
                case Algorithm.Unknow:
                    break;
                case Algorithm.TemplateMatching:
                    {
                        cvRet = pSMD.TemplateMatching.Run(src, dst, pSMD.ROI.Rectangle);
                        break;
                    }
                case Algorithm.HSVExtraction:
                    {
                        cvRet = pSMD.HSVExtraction.Run(src, dst, pSMD.ROI.Rectangle);
                        break;
                    }
                case Algorithm.CodeRecognition:
                    {
                        cvRet = pSMD.CodeRecognition.Run(src, dst, pSMD.ROI.Rectangle);
                        break;
                    }
                case Algorithm.HSVExtractionQty:
                    {
                        cvRet = pSMD.HSVExtractionQty.Run(src, dst, pSMD.ROI.Rectangle);
                        break;
                    }
                default:
                    break;
            }
            return cvRet;
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


        private System.Windows.Point GetOffsetROI(FOV pFOV, Image<Bgr, byte> image)
        {
            try
            {
                System.Windows.Point offset = new System.Windows.Point();
                using (Image<Bgr, byte> src = image.Clone())
                using (Image<Bgr, byte> dst = image.Clone())
                {
                    SMD pSMD = pFOV.SMDs.FirstOrDefault(x => x.SMDType == SMDType.Mark);
                    if (pSMD != null)
                    {
                        src.ROI = pSMD.ROI.Rectangle;
                        dst.ROI = pSMD.ROI.Rectangle;
                        CvResult cvRet;
                        switch (pSMD.Algorithm)
                        {
                            case Algorithm.Unknow:
                                break;
                            //case Algorithm.MarkTracing:
                            //    {
                            //        cvRet = pSMD.MarkTracing.Run(src.Copy(), dst, pSMD.ROI.Rectangle);
                            //        offset.X = cvRet.Center.X - pSMD.MarkTracing.Center.X;
                            //        offset.Y = cvRet.Center.Y - pSMD.MarkTracing.Center.Y;
                            //        break;
                            //    }
                            case Algorithm.TemplateMatching:
                                {
                                    cvRet = pSMD.TemplateMatching.Run(src.Copy(), dst, pSMD.ROI.Rectangle);
                                    offset.X = cvRet.Center.X - pSMD.TemplateMatching.Center.X;
                                    offset.Y = cvRet.Center.Y - pSMD.TemplateMatching.Center.Y;
                                    break;
                                }
                            case Algorithm.CodeRecognition:
                                break;
                            case Algorithm.HSVExtraction:
                                break;
                            default:
                                break;
                        }
                        src.ROI = new Rectangle();
                        dst.ROI = new Rectangle();
                        imbAutoRun.Image = dst.Clone();
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


        private void btntest_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowFOVBitmap(CameraMode mode, Bitmap bmp)
        {
            Dispatcher.Invoke(() =>
            {
                if (bmp != null)
                {
                    if (mode == CameraMode.Top)
                    {
                        _image = bmp.ToImage<Bgr, byte>();
                        imbAutoRun.Image = _image;
                    }
                    else if (mode == CameraMode.Bottom)
                    {
                        _image = bmp.ToImage<Bgr, byte>();
                        imbAutoRun.Image = _image;
                    }
                }
            }
            );
        }
        #region Log
        public void UpdateLogInfo(string message)
        {
            string currentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            int MAX_LINES = 500;
            Dispatcher.Invoke(() =>
            {
                // Thêm dòng mới vào Textbox
                txtLog.AppendText(currentTime + "   " + "[INFO]" + "   " + message + Environment.NewLine);
                // Kiểm tra số lượng dòng và xóa các dòng cũ hơn nếu vượt quá giới hạn
                int lineCount = txtLog.LineCount;
                if (lineCount > MAX_LINES)
                {
                    int indexFirstLineToRemove = txtLog.GetCharacterIndexFromLineIndex(0);
                    int indexLastLineToRemove = txtLog.GetCharacterIndexFromLineIndex(lineCount - MAX_LINES);
                    txtLog.Select(indexFirstLineToRemove, indexLastLineToRemove);
                    txtLog.SelectedText = "";
                }
            });
            Console.WriteLine(message);
            _logger.Info(message);
        }


        public void UpdateLogError(string message)
        {
            string currentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            int MAX_LINES = 500;
            Dispatcher.Invoke(() =>
            {
                // Thêm dòng mới vào Textbox
                txtLog.AppendText(currentTime + "   " + "[ERROR]" + "   " + message + Environment.NewLine);
                // Kiểm tra số lượng dòng và xóa các dòng cũ hơn nếu vượt quá giới hạn
                int lineCount = txtLog.LineCount;
                if (lineCount > MAX_LINES)
                {
                    int indexFirstLineToRemove = txtLog.GetCharacterIndexFromLineIndex(0);
                    int indexLastLineToRemove = txtLog.GetCharacterIndexFromLineIndex(lineCount - MAX_LINES);
                    txtLog.Select(indexFirstLineToRemove, indexLastLineToRemove);
                    txtLog.SelectedText = "";
                }
            });
            Console.WriteLine(message);
            _logger.Error(message);
        }


        #endregion



        private void mnuiTool_Click(object sender, RoutedEventArgs e)
        {
            // Tạo một instance mới của form mới
            SetSpeedRobot newWindow = new SetSpeedRobot();

            // Hiển thị form mới
            newWindow.Show();


        }
    }

    public class FOVResult
    {
        private FOVType _type = FOVType.Unknow;
        private int _result = -1;
        private Image<Bgr, byte> _image = null;

        public FOVType Type
        {
            get => _type;
            set => _type = value;
        }

        public int Result
        {
            get => _result;
            set => _result = value;
        }

        public Image<Bgr, byte> Image
        {
            get => _image;
            set => _image = value;
        }
    }
}
