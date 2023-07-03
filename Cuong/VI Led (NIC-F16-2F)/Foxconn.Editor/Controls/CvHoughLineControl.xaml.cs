﻿using Foxconn.Editor.OpenCV;
using System.ComponentModel;
using System.Windows.Controls;

namespace Foxconn.Editor.Controls
{
    /// <summary>
    /// Interaction logic for CvHoughLineControl.xaml
    /// </summary>
    public partial class CvHoughLineControl : UserControl, INotifyPropertyChanged
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

        public CvHoughLineControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetParameters(CvHoughLine param)
        {
            //string[] paths = new string[] { "Id" };
            //DependencyProperty[] properties = new DependencyProperty[] { IdProperty };
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
