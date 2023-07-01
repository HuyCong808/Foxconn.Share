using System.ComponentModel;
using System.Windows.Controls;

namespace Foxconny.AOI.Editor.Controls
{
    /// <summary>
    /// Interaction logic for MachineMonitorControl.xaml
    /// </summary>
    public partial class MachineMonitorControl : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public MachineMonitorControl()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
