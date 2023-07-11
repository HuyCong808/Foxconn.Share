using System;

namespace Foxconn.Threading.Tasks
{
    public abstract class Task
    {
        private Locker _locker = new Locker();
        private Exception _ex;
        private volatile ITaskStatus _status = CreatedStatus.Instance;

        private bool ChangeStatus(ITaskStatus from, ITaskStatus to)
        {
            _locker.Enter();
            int num = from == _status ? 1 : 0;
            if (num != 0)
            {
                _status = to;
                if (_status.IsCompleted())
                    _locker.PulseAll();
            }
            _locker.Exit();
            return num != 0;
        }

        private bool IsInStatus(ITaskStatus status) => _status == status;

        private bool QueryContinue() => _status.QueryContinue();

        private bool WaitCompletedImpl(Func<bool> waitLocker, bool isThrowEx)
        {
            bool flag = true;
            if (!IsCompleted)
            {
                _locker.Enter();
                if (!IsCompleted)
                    flag = waitLocker();
                _locker.Exit();
            }
            if (_ex != null & isThrowEx)
                throw _ex;
            return flag;
        }

        public bool Wait(bool isThrowEx) => WaitCompletedImpl(new Func<bool>(_locker.Wait), isThrowEx);

        public bool Wait(int millisecondsTimeout, bool isThrowEx) => WaitCompletedImpl(() => _locker.Wait(millisecondsTimeout), isThrowEx);

        public bool Wait(TimeSpan timeout, bool isThrowEx) => WaitCompletedImpl(() => _locker.Wait(timeout), isThrowEx);

        private void Cancel(string message) => _status.Cancel(this, message);

        internal virtual void OnCancel()
        {
        }

        protected abstract void OnExecute();

        public void Cancel() => Cancel(null);

        public void CancelAndWait()
        {
            Cancel();
            Wait(false);
        }

        public bool CancelAndWait(int millisecondsTimeout)
        {
            Cancel();
            return Wait(millisecondsTimeout, false);
        }

        public bool CancelAndWait(TimeSpan timeout)
        {
            Cancel();
            return Wait(timeout, false);
        }

        public void Execute() => _status.Execute(this);

        public bool IsCanceled => IsCompleted && _ex is TaskCanceledException;

        public bool IsCompleted => _status.IsCompleted();

        public Exception Exception
        {
            get => _ex;
            private set => _ex = value;
        }

        public void Wait() => Wait(true);

        public bool Wait(int millisecondsTimeout) => Wait(millisecondsTimeout, true);

        public bool Wait(TimeSpan timeout) => Wait(timeout, true);

        public static Task Create(Action action) => action != null ? (Task)new ActionTask(action) : throw new ArgumentNullException(nameof(action));

        public static Task Create(Action<Func<bool>> actionWithQueryCancel) => actionWithQueryCancel != null ? (Task)new QueryCancelTask(actionWithQueryCancel) : throw new ArgumentNullException(nameof(actionWithQueryCancel));

        public static Task Combine(Task prevTask, Task nextTask)
        {
            if (prevTask == null)
                throw new ArgumentNullException(nameof(prevTask));
            return nextTask != null ? (Task)new CompositeTask(prevTask, nextTask) : throw new ArgumentNullException(nameof(nextTask));
        }

        public Task ContinueWith(Action action) => Combine(this, Create(action));

        public Task ContinueWith(Action<Func<bool>> actionWithQueryCancel) => Combine(this, Create(actionWithQueryCancel));

        private class TaskCanceledException : OperationCanceledException
        {
            public TaskCanceledException()
            {
            }

            public TaskCanceledException(string message)
              : base(message)
            {
            }
        }

        private interface ITaskStatus
        {
            void Execute(Task task);

            void Cancel(Task task, string message);

            bool IsCompleted();

            bool QueryContinue();
        }

        private abstract class UncompletedStatus
        {
            public bool IsCompleted() => false;
        }

        private sealed class CreatedStatus : UncompletedStatus, ITaskStatus
        {
            public static readonly ITaskStatus Instance = new CreatedStatus();

            public void Execute(Task task)
            {
                if (!task.ChangeStatus(this, ExecutingStatus.Instance))
                    return;
                try
                {
                    task.OnExecute();
                }
                catch (Exception ex)
                {
                    task.Exception = ex;
                }
                finally
                {
                    if (!task.ChangeStatus(ExecutingStatus.Instance, CompletedStatus.Instance))
                        task.ChangeStatus(CancellingStatus.Instance, CompletedStatus.Instance);
                }
            }

            public void Cancel(Task task, string message)
            {
                if (!task.ChangeStatus(this, CancellingStatus.Instance))
                    return;
                try
                {
                    task.OnCancel();
                    task.Exception = new TaskCanceledException(message);
                }
                catch (Exception ex)
                {
                    task.Exception = ex;
                }
                finally
                {
                    task.ChangeStatus(CancellingStatus.Instance, CompletedStatus.Instance);
                }
            }

            public bool QueryContinue() => true;
        }

        private sealed class ExecutingStatus : UncompletedStatus, ITaskStatus
        {
            public static readonly ITaskStatus Instance = new ExecutingStatus();

            public void Execute(Task task)
            {
            }

            public void Cancel(Task task, string message)
            {
                if (!task.ChangeStatus(this, CancellingStatus.Instance))
                    return;
                try
                {
                    task.OnCancel();
                    task.Exception = new TaskCanceledException(message);
                }
                catch (Exception ex)
                {
                    task.Exception = ex;
                }
            }

            public bool QueryContinue() => true;
        }

        private sealed class CancellingStatus : UncompletedStatus, ITaskStatus
        {
            public static readonly ITaskStatus Instance = new CancellingStatus();

            public void Execute(Task task)
            {
            }

            public void Cancel(Task task, string message)
            {
            }

            public bool QueryContinue() => false;
        }

        private sealed class CompletedStatus : ITaskStatus
        {
            public static readonly ITaskStatus Instance = new CompletedStatus();

            public void Execute(Task task)
            {
            }

            public void Cancel(Task task, string message)
            {
            }

            public bool IsCompleted() => true;

            public bool QueryContinue() => false;
        }

        private class ActionTask : Task
        {
            private Action _action;

            public ActionTask(Action action) => _action = action;

            protected override void OnExecute() => _action();
        }

        private class QueryCancelTask : Task
        {
            private Action<Func<bool>> _action;

            public QueryCancelTask(Action<Func<bool>> action) => _action = action;

            protected override void OnExecute() => _action(new Func<bool>(QueryCancel));

            private bool QueryCancel() => !QueryContinue();
        }

        private class CompositeTask : Task
        {
            private Task _first;
            private Task _last;

            public CompositeTask(Task first, Task last)
            {
                _first = first;
                _last = last;
            }

            protected override void OnExecute()
            {
                _first.Execute();
                if (_first.Exception != null)
                    throw _first.Exception;
                _last.Execute();
                if (_last.Exception != null)
                    throw _last.Exception;
            }

            internal override void OnCancel()
            {
                _last.Cancel();
                _first.Cancel();
            }
        }
    }
}
