using System.Windows;

namespace Foxconn.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for CustomizeDialog.xaml
    /// </summary>
    public partial class CustomizeDialog : Window
    {
        public CustomizeDialog()
        {
            InitializeComponent();
            Title = "Customize";
            Owner = Application.Current.MainWindow;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
