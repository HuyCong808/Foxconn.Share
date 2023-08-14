using Foxconn.Editor.Enums;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace Foxconn.Editor.Controls
{
    /// <summary>
    /// Interaction logic for ParamsCameraControl.xaml
    /// </summary>
    public partial class ParamsCameraControl : UserControl, INotifyPropertyChanged
    {
        public bool IsEnabled1
        {
            get => MachineParams.Current.Camera1.IsEnabled;
            set => MachineParams.Current.Camera1.IsEnabled = value;
        }

        public string CameraName1
        {
            get => MachineParams.Current.Camera1.UserDefinedName;
            set => MachineParams.Current.Camera1.UserDefinedName = value;
        }

        public CameraType CameraType1
        {
            get => MachineParams.Current.Camera1.Type;
            set => MachineParams.Current.Camera1.Type = value;
        }

        public bool IsEnabled2
        {
            get => MachineParams.Current.Camera2.IsEnabled;
            set => MachineParams.Current.Camera2.IsEnabled = value;
        }

        public string CameraName2
        {
            get => MachineParams.Current.Camera2.UserDefinedName;
            set => MachineParams.Current.Camera2.UserDefinedName = value;
        }

        public CameraType CameraType2
        {
            get => MachineParams.Current.Camera2.Type;
            set => MachineParams.Current.Camera2.Type = value;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public ParamsCameraControl()
        {
            InitializeComponent();
            DataContext = this;
            cmbCameraType1.ItemsSource = Enum.GetValues(typeof(CameraType));
            cmbCameraType2.ItemsSource = Enum.GetValues(typeof(CameraType));
        }

    }
}
