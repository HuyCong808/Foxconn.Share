using System;
using System.Runtime.InteropServices;

namespace Foxconn.Editor
{
    public class Extensions
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private readonly static IntPtr _hWnd = GetConsoleWindow();
        private static bool _isConsole = false;
        private static bool _visibility = false;

        public static bool IsConsole
        {
            get
            {
                if (_hWnd != IntPtr.Zero)
                {
                    Console.OutputEncoding = System.Text.Encoding.UTF8;
                    _isConsole = true;
                    _visibility = true;
                    return true;
                }
                return false;
            }
        }

        public static void ShowConsole()
        {
            _visibility = true;
            ShowWindow(_hWnd, 5);
        }

        public static void HideConsole()
        {
            _visibility = false;
            ShowWindow(_hWnd, 0);
        }

        public static void SwitchConsole()
        {
            if (_isConsole)
            {
                if (_visibility)
                {
                    HideConsole();
                }
                else
                {
                    ShowConsole();
                }
            }
        }
    }
}
