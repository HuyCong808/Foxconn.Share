using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace Foxconn.AOI.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : Window, IComponentConnector
    {
        public static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register(nameof(TitleText), typeof(string), typeof(ProgressDialog));
        public static readonly DependencyProperty StateTextProperty = DependencyProperty.Register(nameof(StateText), typeof(string), typeof(ProgressDialog));
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(nameof(Progress), typeof(int), typeof(ProgressDialog));
        private BackgroundWorker backgroundWorker;
        private object argument;

        public string TitleText
        {
            get => (string)GetValue(TitleTextProperty);
            set => SetValue(TitleTextProperty, value);
        }

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

        public bool SupportsCancellation => backgroundWorker.WorkerSupportsCancellation;

        public BackgroundWorker BackgroundWorker => backgroundWorker;

        public ProgressDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            DataContext = this;
        }

        public ProgressDialog(BackgroundWorker backgroundWorker, object argument)
        {
            this.argument = argument;
            this.backgroundWorker = backgroundWorker;
            //this.backgroundWorker.WorkerReportsProgress = true;
            //this.backgroundWorker.WorkerSupportsCancellation = false;
            this.backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(BackgroundWorker_ProgressChanged);
            this.backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            DataContext = this;
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
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

        private void CancelButton_Click(object sender, RoutedEventArgs args)
        {
            if (!backgroundWorker.WorkerSupportsCancellation)
                return;
            backgroundWorker.CancelAsync();
            Close();
        }
    }
}
