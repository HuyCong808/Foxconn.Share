using Foxconn.Editor.Dialogs;
using Foxconn.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;

namespace Foxconn.Editor
{
    internal class SoftwareUpdate
    {
        public static SoftwareUpdate Current => __current;
        private static SoftwareUpdate __current = new SoftwareUpdate();
        static SoftwareUpdate() { }
        private SoftwareUpdate() { }

        public void CheckForUpdates()
        {
            try
            {
                MachineParams param = MachineParams.Current;
                if (param.SCP.IsEnabled)
                {
                    Trace.WriteLine($"Software Update");
                    Trace.WriteLine($"Host: {param.SCP.Host}");
                    Trace.WriteLine($"User: {param.SCP.User}");
                    Trace.WriteLine($"Password: {param.SCP.Password}");
                    Trace.WriteLine($"Version file: {param.SCP.VersionFile}");
                    Trace.WriteLine($"Update file: {param.SCP.UpdateFile}");
                    Trace.WriteLine($"Logs directory: {param.SCP.LogsDirectory}");

                    bool doUpdate = true;
                    string versionFile = $"{AppDomain.CurrentDomain.BaseDirectory}version.txt";
                    string updateFile = $"{AppDomain.CurrentDomain.BaseDirectory}update.7z";
                    SCPClient scpClient = new SCPClient
                    {
                        Protocol = (WinSCP.Protocol)param.SCP.Protocol,
                        Host = param.SCP.Host,
                        Port = param.SCP.Port,
                        User = param.SCP.User,
                        Password = param.SCP.Password,
                        Key = param.SCP.Key
                    };

                    if (scpClient.Ping() != 1)
                    {
                        string message = $"Cannot ping to {param.SCP.Host}:{param.SCP.Port}";
                        Trace.WriteLine(message);
                        MessageBox.Show(message, "Software Update", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }

                    WaitingDialog.DoWork("Checking for updates...", Task.Create(queryCanncel =>
                    {
                        int download = scpClient.DownloadFile(versionFile, param.SCP.VersionFile);
                        if (download == 1)
                        {
                            if (!UpdateAvailable())
                            {
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }), true);

                    MessageBoxResult r = MessageBox.Show("Download and Install", "Software Update", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    if (r != MessageBoxResult.OK)
                    {
                        doUpdate = false;
                    }
                    if (doUpdate)
                    {
                        WaitingDialog.DoWork("Downloading...", Task.Create(queryCanncel =>
                        {
                            int download = scpClient.DownloadFile(updateFile, param.SCP.UpdateFile);
                            if (download == 1)
                            {
                                Process.Start("Update.exe", AppDomain.CurrentDomain.BaseDirectory);
                            }
                        }), true);
                    }
                }
                else
                {
                    string message = $"Software Update: Disabled";
                    Trace.WriteLine(message);
                    MessageBox.Show(message, "Software Update", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        private bool UpdateAvailable()
        {
            try
            {
                string versionFile = $"{AppDomain.CurrentDomain.BaseDirectory}version.txt";
                string contents = File.ReadAllText(versionFile);
                DateTime cloudTime = new DateTime();
                //DateTime cloudTime = DateTime.ParseExact(contents.Trim(), "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                CultureInfo enUS = new CultureInfo("en-US");
                if (!DateTime.TryParseExact(contents.Trim(), "yyyy/MM/dd HH:mm:ss", enUS, DateTimeStyles.None, out cloudTime))
                {
                    if (!DateTime.TryParseExact(contents.Trim(), "yyyyMMddHHmmss", enUS, DateTimeStyles.None, out cloudTime))
                    {
                        Trace.WriteLine($"Version File (Format: \'yyyy/MM/dd HH:mm:ss\' or \'yyyyMMddHHmmss\'): {contents.Trim()}");
                        return false;
                    }
                }
                DateTime localTime = Assembly.LastWriteTime;
                double totalMinutes = (cloudTime - localTime).TotalMinutes;
                Trace.WriteLine($"Cloud version: {cloudTime}");
                Trace.WriteLine($"Local version: {localTime}");
                return totalMinutes > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
