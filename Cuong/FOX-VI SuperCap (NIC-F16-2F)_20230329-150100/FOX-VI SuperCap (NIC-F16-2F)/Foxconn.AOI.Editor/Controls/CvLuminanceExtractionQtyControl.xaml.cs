using Foxconn.AOI.Editor.OpenCV;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Foxconn.AOI.Editor.Controls
{
    /// <summary>
    /// Interaction logic for CvLuminanceExtractionQtyControl.xaml
    /// </summary>
    public partial class CvLuminanceExtractionQtyControl : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty ThresholdProperty = DependencyProperty.Register("Threshold", typeof(ValueRange), typeof(CvLuminanceExtractionQtyControl));
        public static readonly DependencyProperty OKRangeProperty = DependencyProperty.Register("OKRange", typeof(ValueRange), typeof(CvLuminanceExtractionQtyControl));
        public static readonly DependencyProperty IsEnabledReverseSearchProperty = DependencyProperty.Register("IsEnabledReverseSearch", typeof(bool), typeof(CvLuminanceExtractionQtyControl));
        public static readonly DependencyProperty QtyProperty = DependencyProperty.Register("Qty", typeof(double), typeof(CvLuminanceExtractionQtyControl));

        public ValueRange Threshold
        {
            get => (ValueRange)GetValue(ThresholdProperty);
            set => SetValue(ThresholdProperty, value);
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

        public CvLuminanceExtractionQtyControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetParameters(CvLuminanceExtractionQty param)
        {
            string[] paths = new string[] { "Threshold", "OKRange", "IsEnabledReverseSearch", "Qty" };
            DependencyProperty[] properties = new DependencyProperty[] { ThresholdProperty, OKRangeProperty, IsEnabledReverseSearchProperty, QtyProperty };
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
