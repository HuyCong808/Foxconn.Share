using Foxconn.AOI.Editor.Configuration;
using System.Windows;

namespace Foxconn.AOI.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for NewProgramWizard.xaml
    /// </summary>
    public partial class NewProgramWizard : Window
    {
        private Board _program = new Board();
        private bool _created = false;
        private int _selectedPageIndex = 1;
        private string _boardName = "PROGRAM";
        private double _boardLength = 100;
        private double _boardWidth = 100;
        private double _boardThickness = 1;

        public Board NewProgram => _program;

        public bool Created
        {
            get => _created;
            set => _created = value;
        }

        public int SelectedPageIndex
        {
            get => _selectedPageIndex;
            set => _selectedPageIndex = value;
        }

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

        public NewProgramWizard()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            DataContext = this;
        }

        private void btnBackButton_Click(object sender, RoutedEventArgs e)
        {
            NagivationPage(_selectedPageIndex - 1);
        }

        private void btnNextButton_Click(object sender, RoutedEventArgs e)
        {
            NagivationPage(_selectedPageIndex + 1);
        }

        private void btnCancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rdoWithCADFile_Checked(object sender, RoutedEventArgs e)
        {
            bdWithCADFile.Visibility = Visibility.Hidden;
            bdNoCADFile.Visibility = Visibility.Visible;
        }

        private void rdoNoCADFile_Checked(object sender, RoutedEventArgs e)
        {
            bdWithCADFile.Visibility = Visibility.Visible;
            bdNoCADFile.Visibility = Visibility.Hidden;
        }

        private void btnBrowseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBOMBrowseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rdoCADFormat_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void rdoPPFormat_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnNewFormatButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDeleteFormatButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditFormatButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NagivationPage(int pageID)
        {
            if (pageID >= 1 && pageID <= 5)
            {
                if (rdoWithCADFile.IsChecked == true)
                {
                    ShowPage(pageID);
                }
                if (rdoNoCADFile.IsChecked == true)
                {
                    if (pageID == 2)
                    {
                        ShowPage(4);
                    }
                    else if (pageID == 3)
                    {
                        ShowPage(1);
                    }
                    else
                    {
                        ShowPage(pageID);
                    }
                }
            }
            else if (pageID == 6)
            {
                _created = true;
                _program.Name = _boardName;
                _program.BoardLength = _boardLength;
                _program.BoardWidth = _boardWidth;
                _program.BoardThickness = _boardThickness;
                Close();
            }
        }

        private void ShowPage(int pageID)
        {
            _selectedPageIndex = pageID;
            switch (pageID)
            {
                case 1:
                    Page1.Visibility = Visibility.Visible;
                    Page2.Visibility = Visibility.Hidden;
                    Page3.Visibility = Visibility.Hidden;
                    Page4.Visibility = Visibility.Hidden;
                    Page5.Visibility = Visibility.Hidden;
                    break;
                case 2:
                    Page1.Visibility = Visibility.Hidden;
                    Page2.Visibility = Visibility.Visible;
                    Page3.Visibility = Visibility.Hidden;
                    Page4.Visibility = Visibility.Hidden;
                    Page5.Visibility = Visibility.Hidden;
                    break;
                case 3:
                    Page1.Visibility = Visibility.Hidden;
                    Page2.Visibility = Visibility.Hidden;
                    Page3.Visibility = Visibility.Visible;
                    Page4.Visibility = Visibility.Hidden;
                    Page5.Visibility = Visibility.Hidden;
                    break;
                case 4:
                    Page1.Visibility = Visibility.Hidden;
                    Page2.Visibility = Visibility.Hidden;
                    Page3.Visibility = Visibility.Hidden;
                    Page4.Visibility = Visibility.Visible;
                    Page5.Visibility = Visibility.Hidden;
                    break;
                case 5:
                    Page1.Visibility = Visibility.Hidden;
                    Page2.Visibility = Visibility.Hidden;
                    Page3.Visibility = Visibility.Hidden;
                    Page4.Visibility = Visibility.Hidden;
                    Page5.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }
    }
}
