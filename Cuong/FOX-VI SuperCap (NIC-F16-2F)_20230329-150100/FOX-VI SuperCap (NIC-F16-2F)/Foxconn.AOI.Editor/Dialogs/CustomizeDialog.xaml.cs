using System.Windows;

namespace Foxconn.AOI.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for CustomizeDialog.xaml
    /// </summary>
    public partial class CustomizeDialog : Window
    {
        public CustomizeDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
        }
    }
}
