using Foxconn.App.Helper;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Foxconn.App.Views
{
    /// <summary>
    /// Interaction logic for CustomizeWindow.xaml
    /// </summary>
    public partial class CustomizeWindow : Window
    {
        private readonly MainWindow Root = MainWindow.Current;

        public CustomizeWindow()
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
                Root.CodeFlow("CUSTOMIZE");
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }
    }
}
