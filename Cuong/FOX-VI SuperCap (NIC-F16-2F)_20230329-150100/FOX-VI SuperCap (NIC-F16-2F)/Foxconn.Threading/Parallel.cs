using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Foxconn.Threading
{
    public static class Parallel
    {
        private static int __numThreads = Environment.ProcessorCount;

        private static IEnumerable<int> EnumerateInt(int start, int end, int step)
        {
            for (int i = start; i < end; i += step)
                yield return i;
        }

        public static void ForEach<T>(IEnumerable<T> enumerable, Action<T> action)
        {
            if (__numThreads == 1)
            {
                foreach (T obj in enumerable)
                    action(obj);
            }
            else
            {
                IEnumerator<T> enumerator = enumerable.GetEnumerator();
                WaitHandle[] waitHandleArray = new WaitHandle[__numThreads];
                ManualResetEvent state = new ManualResetEvent(false);
                int done = __numThreads;
                for (int index = 0; index < __numThreads; ++index)
                    ThreadPool.QueueUserWorkItem(stateObject =>
                    {
                        try
                        {
                            while (true)
                            {
                                T current;
                                lock (stateObject)
                                {
                                    if (!enumerator.MoveNext())
                                        break;
                                    current = enumerator.Current;
                                }
                                action(current);
                            }
                        }
                        finally
                        {
                            if (Interlocked.Decrement(ref done) == 0)
                                ((EventWaitHandle)stateObject).Set();
                        }
                    }, state);
                state.WaitOne();
                state.Close();
            }
        }

        public static void For(int start, int end, int step, Action<int> action) => ForEach(EnumerateInt(start, end, step), action);

        public static void For(int start, int end, Action<int> action) => For(start, end, 1, action);

        private static IEnumerable<long> EnumerateBusyTask(int milliseconds)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < milliseconds)
                yield return sw.ElapsedMilliseconds;
            sw.Stop();
        }

        [Conditional("DEBUG")]
        public static void Busy(int milliseconds)
        {
            long max = 0;
            ForEach(EnumerateBusyTask(milliseconds), t => max = Math.Max(max, t));
        }
    }
}
