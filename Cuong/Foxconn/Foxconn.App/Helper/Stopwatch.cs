namespace Foxconn.App.Helper
{
    public class Stopwatch
    {
        private readonly object _syncObject = new object();
        private static System.Diagnostics.Stopwatch _stopwatch { get; set; }
        private Stopwatch() { }
        private static Stopwatch _instance;
        public static Stopwatch Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Stopwatch();
                    _stopwatch = new System.Diagnostics.Stopwatch();
                }
                return _instance;
            }
        }

        public void Start()
        {
            if (_stopwatch == null)
                return;

            lock (_syncObject)
            {
                if (!_stopwatch.IsRunning)
                {
                    _stopwatch.Start();
                }
                else
                {
                    _stopwatch.Stop();
                    _stopwatch.Reset();
                    _stopwatch.Start();
                }
            }
        }

        public string GetTime()
        {
            if (_stopwatch == null)
                return string.Empty;

            lock (_syncObject)
            {
                return _stopwatch.IsRunning ? _stopwatch.Elapsed.ToString() : string.Empty;
            }
        }

        public string Elapsed()
        {
            if (_stopwatch == null)
                return string.Empty;

            lock (_syncObject)
            {
                var time = string.Empty;
                if (_stopwatch.IsRunning)
                {
                    _stopwatch.Stop();
                    time = _stopwatch.Elapsed.ToString();
                    _stopwatch.Reset();
                }
                return time;
            }
        }
    }
}
