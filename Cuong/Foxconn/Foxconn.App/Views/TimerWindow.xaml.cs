using Foxconn.App.Helper;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Foxconn.App.Views
{
    /// <summary>
    /// Interaction logic for TimerWindow.xaml
    /// </summary>
    public partial class TimerWindow : Window
    {
        private readonly MainWindow Root = MainWindow.Current;

        public TimerWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => { Init(); });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Init()
        {
            try
            {
                Root.CodeFlow("TIMER");

                Dispatcher.Invoke(() =>
                {
                    var timer = Root.AppManager.DatabaseManager.Basic.Timer;

                    txtBasic.TextChanged -= txtBasic_TextChanged;
                    txtAdvanced.TextChanged -= txtAdvanced_TextChanged;
                    txtOpenCamera0.TextChanged -= txtOpenCamera0_TextChanged;
                    txtOpenCamera1.TextChanged -= txtOpenCamera1_TextChanged;
                    txtOpenCamera2.TextChanged -= txtOpenCamera2_TextChanged;
                    txtProcessCamera0.TextChanged -= txtProcessCamera0_TextChanged;
                    txtProcessCamera1.TextChanged -= txtProcessCamera1_TextChanged;
                    txtProcessCamera2.TextChanged -= txtProcessCamera2_TextChanged;
                    txtReset.TextChanged -= txtReset_TextChanged;

                    txtBasic.Text = timer.Basic.ToString();
                    txtAdvanced.Text = timer.Advanced.ToString();
                    txtOpenCamera0.Text = timer.Open0.ToString();
                    txtOpenCamera1.Text = timer.Open1.ToString();
                    txtOpenCamera2.Text = timer.Open2.ToString();
                    txtProcessCamera0.Text = timer.Processing0.ToString();
                    txtProcessCamera1.Text = timer.Processing1.ToString();
                    txtProcessCamera2.Text = timer.Processing2.ToString();
                    txtReset.Text = timer.Resetting.ToString();

                    txtBasic.TextChanged += txtBasic_TextChanged;
                    txtAdvanced.TextChanged += txtAdvanced_TextChanged;
                    txtOpenCamera0.TextChanged += txtOpenCamera0_TextChanged;
                    txtOpenCamera1.TextChanged += txtOpenCamera1_TextChanged;
                    txtOpenCamera2.TextChanged += txtOpenCamera2_TextChanged;
                    txtProcessCamera0.TextChanged += txtProcessCamera0_TextChanged;
                    txtProcessCamera1.TextChanged += txtProcessCamera1_TextChanged;
                    txtProcessCamera2.TextChanged += txtProcessCamera2_TextChanged;
                    txtReset.TextChanged += txtReset_TextChanged;
                });
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void txtBasic_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Timer.Basic = Utilities.ConvertStringToInt(txtBasic.Text);
        }

        private void txtAdvanced_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Timer.Advanced = Utilities.ConvertStringToInt(txtAdvanced.Text);
        }

        private void txtOpenCamera0_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Timer.Open0 = Utilities.ConvertStringToInt(txtOpenCamera0.Text);
        }

        private void txtOpenCamera1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Timer.Open1 = Utilities.ConvertStringToInt(txtOpenCamera1.Text);
        }

        private void txtOpenCamera2_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Timer.Open2 = Utilities.ConvertStringToInt(txtOpenCamera2.Text);
        }

        private void txtProcessCamera0_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Timer.Processing0 = Utilities.ConvertStringToInt(txtProcessCamera0.Text);
        }

        private void txtProcessCamera1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Timer.Processing1 = Utilities.ConvertStringToInt(txtProcessCamera1.Text);
        }

        private void txtProcessCamera2_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Timer.Processing2 = Utilities.ConvertStringToInt(txtProcessCamera2.Text);
        }

        private void txtReset_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Timer.Resetting = Utilities.ConvertStringToInt(txtReset.Text);
        }
    }
}
