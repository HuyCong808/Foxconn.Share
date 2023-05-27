using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Foxconn.App.Controllers.Plc
{
    public class Slmp
    {
        /// <summary>
        /// Lock for safety thread
        /// </summary>
        private readonly object _syncObject = new object();
        private const int _delay = 10;
        private const int _writeTimeout = 500;
        private const int _readTimeout = 500;
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
        /// <summary>
        /// The last time send command to plc. It use for delay between two sending command time.
        /// </summary>
        private DateTime _dateTime { get; set; }

        public Slmp()
        {
            Initialize();
        }

        private void Initialize()
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
        /// Ping to host & port
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool IsPingSuccessAsync(string host = null, int port = 0)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    client.SendTimeout = _writeTimeout;
                    client.ReceiveTimeout = _readTimeout;
                    return client.ConnectAsync(host ?? _host, port != 0 ? port : _port).Wait(500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                _dateTime = DateTime.Now;
            }
        }
        /// <summary>
        /// Ping to host
        /// </summary>
        /// <param name="_ip"></param>
        /// <returns></returns>
        public bool IsPingSuccess(string host = null)
        {
            try
            {
                var ping = new Ping();
                var pingReply = ping.Send(host ?? _host);
                return pingReply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                _dateTime = DateTime.Now;
            }
        }
        /// <summary>
        /// Send command to PLC
        /// </summary>
        /// <param name="command">Command string</param>
        /// <param name="result">result</param>
        /// <returns>0: Success, -1: Fail</returns>
        private int SendCommand(string command, ref string result)
        {
            lock (_syncObject)
            {
                try
                {
                    var client = new TcpClient(_host, _port)
                    {
                        SendTimeout = _writeTimeout,
                        ReceiveTimeout = _readTimeout
                    };
                    var stream = client.GetStream();
                    byte[] commandBytes = Encoding.UTF8.GetBytes(command);

                    // Sleep if two times send command is smaller than DELAY time.
                    var elapsed = DateTime.Now.Subtract(_dateTime).Milliseconds;
                    if (elapsed < _delay)
                    {
                        Thread.Sleep(_delay - elapsed);
                    }

                    stream.Write(commandBytes, 0, commandBytes.Length);
                    byte[] buff = new byte[1024];
                    stream.Read(buff, 0, 1024);
                    stream.Close();
                    client.Close();
                    string buffer = Encoding.UTF8.GetString(buff);
                    result = buffer.Trim(new char[] { '\0' });
                    _dateTime = DateTime.Now;
                    return 0;
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                    return -1;
                }
            }
        }
        /// <summary>
        /// Set value to device
        /// </summary>
        /// <param name="device">Register of PLC</param>
        /// <param name="value">16bit value</param>
        /// <returns></returns>
        public int SetDevice(string device, int value)
        {
            try
            {
                string acommand = string.Empty;
                string ccommand = string.Empty;
                string bcommand = string.Empty;
                if (_listsp.Contains(device.Substring(0, 1).ToUpper()))
                {
                    acommand = _subHeader + _network + _station + _moduleio + _multidrop;
                    ccommand = "D000" + _network + _station + _moduleio + _multidrop;
                    int length = 24;
                    if (device.Substring(0, 1).ToUpper() is "D" or "W")
                    {
                        if (value < 655535 && value >= 0)
                        {
                            length += 4;
                            bcommand = acommand + length.ToString("X4") + _reserved + _write + _word
                                     + device.Substring(0, 1).ToUpper() + (char)0x2A
                                     + Convert.ToInt32(device.Substring(1).ToUpper()).ToString("D6")
                                     + "0001" + value.ToString("X4");
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else if (device.Substring(0, 1).ToUpper() is "X" or "Y")
                    {
                        var dec2 = Convert.ToInt32(device.Substring(1), 8);
                        var hex2 = dec2.ToString("x");
                        var _deviceNo = hex2.PadLeft(6, '0');
                        //var _deviceNo = dec2.ToString("x6");
                        length += 1;
                        bcommand = acommand + length.ToString("X4") + _reserved + _write + _bit
                                 + device.Substring(0, 1).ToUpper() + (char)0x2A
                                 + _deviceNo
                                 + "0001" + value.ToString("X1");
                    }
                    else
                    {
                        if (value == 1 || value == 0)
                        {
                            length += 1;
                            bcommand = acommand + length.ToString("X4") + _reserved + _write + _bit
                                     + device.Substring(0, 1).ToUpper() + (char)0x2A
                                     + Convert.ToInt32(device.Substring(1).ToUpper()).ToString("D6")
                                     + "0001" + value.ToString("X1");
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    string result = string.Empty;
                    var r = SendCommand(bcommand, ref result);
                    if ((r == -1) || (result == ""))
                        return -1;
                    if (Convert.ToInt32(result.Substring(ccommand.Length + 4)) == 0)
                        return 1;
                    else
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
        /// Get value from device
        /// </summary>
        /// <param name="device"></param>
        /// <param name="value"></param>
        /// <returns>return number of device want get value, -1: Fail</returns>
        public int GetDevice(string device, ref int value)
        {
            try
            {
                string acommand = string.Empty;
                string ccommand = string.Empty;
                string bcommand = string.Empty;
                if (_listsp.Contains(device.Substring(0, 1).ToUpper()))
                {
                    acommand = _subHeader + _network + _station + _moduleio + _multidrop;
                    ccommand = "D000" + _network + _station + _moduleio + _multidrop;
                    int length = 24;
                    if (device.Substring(0, 1).ToUpper() is "D" or "W")
                    {
                        bcommand = acommand + length.ToString("X4") + _reserved + _read + _word
                                 + device.Substring(0, 1).ToUpper() + (char)0x2A
                                 + Convert.ToInt32(device.Substring(1).ToUpper()).ToString("D6") + "0001";
                    }
                    else if (device.Substring(0, 1).ToUpper() is "X" or "Y")
                    {
                        var dec2 = Convert.ToInt32(device.Substring(1), 8);
                        var hex2 = dec2.ToString("x");
                        var _deviceNo = hex2.PadLeft(6, '0');
                        //var _deviceNo = dec2.ToString("x6");
                        bcommand = acommand + length.ToString("X4") + _reserved + _read + _bit
                                 + device.Substring(0, 1).ToUpper() + (char)0x2A
                                 + _deviceNo + "0001";
                    }
                    else
                    {
                        bcommand = acommand + length.ToString("X4") + _reserved + _read + _bit
                           + device.Substring(0, 1).ToUpper() + (char)0x2A
                           + Convert.ToInt32(device.Substring(1).ToUpper()).ToString("D6") + "0001";
                    }

                    string result = string.Empty;
                    var r = SendCommand(bcommand, ref result);
                    if ((r == -1) || (result.Length == 0))
                        return -1;
                    if (Convert.ToInt32(result.Substring(ccommand.Length + 4, 4), 16) == 0)
                        value = Convert.ToInt32(result.Substring(ccommand.Length + 8), 16);
                }
                return 1;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// Stop device
        /// </summary>
        /// <returns></returns>
        public int Run()
        {
            try
            {
                string acommand = string.Empty;
                string ccommand = string.Empty;
                string bcommand = string.Empty;
                acommand = _subHeader + _network + _station + _moduleio + _multidrop;
                ccommand = "D000" + _network + _station + _moduleio + _multidrop;

                int length = 20;
                bcommand = acommand + length.ToString("X4") + _reserved + "1001000000030000";

                string result = string.Empty;
                var r = SendCommand(bcommand, ref result);
                if ((r == -1) || (result == ""))
                    return -1;
                if (Convert.ToInt32(result.Substring(ccommand.Length + 4)) == 0)
                    return 1;
                else
                    return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// Pause device
        /// </summary>
        /// <returns></returns>
        public int Pause()
        {
            try
            {
                string acommand = string.Empty;
                string ccommand = string.Empty;
                string bcommand = string.Empty;
                acommand = _subHeader + _network + _station + _moduleio + _multidrop;
                ccommand = "D000" + _network + _station + _moduleio + _multidrop;

                int length = 16;
                bcommand = acommand + length.ToString("X4") + _reserved + "100300000003";

                string result = string.Empty;
                var r = SendCommand(bcommand, ref result);
                if ((r == -1) || (result == ""))
                    return -1;
                if (Convert.ToInt32(result.Substring(ccommand.Length + 4)) == 0)
                    return 1;
                else
                    return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// Reset device
        /// </summary>
        /// <returns></returns>
        public int Reset()
        {
            try
            {
                string acommand = string.Empty;
                string ccommand = string.Empty;
                string bcommand = string.Empty;
                acommand = _subHeader + _network + _station + _moduleio + _multidrop;
                ccommand = "D000" + _network + _station + _moduleio + _multidrop;

                int length = 16;
                bcommand = acommand + length.ToString("X4") + _reserved + "100600000001";

                string result = string.Empty;
                var r = SendCommand(bcommand, ref result);
                if ((r == -1) || (result == ""))
                    return -1;
                if (Convert.ToInt32(result.Substring(ccommand.Length + 4)) == 0)
                    return 1;
                else
                    return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// Stop device
        /// </summary>
        /// <returns></returns>
        public int Stop()
        {
            try
            {
                string acommand = string.Empty;
                string ccommand = string.Empty;
                string bcommand = string.Empty;
                acommand = _subHeader + _network + _station + _moduleio + _multidrop;
                ccommand = "D000" + _network + _station + _moduleio + _multidrop;

                int length = 16;
                bcommand = acommand + length.ToString("X4") + _reserved + "100200000001";

                string result = string.Empty;
                var r = SendCommand(bcommand, ref result);
                if ((r == -1) || (result == ""))
                    return -1;
                if (Convert.ToInt32(result.Substring(ccommand.Length + 4)) == 0)
                    return 1;
                else
                    return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}
