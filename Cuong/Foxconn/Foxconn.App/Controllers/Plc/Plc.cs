using Foxconn.App.Helper.Enums;

namespace Foxconn.App.Controllers.Plc
{
    public class Plc : Slmp
    {
        private readonly MainWindow Root = MainWindow.Current;
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
        public int Port
        {
            get => _port;
            set => _port = value;
        }
        public ConnectionStatus Status
        {
            get => _status;
            set => _status = value;
        }
        private int _index { get; set; }
        private string _alias { get; set; }
        private ConnectionStatus _status { get; set; }
        private int _loopTimes { get; set; }

        public Plc() : base()
        {
            _status = ConnectionStatus.None;
            _loopTimes = 10;
        }

        public bool StartPlc()
        {
            if (IsPingSuccessAsync())
            {
                _status = ConnectionStatus.Connected;
                Root.ShowMessage($"[PLC {_index}] Connected ({_host}:{_port})");
            }
            else
            {
                _status = ConnectionStatus.Disconnected;
                Root.ShowMessage($"[PLC {_index}] Cannot ping to ({_host}:{_port})", AppColor.Red);
            }
            return _status == ConnectionStatus.Connected;
        }

        public void CheckConnection()
        {
            if (IsPingSuccessAsync())
            {
                if (_status == ConnectionStatus.Disconnected)
                {
                    Root.ShowMessage($"[PLC {_index}] Connected ({_host}:{_port})");
                }
                _status = ConnectionStatus.Connected;
            }
            else
            {
                _status = ConnectionStatus.Disconnected;
                Root.ShowMessage($"[PLC {_index}] Cannot ping to ({_host}:{_port})", AppColor.Red);
            }
        }

        /// <summary>
        /// Set value for 16 bit register
        /// </summary>
        /// <param name="device">Register 16 bit</param>
        /// <param name="value">Value set</param>
        /// <returns></returns>
        public new bool SetDevice(string device, int value)
        {
            if (_status == ConnectionStatus.Connected)
            {
                for (int i = 0; i < _loopTimes; i++)
                {
                    if (base.SetDevice(device, value) == 1)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
        /// <summary>
        /// Get value from 16 bit register
        /// </summary>
        /// <param name="device">Register 16 bit</param>
        /// <param name="value">Reference value</param>
        /// <returns></returns>
        public new bool GetDevice(string device, ref int value)
        {
            if (_status == ConnectionStatus.Connected)
            {
                for (int i = 0; i < _loopTimes; i++)
                {
                    if (base.GetDevice(device, ref value) == 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Set value for 32 bit register
        /// </summary>
        /// <example>
        /// Example for get 32 bit device
        /// <code>
        /// var res = SetDevice32Bit("D8340", "D8341", value);
        /// </code>
        /// </example>
        /// <param name="label_low"></param>
        /// <param name="label_high"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetDevice32Bit(string label_low, string label_high, int value)
        {
            if (_status == ConnectionStatus.Connected)
            {
                ushort high = (ushort)(value >> 16);
                ushort low = (ushort)(value & 0xffff);
                var result = Convert32(low, high);
                var result1 = SetDevice(label_low, low);
                var result2 = SetDevice(label_high, high);
                return result1 && result2;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Get value from 32 bit register
        /// </summary>
        /// <example>
        /// Example for get 32 bit device
        /// <code>
        /// var res = GetDevice32Bit("D8340", "D8341", ref value);
        /// </code>
        /// </example>
        /// <param name="label_low"></param>
        /// <param name="label_high"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetDevice32Bit(string label_low, string label_high, ref int value)
        {
            if (_status == ConnectionStatus.Connected)
            {
                int value1 = -1, value2 = -1;
                bool result1 = GetDevice(label_low, ref value1);
                bool result2 = GetDevice(label_high, ref value2);
                if (result1 && result2 && value1 != -1 && value2 != -1)
                {
                    value = Convert32(value1, value2);
                    return true;
                }
                return false;
            }
            else
            {
                return false;
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
    }
}
