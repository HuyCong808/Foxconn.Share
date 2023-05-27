using Foxconn.App.Helper.Enums;
using Foxconn.App.Models;
using Foxconn.App.Views;
using System;
using System.Windows;

namespace Foxconn.App.Helper
{
    public class MainApp
    {
        private static readonly MainWindow Root = MainWindow.Current;

        public static void TechnicalSupport()
        {
            AppUi.ShowMessage("Engineer: Nguyen Quang Tiep\r\nMobile: (+84)90 29 65789");
        }

        public static void About(bool messageBox = true)
        {
            if (messageBox)
            {
                AppUi.ShowMessage($"Authors: {Appsettings.Config.Authors}\r\n" +
                    $"Company: {Appsettings.Config.Company}\r\n" +
                    $"Product: {Appsettings.Config.Product}\r\n" +
                    $"Version: {Appsettings.Config.Version}\r\n" +
                    $"Description: {Appsettings.Config.Description}\r\n" +
                    $"Copyright: {Appsettings.Config.Copyright}\r\n" +
                    $"Date Created: {Appsettings.Config.DateCreated}\r\n" +
                    $"Date Modified: {Utilities.GetDateModified()}");
            }
            else
            {
                Root.ShowMessage($"Authors: {Appsettings.Config.Authors}");
                Root.ShowMessage($"Company: {Appsettings.Config.Company}");
                Root.ShowMessage($"Product: {Appsettings.Config.Product}");
                Root.ShowMessage($"Version: {Appsettings.Config.Version}");
                Root.ShowMessage($"Description: {Appsettings.Config.Description}");
                Root.ShowMessage($"Copyright: {Appsettings.Config.Copyright}");
                Root.ShowMessage($"Date Created: {Appsettings.Config.DateCreated}");
                Root.ShowMessage($"Date Modified: {Utilities.GetDateModified()}");
            }
        }

        public static void ResetWindowsLayout()
        {
            Root.Dispatcher.Invoke(() =>
            {
                // Set windows size
                Application.Current.MainWindow = Root;
                Application.Current.MainWindow.Width = 1150;
                Application.Current.MainWindow.Height = 710;
                Root.WindowState = WindowState.Normal;
                Root.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                // Set grid size
                if (Root.gridCenter.ColumnDefinitions.Count >= 3)
                {
                    Root.gridCenter.ColumnDefinitions[0].Width = new GridLength(1150 - 271, GridUnitType.Star);
                    Root.gridCenter.ColumnDefinitions[1].Width = new GridLength(5);
                    Root.gridCenter.ColumnDefinitions[2].Width = new GridLength(250);
                }
                // Move to center screen
                var hwnd = WinApi.GetWindowHandle(Appsettings.Config.Title);
                WinApi.PlacementCenterWindowInMonitorWin32(hwnd);
            });
        }

        public static User Login()
        {
            var login = new LoginWindow
            {
                Owner = Root
            };
            login.ShowDialog();
            login.Close();
            if (login.Username == Enum.GetName(typeof(User), (int)User.Admin) && login.Password == "0902965789")
            {
                return User.Admin;
            }
            else if (login.Username == Enum.GetName(typeof(User), (int)User.Engineer) && login.Password == "789")
            {
                return User.Engineer;
            }
            else if (login.Username == Enum.GetName(typeof(User), (int)User.Operator) && login.Password == "123456")
            {
                return User.Operator;
            }
            else
            {
                Root.ShowMessage("Your App ID or password was incorrect. Forgot App ID or password?", AppColor.Red);
                return User.None;
            }
        }

        public static string Keyboard()
        {
            var keyboard = new KeyboardWindow
            {
                Owner = Root
            };
            keyboard.ShowDialog();
            keyboard.Close();
            return keyboard.Data;
        }

        public static void Exit()
        {
            // Call dispose for app manager.
            Root.AppManager?.Dispose();
            // Shuts down an application.
            Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }
}
