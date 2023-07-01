using Foxconn.AOI.Editor.Configuration;
using Foxconn.AOI.Editor.OpenCV;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Foxconn.AOI.Editor.Controls
{
    /// <summary>
    /// Interaction logic for CvMarkTracingControl.xaml
    /// </summary>
    public partial class CvMarkTracingControl : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty ThresholdProperty = DependencyProperty.Register("Threshold", typeof(ValueRange), typeof(CvMarkTracingControl));
        public static readonly DependencyProperty OKRangeProperty = DependencyProperty.Register("OKRange", typeof(ValueRange), typeof(CvMarkTracingControl));
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(CvMarkTracingControl));
        public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register("Score", typeof(double), typeof(CvMarkTracingControl));
        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register("Center", typeof(JPoint), typeof(CvMarkTracingControl));

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

        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        public JPoint Center
        {
            get => (JPoint)GetValue(CenterProperty);
            set => SetValue(CenterProperty, value);
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

        public CvMarkTracingControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetParameters(CvMarkTracing param)
        {
            string[] paths = new string[] { "Threshold", "OKRange", "Radius", "Center" };
            DependencyProperty[] properties = new DependencyProperty[] { ThresholdProperty, OKRangeProperty, RadiusProperty, CenterProperty };
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
