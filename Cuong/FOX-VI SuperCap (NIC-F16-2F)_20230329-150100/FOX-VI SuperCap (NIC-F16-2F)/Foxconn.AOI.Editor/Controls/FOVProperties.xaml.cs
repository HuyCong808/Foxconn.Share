using Foxconn.AOI.Editor.Configuration;
using Foxconn.AOI.Editor.Enums;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Foxconn.AOI.Editor.Controls
{
    /// <summary>
    /// Interaction logic for FOVProperties.xaml
    /// </summary>
    public partial class FOVProperties : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty IDProperty = DependencyProperty.Register("ID", typeof(int), typeof(FOVProperties));
        public static new readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(FOVProperties));
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(FOVProperties));
        public static new readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(FOVProperties));
        public static readonly DependencyProperty ExposureTimeProperty = DependencyProperty.Register("ExposureTime", typeof(int), typeof(FOVProperties));
        public static readonly DependencyProperty FOVTypeProperty = DependencyProperty.Register("FOVType", typeof(FOVType), typeof(FOVProperties));
        public static readonly DependencyProperty CameraModeProperty = DependencyProperty.Register("CameraMode", typeof(CameraMode), typeof(FOVProperties));
        public static readonly DependencyProperty FOVPositionProperty = DependencyProperty.Register("FOVPosition", typeof(FOVPosition), typeof(FOVProperties));

        public int ID
        {
            get => (int)GetValue(IDProperty);
            set => SetValue(IDProperty, value);
        }

        public new int Name
        {
            get => (int)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public new bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        public int ExposureTime
        {
            get => (int)GetValue(ExposureTimeProperty);
            set => SetValue(ExposureTimeProperty, value);
        }

        public FOVType FOVType
        {
            get => (FOVType)GetValue(FOVTypeProperty);
            set => SetValue(FOVTypeProperty, value);
        }

        public CameraMode CameraMode
        {
            get => (CameraMode)GetValue(CameraModeProperty);
            set => SetValue(CameraModeProperty, value);
        }

        public FOVPosition FOVPosition
        {
            get => (FOVPosition)GetValue(FOVPositionProperty);
            set => SetValue(FOVPositionProperty, value);
        }

        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public FOVProperties()
        {
            InitializeComponent();
            cmbFOVType.ItemsSource = Enum.GetValues(typeof(FOVType)).Cast<FOVType>();
            cmbCameraMode.ItemsSource = Enum.GetValues(typeof(CameraMode)).Cast<CameraMode>();
            DataContext = this;
        }

        public void SetParameters(FOV param)
        {
            string[] paths = new string[] { "ID", "Name", "Description", "IsEnabled", "ExposureTime", "Type", "CameraMode", "FOVPosition" };
            DependencyProperty[] properties = new DependencyProperty[] { IDProperty, NameProperty, DescriptionProperty, IsEnabledProperty, ExposureTimeProperty, FOVTypeProperty, CameraModeProperty, FOVPositionProperty };
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
