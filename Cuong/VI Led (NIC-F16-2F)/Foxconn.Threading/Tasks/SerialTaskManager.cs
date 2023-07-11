using System;
using System.Globalization;
using System.Threading;

namespace Foxconn.Threading.Tasks
{
    public class SerialTaskManager
    {
        private Locker _locker = new Locker();
        private PriorityQueue<Task> _queue;
        private int _waitTaskTimeout;
        private string _name;
        private volatile Thread _pumpThread;
        private CultureInfo _currentUICulture = Thread.CurrentThread.CurrentUICulture;
        private Task _currentTask;
        private ThreadPriority _threadPriority = ThreadPriority.Normal;

        public ThreadPriority ThreadPriority
        {
            get => _threadPriority;
            set => _threadPriority = value;
        }

        public bool IsPumping => _pumpThread != null;

        protected virtual void OnPumpStarted()
        {
        }

        protected virtual void OnPumpStopped()
        {
        }

        private void Pump(object state)
        {
            OnPumpStarted();
            Task task;
            do
            {
                _locker.Enter();
                _currentTask = null;
                if (_queue.Count == 0)
                {
                    _locker.Wait(_waitTaskTimeout);
                    if (_queue.Count == 0)
                    {
                        try
                        {
                            if (Idle != null)
                            {
                                Idle(this, EventArgs.Empty);
                                goto label_12;
                            }
                            else
                                goto label_12;
                        }
                        finally
                        {
                            _pumpThread = null;
                            _locker.Exit();
                        }
                    }
                }
                task = _queue.Dequeue();
                _currentTask = task;
                _locker.Exit();
                task.Execute();
            }
            while (!(task.Exception is ExitPumpException));
            _locker.Enter();
            while (_queue.Count != 0)
            {
                try
                {
                    _queue.Dequeue().Cancel();
                }
                catch
                {
                }
            }
            _pumpThread = null;
            _locker.Exit();
        label_12:
            OnPumpStopped();
        }

        private void StartPump()
        {
            Thread thread = new Thread(new ParameterizedThreadStart(Pump));
            thread.Name = _name;
            thread.IsBackground = true;
            thread.Priority = _threadPriority;
            thread.CurrentUICulture = _currentUICulture;
            _pumpThread = thread;
            thread.Start();
        }

        public void ExitPump()
        {
            if (!IsPumping)
                return;
            AddTask(_queue.PriorityCount - 1, Task.Create(() =>
            {
                throw new ExitPumpException();
            }));
        }

        public SerialTaskManager(string name, int priorityCount, int waitTaskTimeout)
        {
            _name = name;
            _queue = new PriorityQueue<Task>(priorityCount);
            _waitTaskTimeout = waitTaskTimeout;
        }

        public SerialTaskManager(int priorityCount, int waitTaskTimeout)
          : this(null, priorityCount, waitTaskTimeout)
        {
        }

        public SerialTaskManager(string name, int priorityCount)
          : this(name, priorityCount, 1000)
        {
        }

        public SerialTaskManager(int priorityCount)
          : this(priorityCount, 1000)
        {
        }

        public void AddTask(int priority, Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            _locker.Enter();
            _queue.Enqueue(priority, task);
            if (!IsPumping)
                StartPump();
            else
                _locker.Pulse();
            _locker.Exit();
        }

        public void CancelAll()
        {
            _locker.Enter();
            while (_queue.Count != 0)
                _queue.Dequeue().Cancel();
            _locker.Exit();
            _currentTask?.Cancel();
        }

        public int TaskCount
        {
            get
            {
                _locker.Enter();
                int count = _queue.Count;
                _locker.Exit();
                return count;
            }
        }

        public Task CurrentTask => _currentTask;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public Task CreateTask(int priority, Action action)
        {
            Task task = Task.Create(action);
            AddTask(priority, task);
            return task;
        }

        public Task CreateTask(int priority, Action<Func<bool>> actionWithQueryCancel)
        {
            Task task = Task.Create(actionWithQueryCancel);
            AddTask(priority, task);
            return task;
        }

        public Future<T> CreateFuture<T>(int priority, Func<T> func)
        {
            Future<T> future = Future.Create(func);
            AddTask(priority, future);
            return future;
        }

        public event EventHandler Idle;

        private class ExitPumpException : Exception
        {
        }
    }
}
