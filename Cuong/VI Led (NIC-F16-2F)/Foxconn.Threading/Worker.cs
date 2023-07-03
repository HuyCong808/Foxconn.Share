using System;
using System.Threading;

namespace Foxconn.Threading
{
    public class Worker
    {
        private Thread _workerThread;
        private ManualResetEvent _stopSignal = new ManualResetEvent(false);
        private ThreadStart _action;
        private bool _isBackground;

        public bool ShoudStop => _stopSignal.WaitOne(0, false);

        public bool WaitStopSignal(TimeSpan timeout) => _stopSignal.WaitOne(timeout, false);

        public bool WaitStopSignal(int millisecondsTimeout) => _stopSignal.WaitOne(millisecondsTimeout, false);

        public Worker(ThreadStart action) : this(action, true)
        {
        }

        public Worker(ThreadStart action, bool isBackground)
        {
            _action = action != null ? action : throw new ArgumentNullException(nameof(action));
            _isBackground = isBackground;
        }

        public void Start()
        {
            if (IsRunning)
                return;
            _stopSignal.Reset();
            _workerThread = new Thread(_action);
            _workerThread.IsBackground = _isBackground;
            _workerThread.Start();
        }

        public void Stop()
        {
            if (!IsRunning)
                return;
            RequestStop();
            while (IsRunning)
                Thread.Sleep(1);
        }

        public void RequestStop()
        {
            if (!IsRunning)
                return;
            _stopSignal.Set();
        }

        public bool IsRunning => _workerThread != null && _workerThread.IsAlive;
    }
}
