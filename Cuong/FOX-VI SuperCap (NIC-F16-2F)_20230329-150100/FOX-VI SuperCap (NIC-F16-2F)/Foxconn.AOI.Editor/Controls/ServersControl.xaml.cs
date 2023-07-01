using Foxconn.AOI.Editor.Configuration;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Foxconn.AOI.Editor.Controls
{
    /// <summary>
    /// Interaction logic for ServerConfigurationControl.xaml
    /// </summary>
    public partial class ServerConfigurationControl : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty ServersProperty = DependencyProperty.Register("Servers", typeof(ObservableCollection<Server>), typeof(ServerConfigurationControl));
        public static readonly DependencyProperty IDProperty = DependencyProperty.Register("ID", typeof(int), typeof(ServerConfigurationControl));
        public static new readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(ServerConfigurationControl));
        public static new readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(ServerConfigurationControl));
        public static readonly DependencyProperty LocalHostProperty = DependencyProperty.Register("LocalHost", typeof(string), typeof(ServerConfigurationControl));
        public static readonly DependencyProperty LocalPortProperty = DependencyProperty.Register("LocalPort", typeof(int), typeof(ServerConfigurationControl));
        public static readonly DependencyProperty VNCHostProperty = DependencyProperty.Register("VNCHost", typeof(string), typeof(ServerConfigurationControl));
        public static readonly DependencyProperty VNCPasswordProperty = DependencyProperty.Register("VNCPassword", typeof(string), typeof(ServerConfigurationControl));

        public ObservableCollection<Server> Servers
        {
            get => (ObservableCollection<Server>)GetValue(ServersProperty);
            set
            {
                SetValue(ServersProperty, value);
                UpdateServers();
            }
        }

        public int ID
        {
            get => (int)GetValue(IDProperty);
            set => SetValue(IDProperty, value);
        }

        public new int Name
        {
            get => (int)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        public new bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        public string LocalHost
        {
            get => (string)GetValue(LocalHostProperty);
            set => SetValue(LocalHostProperty, value);
        }

        public int LocalPort
        {
            get => (int)GetValue(LocalPortProperty);
            set => SetValue(LocalPortProperty, value);
        }

        public string VNCHost
        {
            get => (string)GetValue(VNCHostProperty);
            set => SetValue(VNCHostProperty, value);
        }

        public string VNCPassword
        {
            get => (string)GetValue(VNCPasswordProperty);
            set => SetValue(VNCPasswordProperty, value);
        }

        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public ServerConfigurationControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void btnAddServerButton_Click(object sender, RoutedEventArgs e)
        {
            int id = Servers.Count;
            if (MessageBox.Show($"Are you sure want to add new server SERVER_{id}?", "Add", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
            {
                Server item = new Server
                {
                    ID = id,
                    Name = $"SERVER_{id}",
                    IsEnabled = true,
                    LocalHost = "localhost",
                    LocalPort = 27000 + id
                };
                Servers.Add(item);
                UpdateServers();
            }
        }

        private void btnRemoveServerButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = cmbServerArray.SelectedIndex;
            if (selectedIndex > -1)
            {
                if (MessageBox.Show($"Are you sure want to remove the server {Servers[selectedIndex].Name}?", "Remove Server", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                {
                    Servers.RemoveAt(selectedIndex);
                    UpdateServers();
                }
            }
        }

        private void cmbServerArray_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = cmbServerArray.SelectedIndex;
            if (selectedIndex > -1)
            {
                SetParameters(Servers[selectedIndex]);
            }
        }

        private void UpdateServers()
        {
            cmbServerArray.Items.Clear();
            if (Servers.Count > 0)
            {
                for (int i = 0; i < Servers.Count; i++)
                    cmbServerArray.Items.Add(Servers[i].Name);
            }
        }

        private void SetParameters(Server param)
        {
            string[] paths = new string[] { "ID", "Name", "IsEnabled", "LocalHost", "LocalPort", "VNCHost", "VNCPassword" };
            DependencyProperty[] properties = new DependencyProperty[] { IDProperty, NameProperty, IsEnabledProperty, LocalHostProperty, LocalPortProperty, VNCHostProperty, VNCPasswordProperty };
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
