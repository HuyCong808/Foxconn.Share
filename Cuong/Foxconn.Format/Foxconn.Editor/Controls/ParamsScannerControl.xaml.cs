using System.ComponentModel;
using System.Windows.Controls;

namespace Foxconn.Editor.Controls
{
    public partial class ParamsScannerControl : UserControl, INotifyPropertyChanged
    {
        public bool IsEnable
        {
            get => MachineParams.Current.Scanner.IsEnabled;
            set => MachineParams.Current.Scanner.IsEnabled = value;
        }
        public string PortName
        {
            get => MachineParams.Current.Scanner.PortName;
            set => MachineParams.Current.Scanner.PortName = value;
        }
        public int Length
        {
            get => MachineParams.Current.Scanner.Length;
            set => MachineParams.Current.Scanner.Length = value;
        }
        public bool IsSubstring
        {
            get => MachineParams.Current.Scanner.IsSubstring;
            set => MachineParams.Current.Scanner.IsSubstring = value;
        }
        public int StartIndex
        {
            get => MachineParams.Current.Scanner.StartIndex;
            set => MachineParams.Current.Scanner.StartIndex = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public ParamsScannerControl()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
