using Foxconn.AOI.Editor.OpenCV;
using System.ComponentModel;
using System.Windows.Controls;

namespace Foxconn.AOI.Editor.Controls
{
    /// <summary>
    /// Interaction logic for CvDeepLearningControl.xaml
    /// </summary>
    public partial class CvDeepLearningControl : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
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
            //string[] paths = new string[] { "ID" };
            //DependencyProperty[] properties = new DependencyProperty[] { IDProperty };
            //for (int i = 0; i < paths.Length; i++)
            //{
            //    Binding binding = new Binding(paths[i])
            //    {
            //        Source = param,
            //        Mode = BindingMode.TwoWay,
            //        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            //    };
            //    SetBinding(properties[i], binding);
            //}
            //NotifyPropertyChanged();
        }
    }
}
