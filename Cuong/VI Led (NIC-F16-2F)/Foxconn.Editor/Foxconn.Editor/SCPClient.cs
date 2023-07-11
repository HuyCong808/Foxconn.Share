using System;
using System.Diagnostics;
using System.IO;
using WinSCP;

namespace Foxconn.Editor
{
    public class SCPClient
    {
        private Protocol _protocol = Protocol.Sftp;
        private string _host = "10.220.145.202";
        private int _port = 4422;
        private string _user = "at";
        private string _password = "foxconnat";
        private string _key = "ssh-rsa 1024 8uZSgC2DMdG5JN5I0SJh0qb8/iGkqvnYT9Jz062zwEE=";

        public Protocol Protocol
        {
            get => _protocol;
            set => _protocol = value;
        }

        public string Host
        {
            get => _host;
            set => _host = value;
        }

        public int Port
        {
            get => _port;
            set => _port = value;
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

        public string Key
        {
            get => _key;
            set => _key = value;
        }

        public int Ping()
        {
            try
            {
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = _protocol,
                    HostName = _host,
                    PortNumber = _port,
                    UserName = _user,
                    Password = _password,
                    SshHostKeyFingerprint = _key
                };

                using (Session session = new Session())
                {
                    // Connect
                    session.Timeout = TimeSpan.FromMilliseconds(60000);
                    session.Open(sessionOptions);
                    return session.Opened ? 1 : 0;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error: {0}", e.ToString());
                return -1;
            }
        }

        public int DownloadFile(string localPath, string remotePath)
        {
            try
            {
                // Setup session options
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = _protocol,
                    HostName = _host,
                    PortNumber = _port,
                    UserName = _user,
                    Password = _password,
                    SshHostKeyFingerprint = _key
                };

                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    // Download files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;

                    TransferOperationResult transferResult;
                    transferResult = session.GetFiles(remotePath, localPath, false, transferOptions);
                    //transferResult = session.GetFiles("/home/user/test.txt", @"d:\download\", false, transferOptions);

                    // Throw on any error
                    transferResult.Check();

                    // Print results
                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        Trace.WriteLine($"Download of {transfer.FileName} succeeded");
                    }
                }

                return 1;
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error: {0}", e.ToString());
                return -1;
            }
        }

        public int DownloadDirectory(string localPath, string remotePath)
        {
            try
            {
                // Setup session options
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = _protocol,
                    HostName = _host,
                    PortNumber = _port,
                    UserName = _user,
                    Password = _password,
                    SshHostKeyFingerprint = _key
                };

                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    // Download files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;

                    TransferOperationResult transferResult;
                    transferResult = session.GetFiles(Path.Combine(remotePath, "*"), localPath, false, transferOptions);
                    //transferResult = session.GetFiles("/home/user/*", @"d:\download\", false, transferOptions);

                    // Throw on any error
                    transferResult.Check();

                    // Print results
                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        Trace.WriteLine($"Download of {transfer.FileName} succeeded");
                    }
                }

                return 1;
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error: {0}", e.ToString());
                return -1;
            }
        }

        public int UploadFile(string localPath, string remotePath)
        {
            try
            {
                // Setup session options
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = _protocol,
                    HostName = _host,
                    PortNumber = _port,
                    UserName = _user,
                    Password = _password,
                    SshHostKeyFingerprint = _key
                };

                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    // Check if directory exists
                    string path = string.Empty;
                    string[] dirs = remotePath.Split("/");
                    foreach (string dir in dirs)
                    {
                        if (dir.Length > 0)
                        {
                            path = "/" + Path.Combine(path, dir) + "/";
                            if (!session.FileExists(path))
                            {
                                session.CreateDirectory(path);
                            }
                        }
                    }

                    // Upload files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;

                    TransferOperationResult transferResult;
                    transferResult = session.PutFiles(localPath, remotePath, false, transferOptions);
                    //transferResult = session.PutFiles(@"d:\toupload\test.txt", "/home/user/", false, transferOptions);

                    // Throw on any error
                    transferResult.Check();

                    // Print results
                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        Trace.WriteLine($"Upload of {transfer.FileName} succeeded");
                    }
                }

                return 1;
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error: {0}", e.ToString());
                return -1;
            }
        }

        public int UploadDirectory(string localPath, string remotePath)
        {
            try
            {
                // Setup session options
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = _protocol,
                    HostName = _host,
                    PortNumber = _port,
                    UserName = _user,
                    Password = _password,
                    SshHostKeyFingerprint = _key
                };

                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    // Check if directory exists
                    string path = string.Empty;
                    string[] dirs = remotePath.Split("/");
                    foreach (string dir in dirs)
                    {
                        if (dir.Length > 0)
                        {
                            path = "/" + Path.Combine(path, dir) + "/";
                            if (!session.FileExists(path))
                            {
                                session.CreateDirectory(path);
                            }
                        }
                    }

                    // Upload files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;

                    TransferOperationResult transferResult;
                    transferResult = session.PutFiles(Path.Combine(localPath, "*"), remotePath, false, transferOptions);
                    //transferResult = session.PutFiles(@"d:\toupload\*", "/home/user/", false, transferOptions);

                    // Throw on any error
                    transferResult.Check();

                    // Print results
                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        Trace.WriteLine($"Upload of {transfer.FileName} succeeded");
                    }
                }

                return 1;
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error: {0}", e.ToString());
                return -1;
            }
        }
    }
}
