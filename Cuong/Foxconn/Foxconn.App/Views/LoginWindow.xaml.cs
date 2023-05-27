using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using System;
using System.Windows;
using System.Windows.Input;

namespace Foxconn.App.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public string Username => _username;
        public string Password => _password;
        private string _username { get; set; }
        private string _password { get; set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppUi.ShowImage(this, imgUser, @"Assets/UserAccount.png");
            _username = Enum.GetName(typeof(User), (int)User.Operator);
            _password = string.Empty;
            txtUsername.Text = _username;
            txtPassword.Password = _password;
            txtPassword.Focus();
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
                btnLogin_Click(null, null);
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (txtUsername.Text != string.Empty && txtPassword.Password != string.Empty)
            {
                _username = txtUsername.Text;
                _password = txtPassword.Password;
                Close();
            }
        }
    }
}
