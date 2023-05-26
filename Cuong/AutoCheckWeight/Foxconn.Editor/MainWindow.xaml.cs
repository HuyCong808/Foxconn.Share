using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.AOI.Editor.Controls;
using Foxconn.Editor.Configuration;
using Foxconn.Editor.FoxconnEdit;
using Foxconn.Editor.FoxconnEdit.Camera;
using Foxconn.Editor.FoxconnEdit.Controls;
using Foxconn.Editor.FoxconnEdit.Enums;
using Foxconn.Editor.FoxconnEdit.OpenCV;
using NLog;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static Foxconn.Editor.Enums;

namespace Foxconn.Editor
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
        private CvHSVExtractionControl _hsvExtractionControl = new CvHSVExtractionControl();

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

        bool alowZoom = false;
        private void mnuiZoom_Click(object sender, RoutedEventArgs e)
        {
            
            if (_image !=null)
            {
                alowZoom = !alowZoom;
                if (alowZoom == false)
                {
                    imbCamera.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.Minimum;
                    mnuiZoom.IsChecked = false;
                }
                else
                {
                    imbCamera.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.PanAndZoom;
                    imbCamera.AutoScrollOffset = new System.Drawing.Point(0, 0);
                    mnuiZoom.IsChecked = true;
                    imbCamera.Location = new System.Drawing.Point(0, 0);
                }

            }

        }

        private void mnuiAutoRun_Click(object sender, RoutedEventArgs e)
        {
            AutoRun autoRun = new AutoRun();
            Current.Hide();
            autoRun.ShowDialog();
        }

        private void mnuiTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                int fovindex = cmbFOVs.SelectedIndex;
                int smdindex = cmbSMDs.SelectedIndex;

                if (smdindex > -1)
                {
                    var item = _program.FOVs[fovindex].SMDs[smdindex];

                    string _filePath = _program.FOVs[fovindex].ImagePath;
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
                                    case SMDAlgorithm.TemplateMatching:
                                        {
                                            CvResult cvRet = item.TemplateMatching.Run(src.Copy(), dst, item.ROI.Rectangle);
                                            if (cvRet.Result)
                                            {
                                                item.TemplateMatching.Center = new JPoint { X = cvRet.Center.X, Y = cvRet.Center.Y };
                                            }
                                            _templateMatchingControl.Score = cvRet.Score;
                                            string message = $"{item.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}\r\nCenter: {cvRet.Center}";
                                            MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                            DataContext = cvRet;
                                            break;
                                        }
                                    case SMDAlgorithm.HSVExtraction:
                                        {
                                            CvResult cvRet = item.HSVExtraction.Preview(src.Copy(), dst, item.ROI.Rectangle);
                                            _hsvExtractionControl.Score = cvRet.Score;
                                            item.HSVExtraction.Score = cvRet.Score;
                                            string message = $"{item.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}\r\nQty: {cvRet.Qty}";
                                            MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                            DataContext = cvRet;
                                            break;
                                        }
                                    default:
                                        break;
                                }

                                src.ROI = new System.Drawing.Rectangle();
                                dst.ROI = new System.Drawing.Rectangle();
                                Dispatcher.Invoke(() =>
                                {
                                    imbCamera.Image = dst.Clone();
                                });
                                DataContext = item;
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        private void btnAddFOV_Click(object sender, RoutedEventArgs e)
        {
            _program.AddFOV();
            LoadFOV();
        }

        private void btnDeleteFOV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                if (fovindex > -1)
                {
                    FOV FOVitem = _program.FOVs[fovindex];
                    _program.RemoveFOV(FOVitem);
                    LoadFOV();
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
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
                SMD smd = _program.FOVs[fovindex].SMDs[smdindex];
                _program.RemoveSMD(smd);
                LoadSMD();
            }
        }

        private void cmbFOVs_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int fovindex = cmbFOVs.SelectedIndex;
            if (fovindex > -1)
            {
                var fov = _program.FOVs[fovindex];
                LoadFOVProperties();
                FOVProperties();
                LoadSMD();
                SelectCameraMode(fov);

                GetImage(fov.ImagePath);

            }
        }

        private void cmbSMDs_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int fovindex = cmbFOVs.SelectedIndex;
            int smdindex = cmbSMDs.SelectedIndex;

            if (cmbSMDs.SelectedIndex > -1)
            {
                var item = _program.FOVs[fovindex].SMDs[smdindex];
                LoadSMDProperties();
                LoadROI();
                ShowSMDAlgorithm(item);

                if (cmbSMDAlgorithm.SelectedIndex == (int)SMDAlgorithm.TemplateMatching)
                {
                    imbTemplateImage.Image = item.TemplateMatching.Template;
                }
                else
                {
                    imbTemplateImage.Image = null;
                }
            }
            else
            {
                txtROIX.Text = null;
                txtROIY.Text = null;
                txtROIH.Text = null;
                txtROIW.Text = null;
            }
        }

        public void ShowSMDAlgorithm(SMD item)
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                int smdindex = cmbSMDs.SelectedIndex;
                var smd = _program.FOVs[fovindex].SMDs[smdindex];

                switch (item.Algorithm)
                {
                    case SMDAlgorithm.Unknow:
                        break;
                    case SMDAlgorithm.CodeRecognition:
                        {
                            tbiBarcode.IsSelected = true;
                            _codeRecognitionControl.SetParameters(item.CodeRecognition);

                            cmbMode.ItemsSource = Enum.GetValues(typeof(CodeMode)).Cast<CodeMode>();
                            cmbFormat.ItemsSource = Enum.GetValues(typeof(CodeFormat)).Cast<CodeFormat>();
                        }
                        break;

                    case SMDAlgorithm.HSVExtraction:
                        {
                            tbiHSV.IsSelected = true;
                            _hsvExtractionControl.SetParameters(item.HSVExtraction);
                          
                        }
                        break;

                    case SMDAlgorithm.TemplateMatching:
                        {
                            tbiTemplateMatching.IsSelected = true;
                            _templateMatchingControl.SetParameters(item.TemplateMatching);
                        }
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

        private void cmbSMDAlgorithm_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int fovindex = cmbFOVs.SelectedIndex;
            int smdindex = cmbSMDs.SelectedIndex;

            if (fovindex > -1 && smdindex > -1)
            {
                var item = _program.FOVs[fovindex].SMDs[smdindex];
                ShowSMDAlgorithm(item);
            }
        }


        #region LoadProperties
        public void LoadFOV()
        {
            try
            {
                cmbFOVs.Items.Clear();
                foreach (var itemFOV in _program.FOVs)
                {
                    cmbFOVs.Items.Add(itemFOV.Name);
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
            ckbIsEnableFOV.DataContext = DataContext;
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

        //public void ShowImage()
        //{
        //    try
        //    {
        //        string Path = @"data\images\anhupdate.bmp";
        //        var img = new Bitmap(Path).ToImage<Bgr, byte>();
        //        _image = img;
        //        imbCamera.Image = img;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError(ex.Message);
        //    }
        //}

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
                            var image = new Image<Bgr, byte>(_program.FOVs[fovIndex].ImagePath);
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
                var position = $"X = {point.X} | Y = {point.Y}";
                txtPosition.Text = position;
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


        private void imbCamera_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            // Lấy giá trị cuộn chuột
            int delta = e.Delta;

            // Lấy tỷ lệ zoom hiện tại
            double currentZoom = imbCamera.ZoomScale;

            // Tính toán tỷ lệ zoom mới
            double newZoom = currentZoom + (delta > 0 ? 0.2 : -0.2);

            // Giới hạn tỷ lệ zoom trong khoảng từ 0.1 đến 10 (tuỳ ý)
            newZoom = Math.Max(0.1, Math.Min(10, newZoom));

            // Đặt tỷ lệ zoom mới
            imbCamera.SetZoomScale(newZoom, new System.Drawing.Point(e.X, e.Y));

            // Ngăn chặn sự kiện cuộn chuột lan ra các phần tử khác
           // e.Handled = true;
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
            Console.WriteLine("Error: " + message);
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
                int fovindex = cmbFOVs.SelectedIndex;
                if (fovindex > -1)
                {
                    string imagePath = $"data\\images\\image_FOV{fovindex}.bmp";
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
                            Dispatcher.Invoke(() => imbCamera.Image = _image);

                            bitmapClone.Save(imagePath);
                            _program.FOVs[fovindex].ImagePath = imagePath;
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

        public void GetImage(string data)
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                if (fovindex > -1)
                {
                    FOV fov = _program.FOVs[fovindex];
                    if (fov.ImagePath != "")
                    {
                        var img = new Bitmap(data).ToImage<Bgr, byte>().Clone();
                        _image = img;
                        imbCamera.Image = _image;
                    }
                    else
                    {
                        imbCamera.Image = null;
                        _image = null;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        private void btnUpdateImageBlock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                string imagePath = $"data\\images\\image_FOV{fovindex}.bmp";
                if (fovindex > -1)
                {
                    FOV fov = _program.FOVs[fovindex];
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
                                    //_program.SetImageBlock(item.ImageBlockName, (System.Drawing.Bitmap)bmp.Clone());
                                    Bitmap bitmapClone = (Bitmap)bmp.Clone();
                                    _image = bitmapClone.ToImage<Bgr, byte>();
                                    imbCamera.Image = _image;

                                    bmp.Save(imagePath);
                                    fov.ImagePath = imagePath;
                                }
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

        private void btnUpdateTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                int smdindex = cmbSMDs.SelectedIndex;
                if (cmbSMDAlgorithm.SelectedIndex == (int)SMDAlgorithm.TemplateMatching && smdindex > -1)
                {
                    if (fovindex > -1 && smdindex > -1)
                    {
                        var item = _program.FOVs[fovindex].SMDs[smdindex];

                        // _rect = new Rectangle(_mouse.LastRectangle.X, _mouse.LastRectangle.Y, _mouse.LastRectangle.Width, _mouse.LastRectangle.Height);
                        _rect = item.ROI.Rectangle;
                        if (_image != null)
                        {
                            var dst = _image.Clone();
                            dst.ROI = _rect;
                            dst.Save(@$"temp\UpdateTemplateSMD{smdindex}.png");
                            var image = dst.Copy();
                            image.ROI = Rectangle.Empty;

                            if (!image.ROI.IsEmpty)
                            {
                                item.TemplateMatching.Template = image;
                                imbTemplateImage.Image = item.TemplateMatching.Template;
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

        private void btnGetHSVColorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                int smdindex = cmbSMDs.SelectedIndex;
                if (smdindex > -1 && cmbSMDAlgorithm.SelectedIndex == (int)SMDAlgorithm.HSVExtraction)
                {
                    var item = _program.FOVs[fovindex].SMDs[smdindex];
                    _rect = item.ROI.Rectangle;
                    // _rect = new Rectangle(_mouse.LastRectangle.X, _mouse.LastRectangle.Y, _mouse.LastRectangle.Width, _mouse.LastRectangle.Height);
                    if (_image != null)
                    {
                        var dst = _image.Clone();
                        dst.ROI = _rect;
                        var image = dst.Copy();

                        if (!image.ROI.IsEmpty)
                        {
                            (ValueRange H, ValueRange S, ValueRange V) = item.HSVExtraction.HSVRange(image);
                            item.HSVExtraction.Hue = H;
                            item.HSVExtraction.Saturation = S;
                            item.HSVExtraction.Value = V;
                        }
                       // image.ROI = new System.Drawing.Rectangle();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

    }
}
