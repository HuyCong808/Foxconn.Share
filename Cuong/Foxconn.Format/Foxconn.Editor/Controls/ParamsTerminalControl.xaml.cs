using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Foxconn.Editor.MachineParams;

namespace Foxconn.Editor.Controls
{
    public partial class ParamsTerminalControl : UserControl, INotifyPropertyChanged
    {
        public static new readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(ParamsTerminalControl));
        public static readonly DependencyProperty PortNameProperty = DependencyProperty.Register("PortName", typeof(string), typeof(ParamsTerminalControl));
        public static readonly DependencyProperty UndoProperty = DependencyProperty.Register("Undo", typeof(string), typeof(ParamsTerminalControl));
        public static readonly DependencyProperty UserProperty = DependencyProperty.Register("Users", typeof(string), typeof(ParamsTerminalControl));
        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register("Format", typeof(string), typeof(ParamsTerminalControl));

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
        public string Undo
        {
            get => (string)GetValue(UndoProperty);
            set => SetValue(UndoProperty, value);
        }
        public string User
        {
            get => (string)GetValue(UserProperty);
            set => SetValue(UserProperty, value);
        }
        public string Format
        {
            get => (string)GetValue(FormatProperty);
            set=> SetValue(FormatProperty, value);
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
        public void SetParameters(TerminalParams param)
        {
            string[] paths = new string[] { "IsEnabled", "PortName", "Undo", "User", "Format" };
            DependencyProperty[] properties = new DependencyProperty[] { IsEnabledProperty, PortNameProperty, UndoProperty, UserProperty, FormatProperty };
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
