namespace Foxconn.AOI.Editor.Configuration
{
    public class Server : NotifyProperty
    {
        private int _id { get; set; }
        private string _name { get; set; }
        private bool _isEnabled { get; set; }
        private string _localHost { get; set; }
        private int _localPort { get; set; }
        private string _VNCHost { get; set; }
        private string _VNCPassword { get; set; }

        public int ID
        {
            get => _id;
            set
            {
                _id = value;
                NotifyPropertyChanged(nameof(ID));
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                NotifyPropertyChanged(nameof(IsEnabled));
            }
        }

        public string LocalHost
        {
            get => _localHost;
            set
            {
                _localHost = value;
                NotifyPropertyChanged(nameof(LocalHost));
            }
        }

        public int LocalPort
        {
            get => _localPort;
            set
            {
                _localPort = value;
                NotifyPropertyChanged(nameof(LocalPort));
            }
        }

        public string VNCHost
        {
            get => _VNCHost;
            set
            {
                _VNCHost = value;
                NotifyPropertyChanged(nameof(VNCHost));
            }
        }

        public string VNCPassword
        {
            get => _VNCPassword;
            set
            {
                _VNCPassword = value;
                NotifyPropertyChanged(nameof(VNCPassword));
            }
        }

        public Server()
        {
            _id = 0;
            _name = "SERVER";
            _isEnabled = false;
            _localHost = "localhost";
            _localPort = 27000;
            _VNCHost = "127.0.0.1";
            _VNCPassword = "123456";
        }
    }
}
