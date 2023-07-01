using Foxconn.AOI.Editor.OpenCV;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Foxconn.AOI.Editor.Controls
{
    /// <summary>
    /// Interaction logic for CvHSVExtractionControl.xaml
    /// </summary>
    public partial class CvHSVExtractionControl : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty HueProperty = DependencyProperty.Register("Hue", typeof(ValueRange), typeof(CvHSVExtractionControl));
        public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register("Saturation", typeof(ValueRange), typeof(CvHSVExtractionControl));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(ValueRange), typeof(CvHSVExtractionControl));
        public static readonly DependencyProperty OKRangeProperty = DependencyProperty.Register("OKRange", typeof(ValueRange), typeof(CvHSVExtractionControl));
        public static readonly DependencyProperty IsEnabledReverseSearchProperty = DependencyProperty.Register("IsEnabledReverseSearch", typeof(bool), typeof(CvHSVExtractionControl));
        public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register("Score", typeof(double), typeof(CvHSVExtractionControl));

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

        public double Score
        {
            get => (double)GetValue(ScoreProperty);
            set => SetValue(ScoreProperty, value);
        }

        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public CvHSVExtractionControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetParameters(CvHSVExtraction param)
        {
            string[] paths = new string[] { "Hue", "Saturation", "Value", "OKRange", "IsEnabledReverseSearch" };
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
