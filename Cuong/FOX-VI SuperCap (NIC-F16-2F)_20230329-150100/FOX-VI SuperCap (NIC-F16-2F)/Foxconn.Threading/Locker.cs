using System;
using System.Threading;

namespace Foxconn.Threading
{
    internal class Locker
    {
        public void Enter() => Monitor.Enter(this);

        public bool TryEnter() => Monitor.TryEnter(this);

        public bool TryEnter(int millisecondsTimeout) => Monitor.TryEnter(this, millisecondsTimeout);

        public bool TryEnter(TimeSpan timeout) => Monitor.TryEnter(this, timeout);

        public void Exit() => Monitor.Exit(this);

        public void Pulse() => Monitor.Pulse(this);

        public void PulseAll() => Monitor.PulseAll(this);

        public bool Wait() => Monitor.Wait(this);

        public bool Wait(int millisecondsTimeout) => Monitor.Wait(this, millisecondsTimeout);

        public bool Wait(TimeSpan timeout) => Monitor.Wait(this, timeout);

        public bool Wait(int millisecondsTimeout, bool exitContext) => Monitor.Wait(this, millisecondsTimeout, exitContext);

        public bool Wait(TimeSpan timeout, bool exitContext) => Monitor.Wait(this, timeout, exitContext);
    }
}
