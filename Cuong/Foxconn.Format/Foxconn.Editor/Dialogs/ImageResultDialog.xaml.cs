
using System;
using System.ComponentModel;
using System.Windows;

namespace Foxconn.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for ImageResultDialogs.xaml
    /// </summary>
    public partial class ImageResultDialog : Window, INotifyPropertyChanged
    {

        public ImageResultDialog()
        {
            InitializeComponent();
        }

        #region Binding Property
        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public virtual void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {


        }

        private void Window_Closed(object sender, EventArgs e)
        {
            AutoRunDialog autoRun = AutoRunDialog.Current;
            autoRun.IsOpenResultDialog = false;
        }

        private void imbImageResult_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (imbImageResult.Image != null)
            {
                double newZoom = imbImageResult.ZoomScale * (e.Delta > 0 ? 1.2 : 0.8);
                newZoom = Math.Max(0.5, Math.Min(5, newZoom)); // gioi han ti le zoom
                imbImageResult.SetZoomScale(newZoom, e.Location);
            }
        }
    }
}
