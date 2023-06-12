using System.Windows;

namespace Foxconn.Editor.Dialogs
{
    public partial class LoginDialog : Window
    {
        private bool _cancel = false;
        public LoginDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            txtUsername.Focus();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            Close();

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _cancel = true;
            Close();
        }
        public bool Cancel()
        {
            if (_cancel)
            {
                return true;
            }
            return false;

        }
    }
}
