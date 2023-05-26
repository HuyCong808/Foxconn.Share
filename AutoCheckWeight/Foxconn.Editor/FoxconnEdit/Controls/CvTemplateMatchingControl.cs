
using Foxconn.Editor.Configuration;
using Foxconn.Editor.FoxconnEdit.Converts;
using Foxconn.Editor.FoxconnEdit.OpenCV;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Foxconn.AOI.Editor.Controls
{
    /// <summary>
    /// Interaction logic for CvTemplateMatchingControl.xaml
    /// </summary>
    public partial class CvTemplateMatchingControl : UserControl
    {
        #region Binding Property
        public static readonly DependencyProperty TemplateImageProperty = DependencyProperty.Register("TemplateImage", typeof(ImageSource), typeof(CvTemplateMatchingControl));
        public static readonly DependencyProperty OKRangeProperty = DependencyProperty.Register("OKRange", typeof(ValueRange), typeof(CvTemplateMatchingControl));
        public static readonly DependencyProperty IsEnabledReverseSearchProperty = DependencyProperty.Register("IsEnabledReverseSearch", typeof(bool), typeof(CvTemplateMatchingControl));
        public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register("Score", typeof(double), typeof(CvTemplateMatchingControl));
        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register("Center", typeof(JPoint), typeof(CvTemplateMatchingControl));

        public ImageSource TemplateImage
        {
            get => (ImageSource)GetValue(TemplateImageProperty);
            set => SetValue(TemplateImageProperty, value);
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
        public CvTemplateMatchingControl()
        {
            DataContext = this;
        }
        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public void SetParameters(CvTemplateMatching param)
        {
            string[] paths = new string[] { "Template", "OKRange", "IsEnabledReverseSearch", "Score", "Center" };
            DependencyProperty[] properties = new DependencyProperty[] { TemplateImageProperty, OKRangeProperty, IsEnabledReverseSearchProperty, ScoreProperty, CenterProperty };
            for (int i = 0; i < paths.Length; i++)
            {
                Binding binding = new Binding(paths[i])
                {
                    Source = param,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                if (paths[i] == "Template")
                {
                    binding.Converter = new OpenCVImageToBitmapSourceConverter();
                }
                SetBinding(properties[i], binding);
            }
            NotifyPropertyChanged();
        }
    }
}
