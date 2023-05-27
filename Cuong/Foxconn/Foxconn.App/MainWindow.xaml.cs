using Foxconn.App.Controllers;
using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using Foxconn.App.Models;
using Foxconn.App.Views;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Foxconn.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Current { get; set; }
        public AppManager AppManager { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Current = this;
        }

        private void Window_Initialized(object sender, System.EventArgs e)
        {
            Appsettings.Instance.Read();
            Console.WriteLine(Appsettings.Config.FtpUser);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                AppManager = new AppManager();
                AppManager.Init();
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //var message = "Are you sure want to close this application?\r\n- Yes to close\r\n- No to cancel";
            //var status = AppUi.ShowMessage(message, MessageBoxImage.Question);
            //e.Cancel = status != MessageBoxResult.Yes;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            MainApp.Exit();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                mnuiDebug_Click(null, null);
            }
        }

        private void mnuiNewFile_Click(object sender, RoutedEventArgs e)
        {
            var userLogin = MainApp.Login();
            if (userLogin != User.None)
            {
                var keyboard = MainApp.Keyboard();
                if (keyboard.Length > 0)
                {
                    AppManager.DatabaseManager.CreateNewFile(keyboard, userLogin);
                }
            }
        }

        private void mnuiDeleteFile_Click(object sender, RoutedEventArgs e)
        {
            var userLogin = MainApp.Login();
            if (userLogin != User.None)
            {
                int selectedIndex = tbcmbModelName.SelectedIndex;
                object selectedItem = tbcmbModelName.SelectedItem;
                if (selectedIndex > -1)
                {
                    AppManager.DatabaseManager.DeleteFile(selectedItem.ToString(), userLogin);
                }
                else
                {
                    AppUi.ShowMessage($"Cannot delete.", MessageBoxImage.Error);
                }
            }
        }

        private void mnuiSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiSaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            var userLogin = MainApp.Login();
            if (userLogin != User.None)
            {
                int selectedIndex = tbcmbModelName.SelectedIndex;
                object selectedItem = tbcmbModelName.SelectedItem;
                if (selectedIndex > -1)
                {
                    AppManager.DatabaseManager.SaveAsFile(selectedItem.ToString(), userLogin);
                    //Appsettings.Instance.Write();
                }
                else
                {
                    AppUi.ShowMessage($"Cannot save.", MessageBoxImage.Error);
                }
            }
        }

        private void mnuiSaveAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiExitApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
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

        private void mnuiClearMessageLog_Click(object sender, RoutedEventArgs e)
        {
            AppUi.ClearDataGrid(this, dgLogRecords);
            AppManager.DatabaseManager.ClearCounter();
        }

        private void mnuiDatabaseAccessHistory_Click(object sender, RoutedEventArgs e)
        {
            if (MainApp.Login() != User.None)
            {
                AppUi.ShowWindow(this, new LogRecordsWindow());
            }
        }

        private void mnuiOpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", AppDomain.CurrentDomain.BaseDirectory);
        }

        private void mnuiCameraSetup_Click(object sender, RoutedEventArgs e)
        {
            if (MainApp.Login() != User.None)
            {
                int selectedIndex = tbcmbModelName.SelectedIndex;
                object selectedItem = tbcmbModelName.SelectedItem;
                if (selectedIndex > -1)
                {
                    AppUi.ShowWindow(this, new CameraSetupWindow());
                }
                else
                {
                    AppUi.ShowMessage("Cannot open camera window.", MessageBoxImage.Error);
                }
            }
        }

        private void mnuiCameraSignal_Click(object sender, RoutedEventArgs e)
        {
            if (MainApp.Login() != User.None)
            {
                AppUi.ShowWindow(this, new CameraSignalWindow());
            }
        }

        private void mnuiDelay_Click(object sender, RoutedEventArgs e)
        {
            if (MainApp.Login() != User.None)
            {
                AppUi.ShowWindow(this, new DelayWindow());
            }
        }

        private void mnuiTimer_Click(object sender, RoutedEventArgs e)
        {
            if (MainApp.Login() != User.None)
            {
                AppUi.ShowWindow(this, new TimerWindow());
            }
        }

        private void mnuiPlc_Click(object sender, RoutedEventArgs e)
        {
            if (MainApp.Login() != User.None)
            {
                AppUi.ShowWindow(this, new PlcWindow());
            }
        }

        private void mnuiRobot_Click(object sender, RoutedEventArgs e)
        {
            if (MainApp.Login() != User.None)
            {
                AppUi.ShowWindow(this, new RobotWindow());
            }
        }

        private void mnuiSerialPort_Click(object sender, RoutedEventArgs e)
        {
            if (MainApp.Login() != User.None)
            {
                AppUi.ShowWindow(this, new SerialPortWindow());
            }
        }

        private void mnuiService_Click(object sender, RoutedEventArgs e)
        {
            if (MainApp.Login() != User.None)
            {
                AppUi.ShowWindow(this, new ServiceWindow());
            }
        }

        private void mnuiPosition_Click(object sender, RoutedEventArgs e)
        {
            if (MainApp.Login() != User.None)
            {
                AppUi.ShowWindow(this, new PositionWindow());
            }
        }

        private void mnuiVnc_Click(object sender, RoutedEventArgs e)
        {
            if (MainApp.Login() != User.None)
            {
                AppUi.ShowWindow(this, new VncWindow());
            }
        }

        private void mnuiCustomize_Click(object sender, RoutedEventArgs e)
        {
            if (MainApp.Login() != User.None)
            {
                AppUi.ShowWindow(this, new CustomizeWindow());
            }
        }

        private void mnuiOptions_Click(object sender, RoutedEventArgs e)
        {
            if (MainApp.Login() != User.None)
            {
                AppUi.ShowWindow(this, new OptionsWindow());
            }
        }

        private void mnuiDebug_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AppManager.Debug();
            }
            catch (Exception ex)
            {
                AppUi.ShowMessage(ex.Message);
            }
        }

        private void mnuiResetWindowLayout_Click(object sender, RoutedEventArgs e)
        {
            MainApp.ResetWindowsLayout();
        }

        private void mnuiViewHelp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiEnglish_Click(object sender, RoutedEventArgs e)
        {
            Helper.Language.Apply(this, "en-US");
            AppUi.ShowMessage(Helper.Resources.GetString("en-US", "Hello"));
        }

        private void mnuiVietnamese_Click(object sender, RoutedEventArgs e)
        {
            Helper.Language.Apply(this, "vi-VN");
            AppUi.ShowMessage(Helper.Resources.GetString("vi-VN", "Hello"));
        }

        private async void mnuiCheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            if (await AppManager.UpdateManager.CheckForUpdates())
            {
                await AppManager.UpdateManager.DownloadAndInstall();
            }
        }

        private void mnuiTechnicalSupport_Click(object sender, RoutedEventArgs e)
        {
            MainApp.TechnicalSupport();
        }

        private void mnuiAbout_Click(object sender, RoutedEventArgs e)
        {
            MainApp.About();
        }

        private void tbcmbModelName_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                int selectedIndex = tbcmbModelName.SelectedIndex;
                object selectedItem = tbcmbModelName.SelectedItem;
                if (selectedIndex > -1)
                {
                    AppManager.DatabaseManager.ModelName = selectedItem.ToString();
                    AppManager.DatabaseManager.SelectedFile(selectedItem.ToString());
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                AppUi.ShowMessage(ex.StackTrace, MessageBoxImage.Error);
            }
        }

        private void tbbtnLoading_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int selectedIndex = tbcmbModelName.SelectedIndex;
                object selectedItem = tbcmbModelName.SelectedItem;
                if (selectedIndex > -1)
                {
                    //AppUI.ShowItem(this, mnuiNew, false);
                    //AppUI.ShowItem(this, mnuiDelete, false);
                    //AppUI.ShowItem(this, mnuiSave, false);
                    //AppUI.ShowItem(this, mnuiSaveAs, false);
                    //AppUI.ShowItem(this, mnuiSaveAll, false);
                    AppUi.ShowButton(this, tbbtnLoading, false);
                    AppUi.ShowComboBox(this, tbcmbModelName, false);
                    AppManager.StartProgram(selectedItem.ToString());
                }
                else
                {
                    AppUi.ShowMessage("Cannot loading.", MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                AppUi.ShowMessage(ex.StackTrace, MessageBoxImage.Error);
            }
        }

        private void tbbtnStart_Click(object sender, RoutedEventArgs e)
        {
            AppManager.Start();
        }

        private void tbbtnPause_Click(object sender, RoutedEventArgs e)
        {
            AppManager.Pause();
        }

        private void tbbtnReset_Click(object sender, RoutedEventArgs e)
        {
            AppManager.Reset();
        }

        private void tbbtnStop_Click(object sender, RoutedEventArgs e)
        {
            AppManager.Stop();
        }

        private void btnBreakFlow_Click(object sender, RoutedEventArgs e)
        {
            var selected = AppUi.ShowMessage($"Break Flow.", MessageBoxImage.Information) == MessageBoxResult.OK;
            AppManager.ForceBreakFlow(selected);
        }

        private void btnVncTools_Click(object sender, RoutedEventArgs e)
        {
            if (AppUi.ShowMessage($"Show All.", MessageBoxImage.Information) == MessageBoxResult.OK)
            {
                AppManager.VncManager.ShowAll();
            }
            else
            {
                AppManager.VncManager.HideAll();
            }
        }

        private async void btnSetSpeed_Click(object sender, RoutedEventArgs e)
        {
            var keyboard = MainApp.Keyboard();
            if (keyboard.Length > 0)
            {
                var value = Utilities.ConvertStringToInt(keyboard);
                if (value >= 0 && value <= 100)
                {
                    await AppManager.RobotManager.Send($"SPEED:{value}", index: 1);
                }
                else if (value > 100)
                {
                    await AppManager.RobotManager.Send($"SPEED:{100}", index: 1);
                }
            }
        }

        private async void btnTurnOffAir_Click(object sender, RoutedEventArgs e)
        {
            await AppManager.RobotManager.Send("TURN_OFF_AIR", index: 1);
        }

        public void CodeFlow(string message)
        {
            AppUi.ShowDataGrid(this, dgLogRecords, message, AppColor.Gray3);
        }

        public void ShowMessage(string message, AppColor color = AppColor.None)
        {
            AppUi.ShowDataGrid(this, dgLogRecords, message, color);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void WindowsFormsHost_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }
    }
}
