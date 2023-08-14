
using System.ComponentModel;
using System.Windows.Controls;

namespace Foxconn.Editor.Controls
{
    public partial class ParamsLightControl : UserControl, INotifyPropertyChanged
    {
        public bool IsEnabled1
        {
            get => MachineParams.Current.Light1.IsEnabled;
            set => MachineParams.Current.Light1.IsEnabled = value;
        }

        public string PortName1
        {
            get => MachineParams.Current.Light1.PortName;
            set => MachineParams.Current.Light1.PortName = value;
        }

        public bool IsEnabled2
        {
            get => MachineParams.Current.Light2.IsEnabled;
            set => MachineParams.Current.Light2.IsEnabled = value;
        }

        public string PortName2
        {
            get => MachineParams.Current.Light2.PortName;
            set => MachineParams.Current.Light2.PortName = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        public ParamsLightControl()
        {
            InitializeComponent();
            DataContext = this;
        }

    }
}
