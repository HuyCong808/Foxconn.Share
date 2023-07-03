using System;
using System.Collections.Generic;
using System.Threading;

namespace Foxconn.Threading
{
    public class ThreadPoolProducerConsumer : IProducerConsumer
    {
        private Queue<Executor> queue = new Queue<Executor>();

        private void QueueProcessor(object obj)
        {
            Monitor.Enter(queue);
            while (queue.Count == 0)
                Monitor.Wait(queue);
            Executor executor = queue.Dequeue();
            Monitor.Exit(queue);
            ThreadPool.QueueUserWorkItem(new WaitCallback(QueueProcessor));
            executor.Execute();
        }

        public void SingleThreaderProducerConsumer() => ThreadPool.QueueUserWorkItem(new WaitCallback(QueueProcessor));

        public void BeginInvoke(Delegate @delegate) => BeginInvoke(@delegate, null);

        public void BeginInvoke(Delegate @delegate, object[] arguments)
        {
            Monitor.Enter(queue);
            queue.Enqueue(new Executor(@delegate, arguments));
            Monitor.Pulse(queue);
            Monitor.Exit(queue);
        }

        private class Executor
        {
            private readonly Delegate Delegate;
            private readonly object[] Arguments;

            public Executor(Delegate @delegate, object[] arguments)
            {
                Delegate = @delegate;
                Arguments = arguments;
            }

            public object Execute() => Delegate.DynamicInvoke(Arguments);
        }
    }
}
