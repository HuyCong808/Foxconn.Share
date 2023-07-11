using System;
using System.Collections;
using System.Collections.Generic;

namespace Foxconn.Threading
{
    public class PriorityQueue<T> : IEnumerable<T>, IEnumerable
    {
        private Queue<T>[] _queues;

        private int _count;

        public int Count => _count;

        public int PriorityCount => _queues.Length;

        public PriorityQueue(int priorityCount)
        {
            if (priorityCount < 1)
            {
                throw new ArgumentOutOfRangeException("priorityCount");
            }

            _queues = new Queue<T>[priorityCount];
            for (int i = 0; i < _queues.Length; i++)
            {
                _queues[i] = new Queue<T>();
            }
        }

        public void Enqueue(int priority, T item)
        {
            _queues[priority].Enqueue(item);
            _count++;
        }

        public T Dequeue()
        {
            if (_count <= 0)
            {
                throw new InvalidOperationException();
            }

            Queue<T>[] queues = _queues;
            foreach (Queue<T> queue in queues)
            {
                if (queue.Count != 0)
                {
                    T result = queue.Dequeue();
                    _count--;
                    return result;
                }
            }

            throw new InvalidOperationException();
        }

        public T Peek()
        {
            if (_count <= 0)
            {
                throw new InvalidOperationException();
            }

            Queue<T>[] queues = _queues;
            foreach (Queue<T> queue in queues)
            {
                if (queue.Count != 0)
                {
                    return queue.Peek();
                }
            }

            throw new InvalidOperationException();
        }

        public void Clear()
        {
            Array.ForEach(_queues, delegate (Queue<T> queue)
            {
                queue.Clear();
            });
        }

        public bool Contains(T item)
        {
            if (_count <= 0)
            {
                return false;
            }

            Queue<T>[] queues = _queues;
            for (int i = 0; i < queues.Length; i++)
            {
                if (queues[i].Contains(item))
                {
                    return true;
                }
            }

            return false;
        }

        public void TrimExcess()
        {
            Array.ForEach(_queues, delegate (Queue<T> queue)
            {
                queue.TrimExcess();
            });
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int num = arrayIndex;
            Queue<T>[] queues = _queues;
            foreach (Queue<T> queue in queues)
            {
                if (queue.Count != 0)
                {
                    queue.CopyTo(array, num);
                    num += queue.Count;
                }
            }
        }

        private IEnumerable<T> Enumerate()
        {
            Queue<T>[] queues = _queues;
            foreach (Queue<T> queue in queues)
            {
                if (queue.Count == 0)
                {
                    continue;
                }

                foreach (T item in queue)
                {
                    yield return item;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }
    }
}
