using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.AOI.Editor.Controls;
using Foxconn.AutoWeight.Configuration;
using Foxconn.AutoWeight.FoxconnEdit;
using Foxconn.AutoWeight.FoxconnEdit.Camera;
using Foxconn.AutoWeight.FoxconnEdit.Controls;
using Foxconn.AutoWeight.FoxconnEdit.Enums;
using Foxconn.AutoWeight.FoxconnEdit.OpenCV;
using NLog;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static Foxconn.AutoWeight.Enums;

namespace Foxconn.AutoWeight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Current;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Board _program
        {
            get => ProgramManager.Current.Program;
            set => ProgramManager.Current.Program = value;
        }
        private bool _drawing { get; set; }
        private Rectangle _rect { get; set; }
        private SelectionMouse _mouse { get; set; }
        private Image<Bgr, byte> _image { get; set; }
        private string _SN { get; set; }
        public Image<Bgr, byte> Image;

        private Worker _loopWorker;
        //private VideoCapture _capture = null;
        //private Mat _frame = new Mat();
        //private bool _isLiveStream = false;
        private ICamera _camera = null;
        private bool _isStreaming = false;

        private CvCodeRecognitionControl _codeRecognitionControl = new CvCodeRecognitionControl();
        private CvTemplateMatchingControl _templateMatchingControl = new CvTemplateMatchingControl();

        DeviceManager device = DeviceManager.Current;

        public MainWindow()
        {
            InitializeComponent();
            Current = this;
            _mouse = new SelectionMouse();
            _drawing = false;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            StartUp();
        }

        private void Window_Closed(object sender, EventArgs e)
        {

            ProgramManager.Current.SaveProgram();
            DeviceManager.Current.Close();
            LogInfo("Shutdown Application");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Exit application?", "Exit", MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) != MessageBoxResult.OK)
                e.Cancel = true;
        }

        public void StartUp()
        {
            try
            {
                LogInfo("StartUp Application");
                ProgramManager.Current.OpenProgram();
                MachineParams.Reload();
                LoadFOV();

                DeviceManager.Current.Open();
                DeviceManager.Current.Ping();

                _loopWorker = new Worker(new ThreadStart(MainProcess));
                _loopWorker.Start();



            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            KeyboardShutcut(e);
        }

        private void KeyboardShutcut(KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                mnuiTest_Click(null, null);
            }
        }

        private void mnuiNewProgram_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiOpenProgram_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiSaveProgram_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiAutoRun_Click(object sender, RoutedEventArgs e)
        {
            AutoRun autoRun = new AutoRun();
            Current.Hide();
            autoRun.ShowDialog();
        }

        private void mnuiTest_Click(object sender, RoutedEventArgs e)
        {
            int fovindex = cmbFOVs.SelectedIndex;
            int smdindex = cmbSMDs.SelectedIndex;

            if (smdindex > -1)
            {
                var item = _program.FOVs[fovindex].SMDs[smdindex];

                string _filePath = @"data\Images\anhupdate.bmp";
                using (var image = new Image<Bgr, byte>(_filePath))
                    if (image != null)
                    {
                        using (var src = image.Clone())
                        using (var dst = image.Clone())
                        {
                            src.ROI = item.ROI.Rectangle;
                            dst.ROI = item.ROI.Rectangle;

                            switch (item.Algorithm)
                            {
                                case SMDAlgorithm.Unknow:
                                    break;
                                case SMDAlgorithm.CodeRecognition:
                                    {
                                        CvResult cvRet = item.CodeRecognition.Run(src.Copy(), dst, item.ROI.Rectangle);
                                        string message = $"{item.Algorithm}: {cvRet.Result}\r\nSN: {cvRet.Content}\r\nLength: {cvRet.Content.Length}";
                                        MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                        break;
                                    }
                                default:
                                    break;
                            }
                            src.ROI = new System.Drawing.Rectangle();
                            dst.ROI = new System.Drawing.Rectangle();
                        }

                    }
            }
        }

        private void btnAddFOV_Click(object sender, RoutedEventArgs e)
        {
            _program.AddFOV();
            LoadFOV();
        }

        private void btnDeleteFOV_Click(object sender, RoutedEventArgs e)
        {
            int fovindex = cmbFOVs.SelectedIndex;
            if (fovindex > -1)
            {
                _program.FOVs.RemoveAt(fovindex);
                _program.SortFOV();
                LoadFOV();
            }
        }

        private void btnAddSMD_Click(object sender, RoutedEventArgs e)
        {
            int fovindex = cmbFOVs.SelectedIndex;
            _program.AddSMD(fovindex);
            LoadSMD();
        }

        private void btnDeleteSMD_Click(object sender, RoutedEventArgs e)
        {
            int fovindex = cmbFOVs.SelectedIndex;
            int smdindex = cmbSMDs.SelectedIndex;
            if (fovindex > -1 && smdindex > -1)
            {
                _program.FOVs[fovindex].SMDs.RemoveAt(smdindex);
                _program.SortSMD(fovindex);
                LoadSMD();
            }
        }

        private void cmbFOVs_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int fovindex = cmbFOVs.SelectedIndex;
            var fov = _program.FOVs[fovindex];
            if (fovindex == 0)
            {
                ShowImage();
            }
            else
            {
                imbCamera.Image = null;
            }


            if (fovindex > -1)
            {
                LoadFOVProperties();
                FOVProperties();
                LoadSMD();
                SelectCameraMode(fov);
            }
        }

        private void cmbSMDs_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int fovindex = cmbFOVs.SelectedIndex;
            int smdindex = cmbSMDs.SelectedIndex;
            if (cmbSMDs.SelectedIndex > -1)
            {
                LoadSMDProperties();
                LoadROI();

            }
            else
            {
                txtROIX.Text = null;
                txtROIY.Text = null;
                txtROIH.Text = null;
                txtROIW.Text = null;
            }
        }

        private void cmbSMDAlgorithm_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                int smdindex = cmbSMDs.SelectedIndex;
                var item = _program.FOVs[fovindex].SMDs[smdindex];

                switch (item.Algorithm)
                {
                    case SMDAlgorithm.Unknow:
                        break;
                    case SMDAlgorithm.CodeRecognition:
                        {
                            tbiBarcode.IsSelected = true;
                            _codeRecognitionControl.SetParameters(item.CodeRecognition);
                            DataContext = _program.FOVs[fovindex].SMDs[smdindex];
                            cmbMode.ItemsSource = Enum.GetValues(typeof(CodeMode)).Cast<CodeMode>();
                            cmbFormat.ItemsSource = Enum.GetValues(typeof(CodeFormat)).Cast<CodeFormat>();
                        }
                        break;
                    case SMDAlgorithm.HSVExtraction:
                        tbiHSV.IsSelected = true;
                        break;
                    case SMDAlgorithm.TemplateMatching:
                        tbiTemplateMatching.IsSelected = true;
                        break;
                    case SMDAlgorithm.Contour:
                        tbiContour.IsSelected = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }


        #region LoadProperties
        public void LoadFOV()
        {
            try
            {
                cmbFOVs.Items.Clear();
                foreach (var fov in _program.FOVs)
                {
                    cmbFOVs.Items.Add(fov.Name);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        public void LoadSMD()
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                cmbSMDs.Items.Clear();
                foreach (var smd in _program.FOVs[fovindex].SMDs)
                {
                    cmbSMDs.Items.Add(smd.name);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        public void LoadFOVProperties()
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                if (fovindex > -1)
                {
                    DataContext = _program.FOVs[fovindex];
                    cmbFOVType.ItemsSource = Enum.GetValues(typeof(FOVType));
                    cmbCameraMode.ItemsSource = Enum.GetValues(typeof(CameraMode));
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        public void FOVProperties()
        {
            txtIDFOV.DataContext = DataContext;
            txtNameFOV.DataContext = DataContext;
            txtExposureTime.DataContext = DataContext;
            cmbFOVType.DataContext = DataContext;
            cmbCameraMode.DataContext = DataContext;
        }

        public void LoadSMDProperties()
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                int smdindex = cmbSMDs.SelectedIndex;
                if (fovindex > -1 && smdindex > -1)
                {
                    DataContext = _program.FOVs[fovindex].SMDs[smdindex];
                    cmbSMDType.ItemsSource = Enum.GetValues(typeof(SMDType));
                    cmbSMDAlgorithm.ItemsSource = Enum.GetValues(typeof(SMDAlgorithm));
                }

            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        #endregion

        #region ROI

        public void ShowImage()
        {
            try
            {
                string Path = @"data\Images\anhupdate.bmp";
                var img = new Bitmap(Path).ToImage<Bgr, byte>();
                _image = img;
                imbCamera.Image = img;
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }
        }

        private void btnAddROI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                int smdindex = cmbSMDs.SelectedIndex;
                if (fovindex > -1 && smdindex > -1)
                {
                    if (!_drawing && imbCamera.Image != null)
                    {
                        _drawing = true;
                    }
                    else
                    {
                        _drawing = false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }
        }

        private void btnDeleteROI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int fovIndex = cmbFOVs.SelectedIndex;
                int smdIndex = cmbSMDs.SelectedIndex;
                if (fovIndex > -1 && smdIndex > -1)
                {
                    var smd = _program.FOVs[fovIndex].SMDs.Find(x => x.Id == smdIndex);
                    if (smd != null)
                    {
                        smd.ROI.X = 0;
                        smd.ROI.Y = 0;
                        smd.ROI.Width = 0;
                        smd.ROI.Height = 0;
                    }
                    LoadROI();
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        private void btnSaveROI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                int smdindex = cmbSMDs.SelectedIndex;
                _program.FOVs[fovindex].SMDs.Find(x => x.Id == smdindex);
                if (smdindex > -1)
                {
                    if (_drawing == true)
                    {
                        SaveROI(smdindex);
                        MessageBox.Show("Save SMD ROI success!", "Save Rect", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                        LoadROI();
                        _drawing = false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }
        }

        private void btnReloadROI_Click(object sender, RoutedEventArgs e)
        {
            ReloadROIArea();
        }

        public void SaveROI(int smdId)
        {
            try
            {
                int fovIndex = cmbFOVs.SelectedIndex;
                int smdIndex = cmbFOVs.SelectedIndex;
                if (fovIndex > -1 && smdIndex > -1)
                {
                    var currentSMD = _program.FOVs[fovIndex].SMDs.Find(x => x.Id == smdId);
                    if (currentSMD != null)
                    {
                        if (_mouse.LastRectangle != null)
                        {
                            int X = currentSMD.ROI.X = _mouse.LastRectangle.X;
                            int Y = currentSMD.ROI.Y = _mouse.LastRectangle.Y;
                            int W = currentSMD.ROI.Width = _mouse.LastRectangle.Width;
                            int H = currentSMD.ROI.Height = _mouse.LastRectangle.Height;

                            Console.WriteLine("X : " + _mouse.LastRectangle.X);
                            Console.WriteLine("Y : " + _mouse.LastRectangle.Y);
                            Console.WriteLine("Width : " + _mouse.LastRectangle.Width);
                            Console.WriteLine("Height : " + _mouse.LastRectangle.Height);

                            // Khởi tạo một đối tượng Image<Bgr, byte> từ một hình ảnh có chứa vùng ROI
                            var image = new Image<Bgr, byte>(@"data\Images\anhupdate.bmp");
                            var roi = new Rectangle(X, Y, W, H);
                            // Cắt ảnh từ vùng ROI
                            var roiImage = image.Copy(roi);
                            // Lưu ảnh cắt được ra file
                            roiImage.Save(@$"temp\roiSMD{smdId}.png");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }

        }

        public void LoadROI()
        {
            try
            {
                int fovIndex = cmbFOVs.SelectedIndex;
                int smdIndex = cmbSMDs.SelectedIndex;
                if (fovIndex > -1 && smdIndex > -1)
                {
                    var fov = _program.FOVs.Find(x => x.ID == fovIndex);
                    if (fov != null)
                    {
                        var smd = fov.SMDs.Find(x => x.Id == smdIndex);
                        if (smd != null)
                        {
                            GetRect(smd);
                            txtROIX.Text = smd.ROI.X.ToString();
                            txtROIY.Text = smd.ROI.Y.ToString();
                            txtROIH.Text = smd.ROI.Height.ToString();
                            txtROIW.Text = smd.ROI.Width.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }

        }

        public JRect GetRect(SMD smd)
        {
            try
            {
                if (_image != null)
                {
                    int X = smd.ROI.X;
                    int Y = smd.ROI.Y;
                    int W = smd.ROI.Width;
                    int H = smd.ROI.Height;
                    _rect = new Rectangle(X, Y, W, H);
                    // Hien thi dst.Clone() ra imageBox
                    var dst = _image.Clone();
                    // Vẽ 1 hình chữ nhật trên ảnh
                    dst.Draw(_rect, new Bgr(System.Drawing.Color.Green), 3);
                    // Hiển thị ảnh với 1 vùng ROI
                    imbCamera.Image = dst.Clone();
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
            return smd.ROI;
        }

        public void ReloadROIArea()
        {
            try
            {
                int fovIndex = cmbFOVs.SelectedIndex;
                int smdIndex = cmbSMDs.SelectedIndex;
                if (fovIndex > -1 && smdIndex > -1)
                {
                    var smd = _program.FOVs[fovIndex].SMDs.Find(x => x.Id == smdIndex);
                    if (smd != null)
                    {
                        smd.ROI.X = int.Parse(txtROIX.Text);
                        smd.ROI.Y = int.Parse(txtROIY.Text);
                        smd.ROI.Width = int.Parse(txtROIW.Text);
                        smd.ROI.Height = int.Parse(txtROIH.Text);
                        LoadROI();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }
        }

        #endregion

        #region MouseEvent
        private void imbCamera_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var point = _mouse.GetMousePosition(sender, e);
            if (!point.IsEmpty)
            {
                _mouse.IsMouseDown = true;
                _mouse.StartPoint = point;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                imbCamera.SetZoomScale(1, e.Location);
                imbCamera.AutoScrollOffset = new System.Drawing.Point(0, 0);
            }
        }

        private void imbCamera_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {

                var point = _mouse.GetMousePosition(sender, e);
                var position = $"x = {point.X}, y = {point.Y}";
                if (e.Button != System.Windows.Forms.MouseButtons.Left || imbCamera.Image == null)
                    return;
                if (_mouse.IsMouseDown && !point.IsEmpty)
                {
                    _mouse.EndPoint = point;
                    using (var imageDraw = _image.Clone())
                    {
                        var color = _drawing ? new Bgr(65, 205, 40) : new Bgr(48, 59, 255);
                        imageDraw.Draw(_mouse.Rectangle(), color, 2);
                        imbCamera.Image = imageDraw;
                        imbCamera.Refresh();
                    }
                }

            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }

        }

        private void imbCamera_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _mouse.Clear();
        }

        #endregion

        #region Logs
        public void LogInfo(string message)
        {
            logger.Info(message);
            Console.WriteLine(message);
        }

        public void LogError(string message)
        {
            logger.Error(message);
            Console.WriteLine(message);
        }

        #endregion

        #region Communication

        public void SendtoPort(string message)
        {
            try
            {
                MachineParams param = MachineParams.Current;
                if (param.Terminal.IsEnabled)
                {
                    device.Terminal.SendData(message);
                    Console.WriteLine("Sent to Serial Port: " + message);
                    MessageBox.Show("Sent to Serial Port: " + message);
                }
                else
                {
                    MessageBox.Show("SerialPort is not Open");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void SendtoTcpClient(string message)
        {
            try
            {
                MachineParams param = MachineParams.Current;
                if (param.Robot.IsEnabled)
                {
                    device.TCPClient.SocketWriteData(message);
                    MessageBox.Show("Sent to TcpClient: " + message);
                    Console.WriteLine("Sent to TcpClient: " + message);
                }
                else
                {
                    MessageBox.Show("Terminal is not Open");
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        #endregion

        #region Connect Camera
        //private void ProcessFrame(object sender, EventArgs arg)
        //{
        //    if (_capture != null && _capture.Ptr != IntPtr.Zero)
        //    {
        //        _capture.Retrieve(_frame, 0);
        //        imbCamera.Image = _frame;
        //    }
        //}

        private void SelectCameraMode(FOV item)
        {
            DeviceManager device = DeviceManager.Current;
            if (item.CameraMode == CameraMode.Top)
            {
                _camera = device.Camera1;
            }
            else if (item.CameraMode == CameraMode.Bottom)
            {
                _camera = device.Camera2;
            }
            else
            {
                _camera = null;
            }
        }

        private void btnGrabFrame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int fovIndex = cmbFOVs.SelectedIndex;
                if (fovIndex > -1)
                {
                    if (_camera == null)
                    {
                        MessageBox.Show("Camera unavailable.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }

                    _camera.ClearImageBuffer();
                    _camera.SetParameter(KeyName.TriggerSoftware, 1);
                    _isStreaming = false;
                    using (System.Drawing.Bitmap bmp = _camera.GrabFrame())
                    {
                        if (bmp != null)
                        {
                            Bitmap bitmapClone = (Bitmap)bmp.Clone();
                            _image = bitmapClone.ToImage<Bgr, byte>();
                            Dispatcher.Invoke(() => imbCamera.Image = bitmapClone.ToImage<Bgr, byte>());
                        }
                    }


                    //if (imbCamera.Image != null)
                    //{
                    //    string filename = @"temp\" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
                    //    Bitmap bmp = new Bitmap(imbCamera.Width, imbCamera.Height);
                    //    imbCamera.DrawToBitmap(bmp, new Rectangle(0, 0, imbCamera.Width, imbCamera.Height));
                    //    bmp.Save(filename);
                    //}
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnStreaming_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (!_loopWorker.IsRunning)
                //    _loopWorker.Start();
                //if (_camera != null)
                //{
                //    _isStreaming = !_isStreaming;
                //}

                if (!_loopWorker.IsRunning)
                    _loopWorker.Start();
                if (_camera != null)
                {
                    bool temp = _isStreaming;
                    if (temp)
                    {
                        _isStreaming = !temp;
                        imgStreaming.Source = BitmapFrame.Create(new Uri("pack://application:,,,/Icons/video_call_32px_off.png", UriKind.RelativeOrAbsolute));
                    }
                    else
                    {
                        _isStreaming = !temp;
                        imgStreaming.Source = BitmapFrame.Create(new Uri("pack://application:,,,/Icons/video_call_32px_on.png", UriKind.RelativeOrAbsolute));
                    }
                }
                //_isLiveStream = !_isLiveStream;


                //if (_capture != null)
                //{
                //    if (!_captureInProcess)
                //    {
                //        _capture.Start();
                //        _captureInProcess = true;
                //    }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnUpdateImageBlock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                FOV item = _program.FOVs[fovindex];
                using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
                {
                    ofd.Filter = "All Picture Files (*.bmp, *.png, *.jpg, *.jpeg)|*.bmp; *.png; *.jpg; *.jpeg|All files (*.*)|*.*";
                    ofd.FilterIndex = 0;
                    ofd.RestoreDirectory = true;
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ofd.FileName))
                        {
                            if (bmp != null)
                            {
                                _program.SetImageBlock(item.ImageBlockName, (System.Drawing.Bitmap)bmp.Clone());
                                Bitmap bitmapClone = (Bitmap)bmp.Clone();
                                _image = bitmapClone.ToImage<Bgr, byte>();
                                imbCamera.Image = bmp.ToImage<Bgr, byte>();
                                //Bitmap bitmapClone = (Bitmap)bmp.Clone();
                                //_image = bitmapClone.ToImage<Bgr, byte>();
                                //Dispatcher.Invoke(() => imbCamera.Image = _image);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }




        }

        //private void LoopSettings()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            if (_isLiveStream)
        //            {
        //                _capture.Start();
        //                _capture.Retrieve(_frame, 0);
        //                if (_frame != null)
        //                {
        //                    imbCamera.Image = _frame;
        //                }
        //                _capture.Pause();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //        Thread.Sleep(100);
        //    }
        //}


        private void MainProcess()
        {
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Main thread.";
            while (!_loopWorker.WaitStopSignal(25))
            {
                try
                {
                    if (_isStreaming)
                    {
                        if (_camera == null)
                            continue;
                        _camera.ClearImageBuffer();
                        _camera.SetParameter(KeyName.TriggerSoftware, 1);
                        using (System.Drawing.Bitmap bmp = _camera.GrabFrame())
                        {
                            if (bmp != null)
                            {
                                Bitmap bitmapClone = (Bitmap)bmp.Clone();
                                _image = bitmapClone.ToImage<Bgr, byte>();
                                Dispatcher.Invoke(() => imbCamera.Image = bmp.ToImage<Bgr, byte>());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex.Message);
                }
                Thread.Sleep(10);
            }
        }

        #endregion


        private void btnUpdateTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            int fovindex = cmbFOVs.SelectedIndex;
            int smdindex = cmbSMDs.SelectedIndex;
            if (fovindex > -1 && smdindex > -1)
            {
                var item = _program.FOVs[fovindex].SMDs[smdindex];
                using (Image<Bgr, byte> image = _program.GetImageBlock(item)?.Clone())
                {
                    if (image != null)
                    {
                        image.ROI = item.ROI.Rectangle;
                        item.TemplateMatching.Template = image.Copy();
                        image.ROI = new System.Drawing.Rectangle();
                    }
                }

            }

        }
    }
}
