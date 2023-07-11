using Foxconn.Editor.Configuration;
using System.ComponentModel;
using System.Windows;

namespace Foxconn.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for NewProgramWizard.xaml
    /// </summary>
    public partial class NewProgramDialog : Window, INotifyPropertyChanged
    {
        #region Binding Property
        private Board _program = new Board();
        private string _boardName = "PROGRAM";
        private double _boardLength = 100;
        private double _boardWidth = 100;
        private double _boardThickness = 1;

        public Board NewProgram => _program;

        public string BoardName
        {
            get => _boardName;
            set => _boardName = value;
        }

        public double BoardLength
        {
            get => _boardLength;
            set => _boardLength = value;
        }

        public double BoardWidth
        {
            get => _boardWidth;
            set => _boardWidth = value;
        }

        public double BoardThickness
        {
            get => _boardThickness;
            set => _boardThickness = value;
        }

        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public NewProgramDialog()
        {
            InitializeComponent();
            Title = "NewProgramDialog";
            Owner = Application.Current.MainWindow;
            DataContext = this;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            _program.Name = _boardName;
            _program.BoardLength = _boardLength;
            _program.BoardWidth = _boardWidth;
            _program.BoardThickness = _boardThickness;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
