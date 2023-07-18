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
    /// <summary>
    /// Interaction logic for ParamsLightControl.xaml
    /// </summary>
    public partial class ParamsLightControl : UserControl, INotifyPropertyChanged
    {

        public static readonly DependencyProperty IsEnable1Property = DependencyProperty.Register("IsEnable1", typeof(bool), typeof(ParamsLightControl));
        public static readonly DependencyProperty PortName1Property = DependencyProperty.Register("PortName1", typeof(PortName), typeof(ParamsLightControl));
        public static readonly DependencyProperty IsEnable2Property = DependencyProperty.Register("IsEnable2", typeof(bool), typeof(ParamsLightControl));
        public static readonly DependencyProperty PortName2Property = DependencyProperty.Register("PortName2", typeof(PortName), typeof(ParamsLightControl));


        public bool IsEnable1
        {
            get => (bool)GetValue(IsEnable1Property);
            set => SetValue(IsEnable1Property, value);
        }

        public PortName PortName1
        {
            get => (PortName)GetValue(PortName1Property);
            set => SetValue(PortName1Property, value);
        }

        public bool IsEnable2
        {
            get => (bool)GetValue(IsEnable2Property);
            set => SetValue(IsEnable2Property, value);
        }

        public PortName PortName2
        {
            get => (PortName)GetValue(PortName2Property);
            set => SetValue(PortName2Property, value);
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
            cmbPortName1.ItemsSource = Enum.GetValues(typeof(PortName)).Cast<PortName>();
            cmbPortName2.ItemsSource = Enum.GetValues(typeof(PortName)).Cast<PortName>();
        }

        public void SetParameters(LightParams param)
        {
            string[] paths = new string[] { "IsEnable1", "PortName1", "IsEnable2", "PortName2" };
            DependencyProperty[] properties = new DependencyProperty[] { IsEnable1Property, PortName1Property, IsEnable2Property, PortName2Property };
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
