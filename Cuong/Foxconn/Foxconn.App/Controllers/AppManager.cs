using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.App.Controllers.Camera;
using Foxconn.App.Controllers.Client;
using Foxconn.App.Controllers.Image;
using Foxconn.App.Controllers.Plc;
using Foxconn.App.Controllers.Robot;
using Foxconn.App.Controllers.SerialPort;
using Foxconn.App.Controllers.Server;
using Foxconn.App.Controllers.Service;
using Foxconn.App.Controllers.Update;
using Foxconn.App.Controllers.Vnc;
using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using Foxconn.App.License;
using Foxconn.App.Models;
using Foxconn.App.ViewModels;
using Foxconn.App.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Foxconn.App.Controllers
{
    public class AppManager : IDisposable
    {
        private readonly MainWindow Root = MainWindow.Current;
        public CameraManager CameraManager { get; set; }
        public ClientManager ClientManager { get; set; }
        public DatabaseManager DatabaseManager { get; set; }
        public ImageManager ImageManager { get; set; }
        public LicenceManager LicenseManager { get; set; }
        public PlcManager PlcManager { get; set; }
        public RobotManager RobotManager { get; set; }
        public SerialPortManager SerialPortManager { get; set; }
        public ServerManager ServerManager { get; set; }
        public ServiceManager ServiceManager { get; set; }
        public UpdateManager UpdateManager { get; set; }
        public VncManager VncManager { get; set; }
        public List<TabHome> TabHome { get; set; }
        private AppStatus _status { get; set; }
        private bool _breakFlow { get; set; }
        private ModelName _modelName { get; set; }
        private string _barcodeText { get; set; }
        private bool _disposed { get; set; }

        public AppManager()
        {
            CameraManager = new CameraManager();
            ClientManager = new ClientManager();
            DatabaseManager = new DatabaseManager();
            ImageManager = new ImageManager();
            LicenseManager = new LicenceManager();
            PlcManager = new PlcManager();
            RobotManager = new RobotManager();
            SerialPortManager = new SerialPortManager();
            ServerManager = new ServerManager();
            ServiceManager = new ServiceManager();
            UpdateManager = new UpdateManager();
            VncManager = new VncManager();
            TabHome = new List<TabHome>();
            _status = AppStatus.None;
            _breakFlow = false;
            _modelName = ModelName.None;
            _barcodeText = string.Empty;
        }

        #region Disposable
        ~AppManager()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        // Public implementation of Dispose pattern callable by consumers.
        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                CameraManager?.Dispose();
                ClientManager?.Dispose();
                DatabaseManager?.Dispose();
                ImageManager?.Dispose();
                LicenseManager?.Dispose();
                PlcManager?.Dispose();
                RobotManager?.Dispose();
                SerialPortManager?.Dispose();
                ServerManager?.Dispose();
                ServiceManager?.Dispose();
                UpdateManager?.Dispose();
                VncManager?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.
            _disposed = true;
        }
        #endregion

        public void Init()
        {
            try
            {
                _disposed = false;
                Root.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.AppStarting; });
                Root.CodeFlow("STARTUP APP");
                ShowAppUI(1);
                CameraManager.Init();
                ClientManager.Init();
                DatabaseManager.Init();
                ImageManager.Init();
                LicenseManager.Init();
                PlcManager.Init();
                RobotManager.Init();
                SerialPortManager.Init();
                ServerManager.Init();
                ServiceManager.Init();
                UpdateManager.Init();
                VncManager.Init();
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                AppUi.ShowMessage(ex.Message, MessageBoxImage.Error);
            }
            finally
            {
                Root.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Arrow; });
                AppUi.ShowLabel(Root, Root.sslblStatus, "Ready");
            }
        }

        public void StartProgram(string modelName = "")
        {
            var loading = new LoadingWindow("Loading...");
            Task.Run(async () =>
            {
                try
                {
                    await Root.Dispatcher.BeginInvoke(() => Root.tabCenter.SelectedIndex = 1);
                    Root.CodeFlow("ABOUT");
                    loading.LabelContent = "About";
                    MainApp.About(messageBox: false);

                    Root.CodeFlow("CAMERA");
                    loading.LabelContent = "Camera";
                    CameraManager.Start();

                    Root.CodeFlow("CLIENT");
                    loading.LabelContent = "Client";
                    ClientManager.Start();

                    Root.CodeFlow("DATABASE");
                    loading.LabelContent = "Database";
                    DatabaseManager.Start(modelName);

                    Root.CodeFlow("IMAGE");
                    loading.LabelContent = "Image";
                    ImageManager.Start();

                    Root.CodeFlow("PLC");
                    loading.LabelContent = "PLC";
                    PlcManager.Start();

                    Root.CodeFlow("ROBOT");
                    loading.LabelContent = "Robot";
                    RobotManager.Start();

                    Root.CodeFlow("SERIAL PORT");
                    loading.LabelContent = "Serial port";
                    SerialPortManager.Start();

                    Root.CodeFlow("SERVER");
                    loading.LabelContent = "Server";
                    ServerManager.Start();

                    Root.CodeFlow("SERVICE");
                    loading.LabelContent = "Service";
                    ServiceManager.Start();

                    Root.CodeFlow("VNC");
                    loading.LabelContent = "VNC";
                    VncManager.Start();

                    Root.CodeFlow("PERFORMANCE CORE");
                    loading.LabelContent = "Performance core";
                    _ = Task.Factory.StartNew(PerformanceCore, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                    Root.CodeFlow("EFFICIENT CORE");
                    loading.LabelContent = "Efficient core";
                    _ = Task.Factory.StartNew(EfficientCore, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                    Root.CodeFlow("LOAD COMPLETED!");
                    loading.LabelContent = "Load completed!";
                    await Task.Delay(1000);
                    loading.KillMe = true;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Write(ex.StackTrace);
                    AppUi.ShowMessage(ex.Message, MessageBoxImage.Error);
                }
                finally
                {
                    await Root.Dispatcher.BeginInvoke(() => Root.tabCenter.SelectedIndex = 0);
                }
            });
            loading.Owner = Root;
            loading.ShowDialog();
        }

        private async Task PerformanceCore()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        if (_status != AppStatus.Running)
                            continue;

                        _breakFlow = false;
                        _modelName = ModelName.None;
                        _barcodeText = string.Empty;

                        var taskSN = await GettingSN();
                        if (taskSN != TaskResult.Succeeded)
                        {
                            await ErrorHandling(null, code: -1);
                            continue;
                        }

                        var server = await GettingServerInstance(_modelName);
                        if (server == null)
                        {
                            await ErrorHandling(null, code: -1);
                            continue;
                        }

                        var taskOpenFixture = await OpenFixture(server);
                        if (taskOpenFixture != TaskResult.Succeeded)
                        {
                            await ErrorHandling(server, code: 1);
                            continue;
                        }

                        var taskTestResult = await SendingTestResult(server);
                        if (taskTestResult != TaskResult.Succeeded)
                        {
                            await ErrorHandling(server, code: 2);
                            continue;
                        }

                        var taskMachineReady = await WaitingForMachineReady(server);
                        if (taskMachineReady != TaskResult.Succeeded)
                        {
                            await ErrorHandling(server, code: 2);
                            continue;
                        }

                        var taskCloseFixture = await CloseFixture(server);
                        if (taskCloseFixture != TaskResult.Succeeded)
                        {
                            await ErrorHandling(server, code: 1);
                            continue;
                        }

                        var text = string.Empty;
                        if (Appsettings.Config.SubString.Enable)
                        {
                            text = _barcodeText.Substring(Appsettings.Config.SubString.StartIndex, Appsettings.Config.SubString.Length);
                        }
                        else
                        {
                            text = _barcodeText;
                        }
                        var taskClientReady = await WaitingForClientReady(server, text);
                        if (taskClientReady != TaskResult.Succeeded)
                        {
                            await ErrorHandling(server, code: 2);
                            continue;
                        }

                        await SaveTestResult(server);
                        server.StartTesting();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        if (_breakFlow)
                        {
                            _breakFlow = false;
                            Root.CodeFlow("START NEW FLOW");
                        }

                        await Task.Delay(100);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private async Task EfficientCore()
        {
            try
            {
                var times = 0;
                while (true)
                {
                    #region Plc
                    PlcManager.CheckConnection(index: 0);
                    PlcManager.CheckConnection(index: 1);
                    //var register = DatabaseManager.Basic.Plc;
                    //if (PlcManager.GetSignal(register.Status.HasStopped, showLog: false))
                    //{
                    //    SetAppStatus(AppStatus.Stopped);
                    //}
                    //else
                    //{
                    //    if (PlcManager.GetSignal(register.Status.HasStarted, showLog: false))
                    //    {
                    //        SetAppStatus(AppStatus.Running);
                    //    }

                    //    if (PlcManager.GetSignal(register.Status.HasPaused, showLog: false))
                    //    {
                    //        SetAppStatus(AppStatus.Pausing);
                    //    }

                    //    if (PlcManager.GetSignal(register.Status.HasReset, showLog: false))
                    //    {
                    //        SetAppStatus(AppStatus.Resetting);
                    //    }

                    //    if (PlcManager.GetSignal(register.Status.HasResetOK, showLog: false))
                    //    {
                    //        SetAppStatus(AppStatus.ResetOK);
                    //    }
                    //}

                    //if (PlcManager.GetDevice(register.Door, showLog: false))
                    //{
                    //    AppUi.ShowBorderBackground(Root, Root.bdDoor, AppColor.Green);
                    //    AppUi.ShowLabel(Root, Root.lblDoor, "Close", AppColor.None, AppColor.White);
                    //}
                    //else
                    //{
                    //    AppUi.ShowBorderBackground(Root, Root.bdDoor, AppColor.Red);
                    //    AppUi.ShowLabel(Root, Root.lblDoor, "Open", AppColor.None, AppColor.White);
                    //}
                    #endregion

                    #region Robot
                    RobotManager.CheckConnection(index: 0);
                    RobotManager.CheckConnection(index: 1);
                    #endregion

                    #region Service
                    // Out of tray
                    if (RobotManager.OutOfTray)
                    {
                        if (PlcManager.SetDevice("M21", 1, show: true, index: 0))
                            RobotManager.OutOfTray = false;
                    }

                    //// Repair
                    //if (RobotManager.CheckRepair)
                    //{
                    //    if (PlcManager.SetDevice("M1", 1, show: true, index: 0))
                    //        RobotManager.CheckRepair = false;
                    //}

                    // Input
                    if (PlcManager.GetDevice("M25", show: true, index: 0))
                    {
                        if (await RobotManager.Send("STOP_INPUT", index: 1))
                            PlcManager.SetDevice("M25", 0, show: true, index: 0);
                    }

                    if (PlcManager.GetDevice("M26", show: true, index: 0))
                    {
                        if (await RobotManager.Send("RESET_INPUT", index: 1))
                            PlcManager.SetDevice("M26", 0, show: true, index: 0);
                    }

                    if (RobotManager.CheckInput)
                    {
                        if (PlcManager.GetDevice("M20", show: true, index: 0))
                        {
                            if (await RobotManager.Send("READY_INPUT1", index: 0))
                                RobotManager.CheckInput = false;
                        }
                        //else if (PlcManager.GetDevice("M1", show: true, index: 0))
                        //{
                        //    if (await RobotManager.Send("READY_INPUT2", index: 1))
                        //        RobotManager.CheckInput = false;
                        //}
                    }

                    //if (RobotManager.ReadyInput1OK)
                    //{
                    //    if (PlcManager.SetDevice("M1", 1, show: true, index: 0))
                    //        RobotManager.ReadyInput1OK = false;
                    //}

                    //if (RobotManager.ReadyInput2OK)
                    //{
                    //    if (PlcManager.SetDevice("M1", 1, show: true, index: 0))
                    //        RobotManager.ReadyInput2OK = false;
                    //}

                    // Output
                    if (PlcManager.GetDevice("M50", show: true, index: 1))
                    {
                        if (await RobotManager.Send("STOP_OUTPUT", index: 1))
                            PlcManager.SetDevice("M50", 0, show: true, index: 1);
                    }

                    if (PlcManager.GetDevice("M52", show: true, index: 1))
                    {
                        if (await RobotManager.Send("RESET_OUTPUT", index: 1))
                            PlcManager.SetDevice("M52", 0, show: true, index: 1);
                    }

                    if (RobotManager.CheckOutput)
                    {
                        if (PlcManager.GetDevice("M20", show: true, index: 1))
                        {
                            if (await RobotManager.Send("READY_OUTPUT1", index: 0))
                            {
                                PlcManager.SetDevice("M20", 0, show: true, index: 1);
                                RobotManager.CheckOutput = false;
                            }
                        }
                        else if (PlcManager.GetDevice("M22", show: true, index: 1))
                        {
                            if (await RobotManager.Send("READY_OUTPUT2", index: 0))
                            {
                                PlcManager.SetDevice("M22", 0, show: true, index: 1);
                                RobotManager.CheckOutput = false;
                            }
                        }
                    }

                    if (RobotManager.ReadyOutput1OK)
                    {
                        if (PlcManager.SetDevice("M21", 1, show: true, index: 1))
                            RobotManager.ReadyOutput1OK = false;
                    }

                    if (RobotManager.ReadyOutput2OK)
                    {
                        if (PlcManager.SetDevice("M23", 1, show: true, index: 1))
                            RobotManager.ReadyOutput2OK = false;
                    }
                    #endregion

                    #region Clear Message Log
                    AppUi.ClearDataGrid(Root, Root.dgLogRecords, 789);
                    #endregion

                    #region Reset Windows Layout
                    if (times >= 600 && _status == AppStatus.Running)
                    {
                        MainApp.ResetWindowsLayout();
                        times = 0;
                    }
                    ++times;
                    #endregion

                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void ShowAppUI(int userInterface = 0)
        {
            AppUi.ShowWindowTitle(Root, Appsettings.Config.Title);
            AppUi.ShowLabel(Root, Root.sslblStatus, "Loading...");
            AppUi.ShowImage(Root, Root.imgLoad, @"Assets/ToolBar/LoadingHidden.png");
            AppUi.ShowImage(Root, Root.imgStart, @"Assets/ToolBar/StartVisible.png");
            AppUi.ShowImage(Root, Root.imgPause, @"Assets/ToolBar/PauseVisible.png");
            AppUi.ShowImage(Root, Root.imgReset, @"Assets/ToolBar/ResetVisible.png");
            AppUi.ShowImage(Root, Root.imgStop, @"Assets/ToolBar/StopVisible.png");
            AppUi.ShowBorderBackground(Root, Root.bdDoor, AppColor.Green);
            AppUi.ShowLabel(Root, Root.lblDoor, "Close", AppColor.None, AppColor.White);

            var currentCulture = Thread.CurrentThread.CurrentCulture.ToString();
            if (currentCulture != string.Empty)
            {
                Language.Apply(Root, currentCulture);
            }

            Root.Dispatcher.Invoke(() =>
            {
                Root.gridTabHome.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                if (userInterface == 0)
                {
                    Root.wfCamera.Visibility = Visibility.Visible;
                }
                else if (userInterface == 1)
                {
                    int row = 5;
                    int column = 5;
                    for (int i = 0; i < row; i++)
                    {
                        Root.gridTabHome.RowDefinitions.Add(new RowDefinition());
                    }
                    for (int i = 0; i < column; i++)
                    {
                        Root.gridTabHome.ColumnDefinitions.Add(new ColumnDefinition());
                    }
                    int index = 0;
                    for (int i = 0; i < row; i++)
                    {
                        for (int j = 0; j < column; j++)
                        {
                            #region Context Menu
                            var contextMenu = new ContextMenu();

                            // VNC
                            var menuItemVnc = new MenuItem { Header = "VNC" };
                            var menuItemVncShow = new MenuItem
                            {
                                Name = $"item{index}",
                                Header = "Show"
                            };
                            menuItemVncShow.Click += MenuItemShowVnc_Click;
                            var menuItemVncHide = new MenuItem
                            {
                                Name = $"item{index}",
                                Header = "Hide"
                            };
                            menuItemVncHide.Click += MenuItemHideVnc_Click;
                            menuItemVnc.Items.Add(menuItemVncShow);
                            menuItemVnc.Items.Add(menuItemVncHide);

                            // Server
                            var menuItemServer = new MenuItem { Header = "Server" };
                            var menuItemServerForceDisconnect = new MenuItem
                            {
                                Name = $"item{index}",
                                Header = "Force disconnect"
                            };
                            menuItemServerForceDisconnect.Click += MenuItemForceDisconnect_Click;
                            menuItemServer.Items.Add(menuItemServerForceDisconnect);

                            // Fixture
                            var menuItemFixture = new MenuItem { Header = "Fixture" };
                            var menuItemFixtureOpen = new MenuItem
                            {
                                Name = $"item{index}",
                                Header = "Open"
                            };
                            menuItemFixtureOpen.Click += MenuItemFixtureOpen_Click;
                            var menuItemFixtureClose = new MenuItem
                            {
                                Name = $"item{index}",
                                Header = "Close"
                            };
                            menuItemFixtureClose.Click += MenuItemFixtureClose_Click;
                            menuItemFixture.Items.Add(menuItemFixtureOpen);
                            menuItemFixture.Items.Add(menuItemFixtureClose);

                            contextMenu.Items.Add(menuItemVnc);
                            contextMenu.Items.Add(menuItemServer);
                            contextMenu.Items.Add(menuItemFixture);
                            #endregion

                            // Border
                            var _border = new Border();
                            Grid.SetRow(_border, i);
                            Grid.SetColumn(_border, j);
                            _border.Name = $"border{i}{j}";
                            _border.CornerRadius = new CornerRadius(5);
                            _border.BorderThickness = new Thickness(1);
                            _border.Margin = new Thickness(1);
                            _border.Background = new SolidColorBrush(Color.FromRgb(199, 199, 204));
                            _border.MouseLeftButtonDown += _border_MouseLeftButtonDown;
                            _border.ContextMenu = contextMenu;

                            // Grid
                            var _gird = new Grid();
                            _gird.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
                            _gird.RowDefinitions.Add(new RowDefinition { MaxHeight = 250 });
                            _gird.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
                            // Label
                            var _labelIpAddress = new Label();
                            Grid.SetRow(_labelIpAddress, 0);
                            _labelIpAddress.Name = $"labelIpAddress_{index}";
                            _labelIpAddress.Content = $"xxx.xxx.xxx.xxx:yyyyy";
                            _labelIpAddress.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                            _labelIpAddress.Background = new SolidColorBrush(Color.FromRgb(199, 199, 204));
                            _labelIpAddress.HorizontalAlignment = HorizontalAlignment.Left;
                            _labelIpAddress.VerticalAlignment = VerticalAlignment.Center;

                            var _labelStatus = new Label();
                            Grid.SetRow(_labelStatus, 1);
                            _labelStatus.Name = $"labelStatus_{index}";
                            _labelStatus.Content = "Waiting";
                            _labelStatus.FontSize = 20;
                            _labelStatus.FontWeight = FontWeights.Normal;
                            _labelStatus.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                            _labelStatus.Background = new SolidColorBrush(Color.FromRgb(199, 199, 204));
                            _labelStatus.HorizontalAlignment = HorizontalAlignment.Center;
                            _labelStatus.VerticalAlignment = VerticalAlignment.Center;

                            var _labelTime = new Label();
                            Grid.SetRow(_labelTime, 2);
                            _labelTime.Name = $"labelTime_{index}";
                            _labelTime.Content = "Waiting Time: 0(s)";
                            _labelTime.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                            _labelTime.Background = new SolidColorBrush(Color.FromRgb(199, 199, 204));
                            _labelTime.HorizontalAlignment = HorizontalAlignment.Left;
                            _labelTime.VerticalAlignment = VerticalAlignment.Center;

                            // Add to grid
                            _gird.Children.Add(_labelIpAddress);
                            _gird.Children.Add(_labelStatus);
                            _gird.Children.Add(_labelTime);

                            // Add to border
                            _border.Child = _gird;

                            // Add gridTabHome
                            Root.gridTabHome.Children.Add(_border);

                            // Add to tabHome
                            TabHome.Add(new TabHome
                            {
                                Index = index,
                                Border = _border,
                                LabelHostPort = _labelIpAddress,
                                LabelStatus = _labelStatus,
                                LabelTime = _labelTime,
                            });

                            index += 1;
                        }
                    }
                }
            });
        }

        private void MenuItemShowVnc_Click(object sender, RoutedEventArgs e)
        {
            var objectName = ((MenuItem)sender).Name;
            var selectedIndex = Convert.ToInt32(objectName.Replace("item", ""));
            VncManager.Show(selectedIndex);
        }

        private void MenuItemHideVnc_Click(object sender, RoutedEventArgs e)
        {
            var objectName = ((MenuItem)sender).Name;
            var selectedIndex = Convert.ToInt32(objectName.Replace("item", ""));
            VncManager.Hide(selectedIndex);
        }

        private void MenuItemForceDisconnect_Click(object sender, RoutedEventArgs e)
        {
            var objectName = ((MenuItem)sender).Name;
            var selectedIndex = Convert.ToInt32(objectName.Replace("item", ""));
            if (AppUi.ShowMessage($"Do you want to force disconnect {selectedIndex}?", MessageBoxImage.Information) == MessageBoxResult.OK)
                ServerManager.ForceDisconnect(selectedIndex);
        }

        private void _border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //var objectName = ((Border)sender).Name;
        }

        private async void MenuItemFixtureOpen_Click(object sender, RoutedEventArgs e)
        {
            var objectName = ((MenuItem)sender).Name;
            var selectedIndex = Convert.ToInt32(objectName.Replace("item", ""));
            var userLogin = MainApp.Login();
            if (userLogin != User.None)
            {
                if (Root.AppManager._status != AppStatus.Stopped)
                {
                    AppUi.ShowMessage("You need to stop the machine before using this operation.", MessageBoxImage.Error);
                }
                else
                {
                    if (AppUi.ShowMessage($"Do you want to open fixture {selectedIndex}?", MessageBoxImage.Information) == MessageBoxResult.OK)
                        await ServerManager.Send("OPEN_FIXTURE", selectedIndex);
                }
            }
        }

        private async void MenuItemFixtureClose_Click(object sender, RoutedEventArgs e)
        {
            var objectName = ((MenuItem)sender).Name;
            var selectedIndex = Convert.ToInt32(objectName.Replace("item", ""));
            var userLogin = MainApp.Login();
            if (userLogin != User.None)
            {
                if (Root.AppManager._status != AppStatus.Stopped)
                {
                    AppUi.ShowMessage("You need to stop the machine before using this operation.", MessageBoxImage.Error);
                }
                else
                {
                    if (AppUi.ShowMessage($"Do you want to close fixture {selectedIndex}?", MessageBoxImage.Information) == MessageBoxResult.OK)
                        await ServerManager.Send("CLOSE_FIXTURE", selectedIndex);
                }
            }
        }

        public void Start()
        {
            SetAppStatus(AppStatus.Running);
        }

        public void Pause()
        {
            SetAppStatus(AppStatus.Pausing);
        }

        public async void Reset()
        {
            SetAppStatus(AppStatus.Resetting);
            await Task.Delay(1000);
            SetAppStatus(AppStatus.ResetOK);
        }

        public async Task<TaskResult> Resetting(int timeout = 60000)
        {
            var register = DatabaseManager.Basic.Plc.Status;
            using (var cts = new CancellationTokenSource())
            {
                var token = cts.Token;
                cts.CancelAfter(TimeSpan.FromMilliseconds(timeout));

                while (!token.IsCancellationRequested)
                {
                    if (PlcManager.GetSignal(register.ResetOK, show: false))
                        return TaskResult.Succeeded;

                    await Task.Delay(100);
                }
            }
            return TaskResult.Failed;
        }

        public async void Stop()
        {
            if (await RobotManager.Send($"STOP", index: 1))
            {
                SetAppStatus(AppStatus.Stopped);
            }
        }

        //public void ByPass()
        //{
        //    SetAppStatus(AppStatus.Bypass);
        //}

        private void SetAppStatus(AppStatus status)
        {
            _status = status;
            switch (status)
            {
                case AppStatus.None:
                    AppUi.ShowButton(Root, Root.tbbtnStart, true);
                    AppUi.ShowButton(Root, Root.tbbtnPause, true);
                    AppUi.ShowButton(Root, Root.tbbtnReset, true);
                    AppUi.ShowButton(Root, Root.tbbtnStop, true);
                    AppUi.ShowImage(Root, Root.imgLoad, @"Assets/ToolBar/LoadingHidden.png");
                    AppUi.ShowImage(Root, Root.imgStart, @"Assets/ToolBar/StartVisible.png");
                    AppUi.ShowImage(Root, Root.imgPause, @"Assets/ToolBar/PauseVisible.png");
                    AppUi.ShowImage(Root, Root.imgReset, @"Assets/ToolBar/ResetVisible.png");
                    AppUi.ShowImage(Root, Root.imgStop, @"Assets/ToolBar/StopVisible.png");
                    AppUi.ShowLabel(Root, Root.lblStatus, "Free", AppColor.White, AppColor.Black);
                    AppUi.ShowDataGrid(Root, Root.dgLogRecords, "Free", AppColor.None);
                    break;
                case AppStatus.Running:
                    AppUi.ShowButton(Root, Root.tbbtnStart, false);
                    AppUi.ShowButton(Root, Root.tbbtnPause, true);
                    AppUi.ShowButton(Root, Root.tbbtnReset, false);
                    AppUi.ShowButton(Root, Root.tbbtnStop, true);
                    AppUi.ShowImage(Root, Root.imgLoad, @"Assets/ToolBar/LoadingHidden.png");
                    AppUi.ShowImage(Root, Root.imgStart, @"Assets/ToolBar/StartHidden.png");
                    AppUi.ShowImage(Root, Root.imgPause, @"Assets/ToolBar/PauseVisible.png");
                    AppUi.ShowImage(Root, Root.imgReset, @"Assets/ToolBar/ResetHidden.png");
                    AppUi.ShowImage(Root, Root.imgStop, @"Assets/ToolBar/StopVisible.png");
                    AppUi.ShowLabel(Root, Root.lblStatus, "Running", AppColor.White, AppColor.Green);
                    AppUi.ShowDataGrid(Root, Root.dgLogRecords, "Running", AppColor.Green);
                    break;
                case AppStatus.Pausing:
                    AppUi.ShowButton(Root, Root.tbbtnStart, true);
                    AppUi.ShowButton(Root, Root.tbbtnPause, false);
                    AppUi.ShowButton(Root, Root.tbbtnReset, false);
                    AppUi.ShowButton(Root, Root.tbbtnStop, true);
                    AppUi.ShowImage(Root, Root.imgLoad, @"Assets/ToolBar/LoadingHidden.png");
                    AppUi.ShowImage(Root, Root.imgStart, @"Assets/ToolBar/StartVisible.png");
                    AppUi.ShowImage(Root, Root.imgPause, @"Assets/ToolBar/PauseHidden.png");
                    AppUi.ShowImage(Root, Root.imgReset, @"Assets/ToolBar/ResetHidden.png");
                    AppUi.ShowImage(Root, Root.imgStop, @"Assets/ToolBar/StopVisible.png");
                    AppUi.ShowLabel(Root, Root.lblStatus, "Pausing", AppColor.White, AppColor.Yellow);
                    AppUi.ShowDataGrid(Root, Root.dgLogRecords, "Pausing", AppColor.Yellow);
                    break;
                case AppStatus.Resetting:
                    AppUi.ShowButton(Root, Root.tbbtnStart, false);
                    AppUi.ShowButton(Root, Root.tbbtnPause, false);
                    AppUi.ShowButton(Root, Root.tbbtnReset, false);
                    AppUi.ShowButton(Root, Root.tbbtnStop, true);
                    AppUi.ShowImage(Root, Root.imgLoad, @"Assets/ToolBar/LoadingHidden.png");
                    AppUi.ShowImage(Root, Root.imgStart, @"Assets/ToolBar/StartHidden.png");
                    AppUi.ShowImage(Root, Root.imgPause, @"Assets/ToolBar/PauseHidden.png");
                    AppUi.ShowImage(Root, Root.imgReset, @"Assets/ToolBar/ResetHidden.png");
                    AppUi.ShowImage(Root, Root.imgStop, @"Assets/ToolBar/StopVisible.png");
                    AppUi.ShowLabel(Root, Root.lblStatus, "Resetting", AppColor.White, AppColor.Blue);
                    AppUi.ShowDataGrid(Root, Root.dgLogRecords, "Resetting", AppColor.Blue);
                    break;
                case AppStatus.ResetOK:
                    AppUi.ShowButton(Root, Root.tbbtnStart, true);
                    AppUi.ShowButton(Root, Root.tbbtnPause, false);
                    AppUi.ShowButton(Root, Root.tbbtnReset, true);
                    AppUi.ShowButton(Root, Root.tbbtnStop, true);
                    AppUi.ShowImage(Root, Root.imgLoad, @"Assets/ToolBar/LoadingHidden.png");
                    AppUi.ShowImage(Root, Root.imgStart, @"Assets/ToolBar/StartVisible.png");
                    AppUi.ShowImage(Root, Root.imgPause, @"Assets/ToolBar/PauseHidden.png");
                    AppUi.ShowImage(Root, Root.imgReset, @"Assets/ToolBar/ResetVisible.png");
                    AppUi.ShowImage(Root, Root.imgStop, @"Assets/ToolBar/StopVisible.png");
                    AppUi.ShowLabel(Root, Root.lblStatus, "Reset OK", AppColor.White, AppColor.Blue);
                    AppUi.ShowDataGrid(Root, Root.dgLogRecords, "Reset OK", AppColor.Blue);
                    break;
                case AppStatus.Stopped:
                    AppUi.ShowButton(Root, Root.tbbtnStart, false);
                    AppUi.ShowButton(Root, Root.tbbtnPause, false);
                    AppUi.ShowButton(Root, Root.tbbtnReset, true);
                    AppUi.ShowButton(Root, Root.tbbtnStop, false);
                    AppUi.ShowImage(Root, Root.imgLoad, @"Assets/ToolBar/LoadingHidden.png");
                    AppUi.ShowImage(Root, Root.imgStart, @"Assets/ToolBar/StartHidden.png");
                    AppUi.ShowImage(Root, Root.imgPause, @"Assets/ToolBar/PauseHidden.png");
                    AppUi.ShowImage(Root, Root.imgReset, @"Assets/ToolBar/ResetVisible.png");
                    AppUi.ShowImage(Root, Root.imgStop, @"Assets/ToolBar/StopHidden.png");
                    AppUi.ShowLabel(Root, Root.lblStatus, "Stopped", AppColor.White, AppColor.Red);
                    AppUi.ShowDataGrid(Root, Root.dgLogRecords, "Stopped", AppColor.Red);
                    break;
                //case AppStatus.Bypass:
                //    AppUi.ShowButton(Root, Root.tbbtnStart, false);
                //    AppUi.ShowButton(Root, Root.tbbtnPause, false);
                //    AppUi.ShowButton(Root, Root.tbbtnReset, false);
                //    AppUi.ShowButton(Root, Root.tbbtnStop, false);
                //    AppUi.ShowImage(Root, Root.imgLoad, @"Assets/ToolBar/LoadingHidden.png");
                //    AppUi.ShowImage(Root, Root.imgStart, @"Assets/ToolBar/StartHidden.png");
                //    AppUi.ShowImage(Root, Root.imgPause, @"Assets/ToolBar/PauseHidden.png");
                //    AppUi.ShowImage(Root, Root.imgReset, @"Assets/ToolBar/ResetHidden.png");
                //    AppUi.ShowImage(Root, Root.imgStop, @"Assets/ToolBar/StopHidden.png");
                //    AppUi.ShowLabel(Root, Root.lblStatus, "Bypass", AppColor.White, AppColor.Red);
                //    AppUi.ShowDataGrid(Root, Root.dgLogRecords, "Bypass", AppColor.Red);
                //    break;
                case AppStatus.Settings:
                    break;
                case AppStatus.SettingsOK:
                    break;
                default:
                    break;
            }
        }

        public void Debug()
        {

        }

        private async Task<TaskResult> WaitingForCamera(Image.Image process)
        {
            _barcodeText = string.Empty;
            var delay = DatabaseManager.Basic.Delay;
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    Root.ShowMessage($"[Camera {process.Index}] Capture: {i}");
                    if (i == 0)
                    {
                        if (process.Index == 0)
                            await Task.Delay(delay.Trigger0);
                        else if (process.Index == 1)
                            await Task.Delay(delay.Trigger1);
                        else
                            await Task.Delay(delay.Trigger2);
                    }

                    if (CaptureImageFromCamera(process))
                    {
                        if (ImageProcessing(process))
                        {
                            if (AnalyzeResults(process))
                            {
                                return TaskResult.Succeeded;
                            }
                            else
                            {
                                //return TaskResult.Failed;
                            }
                        }
                    }
                }
                return TaskResult.Failed;
            }
            finally
            {
                Root.ShowMessage($"SN: {_barcodeText}");
                if (process.Index == 0)
                {
                    if (process.OutputImage != null)
                        AppUi.ShowImageBox(Root, Root.imbCamera0, process.OutputImage.ToImage<Bgr, byte>());
                }
                else
                {
                    if (process.OutputImage != null)
                        AppUi.ShowImageBox(Root, Root.imbCamera0, process.OutputImage.ToImage<Bgr, byte>());
                }
            }
        }

        private bool CaptureImageFromCamera(Image.Image process, int index = 0)
        {
            if (process == null)
                return false;

            if (!process.IsStarted)
                return false;

            if (process.Steps.Count < index)
                return false;

            process.InputImage = null;
            process.OutputImage = null;
            AppUi.ClearImageBox(Root, Root.imbCamera0);

            var step = process.Steps[index];
            if (step.Enable)
            {
                process.Step = step;

                if (process.Camera.ExposureTime != step.Camera.ExposureTime && process.Camera.CameraType != CameraType.Webcam)
                    process.Camera.ExposureTime = step.Camera.ExposureTime;

                process.Camera.ExcuteTriggerSoftware();
                using (var bitmap = process.Camera.GrabFrame())
                {
                    if (bitmap != null)
                    {
                        using (var image = bitmap.ToImage<Bgr, byte>().Rotate((double)step.Camera.Rotate, new Bgr(), false))
                        {
                            if (image != null)
                            {
                                process.InputImage = image.Mat.Clone();
                                process.OutputImage = image.Mat.Clone();
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool ImageProcessing(Image.Image process)
        {
            var result = false;
            using (var input = process.InputImage.ToImage<Bgr, byte>())
            using (var output = process.OutputImage.ToImage<Bgr, byte>())
            {
                foreach (var component in process.Step.Components)
                {
                    if (component.Enable)
                    {
                        process.Component = component;
                        input.ROI = component.Region.Rectangle;
                        output.ROI = component.Region.Rectangle;
                        var text = string.Empty;
                        if (component.Algorithm == Algorithm.BarcodeDetection)
                        {
                            var temp = process.BarcodeText(input.Copy(), ref text, output);
                            if (temp)
                            {
                                _barcodeText = text;
                                result = true;
                                break;
                            }
                        }
                        else
                        {
                            Root.ShowMessage($"[Position {component.Index}]: Not correct algorithm or function", AppColor.Red);
                        }
                    }
                }
                input.ROI = System.Drawing.Rectangle.Empty;
                output.ROI = System.Drawing.Rectangle.Empty;
                //process.InputImage = null;
                process.OutputImage = output.Mat.Clone();
            }
            return result;
        }

        private bool AnalyzeResults(Image.Image process)
        {
            return true;
        }

        private async Task ErrorHandling(Server.Server server, int code)
        {
            if (code == -1)
                return;

            if (code == 1)
            {
                Root.ShowMessage($"Error: {code}", AppColor.Red);
                await RobotManager.Send("STOP", index: 1);
            }

            if (code == 2)
            {
                Root.ShowMessage($"Error: {code}", AppColor.Red);
            }

            if (server != null)
            {
                Root.ShowMessage($"[Server {server.Index}] Error", AppColor.Red);
                AppUi.ShowLabel(Root, Root.lblFlow, $"Error {server.Index}", AppColor.None, AppColor.Red);
                AppUi.ShowLabel(Root, server.TabHome.LabelStatus, $"Error {server.Index}", AppColor.None, AppColor.Red);

                if (server.TestResult != TestResult.None)
                {
                    server.ForcedClose();
                    Root.ShowMessage($"[Server {server.Index}] Forced close");
                }
                else
                {
                    Root.ShowMessage($"[Server {server.Index}] Cannot forced close", AppColor.Red);
                }
            }

            await Task.Delay(100);
        }

        public void ForceBreakFlow(bool breakFlow)
        {
            _breakFlow = breakFlow;
            Root.ShowMessage($"Break flow: {breakFlow}");
        }

        private bool CanContinue(Server.Server server)
        {
            if (_breakFlow)
                return false;

            // TestResult is none when client disconnected
            if (server != null && server.TestResult == TestResult.None)
                return false;

            return true;
        }

        private async Task<TaskResult> GettingSN()
        {
            Root.CodeFlow("GETTING SN");

            while (true)
            {
                if (!CanContinue(null))
                    return TaskResult.Failed;

                if (RobotManager.Scan)
                {
                    RobotManager.Scan = false;
                    var taskCamera = await WaitingForCamera(ImageManager.Process0);
                    if (taskCamera == TaskResult.Succeeded)
                    {
                        await RobotManager.Send("SCANOK", index: 0);
                        return TaskResult.Succeeded;
                    }
                    else
                    {
                        await RobotManager.Send("SCANNG", index: 0);
                        //return TaskResult.Failed;
                    }
                }

                await Task.Delay(100);
            }
        }

        private async Task<Server.Server> GettingServerInstance(ModelName modelName = ModelName.None)
        {
            Root.CodeFlow("GETTING SERVER INSTANCE");

            while (true)
            {
                if (!CanContinue(null))
                    return null;

                var servers = ServerManager.ServerList.FindAll(x => (
                    x.TestResult == TestResult.Init ||
                    x.TestResult == TestResult.Pass ||
                    x.TestResult == TestResult.Fail ||
                    x.TestResult == TestResult.Repair ||
                    x.TestResult == TestResult.InitInit ||
                    x.TestResult == TestResult.PassPass ||
                    x.TestResult == TestResult.FailFail ||
                    x.TestResult == TestResult.PassFail ||
                    x.TestResult == TestResult.FailPass) &&
                    x.ModelName == modelName);
                if (servers.Count > 0)
                {
                    var process = servers.OrderBy(x => x.DateTime).First();
                    if (process != null)
                    {
                        Root.ShowMessage($"[Server {process.Index}] Processing", AppColor.Yellow);
                        AppUi.ShowLabel(Root, Root.lblFlow, $"Processing {process.Index}", AppColor.None, AppColor.Yellow);
                        AppUi.ShowLabel(Root, Root.sslblStatus, $"Model Name: {process.ModelName}", AppColor.None, AppColor.Black);
                        return process;
                    }
                }

                await Task.Delay(100);
            }
        }

        private async Task<TaskResult> OpenFixture(Server.Server server)
        {
            Root.CodeFlow("OPEN FIXTURE");

            if (!CanContinue(server))
                return TaskResult.Failed;

            // Clear before send command
            server.DataReceived = string.Empty;
            var taskCommand = await server.Send("OPEN_FIXTURE");
            if (taskCommand)
            {
                while (true)
                {
                    if (!CanContinue(server))
                        return TaskResult.Failed;

                    if (server.DataReceived == "OPEN_FIXTUREOK")
                    {
                        return TaskResult.Succeeded;
                    }
                    else if (server.DataReceived == "OPEN_FIXTURENG")
                    {
                        return TaskResult.Failed;
                    }

                    await Task.Delay(100);
                }
            }
            return TaskResult.Failed;
        }

        private async Task<TaskResult> CloseFixture(Server.Server server)
        {
            Root.CodeFlow("CLOSE FIXTURE");

            if (!CanContinue(server))
                return TaskResult.Failed;

            // Clear before send command
            server.DataReceived = string.Empty;
            var taskCommand = await server.Send("CLOSE_FIXTURE");
            if (taskCommand)
            {
                while (true)
                {
                    if (!CanContinue(server))
                        return TaskResult.Failed;

                    if (server.DataReceived == "CLOSE_FIXTUREOK")
                    {
                        return TaskResult.Succeeded;
                    }
                    else if (server.DataReceived == "CLOSE_FIXTURENG")
                    {
                        return TaskResult.Failed;
                    }

                    await Task.Delay(100);
                }
            }
            return TaskResult.Failed;
        }

        private async Task<TaskResult> SendingTestResult(Server.Server server)
        {
            Root.CodeFlow("SENDING TEST RESULT TO ROBOT");

            if (!CanContinue(null))
                return TaskResult.Failed;

            var status = false;
            var register = server.Robot.Status;
            switch (server.TestResult)
            {
                case TestResult.None:
                    break;
                case TestResult.Init:
                    status = await RobotManager.Send(register.Repair, index: 0);
                    //status = await RobotManager.Send(register.Init, index: 0);
                    break;
                case TestResult.Pass:
                    status = await RobotManager.Send(register.Pass, index: 0);
                    break;
                case TestResult.Fail:
                    status = await RobotManager.Send(register.Fail, index: 0);
                    break;
                case TestResult.Repair:
                    status = await RobotManager.Send(register.Repair, index: 0);
                    break;
                case TestResult.InitInit:
                    status = await RobotManager.Send(register.InitInit, index: 0);
                    break;
                case TestResult.PassPass:
                    status = await RobotManager.Send(register.PassPass, index: 0);
                    break;
                case TestResult.FailFail:
                    status = await RobotManager.Send(register.FailFail, index: 0);
                    break;
                case TestResult.PassFail:
                    status = await RobotManager.Send(register.PassFail, index: 0);
                    break;
                case TestResult.FailPass:
                    status = await RobotManager.Send(register.FailPass, index: 0);
                    break;
                case TestResult.Locked:
                    break;
                case TestResult.Testing:
                    break;
                default:
                    break;
            }
            await Task.Delay(100);
            return status ? TaskResult.Succeeded : TaskResult.Failed;
        }

        private async Task<TaskResult> WaitingForMachineReady(Server.Server server)
        {
            Root.CodeFlow("WAITING FOR MACHINE READY");

            var register = server.Robot.Status;
            RobotManager.ClearDataReceived(register.Ready);
            while (true)
            {
                if (!CanContinue(null))
                    return TaskResult.Failed;

                if (RobotManager.GettingDataReceived() == register.Ready)
                {
                    return TaskResult.Succeeded;
                }

                await Task.Delay(100);
            }
        }

        private async Task<TaskResult> WaitingForClientReady(Server.Server server, string barcodeText = "")
        {
            Root.CodeFlow("WAITING FOR CLIENT READY");

            if (!CanContinue(server))
                return TaskResult.Failed;

            // Clear before send command
            server.DataReceived = string.Empty;
            var taskCommand = await server.Send($"RUN{barcodeText}");
            if (taskCommand)
            {
                while (true)
                {
                    if (!CanContinue(server))
                        return TaskResult.Failed;

                    //if (server.DataReceived == $"RUN{barcodeText}OK")
                    //{
                    //    return TaskResult.Succeeded;
                    //}
                    //else if (server.DataReceived == $"RUN{barcodeText}NG")
                    //{
                    //    return TaskResult.Failed;
                    //}
                    Console.WriteLine(server.DataReceived);
                    if (server.DataReceived.Contains("RUN") && server.DataReceived.Contains("OK"))
                    {
                        return TaskResult.Succeeded;
                    }
                    else if (server.DataReceived.Contains("RUN") && server.DataReceived.Contains("NG"))
                    {
                        return TaskResult.Failed;
                    }

                    await Task.Delay(100);
                }
            }
            return TaskResult.Failed;
        }

        private async Task SaveTestResult(Server.Server server)
        {
            var passNumber = 0;
            var failNumber = 0;
            switch (server.TestResult)
            {
                case TestResult.None:
                    break;
                case TestResult.Init:
                    break;
                case TestResult.Pass:
                    passNumber += 1;
                    break;
                case TestResult.Fail:
                    failNumber += 1;
                    break;
                case TestResult.Repair:
                    failNumber += 1;
                    break;
                case TestResult.InitInit:
                    break;
                case TestResult.PassPass:
                    passNumber += 2;
                    break;
                case TestResult.FailFail:
                    failNumber += 2;
                    break;
                case TestResult.PassFail:
                    passNumber += 1;
                    failNumber += 1;
                    break;
                case TestResult.FailPass:
                    passNumber += 1;
                    failNumber += 1;
                    break;
                case TestResult.Locked:
                    break;
                case TestResult.Testing:
                    break;
                default:
                    break;
            }
            await DatabaseManager.UpdateCounter(passNumber, failNumber);
            DatabaseManager.ShowCounter();
        }
    }
}
