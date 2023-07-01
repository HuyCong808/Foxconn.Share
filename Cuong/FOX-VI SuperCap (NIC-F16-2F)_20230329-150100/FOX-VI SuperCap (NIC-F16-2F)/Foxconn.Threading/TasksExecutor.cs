using System;
using System.Collections.Generic;

namespace Foxconn.Threading
{
    public class TasksExecutor
    {
        private IDispatcher dispatcher;
        private IEnumerator<Action> taskEnumerator;
        private bool isCompleted;
        private int executedCount;
        private int dispatchedCount;

        private void DispatchTask()
        {
            if (dispatchedCount > 0)
                return;
            ++dispatchedCount;
            dispatcher.BeginInvoke(new Action(Process));
        }

        private void Process()
        {
            --dispatchedCount;
            try
            {
                if (taskEnumerator != null)
                {
                    if (taskEnumerator.MoveNext())
                        goto label_5;
                }
                isCompleted = true;
                return;
            }
            catch (Exception ex)
            {
                isCompleted = true;
                return;
            }
        label_5:
            taskEnumerator.Current.DynamicInvoke();
            ++executedCount;
            DispatchTask();
        }

        public bool IsCompleted => isCompleted;

        public int ExecutedCount => executedCount;

        public void ClearTasks()
        {
            taskEnumerator = null;
            executedCount = 0;
            isCompleted = true;
        }

        public TasksExecutor(IDispatcher dispatcher) => this.dispatcher = dispatcher;

        public void Execute(IEnumerable<Action> taskList)
        {
            ClearTasks();
            taskEnumerator = taskList.GetEnumerator();
            isCompleted = false;
            DispatchTask();
        }
    }
}
