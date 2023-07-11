using System.Diagnostics;
using System.Windows;

namespace Foxconn.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for PasswordInputDialog.xaml
    /// </summary>
    public partial class PasswordInputDialog : Window
    {
        public PasswordInputDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            Password.Focus();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            string pwd = Password.Password.Trim();
            bool flag = false;
            if (true)
                flag = InputCheck(pwd);
            if (pwd == "123456" || flag && !string.IsNullOrEmpty(pwd))
            {
                Trace.WriteLine("Foxconn =====> PasswordInputDialog password = " + pwd + "--isPwdPast=" + flag.ToString());
                Close();
            }
            else
            {
                int num = (int)MessageBox.Show("Wrong password.");
                Password.Focus();
                Password.SelectAll();
            }
        }

        private bool InputCheck(string pwd)
        {
            return true;
        }
    }
}
