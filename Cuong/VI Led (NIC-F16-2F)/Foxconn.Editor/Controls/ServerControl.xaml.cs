using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Foxconn.Editor.TestParams;

namespace Foxconn.Editor.Controls
{
    /// <summary>
    /// Interaction logic for ServerControl.xaml
    /// </summary>
    public partial class ServerControl : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty FixturesProperty = DependencyProperty.Register("Fixtures", typeof(List<FixtureParams>), typeof(ServerControl));
        public static readonly DependencyProperty IdProperty = DependencyProperty.Register("Id", typeof(int), typeof(ServerControl));
        public static new readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(ServerControl));
        public static new readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(ServerControl));
        public static readonly DependencyProperty LocalHostProperty = DependencyProperty.Register("Socket.Host", typeof(string), typeof(ServerControl));
        public static readonly DependencyProperty LocalPortProperty = DependencyProperty.Register("Socket.Port", typeof(int), typeof(ServerControl));
        public static readonly DependencyProperty VNCHostProperty = DependencyProperty.Register("VNC.Host", typeof(string), typeof(ServerControl));
        public static readonly DependencyProperty VNCPasswordProperty = DependencyProperty.Register("VNC.Password", typeof(string), typeof(ServerControl));

        public List<FixtureParams> Fixtures
        {
            get => (List<FixtureParams>)GetValue(FixturesProperty);
            set
            {
                SetValue(FixturesProperty, value);
                UpdateServers();
            }
        }

        public int Id
        {
            get => (int)GetValue(IdProperty);
            set => SetValue(IdProperty, value);
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

        public ServerControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void btnAddServerButton_Click(object sender, RoutedEventArgs e)
        {
            int id = Fixtures.Count;
            if (MessageBox.Show($"Do you want to add a new F{id}?", "Add", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
            {
                FixtureParams f = new FixtureParams
                {
                    Id = id,
                    Name = $"F{id}",
                    IsEnabled = true
                };
                f.Socket.Host = "localhost";
                f.Socket.Port = 27000 + id;
                Fixtures.Add(f);
                UpdateServers();
            }
        }

        private void btnRemoveServerButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = cmbFixtures.SelectedIndex;
            if (selectedIndex > -1)
            {
                if (MessageBox.Show($"Do you want to remove {Fixtures[selectedIndex].Name}?", "Remove", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                {
                    Fixtures.RemoveAt(selectedIndex);
                    UpdateServers();
                }
            }
        }

        private void cmbServerArray_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = cmbFixtures.SelectedIndex;
            if (selectedIndex > -1)
            {
                SetParameters(Fixtures[selectedIndex]);
            }
        }

        private void UpdateServers()
        {
            cmbFixtures.Items.Clear();
            if (Fixtures.Count > 0)
            {
                for (int i = 0; i < Fixtures.Count; i++)
                    cmbFixtures.Items.Add(Fixtures[i].Name);
            }
        }

        private void SetParameters(FixtureParams param)
        {
            string[] paths = new string[] { "Id", "Name", "IsEnabled", "Socket.Host", "Socket.Port", "VNC.Host", "VNC.Password" };
            DependencyProperty[] properties = new DependencyProperty[] { IdProperty, NameProperty, IsEnabledProperty, LocalHostProperty, LocalPortProperty, VNCHostProperty, VNCPasswordProperty };
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
