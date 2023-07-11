using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.Editor.Camera;
using Foxconn.Editor.Configuration;
using Foxconn.Editor.Controls;
using Foxconn.Editor.Dialogs;
using Foxconn.Editor.Enums;
using Foxconn.Editor.OpenCV;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        private object _selectedNavigation = null;
        private Worker _loopWorker;
        private ICamera _camera = null;
        private bool _isStreaming = false;

        private FOVProperties _fovProperties = new FOVProperties();
        private SMDProperties _smdProperties = new SMDProperties();
        private Board _selectedBoard = null;
        private FOV _selectedFOV = null;
        private SMD _selectedSMD = null;
        private ObservableCollection<Board> _programs { get; set; } = new ObservableCollection<Board>();

        private CvCodeRecognitionControl _codeRecognitionControl = new CvCodeRecognitionControl();
        private CvTemplateMatchingControl _templateMatchingControl = new CvTemplateMatchingControl();
        private CvHSVExtractionControl _hsvExtractionControl = new CvHSVExtractionControl();
        private CvLuminanceExtractionControl _luminanceExtractionControl = new CvLuminanceExtractionControl();
        private CvLuminanceExtractionQtyControl _luminanceExtractionQtyControl = new CvLuminanceExtractionQtyControl();
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
            DataContext = this;
            Current = this;

            _templateMatchingControl.UpdateTemplateButton.Click += btnUpdateTemplateButton_Click;
            _hsvExtractionControl.GetHSVColorButton.Click += btnGetHSVColorButton_Click;
            _hsvExtractionControl.btnCopyHSV.Click += btnCopyHSV_Click;
            _hsvExtractionControl.btnPasteHSV.Click += btnPasteHSV_Click;
            _smdProperties.txtROIX.MouseWheel += txtROIX_MouseWheel;
            _smdProperties.txtROIY.MouseWheel += txtROIY_MouseWheel;
            _smdProperties.txtROIWidth.MouseWheel += txtROIWidth_MouseWheel;
            _smdProperties.txtROIHeight.MouseWheel += txtROIHeight_MouseWheel;
            _smdProperties.cmbAlgorithm.SelectionChanged += cmbAlgorithm_SelectionChanged;

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
            if (MessageShow.Question("Exit application?", "Exit") == false)
            {
                e.Cancel = true;
            }
        }


        public void StartUp()
        {
            try
            {
                LogInfo("StartUp Application");

                ProgramManager.Current.OpenProgram();
                MachineParams.Reload();

                trvNavigation.ItemsSource = _programs;
                DeviceManager.Current.Open();
                DeviceManager.Current.Ping();

                _loopWorker = new Worker(new ThreadStart(MainProcess));
                _loopWorker.Start();
                RefreshProgram();
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
            else if(Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                imageBox.FullScreen();
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

        private void mnuiSaveAsThisImage_Click(object sender, RoutedEventArgs e)
        {
            if (imageBox.Source != null)
            {
                var dialog = new System.Windows.Forms.SaveFileDialog
                {
                    DefaultExt = ".png",
                    Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|BMP Files (*.bmp)|*.bmp|All Files (*.*)|*.*"
                };
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        var image = (BitmapSource)imageBox.Source.Clone();

                        // ImageFormat imageFormat = ImageFormat.Png;
                        BitmapEncoder encoder = null;
                        string fileExtension = Path.GetExtension(dialog.FileName).ToLower();

                        switch (fileExtension)
                        {
                            case ".png":
                                {
                                    encoder = new PngBitmapEncoder();
                                    break;
                                }
                            case ".bmp":
                                {
                                    encoder = new BmpBitmapEncoder();
                                    break;
                                }
                            case ".jpeg":
                                {
                                    encoder = new JpegBitmapEncoder();
                                    break;
                                }
                        }
                        using (var fileStream = new FileStream(dialog.FileName, FileMode.Create))
                        {
                            encoder.Frames.Add(BitmapFrame.Create(image));
                            encoder.Save(fileStream);

                            // image.Save(fileStream, imageFormat);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
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

        private void mnuiOptions_Click(object sender, RoutedEventArgs e)
        {


        }

        private void mnuiCheckforUpdate_Click(object sender, RoutedEventArgs e)
        {

        }


        private void mnuiAbout_Click(object sender, RoutedEventArgs e)
        {

        }


        private void mnuiEnableFOV_Click(object sender, RoutedEventArgs e)
        {
            if(_selectedNavigation is FOV)
            {
                FOV fov = _selectedNavigation as FOV;
                if(fov != null)
                {
                    fov.IsEnabled = true;
                }
            }
        }

        private void mnuiDisableFOV_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is FOV)
            {
                FOV fov = _selectedNavigation as FOV;
                if (fov != null)
                {
                    fov.IsEnabled = false;
                }
            }
        }

        private void mnuiEnableSMD_Click(object sender, RoutedEventArgs e)
        {
            if(_selectedNavigation is SMD)
            {
                SMD smd = _selectedNavigation as SMD;
                if(smd !=null)
                {
                    smd.IsEnabled = true;
                }
            }
        }

        private void mnuiDisableSMD_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is SMD)
            {
                SMD smd = _selectedNavigation as SMD;
                if (smd != null)
                {
                    smd.IsEnabled = false;
                }
            }
        }


        private void mnuiTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedNavigation is SMD)


                {
                    SMD smd = _selectedNavigation as SMD;

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
                                    MessageShow.Info(message, "Result");
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
                                    string message = $"{smd.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}\r\nCenter: {cvRet.Center}";
                                    MessageShow.Info(message, "Result");
                                    break;
                                }
                            case SMDAlgorithm.HSVExtraction:
                                {
                                    CvResult cvRet = smd.HSVExtraction.Preview(src.Copy(), dst, smd.ROI.Rectangle);
                                    _hsvExtractionControl.Score = cvRet.Score;
                                    string message = $"{smd.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}\r\nQty: {cvRet.Qty}";
                                    MessageShow.Info(message, "Result");
                                    break;
                                }
                            case SMDAlgorithm.LuminanceExtraction:
                                {
                                    CvResult cvRet = smd.LuminanceExtraction.Preview(src.Copy(), dst, smd.ROI.Rectangle);
                                    _luminanceExtractionControl.Score = cvRet.Score;
                                    string message = $"{smd.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}\r\nQty: {cvRet.Qty}";
                                    MessageShow.Info(message, "Result");
                                    break;
                                }
                            default:
                                break;
                        }

                        src.ROI = new System.Drawing.Rectangle();
                        dst.ROI = new System.Drawing.Rectangle();
                        Dispatcher.Invoke(() =>
                        {
                            imageBox.SourceFromBitmap = dst.ToBitmap();
                        });

                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }



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

        private void btnAddFOV_Click(object sender, RoutedEventArgs e)
        {
            Board board = _program;
            if (board != null)
            {
                if (_selectedNavigation is Board)
                {
                    FOV fov = board.AddFOV();
                    if (fov != null)
                    {
                        _selectedNavigation = fov;
                        RefreshNavigation(fov);
                    }
                }
                else
                {
                    MessageShow.Error("Navigation to program", "Error");
                }
            }
        }

        private void btnAddSMD_Click(object sender, RoutedEventArgs e)
        {
            Board board = _program;
            if (board != null)
            {
                if (_selectedNavigation is FOV)
                {
                    FOV itemFOV = _selectedNavigation as FOV;
                    int id = board.FOVs.IndexOf(itemFOV);
                    SMD smd = board.AddSMD(id);
                    if (smd != null)
                    {
                        _selectedNavigation = smd;
                    }
                    RefreshNavigation(_selectedNavigation);
                }
                else
                {
                    MessageShow.Error("Navigate to FOV.", "Error");
                }
            }
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            Board program = _program;
            if (program != null)
            {
                if (_selectedNavigation is Board)
                {
                    Board itemBoard = _selectedNavigation as Board;
                    if (MessageShow.Question($"Are you sure want to remove{itemBoard.Name}?", "Remove Board"))
                        return;
                }
                else if (_selectedNavigation is FOV)
                {
                    FOV itemFOV = _selectedNavigation as FOV;
                    if (MessageShow.Question($"Are you sure want to remove{itemFOV.Name}?", "Remove FOV"))
                        program.RemoveFOV(itemFOV);
                }
                else if (_selectedNavigation is SMD)
                {
                    SMD itemSMD = _selectedNavigation as SMD;
                    if (MessageShow.Question($"Are you sure want to remove{itemSMD.Name}?", "Remove SMD"))
                        program.RemoveSMD(itemSMD);
                }
                RefreshNavigation(_selectedNavigation);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshNavigation(_selectedNavigation);
        }


        private void btnGrabFrame_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is FOV)
            {
                FOV fov = _selectedNavigation as FOV;
                if (_camera == null)
                {
                    MessageShow.Error("Camera is unavailable.", "Error");
                    return;
                }
                _camera.SetParameter(KeyName.ExposureTime, fov.ExposureTime);
                _camera.ClearImageBuffer();
                _camera.SetParameter(KeyName.TriggerSoftware, 1);
                _isStreaming = false;
                using (Bitmap bmp = _camera.GrabFrame())
                {
                    if (bmp != null)
                    {
                        _program.SetImageBlock(fov.ImageBlockName, (Bitmap)bmp.Clone());
                        imageBox.SourceFromBitmap = (Bitmap)bmp.Clone();
                    }
                }
            }
        }

        private void btnStreaming_Click(object sender, RoutedEventArgs e)
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
        }

        private void btnUpdateImageBlock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedNavigation is FOV)
                {
                    FOV fov = _selectedNavigation as FOV;
                    using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
                    {
                        ofd.Filter = "All Picture Files (*.bmp, *.png, *.jpg, *.jpeg)|*.bmp; *.png; *.jpg; *.jpeg|All files (*.*)|*.*";
                        ofd.FilterIndex = 0;
                        ofd.RestoreDirectory = true;
                        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            using (Bitmap bmp = new Bitmap(ofd.FileName))
                            {
                                if (bmp != null)
                                {
                                    _program.SetImageBlock(fov.ImageBlockName, (System.Drawing.Bitmap)bmp.Clone());
                                    imageBox.SourceFromBitmap = (Bitmap)bmp.Clone();
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

        private void btnAddROI_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is SMD)
            {
                SMD smd = _selectedNavigation as SMD;
                imageBox.IsDrawing = true;
            }
        }

        private void btnDeleteROI_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is SMD)
            {
                SMD smd = _selectedNavigation as SMD;

                smd.ROI.X = 0;
                smd.ROI.Y = 0;
                smd.ROI.Height = 0;
                smd.ROI.Width = 0;
                ShowSMDRect(smd);
                imageBox.IsDrawing = false;
            }
        }

        private void btnSaveROI_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is SMD)
            {
                Rect rect = imageBox.ROI;
                SMD smd = _selectedNavigation as SMD;
                smd.ROI.X = (int)rect.X;
                smd.ROI.Y = (int)rect.Y;
                smd.ROI.Height = (int)rect.Height;
                smd.ROI.Width = (int)rect.Width;

                imageBox.IsDrawing = false;
                MessageShow.Info("Save SMD ROI success!", "Save Rect");
            }
        }

        public void MoveROIUp()
        {
            if (_selectedNavigation is SMD)
            {
                SMD smd = _selectedNavigation as SMD;
                smd.ROI.Y -= 1;
                ShowSMDRect(smd);
            }
        }

        public void MoveROIDown()
        {
            if (_selectedNavigation is SMD)
            {
                SMD smd = _selectedNavigation as SMD;
                smd.ROI.Y += 1;
                ShowSMDRect(smd);
            }
        }

        public void MoveROILeft()
        {
            if (_selectedNavigation is SMD)
            {
                SMD smd = _selectedNavigation as SMD;
                smd.ROI.X -= 1;
                ShowSMDRect(smd);
            }
        }

        public void MoveROIRight()
        {
            if (_selectedNavigation is SMD)
            {
                SMD smd = _selectedNavigation as SMD;
                smd.ROI.X += 1;
                ShowSMDRect(smd);
            }
        }

        private void imageBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

            double zoomScale = imageBox.ZoomScale;
            if (e.Delta > 0)
            {
                zoomScale *= 1.1;
                if (zoomScale < imageBox.MaxScale)
                {
                    imageBox.ZoomScale = zoomScale;
                }
            }
            else
            {
                zoomScale *= 0.9;
                if (zoomScale > imageBox.MinScale)
                {
                    imageBox.ZoomScale = zoomScale;
                }
            }
        }

        private void imageBox_MouseMove(object sender, MouseEventArgs e)
        {
            string position = $"X={imageBox.CurrentPoint.X} | Y={imageBox.CurrentPoint.Y}";
            lblPosition.Content = position;

            if (_selectedNavigation is SMD)
            {
                SMD smd = _selectedNavigation as SMD;
                smd.ROI.X = (int)imageBox.ROI.X;
                smd.ROI.Y = (int)imageBox.ROI.Y;
                smd.ROI.Width = (int)imageBox.ROI.Width;
                smd.ROI.Height = (int)imageBox.ROI.Height;
                NotifyPropertyChanged();
            }
        }

        private void cmbAlgorithm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedNavigation is SMD)
            {
                SMD smd = _selectedNavigation as SMD;

                ShowSMDAlgorithm(smd);
            }
        }

        private void txtROIHeight_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_selectedNavigation is SMD)
            {
                SMD smd = _selectedNavigation as SMD;
                if (e.Delta > 0)
                {
                    smd.ROI.Height += 1;
                }
                else
                {
                    smd.ROI.Height -= 1;
                }
                ShowSMDRect(smd);
            }
        }

        private void txtROIWidth_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_selectedNavigation is SMD)
            {
                SMD smd = _selectedNavigation as SMD;
                if (e.Delta > 0)
                {
                    smd.ROI.Width += 1;
                }
                else
                {
                    smd.ROI.Width -= 1;
                }
                ShowSMDRect(smd);
            }
        }

        private void txtROIY_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                MoveROIDown();
            }
            else
            {
                MoveROIUp();
            }
        }

        private void txtROIX_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                MoveROIRight();
            }
            else
            {
                MoveROILeft();
            }
        }

        private void trvNavigation_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            dpnAlgorithm.Children.Clear();
            _selectedNavigation = e.NewValue;
            if (_selectedNavigation is Board)
            {
                Board item = _selectedNavigation as Board;
            }
            else if (_selectedNavigation is FOV)
            {
                FOV item = _selectedNavigation as FOV;
                ShowFOVProperties(item);
                ShowFOVImage(item);
                SelectCameraMode(item);
            }
            else if (_selectedNavigation is SMD)
            {
                SMD item = _selectedNavigation as SMD;
                ShowSMDProperties(item);
                ShowFOVImage(item);
                ShowSMDRect(item);
                ShowSMDAlgorithm(item);
            }
            else
            {
                Console.WriteLine("Navigation is not selected");
            }
            NotifyPropertyChanged();
        }

        private void trvNavigation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        public void RefreshProgram()
        {
            _programs.Clear();
            _programs.Add(_program);
            trvNavigation.Items.Refresh();
            NotifyPropertyChanged();
        }

        public void ShowFOVProperties(FOV fov)
        {
            dpnProperties.Children.Clear();
            _fovProperties.SetParameters(fov);
            dpnProperties.Children.Add(_fovProperties);
        }

        private void ShowFOVImage(FOV item)
        {
            using (Image<Bgr, byte> image = _program.GetImageBlock(item.ImageBlockName)?.Clone())
            {
                if (image != null)
                {
                    imageBox.SourceFromBitmap = image.ToBitmap();
                    imageBox.ClearSelectedRect();
                }
                else
                {
                    imageBox.ClearSource();
                }
            }
        }

        private void ShowFOVImage(SMD item)
        {
            using (Image<Bgr, byte> image = _program.GetImageBlock(item)?.Clone())
            {
                if (image != null)
                {
                    imageBox.SourceFromBitmap = image.ToBitmap();
                    imageBox.ClearSelectedRect();
                }
                else
                {
                    imageBox.ClearSource();
                }
            }
        }

        public void SelectCameraMode(FOV fov)
        {
            DeviceManager device = DeviceManager.Current;
            if (fov.CameraMode == CameraMode.Top)
            {
                _camera = device.Camera1;
            }
            else if (fov.CameraMode == CameraMode.Bottom)
            {
                _camera = device.Camera2;
            }
            else
            {
                _camera = null;
            }
        }

        public void ShowSMDProperties(SMD smd)
        {
            dpnProperties.Children.Clear();
            _smdProperties.SetParameters(smd);
            dpnProperties.Children.Add(_smdProperties);
        }

        private void ShowSMDRect(SMD item)
        {
            imageBox.DrawRectangle(item.ROI.Rectangle);
        }


        public void ShowSMDAlgorithm(SMD smd)
        {
            try
            {

                switch (smd.Algorithm)
                {
                    case SMDAlgorithm.Unknow:
                        break;
                    case SMDAlgorithm.CodeRecognition:
                        {
                            dpnAlgorithm.Children.Clear();
                            _codeRecognitionControl.SetParameters(smd.CodeRecognition);
                            dpnAlgorithm.Children.Add(_codeRecognitionControl);
                            break;
                        }
                    case SMDAlgorithm.TemplateMatching:
                        {
                            dpnAlgorithm.Children.Clear();
                            _templateMatchingControl.SetParameters(smd.TemplateMatching);
                            dpnAlgorithm.Children.Add(_templateMatchingControl);
                            break;
                        }
                    case SMDAlgorithm.HSVExtraction:
                        {
                            dpnAlgorithm.Children.Clear();
                            _hsvExtractionControl.SetParameters(smd.HSVExtraction);
                            dpnAlgorithm.Children.Add(_hsvExtractionControl);
                            break;
                        }
                    case SMDAlgorithm.LuminanceExtraction:
                        {
                            dpnAlgorithm.Children.Clear();
                            _luminanceExtractionControl.SetParameters(smd.LuminanceExtraction);
                            dpnAlgorithm.Children.Add(_luminanceExtractionControl);
                            break;
                        }
                    case SMDAlgorithm.LuminanceExtractionQty:
                        {
                            dpnAlgorithm.Children.Clear();
                            _luminanceExtractionQtyControl.SetParameters(smd.LuminanceExtractionQty);
                            dpnAlgorithm.Children.Add(_luminanceExtractionQtyControl);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void RefreshNavigation(object obj)
        {
            trvNavigation.Items.Refresh();
            Board program = _program;
            if (program != null)
            {
                if (obj is Board || obj is FOV || obj is SMD)
                {
                    Board selectedBoard = null;
                    FOV selectedFOV = null;
                    SMD selectedSMD = null;
                    if (obj is Board)
                    {
                        selectedBoard = obj as Board;
                        selectedBoard.SortByName();
                    }
                    else if (obj is FOV)
                    {
                        selectedFOV = obj as FOV;
                        //selectedFOV.SortByName();
                    }
                    else if (obj is SMD)
                    {
                        selectedSMD = obj as SMD;
                        //selectedFOV.SortByName();
                        selectedFOV = program.GetFOV(selectedSMD);
                    }
                    TreeViewItem trvProgram = trvNavigation.ItemContainerGenerator.ContainerFromItem(program) as TreeViewItem;
                    if (trvProgram != null)
                    {
                        trvProgram.IsSelected = true;
                        trvProgram.IsExpanded = true;
                        trvProgram.UpdateLayout();
                        if (selectedFOV != null)
                        {
                            TreeViewItem trvFOV = trvProgram.ItemContainerGenerator.ContainerFromItem(selectedFOV) as TreeViewItem;
                            if (_selectedFOV != null)
                            {
                                TreeViewItem trvFOVCmp = trvProgram.ItemContainerGenerator.ContainerFromItem(_selectedFOV) as TreeViewItem;
                                if (trvFOVCmp != null)
                                {
                                    trvFOVCmp.BorderThickness = new Thickness(1);
                                    trvFOVCmp.BorderBrush = System.Windows.Media.Brushes.Yellow;
                                }
                            }
                            if (trvFOV != null)
                            {
                                trvFOV.IsSelected = true;
                                trvFOV.IsExpanded = true;
                                trvFOV.UpdateLayout();
                                if (selectedSMD != null)
                                {
                                    TreeViewItem trvSMD = trvFOV.ItemContainerGenerator.ContainerFromItem(selectedSMD) as TreeViewItem;
                                    if (trvSMD != null)
                                    {
                                        trvSMD.IsSelected = true;
                                    }
                                }
                            }
                        }
                    }
                }
                NotifyPropertyChanged();
            }
        }


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
                        using (Bitmap bmp = _camera.GrabFrame())
                        {
                            if (bmp != null)
                            {
                                Dispatcher.Invoke(() => imageBox.SourceFromBitmap = (Bitmap)bmp.Clone());
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

        private void btnUpdateTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is SMD)
            {
                SMD item = _selectedNavigation as SMD;
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

        private void btnGetHSVColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is SMD)
            {
                SMD item = _selectedNavigation as SMD;
                using (Image<Bgr, byte> image = _program.GetImageBlock(item)?.Clone())
                {
                    if (image != null)
                    {
                        image.ROI = item.ROI.Rectangle;
                        if (!image.ROI.IsEmpty)
                        {
                            (ValueRange H, ValueRange S, ValueRange V) = item.HSVExtraction.HSVRange(image.Copy());
                            item.HSVExtraction.Hue = H;
                            item.HSVExtraction.Saturation = S;
                            item.HSVExtraction.Value = V;
                        }
                        image.ROI = new System.Drawing.Rectangle();
                    }
                }
            }
        }

        private void btnCopyHSV_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is SMD)
            {
                try
                {
                    SMD smd = _selectedNavigation as SMD;
                    HSVExtraction.Hue = smd.HSVExtraction.Hue;
                    HSVExtraction.Saturation = smd.HSVExtraction.Saturation;
                    HSVExtraction.Value = smd.HSVExtraction.Value;
                    HSVExtraction.OKRange = smd.HSVExtraction.OKRange;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void btnPasteHSV_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is SMD)
            {
                try
                {
                    SMD smd = _selectedNavigation as SMD;
                    smd.HSVExtraction.Hue = HSVExtraction.Hue;
                    smd.HSVExtraction.Saturation = HSVExtraction.Saturation;
                    smd.HSVExtraction.Value = HSVExtraction.Value;
                    smd.HSVExtraction.OKRange = HSVExtraction.OKRange;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

    }

}

