using System.IO;
using System.Windows;

namespace Foxconn.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        #region Binding Property
        private string _productName = "Foxconn.Editor";
        private string _buildTime = $"Time: {new FileInfo(Assembly.AssemblyProduct + ".exe").LastWriteTime:dddd, dd MMMM yyyy hh:mm:ss tt}";
        private string _version = "Version: 1.0.0.0 (x64)";
        private string _copyright = "Copyright: © 2017-2023, Nguyen Quang Tiep";
        private string _phone = "Phone: (+84)90 29 65789";
        private string _email = "Email: quang-tiep.nguyen@mail.foxconn.com";

        public string ProductName
        {
            get => _productName;
            set => _productName = value;
        }

        public string BuildTime
        {
            get => _buildTime;
            set => _buildTime = value;
        }

        public string Version
        {
            get => _version;
            set => _version = value;
        }

        public string Copyright
        {
            get => _copyright;
            set => _copyright = value;
        }

        public string Phone
        {
            get => _phone;
            set => _phone = value;
        }

        public string Email
        {
            get => _email;
            set => _email = value;
        }
        #endregion

        public AboutDialog()
        {
            InitializeComponent();
            Title = "About";
            Owner = Application.Current.MainWindow;
            DataContext = this;
        }
    }
}
