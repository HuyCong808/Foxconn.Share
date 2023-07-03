using Foxconn.Editor.OpenCV;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Foxconn.Editor.Controls
{
    /// <summary>
    /// Interaction logic for CvLuminanceExtractionControl.xaml
    /// </summary>
    public partial class CvLuminanceExtractionControl : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty ThresholdProperty = DependencyProperty.Register("Threshold", typeof(ValueRange), typeof(CvLuminanceExtractionControl));
        public static readonly DependencyProperty OKRangeProperty = DependencyProperty.Register("OKRange", typeof(ValueRange), typeof(CvLuminanceExtractionControl));
        public static readonly DependencyProperty IsEnabledReverseSearchProperty = DependencyProperty.Register("IsEnabledReverseSearch", typeof(bool), typeof(CvLuminanceExtractionControl));
        public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register("Score", typeof(double), typeof(CvLuminanceExtractionControl));

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

        public CvLuminanceExtractionControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetParameters(CvLuminanceExtraction param)
        {
            string[] paths = new string[] { "Threshold", "OKRange", "IsEnabledReverseSearch", "Score" };
            DependencyProperty[] properties = new DependencyProperty[] { ThresholdProperty, OKRangeProperty, IsEnabledReverseSearchProperty, ScoreProperty };
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
