using System;
using System.Diagnostics;
using System.Windows;

namespace Foxconn.AOI.Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private LC license = null;
        private Process p;
        private string logOutputControlFile = "NotOutputLog.ctl";

        protected override void OnStartup(StartupEventArgs e)
        {
            bool flag = false;
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
                flag = true;
            if (flag)
            {
                Trace.WriteLine("isProcessRunning = " + flag.ToString());
                MessageBox.Show("Application is already running.", "", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                ShutdownApp();
            }
            else if (FileExplorer.IsHaveFiles(Environment.ExpandEnvironmentVariables("%FOXCONN_ROOT%"), logOutputControlFile))
            {
                Trace.WriteLine("not Add Listeners");
            }
            else
            {
                Trace.WriteLine("Add Listeners");
                Trace.Listeners.Add(new LoggerTraceListener());
            }
            if (Extensions.IsConsole)
                Extensions.HideConsole();
            Trace.WriteLine("");
            Trace.WriteLine("Foxconn  =====> Startup app");
            ProjectLayout.Init();
            IdentityManager.Init();
            Trace.WriteLine("Check license");
            license = new LC();
            license.deadlineAction += () =>
            {
                MessageBox.Show("License expired.", "", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                ShutdownApp();
            };
            if (!license.Verify())
            {
                MessageBox.Show("Can not find license.", "", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                ShutdownApp();
            }
            else
            {
                Trace.WriteLine("Load paramaters");
                MachineParams.Reload();
                Customization.Reload();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (p == null || p.HasExited)
                return;
            p.Kill();
            license.Close();
        }

        private void ShutdownApp()
        {
            Shutdown();
            Environment.Exit(0);
        }
    }
}
