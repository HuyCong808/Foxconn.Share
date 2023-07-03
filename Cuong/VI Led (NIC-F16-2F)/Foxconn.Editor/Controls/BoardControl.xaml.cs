using Foxconn.Editor.Configuration;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Foxconn.Editor.Controls
{
    /// <summary>
    /// Interaction logic for BoardControl.xaml
    /// </summary>
    public partial class BoardControl : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty BoardNameProperty = DependencyProperty.Register("BoardName", typeof(string), typeof(BoardControl));
        public static readonly DependencyProperty BoardLengthProperty = DependencyProperty.Register("BoardLength", typeof(double), typeof(BoardControl));
        public static readonly DependencyProperty BoardWidthProperty = DependencyProperty.Register("BoardWidth", typeof(double), typeof(BoardControl));
        public static readonly DependencyProperty BoardThicknessProperty = DependencyProperty.Register("BoardThickness", typeof(double), typeof(BoardControl));

        public string BoardName
        {
            get => (string)GetValue(BoardNameProperty);
            set => SetValue(BoardNameProperty, value);
        }

        public double BoardLength
        {
            get => (double)GetValue(BoardLengthProperty);
            set => SetValue(BoardLengthProperty, value);
        }

        public double BoardWidth
        {
            get => (double)GetValue(BoardWidthProperty);
            set => SetValue(BoardWidthProperty, value);
        }

        public double BoardThickness
        {
            get => (double)GetValue(BoardThicknessProperty);
            set => SetValue(BoardThicknessProperty, value);
        }

        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public BoardControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetParameters(Board param)
        {
            string[] paths = new string[] { "Name", "BoardLength", "BoardWidth", "BoardThickness" };
            DependencyProperty[] properties = new DependencyProperty[] { BoardNameProperty, BoardLengthProperty, BoardWidthProperty, BoardThicknessProperty };
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
