using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.AOI.Editor.Camera;
using Foxconn.AOI.Editor.Configuration;
using Foxconn.AOI.Editor.Controls;
using Foxconn.AOI.Editor.Dialogs;
using Foxconn.AOI.Editor.Enums;
using Foxconn.AOI.Editor.OpenCV;
using Foxconn.Threading;
using Foxconn.Threading.Tasks;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static Foxconn.AOI.Editor.Configuration.BackupDevice;

namespace Foxconn.AOI.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static MainWindow Current;
        private Properties.Settings _param = Properties.Settings.Default;
        private Worker _loopWorker;
        private Board _program
        {
            get => ProgramManager.Current.Program;
            set => ProgramManager.Current.Program = value;
        }
        private ObservableCollection<Board> _programs { get; set; } = new ObservableCollection<Board>();
        private object _selectedNavigation = null;
        private Board _selectedBoard = null;
        private FOV _selectedFOV = null;
        private SMD _selectedSMD = null;
        private BoardProperties _boardProperties = new BoardProperties();
        private FOVProperties _fovProperties = new FOVProperties();
        private SMDProperties _smdProperties = new SMDProperties();
        private CvCodeRecognitionControl _codeRecognitionControl = new CvCodeRecognitionControl();
        private CvContoursControl _contoursControl = new CvContoursControl();
        private CvDeepLearningControl _deepLearningControl = new CvDeepLearningControl();
        private CvFeatureMatchingControl _featureMatchingControl = new CvFeatureMatchingControl();
        private CvHoughCircleControl _houghCircleControl = new CvHoughCircleControl();
        private CvHoughLineControl _houghLineControl = new CvHoughLineControl();
        private CvHSVExtractionControl _hsvExtractionControl = new CvHSVExtractionControl();
        private CvLuminanceExtractionControl _luminanceExtractionControl = new CvLuminanceExtractionControl();
        private CvLuminanceExtractionQtyControl _luminanceExtractionQtyControl = new CvLuminanceExtractionQtyControl();
        private CvMarkTracingControl _markTracingControl = new CvMarkTracingControl();
        private CvTemplateMatchingControl _templateMatchingControl = new CvTemplateMatchingControl();
        private DeviceManagerControl _deviceManagerControl = new DeviceManagerControl();
        private ServerConfigurationControl _serversControl = new ServerConfigurationControl();
        private ICamera _camera = null;
        private bool _isStreaming = false;

        public bool IsSettings => _loopWorker.IsRunning;

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
            ShutdownApp();
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
                        //Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Icons/app_title.png", UriKind.RelativeOrAbsolute));
                        Title = Assembly.AssemblyProduct;
                        WindowState = WindowState.Maximized;
                        trvNavigation.ItemsSource = _programs;
                    });
                    _loopWorker = new Worker(new ThreadStart(MainProcess));
                    _loopWorker.Start();
                    DeviceManager.Current.Open();
                });

                #region Events
                _boardProperties.DeviceManagerButton.Click += btnDeviceManagerButton_Click;
                //_boardProperties.ServerManagerButton.Click += btnServerManagerButton_Click;
                _fovProperties.SaveFOVPosition.Click += btnSaveFOVPositionButton_Click;
                _fovProperties.MoveFOVPosition.Click += btnMoveFOVPositionButton_Click;
                _deviceManagerControl.BackupDeviceButton.Click += btnBackupDeviceButton_Click;
                _deviceManagerControl.RestoreDeviceButton.Click += btnRestoreDeviceButton_Click;
                _templateMatchingControl.UpdateTemplateButton.Click += btnUpdateTemplateButton_Click;
                _hsvExtractionControl.GetHSVColorButton.Click += btnGetHSVColorButton_Click;
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

        private void ShutdownApp()
        {
            try
            {
                Trace.WriteLine("Foxconn =====> Shutdown app");
                DeviceManager.Current.Close();
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
                ShutdownApp();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
            {
                mnuiNewProgram_Click(null, null);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
            {
                mnuiOpenProgram_Click(null, null);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                mnuiSaveAsProgramAndImage_Click(null, null);
            }
            else if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.S)
            {
                //mnuiSaveAll_Click(null, null);
            }
            else if (e.Key == Key.F5)
            {
                mnuiTest_Click(null, null);
            }
        }

        private void mnuiNewProgram_Click(object sender, RoutedEventArgs e)
        {
            ProgramManager.Current.NewProgram();
            RefreshProgram();
        }

        private void mnuiOpenProgram_Click(object sender, RoutedEventArgs e)
        {
            ProgramManager.Current.OpenProgram();
            RefreshProgram();
        }

        private void mnuiClose_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiSaveProgram_Click(object sender, RoutedEventArgs e)
        {
            //if (ApplicationManager.Current.IsAdmin())
            //    ProgramManager.Current.SaveProgram();
        }

        private void mnuiSaveProgramAndImage_Click(object sender, RoutedEventArgs e)
        {
            //if (ApplicationManager.Current.IsAdmin())
            //    ProgramManager.Current.SaveProgramAndImage();
        }

        private void mnuiSaveAsProgram_Click(object sender, RoutedEventArgs e)
        {
            //if (ApplicationManager.Current.IsAdmin())
            //    ProgramManager.Current.SaveAsProgram();
        }

        private void mnuiSaveAsProgramAndImage_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationManager.Current.IsAdmin())
                ProgramManager.Current.SaveProgramAndImage();
        }

        private void mnuiSaveParameters_Click(object sender, RoutedEventArgs e)
        {
            MachineParams.Current.Save();
            Customization.Current.Save();
        }

        private void mnuiImportBoardImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiImportLibraries_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiImportReferenceImages_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiImportBOM_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiExportBoardImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiExportLibraries_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiExportReferenceImages_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiExportCAD_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiExportAOIInspectionData_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiViewSummary_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiLogin_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiLogout_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiExit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiZoomIn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiZoomOut_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiSwitchConsole_Click(object sender, RoutedEventArgs e)
        {
            Extensions.SwitchConsole();
        }

        private void mnuiOpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory);
        }

        private void mnuiAutoRun_Click(object sender, RoutedEventArgs e)
        {
            AutoRunManager autoRun = new AutoRunManager();
            if (autoRun.Run())
            {
                _loopWorker.Stop();
                MainWindow.Current.Hide();
                autoRun.ShowDialog();
            }
            else
            {
                Trace.WriteLine("Can not auto run");
                MessageBox.Show("Can not auto run!", "Auto Run", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void mnuiReloadMachineParameters_Click(object sender, RoutedEventArgs e)
        {
            MachineParams.Reload();
            Customization.Reload();
        }

        private void mnuiTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedNavigation is SMD)
                {
                    SMD item = _selectedNavigation as SMD;
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
                                    _hsvExtractionControl.Score = cvRet.Score;
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
                        src.ROI = new System.Drawing.Rectangle();
                        dst.ROI = new System.Drawing.Rectangle();
                        Dispatcher.Invoke(() =>
                        {
                            imageBox.SourceFromBitmap = dst.ToBitmap();
                            imageBox.ClearSelectedRect();
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

        private void mnuiUserGuide_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiTechnicalSupport_Click(object sender, RoutedEventArgs e)
        {
            string message = "Engineer: Nguyen Quang Tiep\r\nMobile: (+84)90-29-65789";
            MessageBox.Show(message, "Technical Support", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }

        private void mnuiCheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            SoftwareUpdate.Current.CheckForUpdates();
        }

        private void mnuiAboutAOISystem_Click(object sender, RoutedEventArgs e)
        {
            //AboutDialog aboutDialog = new AboutDialog();
            //aboutDialog.ShowDialog();
        }

        private void trvNavigation_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            dpnProperties2.Children.Clear();
            _selectedNavigation = e.NewValue;
            if (_selectedNavigation is Board)
            {
                Board item = _selectedNavigation as Board;
                ShowBoardProperties(item);
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
                Trace.WriteLine("Navigation is not selected");
            }
            NotifyPropertyChanged();
        }

        private void trvNavigation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_selectedNavigation is Board)
                {
                    _selectedBoard = _selectedNavigation as Board;
                }
                else if (_selectedNavigation is FOV)
                {
                    _selectedFOV = _selectedNavigation as FOV;
                }
                else if (_selectedNavigation is SMD)
                {
                    _selectedSMD = _selectedNavigation as SMD;
                }
                RefreshNavigation(_selectedNavigation);
            }
        }

        private void mnuiEnableFOV_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is FOV)
            {
                FOV item = _selectedNavigation as FOV;
                item.IsEnabled = true;
            }
        }

        private void mnuiDisableFOV_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is FOV)
            {
                FOV item = _selectedNavigation as FOV;
                item.IsEnabled = false;
            }
        }

        private void mnuiEnableSMD_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is SMD)
            {
                SMD item = _selectedNavigation as SMD;
                item.IsEnabled = true;
            }
        }

        private void mnuiDisableSMD_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is SMD)
            {
                SMD item = _selectedNavigation as SMD;
                item.IsEnabled = false;
            }
        }

        private void btnAddFOV_Click(object sender, RoutedEventArgs e)
        {
            Board program = _program;
            if (program != null)
            {
                if (_selectedNavigation is Board)
                {
                    FOV itemFOV = program.AddFOV();
                    if (itemFOV != null)
                    {
                        _selectedNavigation = itemFOV;
                        RefreshNavigation(_selectedNavigation);
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
                if (_selectedNavigation is FOV)
                {
                    FOV itemFOV = _selectedNavigation as FOV;
                    int id = program.FOVs.IndexOf(itemFOV);
                    SMD itemSMD = program.AddSMD(id);
                    if (itemSMD != null)
                    {
                        _selectedNavigation = itemSMD;
                    }
                    RefreshNavigation(_selectedNavigation);
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
                if (_selectedNavigation is Board)
                {
                    Board item = _selectedNavigation as Board;
                    if (MessageBox.Show($"Are you sure want to remove {item.Name}?", "Remove Board", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                        return;
                }
                else if (_selectedNavigation is FOV)
                {
                    FOV item = _selectedNavigation as FOV;
                    if (MessageBox.Show($"Are you sure want to remove {item.Name}?", "Remove FOV", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                        program.RemoveFOV(item);
                }
                else if (_selectedNavigation is SMD)
                {
                    SMD item = _selectedNavigation as SMD;
                    if (MessageBox.Show($"Are you sure want to remove {item.Name}?", "Remove SMD", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                        program.RemoveSMD(item);
                }
                RefreshNavigation(_selectedNavigation);
            }
        }

        private void btnRefreshNavigation_Click(object sender, RoutedEventArgs e)
        {
            RefreshNavigation(_selectedNavigation);
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

        private void RefreshProgram()
        {
            _programs.Clear();
            _programs.Add(_program);
            trvNavigation.Items.Refresh();
            NotifyPropertyChanged();
        }

        private void ShowBoardProperties(Board item)
        {
            dpnProperties1.Children.Clear();
            _boardProperties.SetParameters(item);
            dpnProperties1.Children.Add(_boardProperties);
        }

        private void ShowFOVProperties(FOV item)
        {
            dpnProperties1.Children.Clear();
            _fovProperties.SetParameters(item);
            dpnProperties1.Children.Add(_fovProperties);
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

        private void ShowSMDProperties(SMD item)
        {
            dpnProperties1.Children.Clear();
            _smdProperties.SetParameters(item);
            dpnProperties1.Children.Add(_smdProperties);
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

        private void ShowSMDRect(SMD item)
        {
            imageBox.DrawRectangle(item.ROI.Rectangle);
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
                    _hsvExtractionControl.SetParameters(item.HSVExtraction);
                    dpnProperties2.Children.Add(_hsvExtractionControl);
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
                if (zoomScale < imageBox.MaxScale)
                {
                    imageBox.ZoomScale = zoomScale;
                }
            }
            else
            {
                zoomScale = 0.8 * zoomScale;
                if (zoomScale > imageBox.MinScale)
                {
                    imageBox.ZoomScale = zoomScale;
                }
            }
        }

        private void btnGrabFrame_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is FOV)
            {
                if (_camera == null)
                {
                    MessageBox.Show("Camera unavailable.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                FOV item = _selectedNavigation as FOV;
                Mouse.OverrideCursor = Cursors.Wait;
                _camera.SetParameter(KeyName.ExposureTime, item.ExposureTime);
                _camera.ClearImageBuffer();
                _camera.SetParameter(KeyName.TriggerSoftware, 1);
                using (System.Drawing.Bitmap bmp = _camera.GrabFrame())
                {
                    if (bmp != null)
                    {
                        _program.SetImageBlock(item.ImageBlockName, (System.Drawing.Bitmap)bmp.Clone());
                        imageBox.SourceFromBitmap = (System.Drawing.Bitmap)bmp.Clone();
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
            if (_selectedNavigation is FOV)
            {
                FOV item = _selectedNavigation as FOV;
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
                                imageBox.SourceFromBitmap = (System.Drawing.Bitmap)bmp.Clone();
                            }
                        }
                    }
                }
            }
        }

        private void btnSaveRect_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is SMD)
            {
                Rect rect = imageBox.SelectedRect;
                SMD item = _selectedNavigation as SMD;
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
                        using (System.Drawing.Bitmap bmp = _camera.GrabFrame())
                        {
                            if (bmp != null)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    imageBox.SourceFromBitmap = (System.Drawing.Bitmap)bmp.Clone();
                                });
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

        private void btnDeviceManagerButton_Click(object sender, RoutedEventArgs e)
        {
            dpnProperties2.Children.Clear();
            _deviceManagerControl.SetParameters(_program.BackupDevices);
            dpnProperties2.Children.Add(_deviceManagerControl);
        }

        private void btnBackupDeviceButton_Click(object sender, RoutedEventArgs e)
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
                        _program.BackupDevices.Data.Clear();
                        int first = Convert.ToInt16(_program.BackupDevices.FirstDevice.Replace("D", ""));
                        int last = Convert.ToInt16(_program.BackupDevices.LastDevice.Replace("D", ""));
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
                                    _program.BackupDevices.Data.Add(new Register { Device = label, Value = value });
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

        private void btnRestoreDeviceButton_Click(object sender, RoutedEventArgs e)
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
                        foreach (var item in _program.BackupDevices.Data)
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

        private void btnServerManagerButton_Click(object sender, RoutedEventArgs e)
        {
            dpnProperties2.Children.Clear();
            dpnProperties2.Children.Add(_serversControl);
            if (_program != null)
            {
                _serversControl.Servers = _program.Servers;
            }
        }

        private void btnSaveFOVPositionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is FOV)
            {
                FOV item = _selectedNavigation as FOV;
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

        private void btnMoveFOVPositionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNavigation is FOV)
            {
                FOV item = _selectedNavigation as FOV;
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
    }
}
