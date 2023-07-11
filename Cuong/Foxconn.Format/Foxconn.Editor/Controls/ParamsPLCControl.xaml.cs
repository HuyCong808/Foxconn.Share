using Foxconn.Editor.Enums;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Foxconn.Editor.MachineParams;
namespace Foxconn.Editor.Controls
{

    public partial class ParamsPLCControl : UserControl, INotifyPropertyChanged
    {


        public static readonly DependencyProperty IsEnable1Property = DependencyProperty.Register("IsEnable1", typeof(bool), typeof(ParamsPLCControl));
        public static readonly DependencyProperty HostPLC1Property = DependencyProperty.Register("HostPLC1", typeof(string), typeof(ParamsPLCControl));
        public static readonly DependencyProperty PortPLC1Property = DependencyProperty.Register("PortPLC1", typeof(int), typeof(ParamsPLCControl));
        public static readonly DependencyProperty IsEnable2Property = DependencyProperty.Register("IsEnable2", typeof(bool), typeof(ParamsPLCControl));
        public static readonly DependencyProperty HostPLC2Property = DependencyProperty.Register("HostPLC2", typeof(string), typeof(ParamsPLCControl));
        public static readonly DependencyProperty PortPLC2Property = DependencyProperty.Register("PortPLC2", typeof(int), typeof(ParamsPLCControl));

        public bool IsEnable1
        {
            get => (bool)GetValue(IsEnable1Property);
            set => SetValue(IsEnable1Property, value);
        }

        public string HostPLC1
        {
            get => (string)GetValue(HostPLC1Property);
            set => SetValue(HostPLC1Property, value);
        }

        public int PortPLC1
        {
            get => (int)GetValue(PortPLC1Property);
            set => SetValue(PortPLC2Property, value);
        }

        public bool IsEnable2
        {
            get => (bool)GetValue(IsEnable2Property);
            set => SetValue (IsEnable2Property, value);
        }

        public string HostPLC2
        {
            get => (string)GetValue(HostPLC2Property);
            set => SetValue(HostPLC2Property, value);
        }

        public int PortPLC2
        {
            get => (int)GetValue(PortPLC2Property);
            set => SetValue(PortPLC2Property, value);
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

        public void SetParameters(SocketParams param)
        {
            string[] paths = new string[] { "IsEnable1", "HostPLC1", "PortPLC1", "IsEnable2", "HostPLC2", "PortPLC2" };
            DependencyProperty[] properties = new DependencyProperty[] { IsEnable1Property, HostPLC1Property, PortPLC1Property, IsEnable2Property, HostPLC2Property, PortPLC2Property };
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
