using System;
using System.Threading;

namespace Foxconn.Threading
{
    public class Heartbeat : IHeartbeat
    {
        private object _syncRoot;
        private int _count;

        public Heartbeat(object syncRoot) => _syncRoot = syncRoot;

        public Heartbeat() : this(new object())
        {
        }

        public IDisposable Lock() => new Locker(_syncRoot);

        private void EnterLock() => Monitor.Enter(_syncRoot);

        private void ExitLock() => Monitor.Exit(_syncRoot);

        private int WaitPulse(int oldCount, int millisecondsTimeout)
        {
            EnterLock();
            if (oldCount == _count)
                Monitor.Wait(_syncRoot, millisecondsTimeout);
            int count = _count;
            ExitLock();
            return count;
        }

        public void Pulse()
        {
            EnterLock();
            ++_count;
            Monitor.PulseAll(_syncRoot);
            ExitLock();
        }

        public IHeartbeatMonitor CreateMonitor() => new HeartbeatMonitor(this);

        public int Count
        {
            get
            {
                EnterLock();
                int count = _count;
                ExitLock();
                return count;
            }
        }

        private class Locker : IDisposable
        {
            private object obj;

            public Locker(object obj)
            {
                this.obj = obj;
                Monitor.Enter(this.obj);
            }

            public void Dispose() => Monitor.Exit(obj);
        }

        private class HeartbeatMonitor : IHeartbeatMonitor
        {
            private Heartbeat _heartbeat;
            private int _count;

            public HeartbeatMonitor(Heartbeat heartbeat)
            {
                _heartbeat = heartbeat;
                _count = heartbeat.Count;
            }

            private object SyncRoot => _heartbeat._syncRoot;

            public void Wait() => _count = _heartbeat.WaitPulse(_count, -1);

            public bool Wait(int millisecondsTimeout)
            {
                int count = _count;
                _count = _heartbeat.WaitPulse(count, millisecondsTimeout);
                return count != _count;
            }

            public int Count => _count;
        }
    }
}
