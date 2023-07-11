using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Foxconn.Editor.MachineParams;

namespace Foxconn.Editor.Controls
{


    public partial class ParamsScannerControl : UserControl, INotifyPropertyChanged
    {

        public static new readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(ParamsScannerControl));
        public static readonly DependencyProperty PortNameProperty = DependencyProperty.Register("PortName", typeof(string), typeof(ParamsScannerControl));
        public static readonly DependencyProperty LengthProperty = DependencyProperty.Register("Length", typeof(int), typeof(ParamsScannerControl));
        public static readonly DependencyProperty IsSubstringProperty = DependencyProperty.Register("IsSubstring", typeof(bool), typeof(ParamsScannerControl));
        public static readonly DependencyProperty StartIndexProperty = DependencyProperty.Register("StartIndex", typeof(int), typeof(ParamsScannerControl));

        public new bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }
        public string PortName
        {
            get => (string)GetValue(PortNameProperty);
            set => SetValue(PortNameProperty, value);
        }
        public int Length
        {
            get => (int)GetValue(LengthProperty);
            set => SetValue(LengthProperty, value);
        }
        public bool IsSubstring
        {
            get => (bool)GetValue(IsSubstringProperty);
            set => SetValue(IsSubstringProperty, value);
        }
        public string StartIndex
        {
            get => (string)GetValue(StartIndexProperty);
            set => SetValue(StartIndexProperty, value);
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

        public void SetParameters(ScannerParams param)
        {
            string[] paths = new string[] { "IsEnabled", "PortName", "Length", "IsSubstring", "StartIndex" };
            DependencyProperty[] properties = new DependencyProperty[] { IsEnabledProperty, PortNameProperty, LengthProperty, IsSubstringProperty, StartIndexProperty };
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
