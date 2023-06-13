using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Foxconn.Editor
{
    public class SLMP
    {
        private readonly object _lockObj = new object();
        private const int _writeTimeout = 500;
        private const int _readTimeout = 500;
        private const int _delay = 10;
        private const int _times = 10;
        protected string _host { get; set; }
        protected int _port { get; set; }
        private string _subHeader { get; set; }
        private string _network { get; set; }
        private string _station { get; set; }
        private string _moduleio { get; set; }
        private string _multidrop { get; set; }
        private string _reserved { get; set; }
        private string _write { get; set; }
        private string _read { get; set; }
        private string _bit { get; set; }
        private string _word { get; set; }
        private string _listsp { get; set; }
        private DateTime _dateTime { get; set; }

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

        public SLMP()
        {
            Init();
        }

        private void Init()
        {
            _subHeader = "5000";
            _network = "00";
            _station = "FF";
            _moduleio = "03FF";
            _multidrop = "00";
            _reserved = "0010";
            _write = "1401";
            _read = "0401";
            _bit = "0001";
            _word = "0000";
            _listsp = "XYMZDLFVBSW";
            _dateTime = DateTime.Now;
        }
        /// <summary>
        /// Ping to device
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public int Ping(string host, int port)
        {
            try
            {
                if (host != null && port <= 0)
                {
                    using (Ping ping = new Ping())
                    {
                        PingReply pingReply = ping.Send(host);
                        if (pingReply.Status == IPStatus.Success)
                            return 1;
                    }
                }
                else if (host != null && port > 0)
                {
                    using (TcpClient tcpClient = new TcpClient())
                    {
                        tcpClient.SendTimeout = _writeTimeout;
                        tcpClient.ReceiveTimeout = _readTimeout;
                        if (tcpClient.ConnectAsync(host, port).Wait(500))
                            return 1;
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
            finally
            {
                _dateTime = DateTime.Now;
            }
        }
        /// <summary>
        /// Send command
        /// </summary>
        /// <param name="command">Command string</param>
        /// <param name="result">result</param>
        /// <returns>0: Success, -1: Fail</returns>
        private int SendCommand(string command, ref string result)
        {
            try
            {
                lock (_lockObj)
                {
                    int elapsed = DateTime.Now.Subtract(_dateTime).Milliseconds;
                    if (elapsed < _delay)
                    {
                        Thread.Sleep(_delay - elapsed);
                    }

                    using (TcpClient tcpClient = new TcpClient(_host, _port))
                    {
                        tcpClient.SendTimeout = _writeTimeout;
                        tcpClient.ReceiveTimeout = _readTimeout;
                        using (NetworkStream stream = tcpClient.GetStream())
                        {
                            byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                            stream.Write(commandBytes, 0, commandBytes.Length);
                            byte[] buff = new byte[1024];
                            stream.Read(buff, 0, 1024);
                            string buffer = Encoding.UTF8.GetString(buff);
                            result = buffer.Trim(new char[] { '\0' });
                        }
                    }
                    return 0;
                }
            }
            catch (Exception)
            {
                return -1;
            }
            finally
            {
                _dateTime = DateTime.Now;
            }
        }
        /// <summary>
        /// Convert 32
        /// </summary>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        private int Convert32(int low, int high)
        {
            return high << 16 | low;
        }
        /// <summary>
        /// Get value from device
        /// </summary>
        /// <param name="device"></param>
        /// <param name="value"></param>
        /// <param name="times"></param>
        /// <returns>return number of device want get value, -1: Fail</returns>
        public int GetBitDevice(string device, ref int value)
        {
            try
            {
                string acommand = string.Empty;
                string ccommand = string.Empty;
                string bcommand = string.Empty;
                if (_listsp.Contains(device[..1].ToUpper()))
                {
                    acommand = _subHeader + _network + _station + _moduleio + _multidrop;//5000 + 00 + FF + 03FF +00;
                    ccommand = "D000" + _network + _station + _moduleio + _multidrop;
                    int length = 24;
                    if (device[..1].ToUpper() is "X" or "Y")
                    {
                        bcommand = acommand + length.ToString("X4") + _reserved + _read + _bit
                                 + device[..1].ToUpper() + (char)0x2A
                                 + Convert.ToInt32(device[1..], 8).ToString("X6") + "0001";
                    }
                    else if (device[..1].ToUpper() is "D" or "W")
                    {
                        bcommand = acommand + length.ToString("X4") + _reserved + _read + _word
                                 + device[..1].ToUpper() + (char)0x2A
                                 + Convert.ToInt32(device[1..].ToUpper()).ToString("D6") + "0001";
                    }
                    else
                    {
                        bcommand = acommand + length.ToString("X4") + _reserved + _read + _bit
                                 + device[..1].ToUpper() + (char)0x2A
                                 + Convert.ToInt32(device[1..].ToUpper()).ToString("D6") + "0001";
                    }

                    string result = string.Empty;
                    int status = SendCommand(bcommand, ref result);
                    if (status == -1 || result == string.Empty)
                        return -1;
                    if (Convert.ToInt32(result.Substring(ccommand.Length + 4, 4), 16) == 0)
                        value = Convert.ToInt32(result[(ccommand.Length + 8)..], 16);
                }
                return 1;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// Get value from 32 bit register
        /// </summary>
        /// <param name="device"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int ReadDeviceBlock(string device, ref int value)
        {
            int lowValue = -1;
            int highValue = -1;
            string lowDevice = device;
            string highDevice = $"{device[..1]}{Convert.ToInt32(device[1..]) + 1}";
            int lowRet = -1;
            int hightRet = -1;
            for (int i = 0; i < _times; i++)
            {
                if (lowRet == -1)
                    lowRet = GetBitDevice(lowDevice, ref lowValue);
                if (hightRet == -1)
                    hightRet = GetBitDevice(highDevice, ref highValue);
                if (lowRet == 1 && hightRet == 1)
                {
                    value = Convert32(lowValue, highValue);
                    return 1;
                }
            }
            return -1;
        }
        /// <summary>
        /// Get value from 32 bit register
        /// </summary>
        /// <example>
        /// Example for get 32 bit device
        /// <code>
        /// int result = ReadDeviceBlock("D8340", "D8341", ref value);
        /// </code>
        /// </example>
        /// <param name="lowDevice"></param>
        /// <param name="highDevice"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int ReadDeviceBlock(string lowDevice, string highDevice, ref int value)
        {
            int lowValue = -1;
            int highValue = -1;
            int lowRet = -1;
            int hightRet = -1;
            for (int i = 0; i < _times; i++)
            {
                if (lowRet == -1)
                    lowRet = GetBitDevice(lowDevice, ref lowValue);
                if (hightRet == -1)
                    hightRet = GetBitDevice(highDevice, ref highValue);
                if (lowRet == 1 && hightRet == 1)
                {
                    value = Convert32(lowValue, highValue);
                    return 1;
                }
            }
            return -1;
        }
        /// <summary>
        /// Set value to device
        /// </summary>
        /// <param name="device">Register of PLC</param>
        /// <param name="value">16bit value</param>
        /// <param name="times"></param>
        /// <returns></returns>
        public int SetBitDevice(string device, int value)
        {
            try
            {
                string acommand = string.Empty;
                string ccommand = string.Empty;
                string bcommand = string.Empty;
                int length = 24;
                if (_listsp.Contains(device[..1].ToUpper()))
                {
                    acommand = _subHeader + _network + _station + _moduleio + _multidrop;
                    ccommand = "D000" + _network + _station + _moduleio + _multidrop;
                    if (device[..1].ToUpper() is "X" or "Y")
                    {
                        length += 1;
                        bcommand = acommand + length.ToString("X4") + _reserved + _write + _bit
                                 + device[..1].ToUpper() + (char)0x2A
                                 + Convert.ToInt32(device[1..], 8).ToString("X6")
                                 + "0001" + value.ToString("X1");
                    }
                    else if (device[..1].ToUpper() is "D" or "W")
                    {
                        if (value < 655535 && value >= 0)
                        {
                            length += 4;
                            bcommand = acommand + length.ToString("X4") + _reserved + _write + _word
                                     + device[..1].ToUpper() + (char)0x2A
                                     + Convert.ToInt32(device[1..].ToUpper()).ToString("D6")
                                     + "0001" + value.ToString("X4");
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        if (value == 1 || value == 0)
                        {
                            length += 1;
                            bcommand = acommand + length.ToString("X4") + _reserved + _write + _bit
                                     + device[..1].ToUpper() + (char)0x2A
                                     + Convert.ToInt32(device[1..].ToUpper()).ToString("D6")
                                     + "0001" + value.ToString("X1");
                        }
                        else
                        {
                            return -1;
                        }
                    }

                    int i = 0;
                    do
                    {
                        try
                        {
                            int status = -1;
                            string result = string.Empty;
                            status = SendCommand(bcommand, ref result);
                            if (result.Length > ccommand.Length + 4)
                            {
                                if (Convert.ToInt32(result[(ccommand.Length + 4)..]) == 0)
                                    return 1;
                            }
                        }
                        catch (Exception)
                        {

                        }
                        i++;
                    } while (i < _times);
                    return -1;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// Set value for 32 bit register
        /// </summary>
        /// <param name="device"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int WriteDeviceBlock(string device, int value)
        {
            string lowDevice = device;
            string highDevice = $"{device[..1]}{Convert.ToInt32(device[1..]) + 1}";
            ushort highValue = (ushort)(value >> 16);
            ushort lowValue = (ushort)(value & 0xffff);
            int lowRet = -1;
            int highRet = -1;
            for (int i = 0; i < _times; i++)
            {
                if (lowRet == -1)
                    lowRet = SetBitDevice(lowDevice, lowValue);
                if (highRet == -1)
                    highRet = SetBitDevice(highDevice, highValue);
                if (lowRet == 1 && highRet == 1)
                    return 1;
            }
            return -1;
        }
        /// <summary>
        /// Set value for 32 bit register
        /// </summary>
        /// <example>
        /// Example for get 32 bit device
        /// <code>
        /// int result = WriteDeviceBlock("D8340", "D8341", value);
        /// </code>
        /// </example>
        /// <param name="lowDevice"></param>
        /// <param name="highDevice"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int WriteDeviceBlock(string lowDevice, string highDevice, int value)
        {
            ushort highValue = (ushort)(value >> 16);
            ushort lowValue = (ushort)(value & 0xffff);
            int lowRet = -1;
            int highRet = -1;
            for (int i = 0; i < _times; i++)
            {
                if (lowRet == -1)
                    lowRet = SetBitDevice(lowDevice, lowValue);
                if (highRet == -1)
                    highRet = SetBitDevice(highDevice, highValue);
                if (lowRet == 1 && highRet == 1)
                    return 1;
            }
            return -1;
        }
        /// <summary>
        /// Run
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public int Run()
        {
            try
            {
                string acommand = string.Empty;
                string ccommand = string.Empty;
                string bcommand = string.Empty;
                int length = 20;
                acommand = _subHeader + _network + _station + _moduleio + _multidrop;
                ccommand = "D000" + _network + _station + _moduleio + _multidrop;
                bcommand = acommand + length.ToString("X4") + _reserved + "1001000000030000";

                int i = 0;
                do
                {
                    try
                    {
                        int status = -1;
                        string result = string.Empty;
                        status = SendCommand(bcommand, ref result);
                        if (result.Length > ccommand.Length + 4)
                        {
                            if (Convert.ToInt32(result[(ccommand.Length + 4)..]) == 0)
                                return 1;
                        }
                    }
                    catch (Exception)
                    {

                    }
                    i++;
                } while (i < _times);
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// Pause
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public int Pause()
        {
            try
            {
                string acommand = string.Empty;
                string ccommand = string.Empty;
                string bcommand = string.Empty;
                int length = 16;
                acommand = _subHeader + _network + _station + _moduleio + _multidrop;
                ccommand = "D000" + _network + _station + _moduleio + _multidrop;
                bcommand = acommand + length.ToString("X4") + _reserved + "100300000003";

                int i = 0;
                do
                {
                    try
                    {
                        int status = -1;
                        string result = string.Empty;
                        status = SendCommand(bcommand, ref result);
                        if (result.Length > ccommand.Length + 4)
                        {
                            if (Convert.ToInt32(result[(ccommand.Length + 4)..]) == 0)
                                return 1;
                        }
                    }
                    catch (Exception)
                    {

                    }
                    i++;
                } while (i < _times);
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// Reset
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public int Reset()
        {
            try
            {
                string acommand = string.Empty;
                string ccommand = string.Empty;
                string bcommand = string.Empty;
                int length = 16;
                acommand = _subHeader + _network + _station + _moduleio + _multidrop;
                ccommand = "D000" + _network + _station + _moduleio + _multidrop;
                bcommand = acommand + length.ToString("X4") + _reserved + "100600000001";

                int i = 0;
                do
                {
                    try
                    {
                        int status = -1;
                        string result = string.Empty;
                        status = SendCommand(bcommand, ref result);
                        if (result.Length > ccommand.Length + 4)
                        {
                            if (Convert.ToInt32(result[(ccommand.Length + 4)..]) == 0)
                                return 1;
                        }
                    }
                    catch (Exception)
                    {

                    }
                    i++;
                } while (i < _times);
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public int Stop()
        {
            try
            {
                string acommand = string.Empty;
                string ccommand = string.Empty;
                string bcommand = string.Empty;
                int length = 16;
                acommand = _subHeader + _network + _station + _moduleio + _multidrop;
                ccommand = "D000" + _network + _station + _moduleio + _multidrop;
                bcommand = acommand + length.ToString("X4") + _reserved + "100200000001";

                int i = 0;
                do
                {
                    try
                    {
                        int status = -1;
                        string result = string.Empty;
                        status = SendCommand(bcommand, ref result);
                        if (result.Length > ccommand.Length + 4)
                        {
                            if (Convert.ToInt32(result[(ccommand.Length + 4)..]) == 0)
                                return 1;
                        }
                    }
                    catch (Exception)
                    {

                    }
                    i++;
                } while (i < _times);
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public int GetFlag(string device)
        {
            int value = -1;
            int nRet = GetBitDevice(device, ref value);
            if (value == 1)
            {
                Console.WriteLine($"Get {device}: {value}");
                nRet = SetBitDevice(device, 0);
                if (nRet == 1)
                {
                    Trace.WriteLine($"Set {device}: 0");
                }
                else
                {
                    Trace.WriteLine($"Set {device}: {value} (ERROR)");
                }
                return nRet;
            }
            return -1;
        }

        public int GetDevice(string device, ref int value, bool saveLogs = false)
        {
            int nRet = GetBitDevice(device, ref value);
            if (saveLogs)
            {
                Trace.WriteLine($"Get {device}: {value}");
            }
            return nRet;
        }

        public int GetDevice32Bit(string device, ref int value, bool saveLogs = false)
        {
            int nRet = ReadDeviceBlock(device, ref value);
            if (saveLogs)
            {
                Trace.WriteLine($"Get {device}: {value}");
            }
            return nRet;
        }

        public int SetDevice(string device, int value)
        {
            int nRet = SetBitDevice(device, value);
            if (nRet == 1)
            {
                Trace.WriteLine($"Set {device}: {value}");
            }
            else
            {
                Trace.WriteLine($"Set {device}: {value} (ERROR)");
            }
            return nRet;
        }

        public int SetDevice32Bit(string device, int value)
        {
            int nRet = WriteDeviceBlock(device, value);
            if (nRet == 1)
            {
                Trace.WriteLine($"Set {device}: {value}");
            }
            else
            {
                Trace.WriteLine($"Set {device}: {value} (ERROR)");
            }
            return nRet;
        }

        public void LogInfo(string message)
        {
            Logger.Current.Info(message);
        }

        public void LogError(string message)
        {
            Logger.Current.Error(message);
        }

    }
}
