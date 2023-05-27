using Foxconn.App.Helper;
using System;
using System.Windows;
using System.Windows.Input;

namespace Foxconn.App.Views
{
    /// <summary>
    /// Interaction logic for KeyboardWindow.xaml
    /// </summary>
    public partial class KeyboardWindow : Window
    {
        public string Data => _data;
        private string _data { get; set; }

        public KeyboardWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppUi.ShowImage(this, imgKeyboard, @"Assets/Keyboard.png");
            _data = string.Empty;
            txtDataReceived.Focus();
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.Key == Key.Enter)
            {
                btnEnter_Click(null, null);
            }
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            if (txtDataReceived.Text != string.Empty)
            {
                _data = txtDataReceived.Text;
                Close();
            }
        }
    }
}
