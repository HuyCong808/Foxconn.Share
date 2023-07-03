using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.Editor.Camera;
using Foxconn.Editor.Configuration;
using Foxconn.Editor.Controls;
using Foxconn.Editor.Dialogs;
using Foxconn.Editor.Enums;
using Foxconn.Editor.OpenCV;
using Foxconn.Threading;
using Foxconn.Threading.Tasks;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static Foxconn.Editor.Configuration.PLC;

namespace Foxconn.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static MainWindow Current;
        private Worker _loopWorker;
        private Board _program
        {
            get => ProgramManager.Current.Program;
            set => ProgramManager.Current.Program = value;
        }
        private ObservableCollection<Board> _programs { get; set; } = new ObservableCollection<Board>();
        private object _object = null;
        private Board _isBoard = null;
        private FOV _isFOV = null;
        private SMD _isSMD = null;
        private BoardControl _boardControl = new BoardControl();
        private FOVControl _FOVControl = new FOVControl();
        private SMDControl _SMDControl = new SMDControl();
        private CvCodeRecognitionControl _codeRecognitionControl = new CvCodeRecognitionControl();
        private CvContoursControl _contoursControl = new CvContoursControl();
        private CvDeepLearningControl _deepLearningControl = new CvDeepLearningControl();
        private CvFeatureMatchingControl _featureMatchingControl = new CvFeatureMatchingControl();
        private CvHoughCircleControl _houghCircleControl = new CvHoughCircleControl();
        private CvHoughLineControl _houghLineControl = new CvHoughLineControl();
        private CvHSVExtractionControl _HSVExtractionControl = new CvHSVExtractionControl();
        private CvHSVExtraction _HSVExtraction = new CvHSVExtraction();
        private CvLuminanceExtractionControl _luminanceExtractionControl = new CvLuminanceExtractionControl();
        private CvLuminanceExtractionQtyControl _luminanceExtractionQtyControl = new CvLuminanceExtractionQtyControl();
        private CvMarkTracingControl _markTracingControl = new CvMarkTracingControl();
        private CvTemplateMatchingControl _templateMatchingControl = new CvTemplateMatchingControl();
        private PLCControl _PLCControl = new PLCControl();
        private ServerControl _setupServerControl = new ServerControl();
        private ICamera _camera = null;
        private bool _isStreaming = false;

        #region Binding Property
        public bool IsSettings => _loopWorker.IsRunning;

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
            DataContext = this;
            Current = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartupApp();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("Exit application?", "Exit", MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) != MessageBoxResult.OK)
                e.Cancel = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ExitApp();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            KeyboardShutcut(e);
        }

        private void StartupApp()
        {
            try
            {
                WaitingDialog.DoWork("Startup app...", () =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Icons/FoxconnTitle.png", UriKind.RelativeOrAbsolute));
                        Title = Assembly.AssemblyProduct;
                        WindowState = WindowState.Maximized;
                        trvNavigation.ItemsSource = _programs;
                    });
                    _loopWorker = new Worker(new ThreadStart(MainProcess));
                    _loopWorker.Start();
                    DeviceManager.Current.OpenDevices();
                });

                #region Events
                _boardControl.PLCButton.Click += PLCButton_Click;
                _boardControl.ServerButton.Click += ServerButton_Click;
                //_FOVControl.PLCMovePositionButton.Click += PLCMovePositionButton_Click;
                //_FOVControl.PLCSavePositionButton.Click += PLCSavePositionButton_Click;
                _SMDControl.XTextBox.PreviewMouseWheel += XTextBox_PreviewMouseWheel;
                _SMDControl.YTextBox.PreviewMouseWheel += YTextBox_PreviewMouseWheel;
                _SMDControl.WTextBox.PreviewMouseWheel += WTextBox_PreviewMouseWheel;
                _SMDControl.HTextBox.PreviewMouseWheel += HTextBox_PreviewMouseWheel;
                _PLCControl.BackupButton.Click += PLCBackupButton_Click;
                _PLCControl.RestoreButton.Click += PLCRestoreButton_Click;
                _templateMatchingControl.UpdateTemplateButton.Click += UpdateTemplateButton_Click;
                _HSVExtractionControl.GetHSVColorButton.Click += GetHSVColorButton_Click;
                _HSVExtractionControl.CopyHSVButton.Click += CopyHSVButton_Click;
                _HSVExtractionControl.PasteHSVButton.Click += PasteHSVButton_Click;
                #endregion
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
            finally
            {
                NotifyPropertyChanged();
            }
        }

        private void ExitApp()
        {
            try
            {
                Trace.WriteLine("Foxconn =====> Exit app");
                DeviceManager.Current.CloseDevices();
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void KeyboardShutcut(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.F4)
            {
                ExitApp();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
            {
                mnuiNew_Click(null, null);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
            {
                mnuiOpen_Click(null, null);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                mnuiSave_Click(null, null);
            }
            else if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.S)
            {
                //mnuiSaveAll_Click(null, null);
            }
            else if (e.Key == Key.F5)
            {
                mnuiDebug_Click(null, null);
            }
        }

        private void mnuiNew_Click(object sender, RoutedEventArgs e)
        {
            ProgramManager.Current.NewProgram();
            RefreshProgram();
        }

        private void mnuiOpen_Click(object sender, RoutedEventArgs e)
        {
            ProgramManager.Current.OpenProgram();
            RefreshProgram();
        }

        private void mnuiSave_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationManager.Current.IsAdmin())
            {
                //ProgramManager.Current.SaveProgram();
                ProgramManager.Current.SaveProgramAndImage();
                //ProgramManager.Current.SaveAsProgram();
                //ProgramManager.Current.SaveAsProgramAndImage();
            }
        }

        private void mnuiSaveAs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiSaveAsParams_Click(object sender, RoutedEventArgs e)
        {
            MachineParams.Current.Save();
            Customization.Current.Save();
            TestParams.Current.Save();
        }

        private void mnuiSaveAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiImport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiExport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Exit application.", "Exit", MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                ExitApp();
        }

        private void mnuiUndo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiRedo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiCut_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiCopy_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiPaste_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiDuplicate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiSelectAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiZoomIn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiZoomOut_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiZoomToFit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiZoom100_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiFullScreen_Click(object sender, RoutedEventArgs e)
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

        private void mnuiAutoRun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AutoRunManagerDialog dialog = new AutoRunManagerDialog();
                if (dialog.Run())
                {
                    dialog.Show();
                    _loopWorker.Stop();
                }
                else
                {
                    string message = "Cannot run!";
                    Trace.WriteLine(message);
                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                MainWindow.Current.Hide();
            }
        }

        #region Cut images
        private void CutImages(string srcPath, string dstPath)
        {
            FOV pFOV = _program.FOVs[0];
            string[] imageFiles = Directory.GetFiles(srcPath, "*.jpeg"); // or any other image format
            foreach (string file in imageFiles)
            {
                Image<Bgr, byte> image = new Image<Bgr, byte>(file);
                using (Image<Bgr, byte> src = image.Clone())
                using (Image<Bgr, byte> dst = image.Clone())
                {
                    foreach (var pSMD in pFOV.SMDs)
                    {
                        System.Windows.Point offset = GetOffsetROI(pFOV, src);
                        Rectangle offsetROI = new Rectangle
                        {
                            X = pSMD.ROI.Rectangle.X + (int)offset.X,
                            Y = pSMD.ROI.Rectangle.Y + (int)offset.Y,
                            Width = pSMD.ROI.Rectangle.Width,
                            Height = pSMD.ROI.Rectangle.Height
                        };
                        src.ROI = offsetROI;
                        dst.ROI = offsetROI;
                        if (pSMD.Type == SMDType.SMD)
                        {
                            CvImage.Save(dstPath, "", src.Copy().Mat, quality: 50);
                        }
                        src.ROI = new Rectangle();
                        dst.ROI = new Rectangle();
                    }
                }
            }
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
                }
            }
            return offset;
        }
        #endregion

        #region Deep Learning
        //_ = CvDeepLearning.PredictDebug("http://127.0.0.1:5000//predict-binary/test");
        //string srcPath = @$"D:\Documents\Images\SUPER-CAP\pass\L11\";
        //string dstPath = @$"{AppDomain.CurrentDomain.BaseDirectory}data\L11_PASS\";
        //CutImages(srcPath, dstPath);
        #endregion

        private void mnuiDebug_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_object is SMD)
                {
                    SMD item = _object as SMD;
                    using (Image<Bgr, byte> src = _program.GetImageBlock(item)?.Clone())
                    using (Image<Bgr, byte> dst = _program.GetImageBlock(item)?.Clone())
                    {
                        src.ROI = item.ROI.Rectangle;
                        dst.ROI = item.ROI.Rectangle;
                        switch (item.Algorithm)
                        {
                            case SMDAlgorithm.Unknow:
                                break;
                            case SMDAlgorithm.Contour_Text:
                                {
                                    CvResult cvRet = item.Contours.RunText(src.Copy(), dst, item.ROI.Rectangle);
                                    if (cvRet.Result)
                                    {
                                        item.Contours.P0 = new JPoint { X = cvRet.Points[0].X, Y = cvRet.Points[0].Y };
                                        item.Contours.P1 = new JPoint { X = cvRet.Points[1].X, Y = cvRet.Points[1].Y };
                                        item.Contours.P2 = new JPoint { X = cvRet.Points[2].X, Y = cvRet.Points[2].Y };
                                        item.Contours.P3 = new JPoint { X = cvRet.Points[3].X, Y = cvRet.Points[3].Y };
                                    }
                                    string message = $"{item.Algorithm}: {cvRet.Result}\r\nP0: {cvRet.Points[0]}\r\nP1: {cvRet.Points[1]}\r\nP2: {cvRet.Points[2]}\r\nP3: {cvRet.Points[3]}\r\nCenter: {cvRet.Center}";
                                    MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                    break;
                                }
                            case SMDAlgorithm.Contour_Box:
                                {
                                    CvResult cvRet = item.Contours.RunBox(src.Copy(), dst, item.ROI.Rectangle);
                                    if (cvRet.Result)
                                    {
                                        item.Contours.P0 = new JPoint { X = cvRet.Points[0].X, Y = cvRet.Points[0].Y };
                                        item.Contours.P1 = new JPoint { X = cvRet.Points[1].X, Y = cvRet.Points[1].Y };
                                        item.Contours.P2 = new JPoint { X = cvRet.Points[2].X, Y = cvRet.Points[2].Y };
                                        item.Contours.P3 = new JPoint { X = cvRet.Points[3].X, Y = cvRet.Points[3].Y };
                                    }
                                    string message = $"{item.Algorithm}: {cvRet.Result}\r\nP0: {cvRet.Points[0]}\r\nP1: {cvRet.Points[1]}\r\nP2: {cvRet.Points[2]}\r\nP3: {cvRet.Points[3]}\r\nCenter: {cvRet.Center}";
                                    MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                    break;
                                }
                            case SMDAlgorithm.Contour_TextBox:
                                {
                                    CvResult cvRet = item.Contours.RunTextBox(src.Copy(), dst, item.ROI.Rectangle);
                                    if (cvRet.Result)
                                    {
                                        item.Contours.P0 = new JPoint { X = cvRet.Points[0].X, Y = cvRet.Points[0].Y };
                                        item.Contours.P1 = new JPoint { X = cvRet.Points[1].X, Y = cvRet.Points[1].Y };
                                        item.Contours.P2 = new JPoint { X = cvRet.Points[2].X, Y = cvRet.Points[2].Y };
                                        item.Contours.P3 = new JPoint { X = cvRet.Points[3].X, Y = cvRet.Points[3].Y };
                                    }
                                    string message = $"{item.Algorithm}: {cvRet.Result}\r\nP0: {cvRet.Points[0]}\r\nP1: {cvRet.Points[1]}\r\nP2: {cvRet.Points[2]}\r\nP3: {cvRet.Points[3]}\r\nCenter: {cvRet.Center}";
                                    MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                    break;
                                }
                            case SMDAlgorithm.MarkTracing:
                                {
                                    CvResult cvRet = item.MarkTracing.Run(src.Copy(), dst, item.ROI.Rectangle);
                                    if (cvRet.Result)
                                    {
                                        item.MarkTracing.Center = new JPoint { X = cvRet.Center.X, Y = cvRet.Center.Y };
                                    }
                                    _markTracingControl.Score = cvRet.Score;
                                    string message = $"{item.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}\r\nRadius: {cvRet.Radius}\r\nCenter: {cvRet.Center}";
                                    MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                    break;
                                }
                            case SMDAlgorithm.FeatureMatching:
                                {
                                    CvResult cvRet = item.FeatureMatching.Run(src.Copy(), dst, item.ROI.Rectangle);
                                    if (cvRet.Result)
                                    {
                                        item.FeatureMatching.Center = new JPoint { X = cvRet.Center.X, Y = cvRet.Center.Y };
                                    }
                                    string message = $"{item.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}\r\nCenter: {cvRet.Center}";
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
                                    break;
                                }
                            case SMDAlgorithm.CodeRecognition:
                                {
                                    CvResult cvRet = item.CodeRecognition.Run(src.Copy(), dst, item.ROI.Rectangle);
                                    string message = $"{item.Algorithm}: {cvRet.Result}\r\nSN: {cvRet.Content}\r\nLength: {cvRet.Content.Length}";
                                    MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                    break;
                                }
                            case SMDAlgorithm.HSVExtraction:
                                {
                                    CvResult cvRet = item.HSVExtraction.Preview(src.Copy(), dst, item.ROI.Rectangle);
                                    _HSVExtractionControl.Score = cvRet.Score;
                                    string message = $"{item.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}\r\nQty: {cvRet.Qty}";
                                    MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                    break;
                                }
                            case SMDAlgorithm.LuminanceExtraction:
                                {
                                    CvResult cvRet = item.LuminanceExtraction.Preview(src.Copy(), dst, item.ROI.Rectangle);
                                    _luminanceExtractionControl.Score = cvRet.Score;
                                    string message = $"{item.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}\r\nQty: {cvRet.Qty}";
                                    MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                    break;
                                }
                            case SMDAlgorithm.LuminanceExtractionQty:
                                {
                                    CvResult cvRet = item.LuminanceExtractionQty.Preview(src.Copy(), dst, item.ROI.Rectangle);
                                    _luminanceExtractionQtyControl.Qty = cvRet.Qty;
                                    string message = $"{item.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}\r\nQty: {cvRet.Qty}";
                                    MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                    break;
                                }
                            case SMDAlgorithm.DeepLearning:
                                {
                                    CvResult cvRet = item.DeepLearning.Run(src.Copy(), dst, item.ROI.Rectangle);
                                    string message = $"{item.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}";
                                    MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                    break;
                                }
                            default:
                                break;
                        }
                        src.ROI = new Rectangle();
                        dst.ROI = new Rectangle();
                        Dispatcher.Invoke(() =>
                        {
                            imageBox.SourceFromBitmap = dst.ToBitmap();
                            imageBox.HiddenRectangle();
                        });
                    }
                }
                NotifyPropertyChanged();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void mnuiDarkTheme_Click(object sender, RoutedEventArgs e)
        {
            //ThemeManager.Current.Set("Dark");
        }

        private void mnuiLightTheme_Click(object sender, RoutedEventArgs e)
        {
            //ThemeManager.Current.Set("Light");
        }

        private void mnuiCustomize_Click(object sender, RoutedEventArgs e)
        {
            CustomizeDialog dialog = new CustomizeDialog();
            dialog.ShowDialog();
        }

        private void mnuiOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsDialog dialog = new OptionsDialog();
            dialog.ShowDialog();
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

        private void mnuiEnglishLanguage_Click(object sender, RoutedEventArgs e)
        {
            //LanguageManager.Current.Set("English");
        }

        private void mnuiVietnameseLanguage_Click(object sender, RoutedEventArgs e)
        {
            //LanguageManager.Current.Set("Vietnamese");
        }

        private void mnuiCheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            SoftwareUpdate.Current.CheckForUpdates();
        }

        private void mnuiTechnicalSupport_Click(object sender, RoutedEventArgs e)
        {
            string message = "Engineer: Nguyen Quang Tiep\r\nMobile: (+84)90-29-65789";
            MessageBox.Show(message, "Technical Support", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }

        private void mnuiAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog dialog = new AboutDialog();
            dialog.ShowDialog();
        }

        private void trvNavigation_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                dpnProperties2.Children.Clear();
                _object = e.NewValue;
                if (_object is Board)
                {
                    Board item = _object as Board;
                    ShowBoardControl(item);
                }
                else if (_object is FOV)
                {
                    FOV item = _object as FOV;
                    ShowFOVControl(item);
                    ShowFOVImage(item);
                    SelectCameraMode(item);
                }
                else if (_object is SMD)
                {
                    SMD item = _object as SMD;
                    ShowSMDControl(item);
                    ShowFOVImage(item);
                    ShowSMDRectangle(item);
                    ShowSMDAlgorithm(item);
                }
                else
                {
                    Trace.WriteLine("Navigation is not selected");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
            finally
            {
                NotifyPropertyChanged();
            }
        }

        private void trvNavigation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (_object is Board)
                    {
                        _isBoard = _object as Board;
                    }
                    else if (_object is FOV)
                    {
                        _isFOV = _object as FOV;
                    }
                    else if (_object is SMD)
                    {
                        _isSMD = _object as SMD;
                    }
                    RefreshNavigation(_object);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
            finally
            {
                NotifyPropertyChanged();
            }
        }

        private void mnuiEnableFOV_Click(object sender, RoutedEventArgs e)
        {
            if (_object is FOV)
            {
                FOV item = _object as FOV;
                item.IsEnabled = true;
            }
        }

        private void mnuiDisableFOV_Click(object sender, RoutedEventArgs e)
        {
            if (_object is FOV)
            {
                FOV item = _object as FOV;
                item.IsEnabled = false;
            }
        }

        private void mnuiEnableSMD_Click(object sender, RoutedEventArgs e)
        {
            if (_object is SMD)
            {
                SMD item = _object as SMD;
                item.IsEnabled = true;
            }
        }

        private void mnuiDisableSMD_Click(object sender, RoutedEventArgs e)
        {
            if (_object is SMD)
            {
                SMD item = _object as SMD;
                item.IsEnabled = false;
            }
        }

        private void btnAddFOV_Click(object sender, RoutedEventArgs e)
        {
            Board program = _program;
            if (program != null)
            {
                if (_object is Board)
                {
                    FOV itemFOV = program.AddFOV();
                    if (itemFOV != null)
                    {
                        _object = itemFOV;
                        RefreshNavigation(_object);
                    }
                }
                else
                {
                    MessageBox.Show("Navigate to program.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
        }

        private void btnAddSMD_Click(object sender, RoutedEventArgs e)
        {
            Board program = _program;
            if (program != null)
            {
                if (_object is FOV)
                {
                    FOV itemFOV = _object as FOV;
                    int id = program.FOVs.IndexOf(itemFOV);
                    SMD itemSMD = program.AddSMD(id);
                    if (itemSMD != null)
                    {
                        _object = itemSMD;
                    }
                    RefreshNavigation(_object);
                }
                else
                {
                    MessageBox.Show("Navigate to FOV.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            Board program = _program;
            if (program != null)
            {
                if (_object is Board)
                {
                    Board item = _object as Board;
                    if (MessageBox.Show($"Are you sure want to remove {item.Name}?", "Remove Board", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                        return;
                }
                else if (_object is FOV)
                {
                    FOV item = _object as FOV;
                    if (MessageBox.Show($"Are you sure want to remove {item.Name}?", "Remove FOV", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                        program.RemoveFOV(item);
                }
                else if (_object is SMD)
                {
                    SMD item = _object as SMD;
                    if (MessageBox.Show($"Are you sure want to remove {item.Name}?", "Remove SMD", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                        program.RemoveSMD(item);
                }
                RefreshNavigation(_object);
            }
        }

        private void btnRefreshNavigation_Click(object sender, RoutedEventArgs e)
        {
            RefreshNavigation(_object);
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
                            if (_isFOV != null)
                            {
                                TreeViewItem trvFOVCmp = trvProgram.ItemContainerGenerator.ContainerFromItem(_isFOV) as TreeViewItem;
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

        private void RefreshProgram()
        {
            _programs.Clear();
            _programs.Add(_program);
            trvNavigation.Items.Refresh();
            NotifyPropertyChanged();
        }

        private void ShowBoardControl(Board item)
        {
            dpnProperties1.Children.Clear();
            _boardControl.SetParameters(item);
            dpnProperties1.Children.Add(_boardControl);
        }

        private void ShowFOVControl(FOV item)
        {
            dpnProperties1.Children.Clear();
            _FOVControl.SetParameters(item);
            dpnProperties1.Children.Add(_FOVControl);
        }

        private void ShowFOVImage(FOV item)
        {
            using (Image<Bgr, byte> image = _program.GetImageBlock(item.ImageBlockName)?.Clone())
            {
                if (image != null)
                {
                    imageBox.SourceFromBitmap = image.ToBitmap();
                    imageBox.HiddenRectangle();
                }
                else
                {
                    imageBox.Source = null;
                }
            }
        }

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

        private void ShowSMDControl(SMD item)
        {
            dpnProperties1.Children.Clear();
            _SMDControl.SetParameters(item);
            dpnProperties1.Children.Add(_SMDControl);
        }

        private void ShowFOVImage(SMD item)
        {
            using (Image<Bgr, byte> image = _program.GetImageBlock(item)?.Clone())
            {
                if (image != null)
                {
                    imageBox.SourceFromBitmap = image.ToBitmap();
                    imageBox.HiddenRectangle();
                }
                else
                {
                    imageBox.Source = null;
                }
            }
        }

        private void ShowSMDRectangle(SMD item)
        {
            imageBox.ClearRectangle();
            imageBox.SetROI(item.ROI.X, item.ROI.Y, item.ROI.Width, item.ROI.Height);
            imageBox.DrawRectangle();
        }

        private void ShowSMDAlgorithm(SMD item)
        {
            dpnProperties2.Children.Clear();
            switch (item.Algorithm)
            {
                case SMDAlgorithm.Unknow:
                    break;
                case SMDAlgorithm.Contour_Text:
                case SMDAlgorithm.Contour_Box:
                case SMDAlgorithm.Contour_TextBox:
                    _contoursControl.SetParameters(item.Contours);
                    dpnProperties2.Children.Add(_contoursControl);
                    break;
                case SMDAlgorithm.MarkTracing:
                    _markTracingControl.SetParameters(item.MarkTracing);
                    dpnProperties2.Children.Add(_markTracingControl);
                    break;
                case SMDAlgorithm.FeatureMatching:
                    _featureMatchingControl.SetParameters(item.FeatureMatching);
                    dpnProperties2.Children.Add(_featureMatchingControl);
                    break;
                case SMDAlgorithm.TemplateMatching:
                    _templateMatchingControl.SetParameters(item.TemplateMatching);
                    dpnProperties2.Children.Add(_templateMatchingControl);
                    break;
                case SMDAlgorithm.CodeRecognition:
                    _codeRecognitionControl.SetParameters(item.CodeRecognition);
                    dpnProperties2.Children.Add(_codeRecognitionControl);
                    break;
                case SMDAlgorithm.HSVExtraction:
                    _HSVExtractionControl.SetParameters(item.HSVExtraction);
                    dpnProperties2.Children.Add(_HSVExtractionControl);
                    break;
                case SMDAlgorithm.LuminanceExtraction:
                    _luminanceExtractionControl.SetParameters(item.LuminanceExtraction);
                    dpnProperties2.Children.Add(_luminanceExtractionControl);
                    break;
                case SMDAlgorithm.LuminanceExtractionQty:
                    _luminanceExtractionQtyControl.SetParameters(item.LuminanceExtractionQty);
                    dpnProperties2.Children.Add(_luminanceExtractionQtyControl);
                    break;
                case SMDAlgorithm.DeepLearning:
                    _deepLearningControl.SetParameters(item.DeepLearning);
                    dpnProperties2.Children.Add(_deepLearningControl);
                    break;
                default:
                    break;
            }
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

        private void imageBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_object is SMD)
            {
                SMD item = _object as SMD;
                item.ROI.X = (int)imageBox.ROI.X;
                item.ROI.Y = (int)imageBox.ROI.Y;
                item.ROI.Width = (int)imageBox.ROI.Width;
                item.ROI.Height = (int)imageBox.ROI.Height;
                NotifyPropertyChanged();
            }
        }

        private void btnGrabFrame_Click(object sender, RoutedEventArgs e)
        {
            if (_object is FOV)
            {
                if (_camera == null)
                {
                    MessageBox.Show("Camera unavailable.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                FOV item = _object as FOV;
                Mouse.OverrideCursor = Cursors.Wait;
                _camera.SetParameter(KeyName.ExposureTime, item.ExposureTime);
                _camera.ClearImageBuffer();
                _camera.SetParameter(KeyName.TriggerSoftware, 1);
                using (Bitmap bmp = _camera.GrabFrame())
                {
                    if (bmp != null)
                    {
                        _program.SetImageBlock(item.ImageBlockName, (Bitmap)bmp.Clone());
                        imageBox.SourceFromBitmap = (Bitmap)bmp.Clone();
                    }
                }
                Mouse.OverrideCursor = Cursors.Arrow;
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
            if (_object is FOV)
            {
                FOV item = _object as FOV;
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
                                _program.SetImageBlock(item.ImageBlockName, (Bitmap)bmp.Clone());
                                imageBox.SourceFromBitmap = (Bitmap)bmp.Clone();
                            }
                        }
                    }
                }
            }
        }

        private void btnSaveRect_Click(object sender, RoutedEventArgs e)
        {
            if (_object is SMD)
            {
                Rect rect = imageBox.ROI;
                SMD item = _object as SMD;
                item.ROI.X = (int)rect.X;
                item.ROI.Y = (int)rect.Y;
                item.ROI.Width = (int)rect.Width;
                item.ROI.Height = (int)rect.Height;
                MessageBox.Show("Save SMD ROI success!", "Save Rect", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
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
                                Dispatcher.Invoke(() => { imageBox.SourceFromBitmap = (Bitmap)bmp.Clone(); });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
        }

        private void PLCButton_Click(object sender, RoutedEventArgs e)
        {
            dpnProperties2.Children.Clear();
            _PLCControl.SetParameters(_program.PLC);
            dpnProperties2.Children.Add(_PLCControl);
        }

        private void PLCBackupButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationManager.Current.IsAdmin())
            {
                string message = "Success!";
                Stopwatch sw = new Stopwatch();
                sw.Start();
                WaitingDialog.DoWork("Backup device...", Task.Create(queryCanncel =>
                {
                    try
                    {
                        DeviceManager device = DeviceManager.Current;
                        MachineParams param = MachineParams.Current;
                        _program.PLC.Registers.Clear();
                        int first = Convert.ToInt16(_program.PLC.FirstDevice.Replace("D", ""));
                        int last = Convert.ToInt16(_program.PLC.LastDevice.Replace("D", ""));
                        for (int i = first; i <= last; i++)
                        {
                            if (!queryCanncel())
                            {
                                string label = "D" + i;
                                int value = 0;
                                int nRet = -1;
                                for (int j = 0; j < 10; j++)
                                {
                                    nRet = device.PLC1.GetDevice(label, ref value, saveLogs: true);
                                    if (nRet == 1)
                                    {
                                        break;
                                    }
                                }
                                if (nRet == 1)
                                {
                                    _program.PLC.Registers.Add(new Register { Device = label, Value = value });
                                }
                                else
                                {
                                    message = "Failure!";
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
                }), true);
                Trace.WriteLine($"Backup Result: {message}");
                Trace.WriteLine($"Time: {sw.Elapsed}");
                MessageBox.Show(message, "Backup", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void PLCRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationManager.Current.IsAdmin())
            {
                string message = "Success!";
                Stopwatch sw = new Stopwatch();
                sw.Start();
                WaitingDialog.DoWork("Restore device...", Task.Create(queryCanncel =>
                {
                    try
                    {
                        DeviceManager device = DeviceManager.Current;
                        MachineParams param = MachineParams.Current;
                        foreach (var item in _program.PLC.Registers)
                        {
                            if (!queryCanncel())
                            {
                                int nRet = device.PLC1.SetDevice(item.Device, item.Value);
                                if (nRet != 1)
                                {
                                    message = "Failure!";
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
                }), true);
                Trace.WriteLine($"Restore Result: {message}");
                Trace.WriteLine($"Time: {sw.Elapsed}");
                MessageBox.Show(message, "Restore", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void ServerButton_Click(object sender, RoutedEventArgs e)
        {
            dpnProperties2.Children.Clear();
            dpnProperties2.Children.Add(_setupServerControl);
            if (_program != null)
            {
                _setupServerControl.Fixtures = TestParams.Current.Fixtures;
            }
        }

        private void PLCMovePositionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_object is FOV)
            {
                FOV item = _object as FOV;
                DeviceManager device = DeviceManager.Current;
                MachineParams param = MachineParams.Current;
                if (device.PLC1.Ping(param.PLC1.Host, param.PLC1.Port) == 1)
                {
                    int value = 0;
                    device.PLC1.GetDevice32Bit("D1", ref value);
                    item.FOVPosition.X = value;

                    value = 0;
                    device.PLC1.GetDevice32Bit("D1", ref value);
                    item.FOVPosition.Y = value;

                    value = 0;
                    device.PLC1.GetDevice32Bit("D1", ref value);
                    item.FOVPosition.Z1 = value;

                    value = 0;
                    device.PLC1.GetDevice32Bit("D1", ref value);
                    item.FOVPosition.R1 = value;

                    value = 0;
                    device.PLC1.GetDevice32Bit("D1", ref value);
                    item.FOVPosition.Z2 = value;

                    value = 0;
                    device.PLC1.GetDevice32Bit("D1", ref value);
                    item.FOVPosition.R2 = value;

                    value = 0;
                    device.PLC1.GetDevice32Bit("D1", ref value);
                    item.FOVPosition.Z3 = value;

                    value = 0;
                    device.PLC1.GetDevice32Bit("D1", ref value);
                    item.FOVPosition.R3 = value;
                }
            }
        }

        private void XTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_object is SMD)
            {
                SMD item = _object as SMD;
                double currentValue = item.ROI.X;
                double newValue = currentValue + (e.Delta > 0 ? 1 : -1);
                item.ROI.X = newValue;
                ShowSMDRectangle(item);
            }
        }

        private void YTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_object is SMD)
            {
                SMD item = _object as SMD;
                double currentValue = item.ROI.Y;
                double newValue = currentValue + (e.Delta > 0 ? 1 : -1);
                item.ROI.Y = newValue;
                ShowSMDRectangle(item);
            }
        }

        private void WTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_object is SMD)
            {
                SMD item = _object as SMD;
                double currentValue = item.ROI.Width;
                double newValue = currentValue + (e.Delta > 0 ? 1 : -1);
                item.ROI.Width = newValue;
                ShowSMDRectangle(item);
            }
        }

        private void HTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_object is SMD)
            {
                SMD item = _object as SMD;
                double currentValue = item.ROI.Height;
                double newValue = currentValue + (e.Delta > 0 ? 1 : -1);
                item.ROI.Height = newValue;
                ShowSMDRectangle(item);
            }
        }

        private void PLCSavePositionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_object is FOV)
            {
                FOV item = _object as FOV;
                DeviceManager device = DeviceManager.Current;
                MachineParams param = MachineParams.Current;
                if (device.PLC1.Ping(param.PLC1.Host, param.PLC1.Port) == 1)
                {
                    device.PLC1.SetDevice32Bit("D1", item.FOVPosition.X);
                    device.PLC1.SetDevice32Bit("D1", item.FOVPosition.Y);
                    device.PLC1.SetDevice32Bit("D1", item.FOVPosition.Z1);
                    device.PLC1.SetDevice32Bit("D1", item.FOVPosition.R1);
                    device.PLC1.SetDevice32Bit("D1", item.FOVPosition.Z2);
                    device.PLC1.SetDevice32Bit("D1", item.FOVPosition.R2);
                    device.PLC1.SetDevice32Bit("D1", item.FOVPosition.Z3);
                    device.PLC1.SetDevice32Bit("D1", item.FOVPosition.R3);
                }
            }
        }

        private void UpdateTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_object is SMD)
            {
                SMD item = _object as SMD;
                using (Image<Bgr, byte> image = _program.GetImageBlock(item)?.Clone())
                {
                    if (image != null)
                    {
                        image.ROI = item.ROI.Rectangle;
                        item.TemplateMatching.Template = image.Copy();
                        image.ROI = new Rectangle();
                    }
                }
            }
        }

        private void GetHSVColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (_object is SMD)
            {
                SMD item = _object as SMD;
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
                        image.ROI = new Rectangle();
                    }
                }
            }
        }

        private void CopyHSVButton_Click(object sender, RoutedEventArgs e)
        {
            if (_object is SMD)
            {
                SMD item = _object as SMD;
                _HSVExtraction.Hue = item.HSVExtraction.Hue;
                _HSVExtraction.Saturation = item.HSVExtraction.Saturation;
                _HSVExtraction.Value = item.HSVExtraction.Value;
                _HSVExtraction.OKRange = item.HSVExtraction.OKRange;
                _HSVExtraction.IsEnabledReverseSearch = item.HSVExtraction.IsEnabledReverseSearch;
            }
        }

        private void PasteHSVButton_Click(object sender, RoutedEventArgs e)
        {
            if (_object is SMD)
            {
                SMD item = _object as SMD;
                item.HSVExtraction.Hue = _HSVExtraction.Hue;
                item.HSVExtraction.Saturation = _HSVExtraction.Saturation;
                item.HSVExtraction.Value = _HSVExtraction.Value;
                item.HSVExtraction.OKRange = _HSVExtraction.OKRange;
                item.HSVExtraction.IsEnabledReverseSearch = _HSVExtraction.IsEnabledReverseSearch;
            }
        }
    }
}
