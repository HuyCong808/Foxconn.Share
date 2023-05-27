using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using Foxconn.App.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Foxconn.App.Controllers.Update
{
    public class UpdateManager : IDisposable
    {
        private readonly MainWindow Root = MainWindow.Current;
        private string _host { get; set; }
        private string _user { get; set; }
        private string _password { get; set; }
        private string _rootPath { get; set; }
        private string _remoteUpdatePath { get; set; }
        private string _remoteVersionPath { get; set; }
        private string _localUpdatePath { get; set; }
        private string _localVersionPath { get; set; }
        private bool _disposed { get; set; }

        public UpdateManager()
        {
            _host = Appsettings.Config.FtpUrl;
            _user = Appsettings.Config.FtpUser;
            _password = Appsettings.Config.FtpPassword;
            _rootPath = AppDomain.CurrentDomain.BaseDirectory;
            _remoteUpdatePath = Appsettings.Config.FtpUpdateFile;
            _remoteVersionPath = Appsettings.Config.FtpVersionFile;
            _localUpdatePath = _rootPath + Path.GetFileName(Appsettings.Config.FtpUpdateFile);
            _localVersionPath = _rootPath + Path.GetFileName(Appsettings.Config.FtpVersionFile);
        }

        #region Disposable
        ~UpdateManager()
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
                var sourceBatchFile = $@"{_rootPath}DeveloperTools\Update.bat";
                var sourceUnRarFile = $@"{_rootPath}DeveloperTools\UnRAR.exe";
                var destBatchFile = $"{_rootPath}Update.bat";
                var destUnRarFile = $"{_rootPath}UnRAR.exe";
                File.Copy(sourceBatchFile, destBatchFile, true);
                File.Copy(sourceUnRarFile, destUnRarFile, true);
                File.WriteAllText("Version.txt", Utilities.GetDateModified());
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        public async Task<bool> CheckForUpdates()
        {
            try
            {
                var taskGetVersion = await FtpClient.OpenReadAsync(_host, _user, _password, _remoteVersionPath);
                if (taskGetVersion != string.Empty)
                {
                    var ftpVersion = DateTime.ParseExact(taskGetVersion, "dddd, dd MMMM yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
                    var localVersion = DateTime.ParseExact(Utilities.GetDateModified(), "dddd, dd MMMM yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
                    if (ftpVersion > localVersion)
                    {
                        return AppUi.ShowMessage("Download and install.", MessageBoxImage.Information) == MessageBoxResult.OK;
                    }
                    else
                    {
                        AppUi.ShowMessage("This is the latest version of software.", MessageBoxImage.Information);
                        return false;
                    }
                }
                else
                {
                    AppUi.ShowMessage("Unable to check for update.", MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return false;
            }
        }

        //public async Task<bool> CheckForUpdates()
        //{
        //    try
        //    {
        //        var taskGetVersion = await FtpClient.OpenReadAsync(_host, _user, _password, _remoteVersionPath);
        //        if (taskGetVersion != string.Empty)
        //        {
        //            var ftpVersion = DateTime.ParseExact(taskGetVersion, "dddd, dd MMMM yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
        //            var localVersion = DateTime.ParseExact(Utilities.GetDateModified(), "dddd, dd MMMM yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
        //            return ftpVersion > localVersion;
        //        }
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Instance.Write(ex.StackTrace);
        //        return false;
        //    }
        //}

        public async Task<bool> DownloadAndInstall()
        {
            try
            {
                var taskDownload = await FtpClient.DownloadFileAsync(_host, _user, _password, _remoteUpdatePath, _localUpdatePath);
                if (taskDownload == TaskResult.Succeeded)
                {
                    Process.Start("Update.bat", AppDomain.CurrentDomain.BaseDirectory);
                    return true;
                }
                else
                {
                    Console.WriteLine("Unable to install update.");
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return false;
            }
        }
    }
}
