using Foxconn.Editor.Configuration;
using Foxconn.Editor.Enums;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Foxconn.Editor.Controls
{
    /// <summary>
    /// Interaction logic for SMDControl.xaml
    /// </summary>
    public partial class SMDControl : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty IdProperty = DependencyProperty.Register("Id", typeof(int), typeof(SMDControl));
        public static new readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(SMDControl));
        public static new readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(SMDControl));
        public static readonly DependencyProperty ROIProperty = DependencyProperty.Register("ROI", typeof(JRect), typeof(SMDControl));
        public static readonly DependencyProperty SMDTypeProperty = DependencyProperty.Register("SMDType", typeof(SMDType), typeof(SMDControl));
        public static readonly DependencyProperty SMDAlgorithmProperty = DependencyProperty.Register("SMDAlgorithm", typeof(SMDAlgorithm), typeof(SMDControl));

        public int Id
        {
            get => (int)GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        public new int Name
        {
            get => (int)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        public new bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        public JRect ROI
        {
            get => (JRect)GetValue(ROIProperty);
            set => SetValue(ROIProperty, value);
        }

        public SMDType SMDType
        {
            get => (SMDType)GetValue(SMDTypeProperty);
            set => SetValue(SMDTypeProperty, value);
        }

        public SMDAlgorithm SMDAlgorithm
        {
            get => (SMDAlgorithm)GetValue(SMDAlgorithmProperty);
            set => SetValue(SMDAlgorithmProperty, value);
        }

        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public SMDControl()
        {
            InitializeComponent();
            cmbSMDType.ItemsSource = Enum.GetValues(typeof(SMDType)).Cast<SMDType>();
            cmbAlgorithm.ItemsSource = Enum.GetValues(typeof(SMDAlgorithm)).Cast<SMDAlgorithm>();
            DataContext = this;
        }

        public void SetParameters(SMD param)
        {
            string[] paths = new string[] { "Id", "Name", "IsEnabled", "ROI", "Type", "Algorithm" };
            DependencyProperty[] properties = new DependencyProperty[] { IdProperty, NameProperty, IsEnabledProperty, ROIProperty, SMDTypeProperty, SMDAlgorithmProperty };
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
