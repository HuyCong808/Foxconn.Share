using System.Windows;

namespace Foxconn.App.Views
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public string LabelContent
        {
            get => _content;
            set
            {
                _content = value;
                _countSecond = 0;
                Dispatcher.Invoke(() =>
                {
                    lblStatus.Content = _content;
                });
            }
        }

        public bool KillMe
        {
            get => _killMe;
            set
            {
                _killMe = value;
                if (_killMe)
                {
                    Dispatcher.Invoke(() =>
                    {
                        _timer.Enabled = false;
                        Close();
                    });
                }
            }
        }

        private System.Timers.Timer _timer = new System.Timers.Timer(1000);
        private string _content = "Loading...";
        private int _countSecond = 0;
        private int _timeOut = 180;
        private bool _killMe = false;

        private void OntimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            _countSecond++;
            if (_countSecond > _timeOut)
            {
                _timer.Enabled = false;
                KillMe = true;
            }
            Dispatcher.Invoke(() =>
            {
                Title = string.Format("Loading... ({1} {2})", "Loading", _countSecond, _countSecond >= 2 ? "seconds" : "second");
            });
        }

        public LoadingWindow()
        {
            InitializeComponent();
        }

        public LoadingWindow(string content = "Loading...", int timeout = 180)
        {
            InitializeComponent();
            LabelContent = content;
            _timeOut = timeout;
            _timer.Elapsed += OntimedEvent;
            _timer.Enabled = true;
        }

        public LoadingWindow(string content = "Loading...")
        {
            InitializeComponent();
            LabelContent = content;
            _timer.Elapsed += OntimedEvent;
            _timer.Enabled = true;
        }
    }
}
