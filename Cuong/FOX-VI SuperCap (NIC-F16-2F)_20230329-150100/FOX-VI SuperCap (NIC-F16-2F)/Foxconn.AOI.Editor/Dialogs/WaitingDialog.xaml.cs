using Foxconn.Threading.Tasks;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace Foxconn.AOI.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for WaitingDialog.xaml
    /// </summary>
    public partial class WaitingDialog : Window, IComponentConnector
    {
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message), typeof(string), typeof(WaitingDialog));
        private SerialTaskManager _taskMgr = new SerialTaskManager(1, 0);
        private Task _task;
        private bool _isCancelable;

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public bool IsCancelable => _isCancelable;

        public Exception Exception => _task.Exception;

        private WaitingDialog(Task task)
        {
            _task = task;
            _taskMgr.AddTask(0, task);
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            if (_task.IsCompleted)
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Close));
            else
                _taskMgr.CreateTask(0, () => Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Close)));
        }

        public static bool? ShowMessage(Window ownerWindow, string message, WaitHandle waitHandle) => DoWork(ownerWindow, message, () => waitHandle.WaitOne());

        public static bool? DoWork(Window ownerWindow, string message, Action action) => DoWork(ownerWindow, message, Task.Create(action), false);

        public static bool? DoWork(Window ownerWindow, string message, Action<Func<bool>> actionWithQueryCancel)
        {
            return DoWork(ownerWindow, message, Task.Create(actionWithQueryCancel), true);
        }

        public static bool? DoWork(Window ownerWindow, string message, Task task, bool isCancelable)
        {
            WaitingDialog waitingDialog = task != null ? new WaitingDialog(task) : throw new ArgumentNullException(nameof(task));
            waitingDialog.Message = message;
            waitingDialog.Owner = ownerWindow;
            waitingDialog._isCancelable = isCancelable;
            return waitingDialog.ShowDialog();
        }

        public static bool? DoWork(string message, Task task, bool isCancelable) => DoWork(Application.Current.MainWindow, message, task, isCancelable);

        public static bool? DoWork(string message, Action action) => DoWork(Application.Current.MainWindow, message, action);

        public static bool? DoWork(string message, Action<Func<bool>> actionWithQueryCancel) => DoWork(Application.Current.MainWindow, message, actionWithQueryCancel);

        public static bool? ShowMessage(string message, WaitHandle waitHandle) => ShowMessage(Application.Current.MainWindow, message, waitHandle);

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isCancelable)
                return;
            _task.CancelAndWait();
        }
    }
}
