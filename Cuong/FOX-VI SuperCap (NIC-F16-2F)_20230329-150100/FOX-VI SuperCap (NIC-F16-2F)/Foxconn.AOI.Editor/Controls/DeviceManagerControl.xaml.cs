using Foxconn.AOI.Editor.Configuration;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Foxconn.AOI.Editor.Controls
{
    /// <summary>
    /// Interaction logic for DeviceManagerControl.xaml
    /// </summary>
    public partial class DeviceManagerControl : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty FirstDeviceProperty = DependencyProperty.Register("FirstDevice", typeof(string), typeof(DeviceManagerControl));
        public static readonly DependencyProperty LastDeviceProperty = DependencyProperty.Register("LastDevice", typeof(string), typeof(DeviceManagerControl));

        public string FirstDevice
        {
            get => (string)GetValue(FirstDeviceProperty);
            set => SetValue(FirstDeviceProperty, value);
        }

        public string LastDevice
        {
            get => (string)GetValue(LastDeviceProperty);
            set => SetValue(LastDeviceProperty, value);
        }

        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public DeviceManagerControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetParameters(BackupDevice param)
        {
            string[] paths = new string[] { "FirstDevice", "LastDevice" };
            DependencyProperty[] properties = new DependencyProperty[] { FirstDeviceProperty, LastDeviceProperty };
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
