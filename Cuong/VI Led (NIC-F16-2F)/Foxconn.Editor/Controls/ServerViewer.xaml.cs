using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Foxconn.Editor.Controls
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
                grid.RowDefinitions.Add(new RowDefinition());
            }
            for (int y = 0; y < columns; y++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            int i = 0;
            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < columns; y++)
                {
                    #region Context Menu
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
                    #endregion

                    #region Border
                    Border border = new Border
                    {
                        Name = $"SERVER_{i}",
                        CornerRadius = new CornerRadius(4),
                        BorderThickness = new Thickness(4),
                        BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF595959"),
                        Margin = new Thickness(1),
                        Background = Brushes.Transparent,
                        ContextMenu = contextMenu
                    };
                    Grid.SetRow(border, x);
                    Grid.SetColumn(border, y);

                    #region Grid
                    Grid gridInside = new Grid();
                    gridInside.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });
                    gridInside.RowDefinitions.Add(new RowDefinition { MaxHeight = 250 });
                    gridInside.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });

                    #region Row0
                    Label lblInfo = new Label
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
                    Grid.SetRow(lblInfo, 0);
                    #endregion

                    #region Row1
                    Label lblStatus = new Label
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
                    Grid.SetRow(lblStatus, 1);
                    #endregion

                    #region Row2
                    Grid gridMoreInfo = new Grid();
                    gridMoreInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    gridMoreInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    gridMoreInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    Label lblTime = new Label
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
                    Grid.SetColumn(lblTime, 0);

                    Label lblPass = new Label
                    {
                        Name = $"GOLDEN_SAMPLE_PASS_{i}",
                        Content = "",
                        FontSize = 12,
                        FontWeight = FontWeights.Normal,
                        Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFF0F0F0"),
                        Background = Brushes.Transparent,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 4, 0)
                    };
                    Grid.SetColumn(lblPass, 1);

                    Label lblFail = new Label
                    {
                        Name = $"GOLDEN_SAMPLE_FAIL_{i}",
                        Content = "",
                        FontSize = 12,
                        FontWeight = FontWeights.Normal,
                        Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFF0F0F0"),
                        Background = Brushes.Transparent,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 4, 0)
                    };
                    Grid.SetColumn(lblFail, 2);

                    Grid.SetRow(gridMoreInfo, 2);
                    gridMoreInfo.Children.Add(lblTime);
                    gridMoreInfo.Children.Add(lblPass);
                    gridMoreInfo.Children.Add(lblFail);
                    #endregion

                    gridInside.Children.Add(lblInfo);
                    gridInside.Children.Add(lblStatus);
                    gridInside.Children.Add(gridMoreInfo);
                    #endregion

                    border.Child = gridInside;
                    #endregion

                    grid.Children.Add(border);

                    SocketUIs.Add(new SocketTestUI
                    {
                        Id = i,
                        Box = border,
                        Info = lblInfo,
                        Status = lblStatus,
                        Time = lblTime,
                        GoldenSamplePass = lblPass,
                        GoldenSampleFail = lblFail
                    });
                    ++i;
                }
            }
        }

        private void MenuOpenVNC_Click(object sender, RoutedEventArgs e)
        {
            string name = ((MenuItem)sender).Name;
            string id = name.Replace("OPEN_VNC_", "");
            OpenVNC(Convert.ToInt32(id));
        }

        private void MenuCloseVNC_Click(object sender, RoutedEventArgs e)
        {
            string name = ((MenuItem)sender).Name;
            string id = name.Replace("CLOSE_VNC_", "");
            CloseVNC(Convert.ToInt32(id));
        }

        private void MenuOpenFixture_Click(object sender, RoutedEventArgs e)
        {
            string name = ((MenuItem)sender).Name;
            string id = name.Replace("OPEN_FIXTURE_", "");
            OpenFixture(Convert.ToInt32(id));
        }

        private void MenuCloseFixture_Click(object sender, RoutedEventArgs e)
        {
            string name = ((MenuItem)sender).Name;
            string id = name.Replace("CLOSE_FIXTURE_", "");
            CloseFixture(Convert.ToInt32(id));
        }

        private void MenuForceCloseConnection_Click(object sender, RoutedEventArgs e)
        {
            string name = ((MenuItem)sender).Name;
            string id = name.Replace("FORCE_CLOSE_CONNECTION_", "");
            ForceCloseConnection(Convert.ToInt32(id));
        }

        public void OpenVNC(int id)
        {
            DeviceManager device = DeviceManager.Current;
            device.OpenVNC(Convert.ToInt32(id));
        }

        public void CloseVNC(int id)
        {
            DeviceManager device = DeviceManager.Current;
            device.CloseVNC(Convert.ToInt32(id));
        }

        public void OpenFixture(int id)
        {
            DeviceManager device = DeviceManager.Current;
            device.OpenFixture(Convert.ToInt32(id));
        }

        public void CloseFixture(int id)
        {
            DeviceManager device = DeviceManager.Current;
            device.CloseFixture(Convert.ToInt32(id));
        }

        public void ForceCloseConnection(int id)
        {
            DeviceManager device = DeviceManager.Current;
            device.SocketForceClose(Convert.ToInt32(id));
        }
    }

    public class SocketTestUI
    {
        private int _id = 0;
        private Border _socketBox = null;
        private Label _socketInfo = null;
        private Label _socketStatus = null;
        private Label _socketTime = null;
        private Label _goldenSamplePass = null;
        private Label _goldenSampleFail = null;

        public int Id
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

        public Label GoldenSamplePass
        {
            get => _goldenSamplePass;
            set => _goldenSamplePass = value;
        }

        public Label GoldenSampleFail
        {
            get => _goldenSampleFail;
            set => _goldenSampleFail = value;
        }
    }
}
