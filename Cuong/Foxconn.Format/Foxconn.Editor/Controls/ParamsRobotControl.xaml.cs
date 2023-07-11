using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Foxconn.Editor.MachineParams;

namespace Foxconn.Editor.Controls
{
  
    public partial class ParamsRobotControl : UserControl, INotifyPropertyChanged
    {

        public new static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(ParamsRobotControl));
        public static readonly DependencyProperty HostProperty = DependencyProperty.Register("Host", typeof(string), typeof(ParamsRobotControl));
        public static readonly DependencyProperty PortProperty = DependencyProperty.Register("Port", typeof(string), typeof(ParamsRobotControl));

        public new bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }
        public string Host
        {
            get => (string)GetValue(HostProperty);
            set => SetValue(HostProperty, value);
        }
        public string Port
        {
            get =>(string)GetValue(PortProperty);
            set => SetValue(PortProperty,value);
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

        public void SetParameters(SocketParams param)
        {
            string[] paths = new string[] { "IsEnabled", "Host", "Port" };
            DependencyProperty[] properties = new DependencyProperty[] { IsEnabledProperty, HostProperty, PortProperty };
            for (int i = 0; i < paths.Length; i++)
            {
                Binding binding = new Binding(paths[i])
                {
                    Source = param,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                SetBinding(properties[i], binding);
            }
            NotifyPropertyChanged();
        }


    }
}
