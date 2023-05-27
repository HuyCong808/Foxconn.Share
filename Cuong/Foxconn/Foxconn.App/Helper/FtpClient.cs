using FluentFTP;
using Foxconn.App.Helper.Enums;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Foxconn.App.Helper
{
    public class FtpClient
    {
        public static string OpenRead(string host, string user, string password, string path)
        {
            try
            {
                using var ftp = new FluentFTP.FtpClient(host, user, password);
                ftp.Connect();
                if (ftp.FileExists(path))
                {
                    using var istream = ftp.OpenRead(path);
                    try
                    {
                        byte[] bytes = new byte[istream.Length];
                        istream.Read(bytes, 0, (int)istream.Length);
                        string data = Encoding.ASCII.GetString(bytes);
                        return data ?? string.Empty;
                    }
                    finally
                    {
                        istream.Close();
                    }
                }
                else
                {
                    Console.WriteLine("The file not exists.");
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return string.Empty;
            }
        }

        public static async Task<string> OpenReadAsync(string host, string user, string password, string path)
        {
            try
            {
                var token = new CancellationToken();
                using var ftp = new FluentFTP.FtpClient(host, user, password);
                await ftp.ConnectAsync(token);
                if (ftp.FileExists(path))
                {
                    using var istream = await ftp.OpenReadAsync(path, token);
                    try
                    {
                        byte[] bytes = new byte[istream.Length];
                        istream.Read(bytes, 0, (int)istream.Length);
                        string data = Encoding.ASCII.GetString(bytes);
                        return data ?? string.Empty;
                    }
                    finally
                    {
                        Console.WriteLine();
                        istream.Close();
                    }
                }
                else
                {
                    Console.WriteLine("The file not exists.");
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return string.Empty;
            }
        }

        public static TaskResult DownloadFile(string host, string user, string password, string remotePath, string localPath)
        {
            try
            {
                using (var ftp = new FluentFTP.FtpClient(host, user, password))
                {
                    ftp.Connect();
                    if (ftp.IsConnected && ftp.IsAuthenticated)
                    {
                        if (ftp.FileExists(remotePath))
                        {
                            FtpStatus status = ftp.DownloadFile(localPath, remotePath, FtpLocalExists.Overwrite);
                            return status == FtpStatus.Success ? TaskResult.Succeeded : TaskResult.Failed;
                        }
                        else
                        {
                            Console.WriteLine("The file not exists.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("The FTP server has not accepted credentials.");
                    }
                }
                return TaskResult.Failed;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return TaskResult.Error;
            }
        }

        public static async Task<TaskResult> DownloadFileAsync(string host, string user, string password, string remotePath, string localPath)
        {
            try
            {
                var token = new CancellationToken();
                using (var ftp = new FluentFTP.FtpClient(host, user, password))
                {
                    await ftp.ConnectAsync(token);
                    if (ftp.FileExists(remotePath))
                    {
                        FtpStatus status = await ftp.DownloadFileAsync(localPath, remotePath, FtpLocalExists.Overwrite, FtpVerify.Retry);
                        return status == FtpStatus.Success ? TaskResult.Succeeded : TaskResult.Failed;
                    }
                    else
                    {
                        Console.WriteLine("The file not exists.");
                    }
                }
                return TaskResult.Failed;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return TaskResult.Error;
            }
        }

        public static TaskResult DownloadDirectory(string host, string user, string password, string remoteFolder, string localFolder)
        {
            try
            {
                using (var ftp = new FluentFTP.FtpClient(host, user, password))
                {
                    ftp.Connect();
                    if (ftp.IsConnected && ftp.IsAuthenticated)
                    {
                        if (ftp.DirectoryExists(remoteFolder))
                        {
                            ftp.DownloadDirectory(localFolder, remoteFolder, FtpFolderSyncMode.Update);
                            return TaskResult.Succeeded;
                        }
                        else
                        {
                            Console.WriteLine("The path of the directory not exists.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("The FTP server has not accepted credentials.");
                    }
                }
                return TaskResult.Failed;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return TaskResult.Error;
            }
        }

        public static async Task<TaskResult> DownloadDirectoryAsync(string host, string user, string password, string remoteFolder, string localFolder)
        {
            try
            {
                var token = new CancellationToken();
                using (var ftp = new FluentFTP.FtpClient(host, user, password))
                {
                    await ftp.ConnectAsync(token);
                    if (ftp.DirectoryExists(remoteFolder))
                    {
                        await ftp.DownloadDirectoryAsync(localFolder, remoteFolder, FtpFolderSyncMode.Update);
                        return TaskResult.Succeeded;
                    }
                    else
                    {
                        Console.WriteLine("The path of the directory not exists.");
                    }
                }
                return TaskResult.Failed;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return TaskResult.Error;
            }
        }
    }
}
