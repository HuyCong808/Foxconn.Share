using Foxconn.App.Helper;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Foxconn.App.Views
{
    /// <summary>
    /// Interaction logic for DelayWindow.xaml
    /// </summary>
    public partial class DelayWindow : Window
    {
        private readonly MainWindow Root = MainWindow.Current;

        public DelayWindow()
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
                Root.CodeFlow("DELAY");

                Dispatcher.Invoke(() =>
                {
                    var delay = Root.AppManager.DatabaseManager.Basic.Delay;

                    txtBasic.TextChanged -= txtBasic_TextChanged;
                    txtAdvanced.TextChanged -= txtAdvanced_TextChanged;
                    txtTriggerCamera0.TextChanged -= txtTriggerCamera0_TextChanged;
                    txtTriggerCamera1.TextChanged -= txtTriggerCamera1_TextChanged;
                    txtTriggerCamera2.TextChanged -= txtTriggerCamera2_TextChanged;

                    txtBasic.Text = delay.Basic.ToString();
                    txtAdvanced.Text = delay.Advanced.ToString();
                    txtTriggerCamera0.Text = delay.Trigger0.ToString();
                    txtTriggerCamera1.Text = delay.Trigger1.ToString();
                    txtTriggerCamera2.Text = delay.Trigger2.ToString();

                    txtBasic.TextChanged += txtBasic_TextChanged;
                    txtAdvanced.TextChanged += txtAdvanced_TextChanged;
                    txtTriggerCamera0.TextChanged += txtTriggerCamera0_TextChanged;
                    txtTriggerCamera1.TextChanged += txtTriggerCamera1_TextChanged;
                    txtTriggerCamera2.TextChanged += txtTriggerCamera2_TextChanged;
                });
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void txtBasic_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Delay.Basic = Utilities.ConvertStringToInt(txtBasic.Text);
        }

        private void txtAdvanced_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Delay.Advanced = Utilities.ConvertStringToInt(txtAdvanced.Text);
        }

        private void txtTriggerCamera0_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Delay.Trigger0 = Utilities.ConvertStringToInt(txtTriggerCamera0.Text);
        }

        private void txtTriggerCamera1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Delay.Trigger1 = Utilities.ConvertStringToInt(txtTriggerCamera1.Text);
        }

        private void txtTriggerCamera2_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Delay.Trigger2 = Utilities.ConvertStringToInt(txtTriggerCamera2.Text);
        }
    }
}
