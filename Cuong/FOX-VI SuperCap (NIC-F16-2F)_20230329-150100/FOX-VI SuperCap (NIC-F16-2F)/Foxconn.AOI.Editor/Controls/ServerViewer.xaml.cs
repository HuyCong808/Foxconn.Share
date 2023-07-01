using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Foxconn.AOI.Editor.Controls
{
    /// <summary>
    /// Interaction logic for ServerViewer.xaml
    /// </summary>
    public partial class ServerViewer : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty SocketUIProperty = DependencyProperty.Register("SocketUIs", typeof(List<SocketTestUI>), typeof(ServerViewer), new PropertyMetadata(new List<SocketTestUI>()));

        public List<SocketTestUI> SocketUIs
        {
            get => (List<SocketTestUI>)GetValue(SocketUIProperty);
            set => SetValue(SocketUIProperty, value);
        }

        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public ServerViewer()
        {
            InitializeComponent();
            InitUI();
            DataContext = this;
        }

        private void InitUI()
        {
            int rows = 6;
            int columns = 5;
            for (int x = 0; x < rows; x++)
            {
                grMain.RowDefinitions.Add(new RowDefinition());
            }
            for (int y = 0; y < columns; y++)
            {
                grMain.ColumnDefinitions.Add(new ColumnDefinition());
            }
            int i = 0;
            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < columns; y++)
                {
                    ContextMenu contextMenu = new ContextMenu();
                    MenuItem mnuiOpenVNC = new MenuItem() { Header = "Open VNC", Name = $"OPEN_VNC_{i}" };
                    mnuiOpenVNC.Click += MenuOpenVNC_Click;
                    MenuItem mnuiCloseVNC = new MenuItem() { Header = "Close VNC", Name = $"CLOSE_VNC_{i}" };
                    mnuiCloseVNC.Click += MenuCloseVNC_Click;
                    Separator mnuiSpace1 = new Separator();
                    MenuItem mnuiOpenFixture = new MenuItem() { Header = "Open Fixture", Name = $"OPEN_FIXTURE_{i}" };
                    mnuiOpenFixture.Click += MenuOpenFixture_Click;
                    MenuItem mnuiCloseFixture = new MenuItem() { Header = "Close Fixture", Name = $"CLOSE_FIXTURE_{i}" };
                    mnuiCloseFixture.Click += MenuCloseFixture_Click;
                    Separator mnuiSpace2 = new Separator();
                    MenuItem mnuiForceCloseConnection = new MenuItem() { Header = "Force Close Connection", Name = $"FORCE_CLOSE_CONNECTION_{i}" };
                    mnuiForceCloseConnection.Click += MenuForceCloseConnection_Click;
                    contextMenu.Items.Add(mnuiOpenVNC);
                    contextMenu.Items.Add(mnuiCloseVNC);
                    contextMenu.Items.Add(mnuiSpace1);
                    contextMenu.Items.Add(mnuiOpenFixture);
                    contextMenu.Items.Add(mnuiCloseFixture);
                    contextMenu.Items.Add(mnuiSpace2);
                    contextMenu.Items.Add(mnuiForceCloseConnection);

                    Border bd = new Border
                    {
                        Name = $"SERVER_{i}",
                        CornerRadius = new CornerRadius(4),
                        BorderThickness = new Thickness(4),
                        BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF595959"),
                        Margin = new Thickness(1),
                        Background = Brushes.Transparent,
                        ContextMenu = contextMenu
                    };
                    Grid.SetRow(bd, x);
                    Grid.SetColumn(bd, y);

                    Grid gr = new Grid();
                    gr.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });
                    gr.RowDefinitions.Add(new RowDefinition { MaxHeight = 250 });
                    gr.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });

                    Label info = new Label
                    {
                        Name = $"SERVER_INFO_{i}",
                        Content = $"xxx.xxx.xxx.xxx:xxxx",
                        FontSize = 12,
                        FontWeight = FontWeights.Normal,
                        Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFF0F0F0"),
                        Background = Brushes.Transparent,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(4, 0, 0, 0)
                    };
                    Grid.SetRow(info, 0);

                    Label status = new Label
                    {
                        Name = $"SERVER_STATUS_{i}",
                        Content = "-----",
                        FontSize = 24,
                        FontWeight = FontWeights.Normal,
                        Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFF0F0F0"),
                        Background = Brushes.Transparent,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0)
                    };
                    Grid.SetRow(status, 1);

                    Label time = new Label
                    {
                        Name = $"SERVER_TIME_{i}",
                        Content = "Time: 0(s)",
                        FontSize = 12,
                        FontWeight = FontWeights.Normal,
                        Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFF0F0F0"),
                        Background = Brushes.Transparent,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(4, 0, 0, 0)
                    };
                    Grid.SetRow(time, 2);

                    gr.Children.Add(info);
                    gr.Children.Add(status);
                    gr.Children.Add(time);

                    bd.Child = gr;

                    grMain.Children.Add(bd);

                    SocketUIs.Add(new SocketTestUI
                    {
                        ID = i,
                        Box = bd,
                        Info = info,
                        Status = status,
                        Time = time,
                    });
                    ++i;
                }
            }
        }

        private void MenuOpenVNC_Click(object sender, RoutedEventArgs e)
        {
            string name = ((MenuItem)sender).Name;
            string id = name.Replace("OPEN_VNC_", "");
        }

        private void MenuCloseVNC_Click(object sender, RoutedEventArgs e)
        {
            string name = ((MenuItem)sender).Name;
            string id = name.Replace("CLOSE_VNC_", "");
        }

        private void MenuOpenFixture_Click(object sender, RoutedEventArgs e)
        {
            string name = ((MenuItem)sender).Name;
            string id = name.Replace("OPEN_FIXTURE_", "");
        }

        private void MenuCloseFixture_Click(object sender, RoutedEventArgs e)
        {
            string name = ((MenuItem)sender).Name;
            string id = name.Replace("CLOSE_FIXTURE_", "");
        }

        private void MenuForceCloseConnection_Click(object sender, RoutedEventArgs e)
        {
            string name = ((MenuItem)sender).Name;
            string id = name.Replace("FORCE_CLOSE_CONNECTION_", "");
        }
    }

    public class SocketTestUI
    {
        private int _id = 0;
        private Border _socketBox = null;
        private Label _socketInfo = null;
        private Label _socketStatus = null;
        private Label _socketTime = null;

        public int ID
        {
            get => _id;
            set => _id = value;
        }

        public Border Box
        {
            get => _socketBox;
            set => _socketBox = value;
        }

        public Label Info
        {
            get => _socketInfo;
            set => _socketInfo = value;
        }

        public Label Status
        {
            get => _socketStatus;
            set => _socketStatus = value;
        }

        public Label Time
        {
            get => _socketTime;
            set => _socketTime = value;
        }
    }
}
