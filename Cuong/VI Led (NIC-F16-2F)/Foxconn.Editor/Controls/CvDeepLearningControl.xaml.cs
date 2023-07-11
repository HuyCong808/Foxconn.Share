using Foxconn.Editor.Converters;
using Foxconn.Editor.OpenCV;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Foxconn.Editor.Controls
{
    /// <summary>
    /// Interaction logic for CvDeepLearningControl.xaml
    /// </summary>
    public partial class CvDeepLearningControl : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty TemplateImageProperty = DependencyProperty.Register("TemplateImage", typeof(ImageSource), typeof(CvDeepLearningControl));
        public static readonly DependencyProperty OKRangeProperty = DependencyProperty.Register("OKRange", typeof(ValueRange), typeof(CvDeepLearningControl));
        public static readonly DependencyProperty IsEnabledReverseSearchProperty = DependencyProperty.Register("IsEnabledReverseSearch", typeof(bool), typeof(CvDeepLearningControl));
        public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register("Score", typeof(double), typeof(CvDeepLearningControl));

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

        public CvDeepLearningControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetParameters(CvDeepLearning param)
        {
            string[] paths = new string[] { "Template", "OKRange", "IsEnabledReverseSearch" };
            DependencyProperty[] properties = new DependencyProperty[] { TemplateImageProperty, OKRangeProperty, IsEnabledReverseSearchProperty };
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
