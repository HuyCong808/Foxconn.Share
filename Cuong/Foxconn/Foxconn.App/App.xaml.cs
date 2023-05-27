using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace Foxconn.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32")]
        private static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32")]
        private static extern bool OpenIcon(IntPtr hWnd);

        private static Mutex _mutex;
        private const string _appName = "Foxconn.App";
        private const string _windowName = "Foxconn.App";
        private bool _createdNew;

        protected override void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, _appName, out _createdNew);
            if (!_createdNew)
            {
                MessageBox.Show($"{_appName} is already running.");
                System.Windows.Application.Current.Shutdown();
                ShowWindow(_windowName);
            }
            base.OnStartup(e);
        }

        private static void ShowWindow(string windowName)
        {
            var hwnd = FindWindow(null, windowName);
            if (hwnd != IntPtr.Zero)
            {
                SetForegroundWindow(hwnd);
                if (IsIconic(hwnd))
                {
                    OpenIcon(hwnd);
                }
            }
        }
    }
}
//using System.Windows;

//namespace Foxconn.App
//{
//    /// <summary>
//    /// Interaction logic for App.xaml
//    /// </summary>
//    public partial class App : Application
//    {
//    }
//}