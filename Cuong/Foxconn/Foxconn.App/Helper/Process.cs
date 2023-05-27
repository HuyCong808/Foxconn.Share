using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Foxconn.App.Helper
{
    public class Process
    {
        public enum CmdShow
        {
            Hide,
            Normal,
            Minimized,
            Maximized,
            NoActivate,
            Show,
            Minimize,
            MinimizeNoActive,
            NA,
            Restore,
            Default,
            ForceMinimize
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Searching a process by name
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        public static bool Search(string processName)
        {
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }
        /// <summary>
        /// Stop a process by name
        /// </summary>
        /// <param name="processName"></param>
        public static void Stop(string processName)
        {
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                foreach (var process in processes)
                {
                    process.Kill();
                }
            }
        }
        /// <summary>
        /// Start a process
        /// Example: Start(@"C:\Test\Debug\MyApp.exe", @"C:\Test\Debug", false);
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="destinationPath"></param>
        /// <param name="hidden"></param>
        public static void Start(string fileName, string destinationPath, bool hidden = false)
        {
            try
            {
                new System.Diagnostics.Process()
                {
                    StartInfo = {
                        FileName = fileName,
                        Arguments = destinationPath,
                        WindowStyle = hidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                        WorkingDirectory = System.IO.Path.GetDirectoryName(destinationPath),
                    }
                }.Start();
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }
        /// <summary>
        /// Open explorer by path
        /// </summary>
        /// <param name="path"></param>
        public static void OpenExplorer(string path)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        public static void ShowWindow(string windowName, CmdShow cmdShow = CmdShow.Normal)
        {
            var hwnd = FindWindow(null, windowName);
            if (hwnd != IntPtr.Zero)
            {
                ShowWindow(hwnd, (int)cmdShow);
            }
        }

        public static void ShowConsole(CmdShow cmdShow = CmdShow.Normal)
        {
            if (Search("conhost"))
            {
                var hwnd = GetConsoleWindow();
                ShowWindow(hwnd, (int)cmdShow);
            }
        }
    }
}
