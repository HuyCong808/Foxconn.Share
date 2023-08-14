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
            CheckInput();
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

        public void CheckInput()
        {
            if(txtUsername.Text.Length >0)
            {
                btnLogin.IsEnabled = true;
            }
            else
            {
                btnLogin.IsEnabled = false;
            }
        }
        private void txtUsername_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CheckInput();
        }
    }
}
