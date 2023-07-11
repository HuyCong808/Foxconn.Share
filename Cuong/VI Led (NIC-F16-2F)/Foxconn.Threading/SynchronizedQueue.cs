using System.Collections.Generic;
using System.Threading;

namespace Foxconn.Threading
{
    public class SynchronizedQueue<T>
    {
        private Queue<T> _queue = new Queue<T>();

        public T Dequeue()
        {
            Monitor.Enter(_queue);
            while (_queue.Count == 0)
                Monitor.Wait(_queue);
            T obj = _queue.Dequeue();
            Monitor.Exit(_queue);
            return obj;
        }

        public bool TryDequeue(int millisecondsTimeout, out T item)
        {
            Monitor.Enter(_queue);
            bool flag;
            if (_queue.Count == 0)
            {
                Monitor.Wait(_queue, millisecondsTimeout);
                if (_queue.Count == 0)
                {
                    flag = false;
                    item = default(T);
                }
                else
                {
                    item = _queue.Dequeue();
                    flag = true;
                }
            }
            else
            {
                item = _queue.Dequeue();
                flag = true;
            }
            Monitor.Exit(_queue);
            return flag;
        }

        public T Peek()
        {
            Monitor.Enter(_queue);
            while (_queue.Count == 0)
                Monitor.Wait(_queue);
            T obj = _queue.Peek();
            Monitor.Exit(_queue);
            return obj;
        }

        public bool Wait(int millisecondsTimeout)
        {
            bool flag = true;
            Monitor.Enter(_queue);
            if (_queue.Count == 0)
                flag = Monitor.Wait(_queue, millisecondsTimeout);
            Monitor.Exit(_queue);
            return flag;
        }

        public int Count
        {
            get
            {
                Monitor.Enter(_queue);
                int count = _queue.Count;
                Monitor.Exit(_queue);
                return count;
            }
        }

        public bool Contains(T item)
        {
            Monitor.Enter(_queue);
            int num = _queue.Contains(item) ? 1 : 0;
            Monitor.Exit(_queue);
            return num != 0;
        }

        public void Enqueue(T item)
        {
            Monitor.Enter(_queue);
            _queue.Enqueue(item);
            Monitor.Pulse(_queue);
            Monitor.Exit(_queue);
        }

        public void Clear()
        {
            Monitor.Enter(_queue);
            _queue.Clear();
            Monitor.Exit(_queue);
        }
    }
}
