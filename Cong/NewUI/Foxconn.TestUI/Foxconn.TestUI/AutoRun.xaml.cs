using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.TestUI.Camera;
using Foxconn.TestUI.Config;
using Foxconn.TestUI.Enums;
using Foxconn.TestUI.OpenCV;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Foxconn.TestUI.Editor;
using Foxconn.TestUI.View;
using Rectangle = System.Drawing.Rectangle;

namespace Foxconn.TestUI
{
    /// <summary>
    /// Interaction logic for AutoRun.xaml
    /// </summary>
    public partial class AutoRun : Window
    {
        private Board _program => ProgramManager.Instance.Program;
        private MachineParams _param = MachineParams.Current;
        private DeviceManager _device = DeviceManager.Current;
        private VideoCapture _capture = null;
        private Mat _frame;
        private string _SN1 { get; set; }
        private string _SN2 { get; set; }
        public AutoRun()
        {
            InitializeComponent();
            // new Thread(() => { AutoRunProcess(); }).Start();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProgramManager.Instance.OpenProgram();
            //DeviceManager.Current.Open();

        }


        private void AutoRunProcess()
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Auto run thread.";
            FOVType typePCB1 = FOVType.Unknow;
            {
                int num = 1;
                {
                    while (true)
                    {
                        try
                        {
                            if (_device.Robot1.DataReceived == "SCAN")
                            {

                                Stopwatch stopwatch = new Stopwatch();
                                stopwatch.Start();
                                Console.WriteLine($"AutoRunProcess =====> {typePCB1}");
                                int nRet = CheckType(typePCB1);
                                if (nRet == 1)
                                {
                                    _device.Robot2.SocketWriteData("SCANOK" + "\r\n");
                                    Console.WriteLine("SCANOK" + "\r\n");
                                    //LogInfoMessage("SCANOK");
                                }
                                else
                                {
                                    // Fail
                                    _device.Robot2.SocketWriteData("SCANNG" + "\r\n");
                                    Console.WriteLine("SCANNG" + "\r\n");
                                    // LogInfoMessage("SCANNG");
                                }

                                //show result
                                Dispatcher.Invoke(() =>
                                {
                                    if (num == 1)
                                    {
                                        lblPCB1.Content = nRet == 1 ? "PASS" : "FAIL";
                                        lblPCB1.Background = nRet == 1 ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
                                    }
                                    if (num == 2)
                                    {
                                        lblPCB2.Content = nRet == 1 ? "PASS" : "FAIL";
                                        lblPCB2.Background = nRet == 1 ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
                                    }
                                });
                                ++num;
                                if (num > 2)
                                {
                                    Thread.Sleep(1000);
                                    num = 1;
                                    Dispatcher.Invoke(() =>
                                    {
                                        lblPCB1.Content = string.Empty;
                                        lblPCB1.Background = System.Windows.Media.Brushes.Gray;
                                        lblPCB2.Content = string.Empty;
                                        lblPCB2.Background = System.Windows.Media.Brushes.Gray;
                                    });
                                }
                                stopwatch.Stop();
                            }

                        }
                        catch (Exception ex)
                        {


                        }
                    }
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
                        using (Image<Bgr, byte> src = bmp.ToImage<Bgr, byte>())
                        using (Image<Bgr, byte> dst = bmp.ToImage<Bgr, byte>())
                        {
                            IEnumerable<SMD> pSMDs = _program.FOVs[0].SMDs;
                            foreach (SMD pSMD in pSMDs)
                            {
                                Rectangle offsetROI = new Rectangle
                                {
                                    X = pSMD.ROI.Rectangle.X,
                                    Y = pSMD.ROI.Rectangle.Y,
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
                                if (cvRet.Result)
                                {
                                    dst.Draw(offsetROI, new Bgr(System.Drawing.Color.Green), 3);
                                    imbAutoRun.Image = dst.Clone();
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
                        if (cvRet.Result)
                        {
                            if (pSMD.SMDType == SMDType.SN1)
                            {
                                _SN1 = cvRet.Content;
                                Console.WriteLine("SN1:" + _SN1);
                            }
                            else if (pSMD.SMDType == SMDType.SN2)
                            {
                                _SN2 = cvRet.Content;
                                Console.WriteLine("SN2:" + _SN2);
                            }
                        }
                        else
                        {
                            _SN1 = string.Empty;
                            _SN2 = string.Empty;
                            Console.WriteLine("SN empty");
                        }
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


        private void btntest_Click(object sender, RoutedEventArgs e) // k chụp ảnh, check test
        {
            string PathCapture = @"anhupdate.bmp";
            var img = new Bitmap(PathCapture);
            if (img != null)
            {

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                using (Image<Bgr, byte> src = img.ToImage<Bgr, byte>())
                using (Image<Bgr, byte> dst = img.ToImage<Bgr, byte>())
                {
                    var pFOV = _program.FOVs[0];
                    IEnumerable<SMD> pSMDs = _program.FOVs[0].SMDs;
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
                        if (cvRet.Result)
                        {
                            Console.WriteLine("check {0} OK ", pSMD.Name.ToString());
                        }
                        if (!cvRet.Result)
                        {
                            Console.WriteLine("check {0} NG", pSMD.Name.ToString()) ;
                        }
                        src.ROI = new System.Drawing.Rectangle();
                        dst.ROI = new System.Drawing.Rectangle();
                        imbAutoRun.Image = dst.Clone();
                    }
                    stopwatch.Stop();
                    string circleTime = "Circle Time : a Milliseconds";
                    string circleTime2 = circleTime.Replace("a", stopwatch.ElapsedMilliseconds.ToString());
                    lblCircleTime.Content = circleTime2;
                }
            }
        }


        private void mnuiTool_Click(object sender, RoutedEventArgs e)
        {
            // Tạo một instance mới của form mới
            SetSpeedRobot newWindow = new SetSpeedRobot();

            // Hiển thị form mới
            newWindow.Show();
        }
    }
}
