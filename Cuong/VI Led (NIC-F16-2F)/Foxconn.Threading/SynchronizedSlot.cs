using System.Threading;

namespace Foxconn.Threading
{
    public class SynchronizedSlot<T> where T : class
    {
        private object _syncRoot = new object();
        private T _value;

        public SynchronizedSlot()
        {
        }

        public SynchronizedSlot(T value) => Value = value;

        public T Value
        {
            get
            {
                lock (_syncRoot)
                    return _value;
            }
            set
            {
                lock (_syncRoot)
                {
                    if (value == null)
                    {
                        _value = default(T);
                        Monitor.Pulse(_syncRoot);
                    }
                    else
                    {
                        while (_value != null)
                            Monitor.Wait(_syncRoot);
                        _value = value;
                    }
                }
            }
        }

        public static implicit operator T(SynchronizedSlot<T> slot) => slot.Value;

        public void Clear() => Value = default(T);

        public bool HasValue => Value != null;
    }
}
