using System.Windows;

namespace Foxconn.AOI.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        public OptionsDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
        }
    }
}
