// Decompiled with JetBrains decompiler
// Type: Holly.Threading.FixedCapacityQueue`1
// Assembly: Holly.Threading, Version=7.2.1.906, Culture=neutral, PublicKeyToken=null
using System;

namespace Foxconn.Threading
{
    public class FixedCapacityQueue<T>
    {
        private SynchronizedQueue<T> _queue = new SynchronizedQueue<T>();
        private SynchronizedQueue<byte> _freeQueue = new SynchronizedQueue<byte>();
        private int _capacity;

        public FixedCapacityQueue(int capacity)
        {
            _capacity = capacity > 0 ? capacity : throw new ArgumentOutOfRangeException(nameof(capacity));
            for (int index = 0; index < capacity; ++index)
                _freeQueue.Enqueue(0);
        }

        public T Dequeue()
        {
            T obj = _queue.Dequeue();
            _freeQueue.Enqueue(0);
            return obj;
        }

        public bool TryDequeue(int millisecondsTimeout, out T item)
        {
            int num = _queue.TryDequeue(millisecondsTimeout, out item) ? 1 : 0;
            if (num == 0)
                return num != 0;
            _freeQueue.Enqueue(0);
            return num != 0;
        }

        public T Peek() => _queue.Peek();

        public bool Wait(int millisecondsTimeout) => _queue.Wait(millisecondsTimeout);

        public int Count => _queue.Count;

        public void Enqueue(T item)
        {
            int num = _freeQueue.Dequeue();
            _queue.Enqueue(item);
        }

        public int FreeSlots => _freeQueue.Count;

        public int Capacity => _capacity;

        public void Clear()
        {
            while (Count != 0)
                TryDequeue(1000, out T _);
        }
    }
}
