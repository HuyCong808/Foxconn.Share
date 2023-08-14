using System.ComponentModel;
using System.Windows.Controls;

namespace Foxconn.Editor.Controls
{

    public partial class ParamsRobotControl : UserControl, INotifyPropertyChanged
    {
        public bool IsEnable
        {
            get => MachineParams.Current.Robot.IsEnabled;
            set => MachineParams.Current.Robot.IsEnabled = value;
        }
        public string Host
        {
            get => MachineParams.Current.Robot.Host;
            set => MachineParams.Current.Robot.Host = value;
        }
        public int Port
        {
            get => MachineParams.Current.Robot.Port;
            set => MachineParams.Current.Robot.Port = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        public ParamsRobotControl()
        {
            InitializeComponent();
            DataContext = this;
        }


    }
}
