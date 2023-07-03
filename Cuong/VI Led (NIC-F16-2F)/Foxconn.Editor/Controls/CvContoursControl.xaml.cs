using Foxconn.Editor.Configuration;
using Foxconn.Editor.OpenCV;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Foxconn.Editor.Controls
{
    /// <summary>
    /// Interaction logic for CvContoursControl.xaml
    /// </summary>
    public partial class CvContoursControl : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty ThresholdProperty = DependencyProperty.Register("Threshold", typeof(ValueRange), typeof(CvContoursControl));
        public static new readonly DependencyProperty WidthProperty = DependencyProperty.Register("Width", typeof(ValueRange), typeof(CvContoursControl));
        public static new readonly DependencyProperty HeightProperty = DependencyProperty.Register("Height", typeof(ValueRange), typeof(CvContoursControl));
        public static readonly DependencyProperty P0Property = DependencyProperty.Register("P0", typeof(JPoint), typeof(CvContoursControl));
        public static readonly DependencyProperty P1Property = DependencyProperty.Register("P1", typeof(JPoint), typeof(CvContoursControl));
        public static readonly DependencyProperty P2Property = DependencyProperty.Register("P2", typeof(JPoint), typeof(CvContoursControl));
        public static readonly DependencyProperty P3Property = DependencyProperty.Register("P3", typeof(JPoint), typeof(CvContoursControl));

        public ValueRange Threhold
        {
            get => (ValueRange)GetValue(ThresholdProperty);
            set => SetValue(ThresholdProperty, value);
        }

        public new ValueRange Width
        {
            get => (ValueRange)GetValue(WidthProperty);
            set => SetValue(WidthProperty, value);
        }

        public new ValueRange Height
        {
            get => (ValueRange)GetValue(HeightProperty);
            set => SetValue(HeightProperty, value);
        }

        public JPoint P0
        {
            get => (JPoint)GetValue(P0Property);
            set => SetValue(P0Property, value);
        }

        public JPoint P1
        {
            get => (JPoint)GetValue(P1Property);
            set => SetValue(P1Property, value);
        }

        public JPoint P2
        {
            get => (JPoint)GetValue(P2Property);
            set => SetValue(P2Property, value);
        }

        public JPoint P3
        {
            get => (JPoint)GetValue(P3Property);
            set => SetValue(P3Property, value);
        }

        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public CvContoursControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetParameters(CvContours param)
        {
            string[] paths = new string[] { "Threshold", "Width", "Height", "P0", "P1", "P2", "P3" };
            DependencyProperty[] properties = new DependencyProperty[] { ThresholdProperty, WidthProperty, HeightProperty, P0Property, P1Property, P2Property, P3Property };
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
