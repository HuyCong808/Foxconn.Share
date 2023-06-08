using System.Windows;

namespace Foxconn.Editor.Dialogs
{
    public partial class Login : Window
    {
        MainWindow window = new MainWindow();
        private bool _cancel = false;
        public Login()
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
