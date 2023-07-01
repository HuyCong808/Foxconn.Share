using System.Diagnostics;
using System.Windows;

namespace Foxconn.AOI.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for PasswordInputDialog.xaml
    /// </summary>
    public partial class PasswordInputDialog : Window
    {
        public PasswordInputDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
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
                Trace.WriteLine("FOXCONN AOI =====> PasswordInputDialog password = " + pwd + "--isPwdPast=" + flag.ToString());
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
