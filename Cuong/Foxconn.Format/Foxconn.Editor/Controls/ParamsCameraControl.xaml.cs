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
    /// Interaction logic for ParamsCameraControl.xaml
    /// </summary>
    public partial class ParamsCameraControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty IsEnable1Property = DependencyProperty.Register("IsEnable1", typeof(bool), typeof(ParamsCameraControl));
        public static readonly DependencyProperty CameraName1Property = DependencyProperty.Register("CameraName1", typeof(CameraName), typeof(ParamsCameraControl));
        public static readonly DependencyProperty CameraType1Property = DependencyProperty.Register("CameraType1", typeof(CameraType), typeof(ParamsCameraControl));
        public static readonly DependencyProperty IsEnable2Property = DependencyProperty.Register("IsEnable2", typeof(bool), typeof(ParamsCameraControl));
        public static readonly DependencyProperty CameraName2Property = DependencyProperty.Register("CameraName2", typeof(CameraName), typeof(ParamsCameraControl));
        public static readonly DependencyProperty CameraType2Property = DependencyProperty.Register("CameraType2", typeof(CameraType), typeof(ParamsCameraControl));

        public bool IsEnable1
        {
            get => (bool)GetValue(IsEnable1Property);
            set => SetValue(IsEnable1Property, value);
        }

        public CameraName CameraName1
        {
            get => (CameraName)GetValue(CameraName1Property);
            set => SetValue(CameraName1Property, value);
        }

        public CameraType CameraType1
        {
            get => (CameraType)GetValue(CameraType1Property);
            set => SetValue(CameraType1Property, value);
        }

        public bool IsEnable2
        {
            get =>(bool)GetValue(IsEnable2Property);
            set => SetValue(IsEnable2Property, value);
        }

        public CameraName CameraName2
        {
            get => (CameraName)GetValue(CameraName2Property);
            set => SetValue(CameraName2Property, value);
        }

        public CameraType CameraType2
        {
            get => (CameraType)GetValue(CameraType2Property);
            set => SetValue(CameraType2Property, value);
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
            cmbCameraName1.ItemsSource = Enum.GetValues(typeof(CameraName)).Cast<CameraName>();
            cmbCameraType1.ItemsSource = Enum.GetValues(typeof(CameraType)).Cast<CameraType>();
            cmbCameraName2.ItemsSource = Enum.GetValues(typeof(CameraName)).Cast<CameraName>();
            cmbCameraType2.ItemsSource = Enum.GetValues(typeof(CameraType)).Cast<CameraType>();
        }

        public void SetParameters(CameraParams param)
        {
            string[] paths = new string[] { "IsEnable1", "CameraName1", "CameraType1", "IsEnable2", "CameraName2", "CameraType2" };
            DependencyProperty[] properties = new DependencyProperty[] { IsEnable1Property, CameraName1Property, CameraType1Property, IsEnable2Property, CameraName2Property, CameraType2Property };
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
