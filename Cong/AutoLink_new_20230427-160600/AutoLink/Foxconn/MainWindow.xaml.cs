using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.Enums;
using Foxconn.Params;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace Foxconn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Image<Bgr, byte> Image;
        private List<string> listSN = new List<string>();

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private FOVParams _FOVParams { get; set; }
        private MachineParams _MachineParams { get; set; }
        private SelectionMouse _mouse { get; set; }
        private RS232 _serialTerminal { get; set; }
        private TCPClient _tcpRobot { get; set; }
        private TCPClient _tcpSpeed { get; set; }
        private Hikvision _camera { get; set; }
        private bool _drawing { get; set; }
        private Rectangle _rect { get; set; }
        private Rectangle _rect2 { get; set; }
        private Rectangle _rect1 { get; set; }
        private Image<Bgr, byte> _image { get; set; }
        private string _data { get; set; }
        private string _SN { get; set; }
        private Bitmap _bitmap { get; set; }
        private string _SN1 { get; set; }
        private string _SN2 { get; set; }
        int count = 0;
        private Dispatcher _dispatcher = System.Windows.Application.Current.Dispatcher;
        public MainWindow()
        {
            InitializeComponent();
            _mouse = new SelectionMouse();
            _drawing = false;
        }
        private void Window_Initialized(object sender, EventArgs e)
        {

            _FOVParams = new FOVParams();
            _MachineParams = new MachineParams();
            _serialTerminal = new RS232();
            _tcpRobot = new TCPClient();
            _tcpSpeed = new TCPClient();
            _camera = new Hikvision();

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FOVParams.Instance.Read();

            MachineParams.Instance.Read();
            if (!_serialTerminal.Open(MachineParams.Config.Terminal.PortName))
            {
                LogErrorMessage("không thể kết nối port teminal.");
                return;
            }
            _serialTerminal.Send(MachineParams.Config.Terminal.Undo);

            if (!_tcpRobot.Connect(MachineParams.Config.Robot1.IP, MachineParams.Config.Robot1.Port))
            {
                LogInfoMessage("Không thể kết nối đến Robot");
            }

            if (!_tcpSpeed.Connect(MachineParams.Config.Robot2.IP, MachineParams.Config.Robot2.Port))
            {
                LogInfoMessage("Không thể kết nối đến Robot");
            }
            _camera.DeviceListAcq();

            Startup();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Bạn có chắc muốn đóng ứng dụng?", "Xác nhận", MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK)
            {
                e.Cancel = true;
            }

        }


        private void Window_Closed(object sender, EventArgs e)
        {
            _serialTerminal.Close();
            _tcpRobot.Close();
            _camera.Close();
        }

        #region Image


        private void Startup()
        {
            try
            {
                int nRet = _camera.Open("Camera1");
                if (nRet == 1)
                {
                    _camera.SetParameter(KeyName.TriggerMode, 1);
                    _camera.SetParameter(KeyName.TriggerSource, 7);
                    _camera.StartGrabbing();
                    LogInfoMessage("DeviceManager.Open: Camera1 opened");

                }
                else
                {
                    LogInfoMessage("DeviceManager.Open: Camera1 can not open");
                    MessageBox.Show("DeviceManager.Open: Camera1 can not open");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        private void GetImage()
        {
            //_camera.SetParameter(KeyName.ExposureTime, 10000);
            _camera.ClearImageBuffer();
            Thread.Sleep(100);
            _camera.SetParameter(KeyName.TriggerSoftware, 1);
            Thread.Sleep(100);
            using (System.Drawing.Bitmap bmp = _camera.GrabFrame())
            {
                if (bmp != null)
                {
                    Thread.Sleep(100);

                    Bitmap bitmapClone = (Bitmap)bmp.Clone();
                    _bitmap = (Bitmap)bmp.Clone();
                    //Image<Bgr, byte> img = bitmapClone.ToImage<Bgr, byte>();
                    //img = _image;

                    //img.Save("data\\barcodetest123456.png");
                }
            }
        }


        private void SaveImage()
        {
            string folderName = DateTime.Today.ToString("yyyy.MM.dd");
            Directory.CreateDirectory(folderName);
            string photoTime = DateTime.Today.ToString("MM.dd.yyy" + "" + "y HH.mm.ss");
            string imagePath = "logs\\Image" + "\\" + photoTime + _data + ".png";
            //    _image.Save(imagePath);
        }

        private void deleteImage()
        {
            string dirName = "";
            string dateFormat = "yyyy.MM.dd";
            string strBaseDirPath = "logs\\Image";
            DirectoryInfo baseDir = new DirectoryInfo(strBaseDirPath);
            DirectoryInfo[] subDirectories = baseDir.GetDirectories();
            if (subDirectories != null && subDirectories.Length > 0)
            {
                DateTime dtName;
                for (int j = subDirectories.Length - 1; j >= 0; j--)
                {
                    dirName = subDirectories[j].Name.Trim();
                    if (DateTime.TryParseExact(dirName, dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dtName))
                    {
                        if (DateTime.Today.Subtract(dtName).TotalDays > 5)
                            subDirectories[j].Delete(true);
                    }
                }
            }
        }

        private void btnStreamSetting_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            //string folderName = DateTime.Today.ToString("yyyy.MM.dd");
            //Directory.CreateDirectory(folderName);
            //string photoTime = DateTime.Today.ToString("MM.dd.yyy" + "" + "y HH.mm.ss");
            //string imagePath = "data" + "\\" + photoTime + _data + ".png";
            //_image.Save(imagePath);

            string PathCapture = @"data\anhupdate.bmp";
            var img = new Bitmap(PathCapture).ToImage<Bgr, byte>();
            _image = img;
            imbCameraSetting.Image = img;
        }
        private void CloseDevice_Click(object sender, RoutedEventArgs e)
        {
            _camera.Close();
        }

        private void BtnCaptureSetting_Click(object sender, RoutedEventArgs e)
        {
            GetImage();
        }
        #endregion


        #region Mouse Event
        private void imbCameraSetting_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var point = _mouse.GetMousePosition(sender, e);
            var position = $"x = {point.X}, y = {point.Y}";
            if (e.Button != System.Windows.Forms.MouseButtons.Left || imbCameraSetting.Image == null)
                return;
            if (_mouse.IsMouseDown && !point.IsEmpty)
            {
                _mouse.EndPoint = point;
                using (var imageDraw = _image.Clone())
                {
                    var color = _drawing ? new Bgr(65, 205, 40) : new Bgr(48, 59, 255);
                    imageDraw.Draw(_mouse.Rectangle(), color, 1);
                    imbCameraSetting.Image = imageDraw;
                    imbCameraSetting.Refresh();
                }
            }
        }
        private void imbCameraSetting_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _mouse.Clear();
        }
        private void imbCameraSetting_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var point = _mouse.GetMousePosition(sender, e);
            if (!point.IsEmpty)
            {
                _mouse.IsMouseDown = true;
                _mouse.StartPoint = point;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                imbCameraSetting.SetZoomScale(1, e.Location);
                imbCameraSetting.AutoScrollOffset = new System.Drawing.Point(0, 0);
            }
        }
        private void imbCameraSetting_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {

        }
        private Rectangle SetRec(int x)
        {
            _rect = new Rectangle
            {
                X = FOVParams.Config.AppParams.FOVs[0].Components[x].ROI.X,
                Y = FOVParams.Config.AppParams.FOVs[0].Components[x].ROI.Y,
                Width = FOVParams.Config.AppParams.FOVs[0].Components[x].ROI.Width,
                Height = FOVParams.Config.AppParams.FOVs[0].Components[x].ROI.Height,
            };
            System.Drawing.Rectangle box = new System.Drawing.Rectangle(_rect.X, _rect.Y, _rect.Width, _rect.Height);
            // Console.WriteLine("react X" + rect.X);
            //  Console.WriteLine("react Y" + rect.Y);
            //  CvInvoke.Rectangle(Image, box, new Emgu.CV.Structure.MCvScalar(0, 255, 0), 2);
            imbCamera.Image = Image;
            return _rect;

        }
        #endregion


        #region Barcode

        // get Image
        public void ScanSN(Image<Bgr, byte> image)
        {
            _SN = Foxconn.Athgorithm.Barcode.Decode(image);
            Console.WriteLine(_SN);
        }
        #endregion


        #region LoadConfig
        private void Loadconfig(int x)
        {
            txtNameFOV.Text = FOVParams.Config.AppParams.FOVs[0].Name.ToString();
            txtIDFOV.Text = FOVParams.Config.AppParams.FOVs[0].ID.ToString();
            txtExposureTimeFOV.Text = FOVParams.Config.AppParams.FOVs[0].ExposureTime.ToString();
            txtIDSMD.Text = FOVParams.Config.AppParams.FOVs[0].Components[x].ID.ToString();
            cmbAlgorithm.Text = FOVParams.Config.AppParams.FOVs[0].Components[x].Algorithm.ToString();
            TxtROIX.Text = FOVParams.Config.AppParams.FOVs[0].Components[x].ROI.X.ToString();
            TxtROIY.Text = FOVParams.Config.AppParams.FOVs[0].Components[x].ROI.Y.ToString();
            TxtROIW.Text = FOVParams.Config.AppParams.FOVs[0].Components[x].ROI.Width.ToString();
            TxtROIH.Text = FOVParams.Config.AppParams.FOVs[0].Components[x].ROI.Height.ToString();
            txtBarcodePrefix.Text = FOVParams.Config.AppParams.FOVs[0].Components[x].Barcode.Prefix.ToString();
            txtBarcodeLength.Text = FOVParams.Config.AppParams.FOVs[0].Components[x].Barcode.Length.ToString();
            txtBarcodeFormat.Text = FOVParams.Config.AppParams.FOVs[0].Components[x].Barcode.Format.ToString();
        }
        private void reloadSetting_Click(object sender, RoutedEventArgs e)
        {
            if (txtIDSMD.Text == "0")
            {
                Loadconfig(0);
            }
            if (txtIDSMD.Text == "1")
            {
                Loadconfig(1);
            }
        }
        private void ckbSMD1Setting_Checked(object sender, RoutedEventArgs e)
        {
            if (ckbSMD1Setting.IsChecked == true)
            {
                ckbSMD2Setting.IsChecked = false;
                Loadconfig(0);
                //string PathCapture = FOVParams.Config.AppParams.FOVs[0].Components[0].ImagePatch;
                //var img = new Bitmap(PathCapture).ToImage<Bgr, byte>();
                //_image = img;
                //imbCameraSetting.Image = img;
            }
        }

        private void ckbSMD2Setting_Checked(object sender, RoutedEventArgs e)
        {
            if (ckbSMD2Setting.IsChecked == true)
            {
                ckbSMD1Setting.IsChecked = false;
                Loadconfig(1);
                //string PathCapture = FOVParams.Config.AppParams.FOVs[0].Components[1].ImagePatch;
                //var img = new Bitmap(PathCapture).ToImage<Bgr, byte>();
                //_image = img;
                //imbCameraSetting.Image = img;
            }
        }
        private void btnOpenImage_Click(object sender, RoutedEventArgs e)
        {
            string PathCapture = @"data\taytrai.bmp";
            var img = new Bitmap(PathCapture).ToImage<Bgr, byte>();
            _image = img;
            imbCameraSetting.Image = img;
        }
        #endregion



        #region SaveConfig
        private void SaveSetting_Click(object sender, RoutedEventArgs e)
        {
            if (txtIDSMD.Text == "1")
            {
                Saveconfig(0);
                FOVParams.Instance.Write();
                System.Windows.MessageBox.Show("Saved");
            }

            if (txtIDSMD.Text == "2")
            {
                Saveconfig(1);
                System.Windows.MessageBox.Show("Saved");
                FOVParams.Instance.Write();
            }
        }




        private void Saveconfig(int x)
        {
            string input = txtBarcodeLength.Text;
            int number;
            number = Int32.Parse(input);
            FOVParams.Config.AppParams.FOVs[0].Components[x].Barcode.Prefix = txtBarcodePrefix.Text;
            FOVParams.Config.AppParams.FOVs[0].Components[x].Barcode.Format = txtBarcodeFormat.Text;
            FOVParams.Config.AppParams.FOVs[0].Components[x].Barcode.Length = number;
        }
        #endregion



        #region ROI
        private void btnSelecROISetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_drawing && imbCameraSetting.Image != null)
                {
                    _drawing = true;
                }
                else
                {
                    _drawing = false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void btnSaveROISetting_Click(object sender, RoutedEventArgs e)
        {
            if (txtIDSMD.Text == "0")
            {
                SaveROI(0);
                System.Windows.MessageBox.Show("Saved");
            }
            if (txtIDSMD.Text == "1")
            {
                SaveROI(1);
                System.Windows.MessageBox.Show("Saved");
            }
        }
        private void SaveROI(int x)
        {
            try
            {
                FOVParams.Config.AppParams.FOVs[0].Components[x].ROI.X = _mouse.LastRectangle.X;
                FOVParams.Config.AppParams.FOVs[0].Components[x].ROI.Y = _mouse.LastRectangle.Y;
                FOVParams.Config.AppParams.FOVs[0].Components[x].ROI.Width = _mouse.LastRectangle.Width;
                FOVParams.Config.AppParams.FOVs[0].Components[x].ROI.Height = _mouse.LastRectangle.Height;
                Console.WriteLine(_mouse.LastRectangle.X);
                Console.WriteLine(_mouse.LastRectangle.Y);
                Console.WriteLine(_mouse.LastRectangle.Width);
                Console.WriteLine(_mouse.LastRectangle.Height);
                FOVParams.Instance.Write();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion



        #region SFIS
        private int SFIS1(string data)
        {
            _serialTerminal.DataReceived = string.Empty;
            _serialTerminal.Send(data + "\r\n");
            LogInfoMessage($"send to COM2: {data}");
            for (int i = 0; i < 400; i++)
            {
                if (_serialTerminal.DataReceived.Contains("PASS"))
                {
                    LogInfoMessage($"Receive from COM2: {_serialTerminal.DataReceived}");
                    return 1;
                }
                else if (_serialTerminal.DataReceived.Contains("ERRO"))
                {
                    LogInfoMessage($"Receive from COM2: {_serialTerminal.DataReceived}");
                    return -1;
                }
                Thread.Sleep(25);
            }
            return -1;
        }
        #endregion



        #region Log Messages
        private void LogErrorMessage(string message)
        {
            string currentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            int MAX_LINES = 500;
            _dispatcher.Invoke(() =>
            {
                //  SaveImage();
                // Thêm dòng mới vào Textbox
                txtLog.AppendText(currentTime + "   " + "[DEBUG]" + "   " + message + Environment.NewLine);
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
            logger.Debug(message);
        }
        private void LogInfoMessage(string message)
        {
            string currentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            int MAX_LINES = 500;
            _dispatcher.Invoke(() =>
            {
                //  SaveImage();
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
            logger.Info(message);
        }
        #endregion


        #region Set Speed
        private void btnSetSpeed_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() => { SetSpeed(); }).Start();
            string data = "SPEED:X" + "\r\n";
            string speed = txtSetSpeed.Text;
            string data2 = data.Replace("X", speed);
            _tcpSpeed.Send(data2);
            Console.WriteLine(data2);
            LogInfoMessage("SET SPEED : " + speed);
        }
        private void SetSpeed()
        {

        }

        #endregion


        #region Terminal

        private int CheckTerminal(string _SN1, string _SN2)
        {
            string data = MachineParams.Config.Terminal.Format.ToString();
            _data = data.Replace("SN1", _SN1).Replace("SN2", _SN2);
            LogInfoMessage("Data Link: " + _data);
            _serialTerminal.Send(_data);
            Thread.Sleep(100);
            for (int i = 0; i < 100; i++)
            {
                if (_serialTerminal.DataReceived.Contains("PASS"))
                {
                    _serialTerminal.DataReceived = String.Empty;
                    return 1;
                }
                if (_serialTerminal.DataReceived.Contains("ERRO"))
                {
                    _serialTerminal.DataReceived = String.Empty;
                    return 0;
                }
                Thread.Sleep(25);
            }
            return 0;
        }


        #endregion


        private int CheckFOV()
        {
            GetImage();
            using (var image = new Bitmap(_bitmap).ToImage<Bgr, byte>())
            {
                if (image != null)
                {
                    using (var src = image.Clone())
                    using (var dst = image.Clone())
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            src.ROI = SetRec(i);
                            dst.ROI = SetRec(i);
                            // xu ly anh
                            if (i == 0)
                            {
                                ScanSN(src);
                                _rect1 = SetRec(i);
                                _SN1 = _SN;
                                if (_SN1 == "NOT FOUND")
                                {
                                    return 0;
                                }
                                LogInfoMessage("SN1: " + _SN1);
                            }
                            if (i == 1)
                            {
                                ScanSN(src);
                                _rect2 = SetRec(i);
                                _SN2 = _SN;
                                if (_SN2 == "NOT FOUND")
                                {
                                    return 0;
                                }
                                LogInfoMessage("SN2: " + _SN2);
                            }
                            src.ROI = new Rectangle();
                            dst.ROI = new Rectangle();
                        }
                        // Hien thi dst.Clone() ra imageBox
                        // Vẽ 2 hình chữ nhật trên ảnh
                        dst.Draw(_rect1, new Bgr(System.Drawing.Color.Red), 2);
                        dst.Draw(_rect2, new Bgr(System.Drawing.Color.Blue), 2);
                        // Hiển thị ảnh với 2 vùng ROI
                        imbCamera.Image = dst.Clone();
                        //  SaveImage();
                        return 1;
                    }
                }
                else
                {
                    MessageBox.Show("FOV: No Bitmap");
                    return 0;
                }
            }
        }


        #region loop

        private void Autorun()
        {
            while (true)
            {
                try
                {
                    for (count = 0; count < 2; count++)
                    {
                        if (_tcpRobot.Receive().Contains("SCAN"))
                        {
                            LogInfoMessage("Received SCAN");
                            int nRet = CheckFOV();
                            if (nRet == 1)
                            {
                                int nIT = CheckTerminal(_SN1, _SN2);
                                if (nIT == 1)
                                {
                                    //Pass
                                    _tcpRobot.Send("SCANOK" + "\r\n");
                               
                                    LogInfoMessage("SCANOK");
                                }
                                else
                                {
                                    // Fail
                                    _tcpRobot.Send("SCANNG" + "\r\n");
                                    LogInfoMessage("SCANNG");
                                }
                            }
                            else
                            {
                                // Fail
                                _tcpRobot.Send("SCANNG" + "\r\n");
                                LogInfoMessage("SCANNG");
                            }
                        }
                        else
                        {
                            LogErrorMessage(_tcpRobot.Receive());
                        }
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    LogInfoMessage(ex.ToString());
                }
            }
        }

        #endregion

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() => { Autoruntest(); }).Start();
        }
        private void Autoruntest()
        {
            int num = 1;
            while (true)
            {
                try
                {
                    if (_tcpRobot.Receive().Contains("SCAN"))
                    {
                        // Check FOV
                        LogInfoMessage("Received SCAN");
                        int nRet = CheckFOV();
                        if (nRet == 1)
                        {
                            _tcpRobot.Send("SCANOK" + "\r\n");
                            LogInfoMessage("SCANOK");
                        }
                        else
                        {
                            // Fail
                            _tcpRobot.Send("SCANNG" + "\r\n");
                            LogInfoMessage("SCANNG");
                        }
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
                    }
                }
                catch (Exception ex)
                {
                    LogInfoMessage(ex.ToString());
                }
            }
        }
    }
}








