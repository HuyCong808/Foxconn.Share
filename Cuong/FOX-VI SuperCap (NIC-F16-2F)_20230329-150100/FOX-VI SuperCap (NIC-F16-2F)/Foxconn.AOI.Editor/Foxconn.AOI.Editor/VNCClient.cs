using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Foxconn.AOI.Editor
{
    public class VNCClient
    {
        [DllImport("user32", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool bRepaint = true);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

        private string _host = string.Empty;
        private string _password = "123456";
        private string _filename = "VNC-Viewer-6.0.0-Windows-32bit.exe";
        private string _configFile = "VNC.ini";
        private int _width = SystemInformation.WorkingArea.Size.Width / 4;
        private int _height = SystemInformation.WorkingArea.Size.Height / 3;

        public static void RegistryKeyVNC()
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\RealVNC\\vncviewer");
            if (registryKey != null)
            {
                registryKey.SetValue("EulaAccepted", "495139c2b98f2ccd1c353c891250ec12be4f91e4");
                registryKey.Close();
            }
        }

        public int Open(string host, string password, int index)
        {
            try
            {
                _host = host;
                _password = password;
                IntPtr hwnd = FindVNCClient(_host);
                if (hwnd != IntPtr.Zero)
                {
                    ShowWindow(hwnd, 1);
                    SetForegroundWindow(hwnd);
                    SetWindowsOnDesktop(hwnd, index, null);
                    return 1;
                }
                else
                {
                    _filename = IntPtr.Size == 4 ? "VNC-Viewer-6.0.0-Windows-32bit.exe" : "VNC-Viewer-6.0.0-Windows-64bit.exe";
                    _configFile = $"VNC{index}.ini";
                    CreatConfigurationFile(host, password, _configFile);
                    if (File.Exists(_filename) && File.Exists(_configFile))
                    {
                        Process process = new Process()
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = $"{_filename}",
                                Arguments = $"-config \"{_configFile}\""
                            },
                        };
                        process.Start();
                        process.WaitForInputIdle();
                        Thread.Sleep(3000);
                        hwnd = process.MainWindowHandle;
                        SetWindowsOnDesktop(hwnd, index, process.MainWindowTitle);
                        Trace.WriteLine($"VNCClient.Open ({_host}:{_password}): Opened");
                        return 1;
                    }
                    else
                    {
                        Trace.WriteLine($"VNCClient.Open ({_host}:{_password}): Can not found {_filename} or {_configFile}");
                        Trace.WriteLine($"VNCClient.Open ({_host}:{_password}): Can not opened");
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }

        public int Close()
        {
            try
            {
                IntPtr hwnd = FindVNCClient(_host);
                if (hwnd != IntPtr.Zero)
                {
                    SendMessage(hwnd, 16, 0, null);
                    Trace.WriteLine($"VNCClient.Close ({_host}:{_password}): Closed");
                    return 1;
                }
                else
                {
                    Trace.WriteLine($"VNCClient.Close ({_host}:{_password}): Can not closed");
                }
                return 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }

        public IntPtr FindVNCClient(string host)
        {
            foreach (var process in Process.GetProcesses())
            {
                var processName = Path.GetFileNameWithoutExtension(_filename);
                if (process.ProcessName.Contains(processName))
                {
                    var mainWindowTitle = process.MainWindowTitle;
                    var length = mainWindowTitle.IndexOf(" ");
                    if (length > 0 && mainWindowTitle.Contains(host))
                    {
                        return process.MainWindowHandle;
                    }
                }
            }
            return IntPtr.Zero;
        }

        private void SetWindowsOnDesktop(IntPtr hwnd, int index, string title = null)
        {
            int x;
            int y;
            switch (index)
            {
                case 0:
                case 12:
                    {
                        x = 0;
                        y = 0;
                        break;
                    }
                case 1:
                case 13:
                    {
                        x = _width;
                        y = 0;
                        break;
                    }
                case 2:
                case 14:
                    {
                        x = _width * 2;
                        y = 0;
                        break;
                    }
                case 3:
                case 15:
                    {
                        x = _width * 3;
                        y = 0;
                        break;
                    }
                case 4:
                case 16:
                    {
                        x = 0;
                        y = _height;
                        break;
                    }
                case 5:
                case 17:
                    {
                        x = _width;
                        y = _height;
                        break;
                    }
                case 6:
                case 18:
                    {
                        x = _width * 2;
                        y = _height;
                        break;
                    }
                case 7:
                case 19:
                    {
                        x = _width * 3;
                        y = _height;
                        break;
                    }
                case 8:
                case 20:
                    {
                        x = 0;
                        y = _height * 2;
                        break;
                    }
                case 9:
                case 21:
                    {
                        x = _width;
                        y = _height * 2;
                        break;
                    }
                case 10:
                case 22:
                    {
                        x = _width * 2;
                        y = _height * 2;
                        break;
                    }
                case 11:
                case 23:
                    {
                        x = _width * 3;
                        y = _height * 2;
                        break;
                    }
                default:
                    {
                        x = SystemInformation.WorkingArea.Size.Width / 2 - _width / 2;
                        y = SystemInformation.WorkingArea.Size.Height / 2 - _height / 2;
                        break;
                    }
            }
            MoveWindow(hwnd, x, y, _width, _height, true);
            if (title == null)
                return;
            SendMessage(hwnd, 12, 0, title.Replace("(", "").Replace(")", "").Replace("- VNC Viewer", ""));
            var number = title.IndexOf("(");
            if (number <= 0)
                return;
        }

        private void CreatConfigurationFile(string host, string password, string path = "VNC.ini")
        {
            if (File.Exists(path))
            {
                var iniFile = new INIFile(path);
                iniFile.Write("Connection", "Host", host);
                iniFile.Write("Connection", nameof(password), EncryptVNC(password));
            }
            else
            {
                var streamWriter = new StreamWriter(path);
                streamWriter.WriteLine("[Connection]");
                streamWriter.WriteLine($"Host={host}");
                streamWriter.WriteLine($"Password={EncryptVNC(password)}");
                streamWriter.WriteLine("Encryption=Server");
                streamWriter.WriteLine("SecurityNotificationTimeout=2500");
                streamWriter.WriteLine("SingleSignOn=1");
                streamWriter.WriteLine("[Options]");
                streamWriter.WriteLine("UseLocalCursor=1");
                streamWriter.WriteLine("FullScreen=0");
                streamWriter.WriteLine("RelativePtr=0");
                streamWriter.WriteLine("FullColour=0");
                streamWriter.WriteLine("ColourLevel=pal8");
                streamWriter.WriteLine("PreferredEncoding=ZRLE");
                streamWriter.WriteLine("AutoSelect=1");
                streamWriter.WriteLine("Shared=1");
                streamWriter.WriteLine("SendPointerEvents=1");
                streamWriter.WriteLine("SendKeyEvents=1");
                streamWriter.WriteLine("ClientCutText=1");
                streamWriter.WriteLine("ServerCutText=1");
                streamWriter.WriteLine("ShareFiles=1");
                streamWriter.WriteLine("EnableChat=0");
                streamWriter.WriteLine("EnableRemotePrinting=0");
                streamWriter.WriteLine("ChangeServerDefaultPrinter=0");
                streamWriter.WriteLine("PointerEventInterval=0");
                streamWriter.WriteLine("PointerCornerSnapThreshold=30");
                streamWriter.WriteLine("Scaling=Fit");
                streamWriter.WriteLine("MenuKey=F8");
                streamWriter.WriteLine("EnableToolbar=0");
                streamWriter.WriteLine("AutoReconnect=1");
                streamWriter.WriteLine("Protocol3.3=0");
                streamWriter.WriteLine("AcceptBell=1");
                streamWriter.WriteLine("ScalePrintOutput=1");
                streamWriter.WriteLine("VerifyId=0");
                streamWriter.WriteLine("WarnUnencrypted=1");
                streamWriter.WriteLine("DotWhenNoCursor=1");
                streamWriter.WriteLine("FullScreenChangeResolution=0");
                streamWriter.WriteLine("UseAllMonitors=0");
                streamWriter.Close();
            }
        }

        private string EncryptVNC(string password)
        {
            if (password.Length > 8)
            {
                password = password.Substring(0, 8);
            }
            if (password.Length < 8)
            {
                password = password.PadRight(8, '\0');
            }

            byte[] key = { 23, 82, 107, 6, 35, 78, 88, 7 };
            byte[] passwordArray = new ASCIIEncoding().GetBytes(password);
            byte[] response = new byte[passwordArray.Length];
            char[] chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

            // Reverse the byte order
            byte[] newKey = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                // Revert desKey[i]:
                newKey[i] = (byte)(
                    ((key[i] & 0x01) << 7) |
                    ((key[i] & 0x02) << 5) |
                    ((key[i] & 0x04) << 3) |
                    ((key[i] & 0x08) << 1) |
                    ((key[i] & 0x10) >> 1) |
                    ((key[i] & 0x20) >> 3) |
                    ((key[i] & 0x40) >> 5) |
                    ((key[i] & 0x80) >> 7)
                    );
            }
            key = newKey;
            // Reverse the byte order

            DES des = new DESCryptoServiceProvider
            {
                Padding = PaddingMode.None,
                Mode = CipherMode.ECB
            };


            ICryptoTransform enc = des.CreateEncryptor(key, null);
            enc.TransformBlock(passwordArray, 0, passwordArray.Length, response, 0);

            string hexString = string.Empty;
            for (int i = 0; i < response.Length; i++)
            {
                hexString += chars[response[i] >> 4];
                hexString += chars[response[i] & 0xf];
            }
            return hexString.Trim().ToLower();
        }

        private byte[] ToByteArray(string hexString)
        {
            int numberChars = hexString.Length;
            byte[] bytes = new byte[numberChars / 2];

            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return bytes;
        }

        private string DecryptVNC(string password)
        {
            if (password.Length < 16)
            {
                return string.Empty;
            }

            byte[] key = { 23, 82, 107, 6, 35, 78, 88, 7 };
            byte[] passwordArray = ToByteArray(password);
            byte[] response = new byte[passwordArray.Length];

            // Reverse the byte order
            byte[] newKey = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                // Revert key[i]:
                newKey[i] = (byte)
                (
                    ((key[i] & 0x01) << 7) |
                    ((key[i] & 0x02) << 5) |
                    ((key[i] & 0x04) << 3) |
                    ((key[i] & 0x08) << 1) |
                    ((key[i] & 0x10) >> 1) |
                    ((key[i] & 0x20) >> 3) |
                    ((key[i] & 0x40) >> 5) |
                    ((key[i] & 0x80) >> 7)
                );
            }
            key = newKey;
            // Reverse the byte order

            DES des = new DESCryptoServiceProvider
            {
                Padding = PaddingMode.None,
                Mode = CipherMode.ECB
            };

            ICryptoTransform dec = des.CreateDecryptor(key, null);
            dec.TransformBlock(passwordArray, 0, passwordArray.Length, response, 0);

            return Encoding.ASCII.GetString(response);
        }

        public class INIFile
        {
            [DllImport("kernel32", CharSet = CharSet.Unicode)]
            private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

            [DllImport("kernel32", CharSet = CharSet.Unicode)]
            private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

            private string _filePath;

            public string FilePath
            {
                get => _filePath;
                set => _filePath = value;
            }

            public INIFile(string filePath)
            {
                _filePath = filePath;
            }

            public void Write(string section, string key, string value)
            {
                WritePrivateProfileString(section, key, value, _filePath);
            }

            public string Read(string section, string key, string defaultData = "")
            {
                StringBuilder retVal = new StringBuilder(byte.MaxValue);
                GetPrivateProfileString(section, key, "", retVal, byte.MaxValue, _filePath);
                string str = retVal.ToString();
                return !(str != "") ? defaultData : str;
            }
        }
    }
}
