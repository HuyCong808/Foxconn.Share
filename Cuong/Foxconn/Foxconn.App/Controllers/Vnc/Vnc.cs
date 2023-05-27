using Foxconn.App.Helper;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Foxconn.App.Controllers.Vnc
{
    public class Vnc
    {
        [DllImport("user32", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool bRepaint = true);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

        public int Index
        {
            get => _index;
            set => _index = value;
        }
        public string Alias
        {
            get => _alias;
            set => _alias = value;
        }
        public string Host
        {
            get => _host;
            set => _host = value;
        }
        public string Password
        {
            get => _password;
            set => _password = value;
        }
        public string FilePath
        {
            get => _filePath;
            set => _filePath = value;
        }
        public string ConfigFilePath
        {
            get => _configFilePath;
            set => _configFilePath = value;
        }
        public int Width
        {
            get => _width;
            set => _width = value;
        }
        public int Height
        {
            get => _height;
            set => _height = value;
        }
        private int _index { get; set; }
        private string _alias { get; set; }
        private string _host { get; set; }
        private string _password { get; set; }
        private string _filePath { get; set; }
        private string _configFilePath { get; set; }
        private int _width { get; set; }
        private int _height { get; set; }

        public async Task Start()
        {
            var hwnd = FindVncViewer(_host);
            if (hwnd != IntPtr.Zero)
            {
                ShowWindow(hwnd, 1);
                SetForegroundWindow(hwnd);
                SetWindowsOnDesktop(hwnd, _index, null);
            }
            else
            {
                var processStartInfo = new ProcessStartInfo()
                {
                    FileName = $"{_filePath}",
                    Arguments = $"-config \"{_configFilePath}\"",
                };
                var process = new System.Diagnostics.Process()
                {
                    StartInfo = processStartInfo,
                };
                process.Start();
                process.WaitForInputIdle();
                await Task.Delay(2500);
                hwnd = process.MainWindowHandle;
                SetWindowsOnDesktop(hwnd, _index, process.MainWindowTitle);
            }
        }

        public async Task Stop()
        {
            var hwnd = FindVncViewer(_host);
            if (hwnd != IntPtr.Zero)
            {
                SendMessage(hwnd, 16, 0, null);
            }
            await Task.Delay(100);
        }

        public IntPtr FindVncViewer(string host)
        {
            foreach (var process in System.Diagnostics.Process.GetProcesses())
            {
                //var processName = "vncviewer";
                var processName = Path.GetFileNameWithoutExtension(_filePath);
                if (process.ProcessName.Contains(processName))
                {
                    var mainWindowTitle = process.MainWindowTitle;
                    var length = mainWindowTitle.IndexOf(" ");
                    if (length > 0 && mainWindowTitle.Contains(host))
                        return process.MainWindowHandle;
                }
            }
            return IntPtr.Zero;
        }

        public void SetWindowsOnDesktop(IntPtr hwnd, int index, string title = null)
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
                        x = System.Windows.Forms.SystemInformation.WorkingArea.Size.Width / 2 - _width / 2;
                        y = System.Windows.Forms.SystemInformation.WorkingArea.Size.Height / 2 - _height / 2;
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

        public static string EncryptVNC(string password)
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

        public static byte[] ToByteArray(string hexString)
        {
            int numberChars = hexString.Length;
            byte[] bytes = new byte[numberChars / 2];

            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return bytes;
        }

        public static string DecryptVNC(string password)
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

        public void CreatConfigurationFile(string path, string host, string password)
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
    }
}
