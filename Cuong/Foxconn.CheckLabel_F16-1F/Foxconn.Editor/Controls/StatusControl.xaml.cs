using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Foxconn.Editor.Controls
{
    /// <summary>
    /// Interaction logic for StatusControl.xaml
    /// </summary>
    public partial class StatusControl : UserControl, IComponentConnector
    {
        public static readonly DependencyProperty StateTextProperty = DependencyProperty.Register(nameof(StateText), typeof(string), typeof(StatusControl));
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(nameof(Progress), typeof(int), typeof(StatusControl));
        public static readonly DependencyProperty StateColorProperty = DependencyProperty.Register(nameof(StateColor), typeof(SolidColorBrush), typeof(StatusControl));
        private BackgroundWorker backgroundWorker;
        private object argument;

        public string StateText
        {
            get => (string)GetValue(StateTextProperty);
            set => SetValue(StateTextProperty, value);
        }

        public int Progress
        {
            get => (int)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public SolidColorBrush StateColor
        {
            get => (SolidColorBrush)GetValue(StateColorProperty);
            set => SetValue(StateColorProperty, value);
        }

        public bool SupportsCancellation => backgroundWorker.WorkerSupportsCancellation;

        public BackgroundWorker BackgroundWorker => backgroundWorker;

        public StatusControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
            StateText = e.UserState as string;
        }

        private void ProgressDialog_Loaded(object sender, RoutedEventArgs e)
        {
            backgroundWorker.RunWorkerAsync(argument);
        }

        public void Create(BackgroundWorker backgroundWorker, object argument)
        {
            this.argument = argument;
            Loaded += new RoutedEventHandler(ProgressDialog_Loaded);
            this.backgroundWorker = backgroundWorker;
            //this.backgroundWorker.WorkerReportsProgress = true;
            //this.backgroundWorker.WorkerSupportsCancellation = false;
            this.backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(BackgroundWorker_ProgressChanged);
            this.backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
        }
    }
}
