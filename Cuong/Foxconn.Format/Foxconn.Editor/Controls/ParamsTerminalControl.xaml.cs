using System.ComponentModel;
using System.Windows.Controls;

namespace Foxconn.Editor.Controls
{
    public partial class ParamsTerminalControl : UserControl, INotifyPropertyChanged
    {

        public bool IsEnable
        {
            get => MachineParams.Current.Terminal.IsEnabled;
            set => MachineParams.Current.Terminal.IsEnabled = value;
        }
        public string PortName
        {
            get => MachineParams.Current.Terminal.PortName;
            set => MachineParams.Current.Terminal.PortName = value;
        }
        public string Undo
        {
            get => MachineParams.Current.Terminal.Undo;
            set => MachineParams.Current.Terminal.Undo = value;
        }
        public string User
        {
            get => MachineParams.Current.Terminal.User;
            set => MachineParams.Current.Terminal.User = value;
        }

        public string Format
        {
            get => MachineParams.Current.Terminal.Format;
            set => MachineParams.Current.Terminal.Format = value;
        }
        public ParamsTerminalControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
