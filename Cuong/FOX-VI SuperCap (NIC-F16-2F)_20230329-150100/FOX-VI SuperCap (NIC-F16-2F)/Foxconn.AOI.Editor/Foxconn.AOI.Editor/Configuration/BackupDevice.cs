using System.Collections.Generic;

namespace Foxconn.AOI.Editor.Configuration
{
    public class BackupDevice : NotifyProperty
    {
        private string _firstDevice { get; set; }
        private string _lastDevice { get; set; }
        private List<Register> _data { get; set; }

        public string FirstDevice
        {
            get => _firstDevice;
            set
            {
                _firstDevice = value;
                NotifyPropertyChanged(nameof(FirstDevice));
            }
        }

        public string LastDevice
        {
            get => _lastDevice;
            set
            {
                _lastDevice = value;
                NotifyPropertyChanged(nameof(LastDevice));
            }
        }

        public List<Register> Data
        {
            get => _data;
            set
            {
                _data = value;
                NotifyPropertyChanged(nameof(Data));
            }
        }

        public BackupDevice()
        {
            _firstDevice = "D1";
            _lastDevice = "D1000";
            _data = new List<Register>();
        }

        public class Register
        {
            private string _device = "D1";
            private int _value = 0;

            public string Device
            {
                get => _device;
                set => _device = value;
            }

            public int Value
            {
                get => _value;
                set => _value = value;
            }
        }
    }
}
