using System;
using System.Diagnostics;
using System.Windows;

namespace Foxconn.Editor
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.Current.Create();
            Logger.Current.Info("Startup Application");
            ProjectLayout.Init();
            bool isOpenedApp = false;
            if(Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                isOpenedApp = true;   
            }

            if(isOpenedApp)
            {
                Logger.Current.Info("Process is Running = " + isOpenedApp.ToString());
                MessageShow.Warning("Application is runing", "Application");
                ShutdownApp();
            }
        }

        private void ShutdownApp()
        {
            Shutdown();
            Environment.Exit(0);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Logger.Current.Info("Shutdown Application");
        }
    }
}
