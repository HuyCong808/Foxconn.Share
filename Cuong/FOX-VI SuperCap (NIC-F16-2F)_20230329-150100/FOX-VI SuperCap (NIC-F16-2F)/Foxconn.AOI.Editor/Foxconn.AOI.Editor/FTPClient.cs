using FluentFTP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Foxconn.AOI.Editor
{
    public class FTPClient
    {
        private string _host = string.Empty;
        private string _user = string.Empty;
        private string _password = string.Empty;

        public string Host
        {
            get => _host;
            set => _host = value;
        }

        public string User
        {
            get => _user;
            set => _user = value;
        }

        public string Password
        {
            get => _password;
            set => _password = value;
        }

        public int Ping()
        {
            try
            {
                using (FtpClient ftp = new FtpClient(_host, _user, _password))
                {
                    ftp.Connect();
                    if (ftp.IsConnected && ftp.IsAuthenticated)
                    {
                        return 1;
                    }
                }
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        public int DownloadFile(string localFile, string remoteFile)
        {
            try
            {
                using (FtpClient ftp = new FtpClient(_host, _user, _password))
                {
                    ftp.Connect();
                    if (ftp.IsConnected && ftp.IsAuthenticated)
                    {
                        FtpStatus result = ftp.DownloadFile(localFile, remoteFile, FtpLocalExists.Overwrite, FtpVerify.Retry);
                        if (result == FtpStatus.Success)
                        {
                            Trace.WriteLine($"FTPClient.DownloadFile {remoteFile} ({_host}, {_user}, {_password}): Success");
                            return 1;
                        }
                    }
                }
                Trace.WriteLine($"FTPClient.DownloadFile {remoteFile} ({_host}, {_user}, {_password}): Failed");
                return 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }

        public int DownloadDirectory(string localFolder, string remoteFolder)
        {
            try
            {
                using (FtpClient ftp = new FtpClient(_host, _user, _password))
                {
                    ftp.Connect();
                    if (ftp.IsConnected && ftp.IsAuthenticated)
                    {
                        List<FtpResult> results = ftp.DownloadDirectory(localFolder, remoteFolder, FtpFolderSyncMode.Update);
                        results.Find(x => x.IsFailed);
                        FtpResult status = results.Find(x => x.IsFailed);
                        if (status == null)
                        {
                            Trace.WriteLine($"FTPClient.DownloadDirectory {remoteFolder} ({_host}, {_user}, {_password}): Success");
                            return 1;
                        }
                    }
                }
                Trace.WriteLine($"FTPClient.DownloadDirectory {remoteFolder} ({_host}, {_user}, {_password}): Failed");
                return 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }

        public int UploadFile(string localFile, string remoteFile)
        {
            try
            {
                using (FtpClient ftp = new FtpClient(_host, _user, _password))
                {
                    ftp.Connect();
                    if (ftp.IsConnected && ftp.IsAuthenticated)
                    {
                        if (!ftp.DirectoryExists(Path.GetDirectoryName(remoteFile)))
                            ftp.CreateDirectory(Path.GetDirectoryName(remoteFile));
                        FtpStatus result = ftp.UploadFile(localFile, remoteFile, FtpRemoteExists.Overwrite, true, FtpVerify.Retry);
                        if (result == FtpStatus.Success)
                        {
                            Trace.WriteLine($"FTPClient.UploadFile {remoteFile} ({_host}, {_user}, {_password}): Success");
                            return 1;
                        }
                    }
                }
                Trace.WriteLine($"FTPClient.UploadFile {remoteFile} ({_host}, {_user}, {_password}): Failed");
                return 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }

        public int UploadDirectory(string localFolder, string remoteFolder)
        {
            try
            {
                using (FtpClient ftp = new FtpClient(_host, _user, _password))
                {
                    ftp.Connect();
                    if (ftp.IsConnected && ftp.IsAuthenticated)
                    {
                        if (!ftp.DirectoryExists(remoteFolder))
                            ftp.CreateDirectory(remoteFolder);
                        List<FtpResult> results = ftp.UploadDirectory(localFolder, remoteFolder, FtpFolderSyncMode.Update);
                        results.Find(x => x.IsFailed);
                        FtpResult status = results.Find(x => x.IsFailed);
                        if (status == null)
                        {
                            Trace.WriteLine($"FTPClient.UploadDirectory {remoteFolder} ({_host}, {_user}, {_password}): Success");
                            return 1;
                        }
                    }
                }
                Trace.WriteLine($"FTPClient.UploadDirectory {remoteFolder} ({_host}, {_user}, {_password}): Failed");
                return 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }
    }
}
