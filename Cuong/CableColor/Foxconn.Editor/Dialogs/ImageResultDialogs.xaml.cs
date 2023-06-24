using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.Editor.Configuration;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;

namespace Foxconn.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for ImageResultDialogs.xaml
    /// </summary>
    public partial class ImageResultDialogs : Window, INotifyPropertyChanged
    {
        private Board _program
        {
            get => ProgramManager.Current.Program;
            set => ProgramManager.Current.Program = value;
        }
        private SelectionMouse _mouse { get; set; }
        private Image<Bgr, byte> _image { get; set; }
        AutoRun autoRun = new AutoRun();


        public ImageResultDialogs()
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


        public void LoadImage()
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadImage();

        }



    }
}
