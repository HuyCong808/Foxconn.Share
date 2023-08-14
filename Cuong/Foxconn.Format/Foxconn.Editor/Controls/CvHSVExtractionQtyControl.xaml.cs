using Foxconn.Editor.OpenCV;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Foxconn.Editor.Controls
{
    /// <summary>
    /// Interaction logic for CvHSVExtractionControl.xaml
    /// </summary>
    public partial class CvHSVExtractionQtyControl : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty HueProperty = DependencyProperty.Register("Hue", typeof(ValueRange), typeof(CvHSVExtractionQtyControl));
        public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register("Saturation", typeof(ValueRange), typeof(CvHSVExtractionQtyControl));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(ValueRange), typeof(CvHSVExtractionQtyControl));
        public static readonly DependencyProperty OKRangeProperty = DependencyProperty.Register("OKRange", typeof(ValueRange), typeof(CvHSVExtractionQtyControl));
        public static readonly DependencyProperty IsEnabledReverseSearchProperty = DependencyProperty.Register("IsEnabledReverseSearch", typeof(bool), typeof(CvHSVExtractionQtyControl));
        public static readonly DependencyProperty QtyProperty = DependencyProperty.Register("Qty", typeof(double), typeof(CvHSVExtractionQtyControl));

        public ValueRange Hue
        {
            get => (ValueRange)GetValue(HueProperty);
            set => SetValue(HueProperty, value);
        }

        public ValueRange Saturation
        {
            get => (ValueRange)GetValue(SaturationProperty);
            set => SetValue(SaturationProperty, value);
        }

        public ValueRange Value
        {
            get => (ValueRange)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public ValueRange OKRange
        {
            get => (ValueRange)GetValue(OKRangeProperty);
            set => SetValue(OKRangeProperty, value);
        }

        public bool IsEnabledReverseSearch
        {
            get => (bool)GetValue(IsEnabledReverseSearchProperty);
            set => SetValue(IsEnabledReverseSearchProperty, value);
        }

        public double Qty
        {
            get => (double)GetValue(QtyProperty);
            set => SetValue(QtyProperty, value);
        }

        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public CvHSVExtractionQtyControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetParameters(CvHSVExtractionQty param)
        {
            string[] paths = new string[] { "Hue", "Saturation", "Value", "OKRange", "IsEnabledReserveSearch" };
            DependencyProperty[] properties = new DependencyProperty[] { HueProperty, SaturationProperty, ValueProperty, OKRangeProperty, IsEnabledReverseSearchProperty };
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
