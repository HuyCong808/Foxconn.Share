using System.ComponentModel;
using System.Windows.Controls;

namespace Foxconn.Editor.Controls
{

    public partial class ParamsPLCControl : UserControl, INotifyPropertyChanged
    {
        public bool IsEnabled1
        {
            get => MachineParams.Current.PLC1.IsEnabled;
            set => MachineParams.Current.PLC1.IsEnabled = value;
        }

        public string HostPLC1
        {
            get => MachineParams.Current.PLC1.Host;
            set => MachineParams.Current.PLC1.Host = value;
        }

        public int PortPLC1
        {
            get => MachineParams.Current.PLC1.Port;
            set => MachineParams.Current.PLC1.Port = value;
        }

        public bool IsEnabled2
        {
            get => MachineParams.Current.PLC2.IsEnabled;
            set => MachineParams.Current.PLC2.IsEnabled = value;
        }

        public string HostPLC2
        {
            get => MachineParams.Current.PLC2.Host;
            set => MachineParams.Current.PLC2.Host = value;
        }

        public int PortPLC2
        {
            get => MachineParams.Current.PLC2.Port;
            set => MachineParams.Current.PLC2.Port = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public ParamsPLCControl()
        {
            InitializeComponent();
            DataContext = this;
        }

    }
}
