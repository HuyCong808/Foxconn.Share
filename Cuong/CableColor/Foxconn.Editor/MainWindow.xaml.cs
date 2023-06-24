using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.AOI.Editor.Controls;
using Foxconn.Editor.Camera;
using Foxconn.Editor.Configuration;
using Foxconn.Editor.Controls;
using Foxconn.Editor.Dialogs;
using Foxconn.Editor.Enums;
using Foxconn.Editor.OpenCV;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Foxconn.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static MainWindow Current;

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
        private CvLuminanceExtractionControl _luminanceExtractionControl = new CvLuminanceExtractionControl();
        public CvHSVExtraction HSVExtraction = new CvHSVExtraction();

        DeviceManager device = DeviceManager.Current;


        #region Binding Property
        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            Logger.Current.Create();
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
            int fovindex = cmbFOVs.SelectedIndex;
            int smdindex = cmbSMDs.SelectedIndex;

            if (e.Key == Key.F5)
            {
                mnuiTest_Click(null, null);
            }
            else if (e.Key == Key.Up)
            {
                e.Handled = true;
                MoveROIUp(1);

            }
            else if (e.Key == Key.Down)
            {
                e.Handled = true;
                MoveROIDown(1);

            }
            else if (e.Key == Key.Left)
            {
                e.Handled = true;
                MoveROILeft(1);
            }
            else if (e.Key == Key.Right)
            {
                e.Handled = true;
                MoveROIRight(1);
            }   
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                mnuiSaveProgram_Click(null, null);
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
            switch (IsAdmin())
            {
                case 1:
                    ProgramManager.Current.SaveProgram();
                    MessageBox.Show("Saved Program!", "Save Program", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case 0:
                    break;
                case -1:
                    if (MessageBox.Show("Wrong Username or Password! \n\rTry again?", "Save Program", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                        mnuiSaveProgram_Click(null, null);
                    break;
            }

        }

        private void muniCloseProgram_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void mnuiConsoleApp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiFileLocation_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory);
        }

        bool alowZoom = false;
        private void mnuiZoom_Click(object sender, RoutedEventArgs e)
        {

            if (_image != null)
            {
                alowZoom = !alowZoom;
                if (alowZoom == false)
                {
                    mnuiZoom.IsChecked = false;
                }
                else
                {
                    mnuiZoom.IsChecked = true;
                }
            }
        }

        private void mnuiAutoRun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isStreaming)
                {
                    AutoRun autoRun = new AutoRun();
                    if (autoRun.StartUp())
                    {
                        _loopWorker.Stop();
                        Current.Hide();
                        autoRun.ShowDialog();
                    }
                    else
                    {
                        LogError("Can not AutoRun");
                        MessageBox.Show("Can not AutoRun", "AutoRun", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {

                LogError(ex.Message);
            }
        }

        private void mnuiTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                int smdindex = cmbSMDs.SelectedIndex;

                if (smdindex > -1 && cmbSMDAlgorithm.SelectedIndex > -1)
                {
                    SMD smd = _program.FOVs[fovindex].SMDs[smdindex];

                    using (Image<Bgr, byte> src = _program.GetImageBlock(smd)?.Clone())
                    using (Image<Bgr, byte> dst = _program.GetImageBlock(smd)?.Clone())
                    {
                        src.ROI = smd.ROI.Rectangle;
                        dst.ROI = smd.ROI.Rectangle;

                        switch (smd.Algorithm)
                        {
                            case SMDAlgorithm.Unknow:
                                break;

                            case SMDAlgorithm.CodeRecognition:
                                {
                                    CvResult cvRet = smd.CodeRecognition.Run(src.Copy(), dst, smd.ROI.Rectangle);
                                    string message = $"{smd.Algorithm}: {cvRet.Result}\r\nSN: {cvRet.Content}\r\nLength: {cvRet.Content.Length}";
                                    MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                    break;
                                }
                            case SMDAlgorithm.TemplateMatching:
                                {
                                    CvResult cvRet = smd.TemplateMatching.Run(src.Copy(), dst, smd.ROI.Rectangle);
                                    if (cvRet.Result)
                                    {
                                        smd.TemplateMatching.Center = new JPoint { X = cvRet.Center.X, Y = cvRet.Center.Y };
                                    }
                                    _templateMatchingControl.Score = cvRet.Score;
                                    smd.TemplateMatching.Score = cvRet.Score;
                                    string message = $"{smd.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}\r\nCenter: {cvRet.Center}";
                                    MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                    //DataContext = cvRet;
                                    break;
                                }
                            case SMDAlgorithm.HSVExtraction:
                                {
                                    CvResult cvRet = smd.HSVExtraction.Preview(src.Copy(), dst, smd.ROI.Rectangle);
                                    _hsvExtractionControl.Score = cvRet.Score;
                                    smd.HSVExtraction.Score = cvRet.Score;
                                    string message = $"{smd.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}\r\nQty: {cvRet.Qty}";
                                    MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                    //DataContext = cvRet;
                                    break;
                                }
                            case SMDAlgorithm.LuminanceExtraction:
                                {
                                    CvResult cvRet = smd.LuminanceExtraction.Preview(src.Copy(), dst, smd.ROI.Rectangle);
                                    _luminanceExtractionControl.Score = cvRet.Score;
                                    smd.LuminanceExtraction.Score = cvRet.Score;
                                    string message = $"{smd.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}\r\nQty: {cvRet.Qty}";
                                    MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);

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

                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
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

                    FOV fov = _program.FOVs[fovindex];
                    _program.RemoveFOV(fov);
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
                ShowFOVImage(fov);
                LoadSMD();
                SelectCameraMode(fov);
            }
        }

        private void cmbSMDs_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int fovindex = cmbFOVs.SelectedIndex;
            int smdindex = cmbSMDs.SelectedIndex;

            if (smdindex > -1)
            {
                var smd = _program.FOVs[fovindex].SMDs[smdindex];
                LoadSMDProperties();
                // LoadROI();
                GetRect(Color.Green, smd);
                ShowSMDAlgorithm(smd);

                if (cmbSMDAlgorithm.SelectedIndex == (int)SMDAlgorithm.TemplateMatching)
                {
                    imbTemplateImage.Image = smd.TemplateMatching.Template;
                }
                else
                {
                    imbTemplateImage.Image = null;
                }
            }
            else
            {
                txtROIX.Text = string.Empty;
                txtROIY.Text = string.Empty;
                txtROIH.Text = string.Empty;
                txtROIW.Text = string.Empty;
            }
        }

        public void ShowSMDAlgorithm(SMD item)
        {
            try
            {
                switch (item.Algorithm)
                {
                    case SMDAlgorithm.Unknow:
                        break;
                    case SMDAlgorithm.CodeRecognition:
                        {
                            tbiCodeRecognition.IsSelected = true;
                            //   _codeRecognitionControl.SetParameters(item.CodeRecognition);

                            cmbMode.ItemsSource = Enum.GetValues(typeof(CodeMode)).Cast<CodeMode>();
                            cmbFormat.ItemsSource = Enum.GetValues(typeof(CodeFormat)).Cast<CodeFormat>();
                        }
                        break;

                    case SMDAlgorithm.HSVExtraction:
                        {
                            tbiHSV.IsSelected = true;
                            // _hsvExtractionControl.SetParameters(item.HSVExtraction);

                        }
                        break;

                    case SMDAlgorithm.TemplateMatching:
                        {
                            tbiTemplateMatching.IsSelected = true;
                            //  _templateMatchingControl.SetParameters(item.TemplateMatching);
                        }
                        break;
                    //case SMDAlgorithm.Contour:
                    //    tbiContour.IsSelected = true;
                    //    break;
                    case SMDAlgorithm.LuminanceExtraction:
                        tbiLuminanceExtraction.IsSelected = true;
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
                if (fovindex > -1)
                {
                    cmbSMDs.Items.Clear();
                    foreach (var smd in _program.FOVs[fovindex].SMDs)
                    {
                        cmbSMDs.Items.Add(smd.name);
                    }
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
                    // LoadROI();
                    GetRect(Color.Green, smd);
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
                        // LoadROI();
                        GetRect(Color.Green, _program.FOVs[fovindex].SMDs[smdindex]);
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

                            //Console.WriteLine("X : " + _mouse.LastRectangle.X);
                            //Console.WriteLine("Y : " + _mouse.LastRectangle.Y);
                            //Console.WriteLine("Width : " + _mouse.LastRectangle.Width);
                            //Console.WriteLine("Height : " + _mouse.LastRectangle.Height);

                            // Khởi tạo một đối tượng Image<Bgr, byte> từ một hình ảnh có chứa vùng ROI
                            // var image = new Image<Bgr, byte>(_program.FOVs[fovIndex].ImagePath).Clone();
                            var roi = new Rectangle(X, Y, W, H);
                            // Cắt ảnh từ vùng ROI
                            //var roiImage = image.Copy(roi);
                            // Lưu ảnh cắt được ra file
                            //roiImage.Save(@$"temp\roiSMD{smdId}.png");
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
                            GetRect(Color.Green, smd);
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

        public JRect GetRect(System.Drawing.Color color, SMD smd)
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
                    dst.Draw(_rect, new Bgr(color), 3);
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
                        GetRect(Color.Green, smd);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }
        }

        public void MoveROIUp(int value)
        {
            int fovindex = cmbFOVs.SelectedIndex;
            int smdindex = cmbSMDs.SelectedIndex;
            if (isMoveROI)
            {
                if (smdindex > -1 && fovindex > -1)
                {
                    SMD smd = _program.FOVs[fovindex].SMDs[smdindex];
                    smd.ROI.Y -= value;
                    GetRect(Color.Yellow, smd);
                }
            }
        }

        public void MoveROIDown(int value)
        {
            int fovindex = cmbFOVs.SelectedIndex;
            int smdindex = cmbSMDs.SelectedIndex;
            if (isMoveROI)
            {
                if (smdindex > -1 && fovindex > -1)
                {
                    SMD smd = _program.FOVs[fovindex].SMDs[smdindex];
                    smd.ROI.Y += value;
                    GetRect(Color.Yellow, smd);
                }
            }
        }

        public void MoveROILeft(int value)
        {
            int fovindex = cmbFOVs.SelectedIndex;
            int smdindex = cmbSMDs.SelectedIndex;
            if (isMoveROI)
            {
                if (smdindex > -1 && fovindex > -1)
                {
                    SMD smd = _program.FOVs[fovindex].SMDs[smdindex];
                    smd.ROI.X -= value;
                    GetRect(Color.Yellow, smd);
                }
            }
        }

        public void MoveROIRight(int value)
        {
            int fovindex = cmbFOVs.SelectedIndex;
            int smdindex = cmbSMDs.SelectedIndex;
            if (isMoveROI)
            {
                if (smdindex > -1 && fovindex > -1)
                {
                    SMD smd = _program.FOVs[fovindex].SMDs[smdindex];
                    smd.ROI.X += value;
                    GetRect(Color.Yellow, smd);
                }
            }
        }
        #endregion

        #region MouseEvent

        bool isMoveROI = false;
        private void imbCamera_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var point = _mouse.GetMousePosition(sender, e);
            int fovIndex = cmbFOVs.SelectedIndex;
            int smdIndex = cmbSMDs.SelectedIndex;
            if (!point.IsEmpty)
            {
                _mouse.IsMouseDown = true;
                _mouse.StartPoint = point;
                if (fovIndex > -1 && smdIndex > -1)
                {
                    var smd = _program.FOVs[fovIndex].SMDs.Find(x => x.Id == smdIndex);
                    if (point.X >= smd.ROI.X && point.X <= smd.ROI.X + smd.ROI.Width && point.Y >= smd.ROI.Y && point.Y <= smd.ROI.Y + smd.ROI.Height)
                    {
                        Mouse.OverrideCursor = Cursors.Hand;
                        GetRect(Color.Yellow, smd);
                        isMoveROI = true;
                    }
                    else
                    {
                        Mouse.OverrideCursor = Cursors.Arrow;
                        isMoveROI = false;
                        GetRect(Color.Green, smd);
                    }
                }
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
                int fovIndex = cmbFOVs.SelectedIndex;
                int smdIndex = cmbSMDs.SelectedIndex;
                var point = _mouse.GetMousePosition(sender, e);
                var position = $"X = {point.X} | Y = {point.Y}";
                lblPosition.Content = position;
                if (fovIndex > -1 && smdIndex > -1 && isMoveROI)
                {
                    var smd = _program.FOVs[fovIndex].SMDs.Find(x => x.Id == smdIndex);
                    if (point.X >= smd.ROI.X && point.X <= smd.ROI.X + smd.ROI.Width && point.Y >= smd.ROI.Y && point.Y <= smd.ROI.Y + smd.ROI.Height)
                    {
                        Mouse.OverrideCursor = Cursors.Hand;
                    }
                    else
                    {
                        Mouse.OverrideCursor = Cursors.Arrow;
                    }
                }


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
            if (alowZoom)
            {
                if (imbCamera.Image != null)
                {
                    double newZoom = imbCamera.ZoomScale * (e.Delta > 0 ? 1.2 : 0.8);
                    newZoom = Math.Max(0.5, Math.Min(5, newZoom)); // gioi han ti le zoom
                    imbCamera.SetZoomScale(newZoom, e.Location);
                }
            }
        }

        #endregion

        #region Logs
        public void LogInfo(string message)
        {
            Logger.Current.Info(message);
        }

        public void LogError(string message)
        {
            Logger.Current.Error(message);
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
                    device.Terminal.SerialWriteData(message);
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

        #region Camera

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
                    FOV fov = _program.FOVs[fovindex];
                    // string imagePath = $"data\\images\\image_{fov.ImageBlockName}.bmp";
                    if (_camera == null)
                    {
                        MessageBox.Show("Camera unavailable.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }
                    _camera.SetParameter(KeyName.ExposureTime, fov.ExposureTime);
                    _camera.ClearImageBuffer();
                    _camera.SetParameter(KeyName.TriggerSoftware, 1);
                    _isStreaming = false;
                    using (System.Drawing.Bitmap bmp = _camera.GrabFrame())
                    {
                        if (bmp != null)
                        {
                            _program.SetImageBlock(fov.ImageBlockName, (System.Drawing.Bitmap)bmp.Clone());

                            Bitmap bmpClone = (Bitmap)bmp.Clone();
                            _image = bmpClone.ToImage<Bgr, byte>();
                            imbCamera.Image = _image;

                            //bitmapClone.Save(imagePath);
                            //_program.FOVs[fovindex].ImagePath = imagePath;
                        }
                    }
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

                if (!_loopWorker.IsRunning)
                    _loopWorker.Start();
                if (_camera != null)
                {
                    bool temp = _isStreaming;
                    if (temp)
                    {
                        _isStreaming = !temp;
                        imgStreaming.Source = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Icons/video_call_32px_off.png", UriKind.RelativeOrAbsolute));
                    }
                    else
                    {
                        _isStreaming = !temp;
                        imgStreaming.Source = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Icons/video_call_32px_on.png", UriKind.RelativeOrAbsolute));
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
            }
        }

        #endregion


        public int IsAdmin()
        {
            LoginDialog loginDialog = new LoginDialog();
            loginDialog.ShowDialog();
            if (loginDialog.txtUsername.Text == "admin" && loginDialog.Password.Password == "admin" && !loginDialog.Cancel())
            {
                return 1;
            }
            else if (loginDialog.Cancel())
            {
                loginDialog.Close();
                return 0;
            }
            else
                return -1;
        }

        public void ShowFOVImage(FOV item)
        {
            using (Image<Bgr, byte> image = _program.GetImageBlock(item.ImageBlockName)?.Clone())
            {
                if (image != null)
                {
                    Bitmap bmp = image.ToBitmap();
                    _image = bmp.ToImage<Bgr, byte>();
                    imbCamera.Image = _image;
                }
                else
                {
                    _image = null;
                    imbCamera.Image = null;
                }
            }
        }

        private void btnUpdateImageBlock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                if (fovindex > -1)
                {
                    FOV fov = _program.FOVs[fovindex];
                    // string imagePath = $"data\\images\\image_{fov.ImageBlockName}.bmp";
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
                                    _program.SetImageBlock(fov.ImageBlockName, (System.Drawing.Bitmap)bmp.Clone());
                                    Bitmap bitmapClone = (Bitmap)bmp.Clone();
                                    _image = bitmapClone.ToImage<Bgr, byte>();
                                    imbCamera.Image = _image;
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
                        //SMD item = _program.FOVs[fovindex].SMDs[smdindex];
                        //_rect = item.ROI.Rectangle;
                        //if (_image != null)
                        //{
                        //    var dst = _image.Clone();
                        //    dst.ROI = _rect;
                        //    var image = dst.Copy();
                        //    image.ROI = Rectangle.Empty;

                        //    if (!image.ROI.IsEmpty)
                        //    {
                        //        item.TemplateMatching.Template = image;
                        //        imbTemplateImage.Image = item.TemplateMatching.Template;
                        //    }
                        //}

                        SMD item = _program.FOVs[fovindex].SMDs[smdindex];
                        using (Image<Bgr, byte> image = _program.GetImageBlock(item)?.Clone())
                        {
                            if (image != null)
                            {
                                image.ROI = item.ROI.Rectangle;
                                item.TemplateMatching.Template = image.Copy();
                                image.ROI = new System.Drawing.Rectangle();
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
                    var smd = _program.FOVs[fovindex].SMDs[smdindex];
                    _rect = smd.ROI.Rectangle;

                    if (_image != null)
                    {
                        using (Image<Bgr, byte> image = _program.GetImageBlock(smd)?.Clone())
                        {
                            if (image != null)
                            {
                                image.ROI = smd.ROI.Rectangle;
                                if (!image.ROI.IsEmpty)
                                {
                                    (ValueRange H, ValueRange S, ValueRange V) = smd.HSVExtraction.HSVRange(image.Copy());
                                    smd.HSVExtraction.Hue = H;
                                    smd.HSVExtraction.Saturation = S;
                                    smd.HSVExtraction.Value = V;
                                }
                                image.ROI = new Rectangle();
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

        private void ckbIsEnableFOV_Click(object sender, RoutedEventArgs e)
        {
            if (ckbIsEnableFOV.IsChecked == false)
            {

            }
        }

        private void btnCopyHSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                int smdindex = cmbSMDs.SelectedIndex;
                if (fovindex > -1 && smdindex > -1)
                {
                    var item = _program.FOVs[fovindex].SMDs[smdindex];
                    if (item != null && cmbSMDAlgorithm.SelectedIndex == (int)SMDAlgorithm.HSVExtraction)
                    {
                        HSVExtraction.Hue = item.HSVExtraction.Hue;
                        HSVExtraction.Saturation = item.HSVExtraction.Saturation;
                        HSVExtraction.Value = item.HSVExtraction.Value;
                        HSVExtraction.OKRange = item.HSVExtraction.OKRange;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        private void btnPasteHSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int fovindex = cmbFOVs.SelectedIndex;
                int smdindex = cmbSMDs.SelectedIndex;
                if (fovindex > -1 && smdindex > -1)
                {
                    var item = _program.FOVs[fovindex].SMDs[smdindex];
                    if (item != null && cmbSMDAlgorithm.SelectedIndex == (int)SMDAlgorithm.HSVExtraction)
                    {
                        item.HSVExtraction.Hue = HSVExtraction.Hue;
                        item.HSVExtraction.Saturation = HSVExtraction.Saturation;
                        item.HSVExtraction.Value = HSVExtraction.Value;
                        item.HSVExtraction.OKRange = HSVExtraction.OKRange;
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

