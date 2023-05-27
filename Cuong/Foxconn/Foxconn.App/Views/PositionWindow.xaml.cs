using Foxconn.App.Controllers.Plc;
using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Foxconn.App.Models.RuntimeConfiguration;
using static Foxconn.App.Models.RuntimeConfiguration.Position;

namespace Foxconn.App.Views
{
    /// <summary>
    /// Interaction logic for PositionWindow.xaml
    /// </summary>
    public partial class PositionWindow : Window
    {
        private readonly MainWindow Root = MainWindow.Current;
        private Plc _plc { get; set; }

        public PositionWindow()
        {
            InitializeComponent();
            _plc = null;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => { Init(); });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Init()
        {
            try
            {
                Root.CodeFlow("POSITION");

                #region Image Icon
                AppUi.ShowImage(this, imgAddPosition, @"Assets/Plus.png");
                AppUi.ShowImage(this, imgRemovePosition, @"Assets/Subtract.png");
                #endregion

                #region ComboBox
                Dispatcher.Invoke(() =>
                {
                    cmbModelName.ItemsSource = Enum.GetValues(typeof(ModelName));
                    cmbModelName.Items.Refresh();
                });
                #endregion

                #region TCP/IP (PLC)
                Dispatcher.Invoke(() =>
                {
                    var plc = Root.AppManager.DatabaseManager.Basic.Plc.Devices.Find(x => x.Index == 0);
                    if (plc != null)
                    {
                        txtPlcHost.TextChanged -= txtPlcHost_TextChanged;
                        txtPlcPort.TextChanged -= txtPlcPort_TextChanged;

                        txtPlcHost.Text = plc.Host;
                        txtPlcPort.Text = plc.Port.ToString();

                        txtPlcHost.TextChanged += txtPlcHost_TextChanged;
                        txtPlcPort.TextChanged += txtPlcPort_TextChanged;
                    }
                });
                #endregion

                ShowPosition();
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void tbcmbPositions_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                ShowParameter(selectedIndex);
            }
        }

        private void tbbtnAddPosition_Click(object sender, RoutedEventArgs e)
        {
            int count = Root.AppManager.DatabaseManager.Runtime.Positions.Count;
            Root.AppManager.DatabaseManager.Runtime.Positions.Add(new Position { Index = count });
            Root.ShowMessage($"[Position] Add: {count}");
            ShowPosition();
        }

        private void tbbtnRemovePosition_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions.RemoveAt(selectedIndex);
                Root.ShowMessage($"[Position] Remove {selectedIndex}");
                ShowPosition();
            }
            else
            {
                Root.ShowMessage($"[Position] Cannot remove position.", AppColor.Red);
                AppUi.ShowMessage("Cannot remove position.", MessageBoxImage.Error);
            }
        }

        private void chkStatus_Checked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Enable = chkStatus.IsChecked == true;
                chkStatus.Content = chkStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void chkStatus_Unchecked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Enable = chkStatus.IsChecked == true;
                chkStatus.Content = chkStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void txtIndex_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Index = Utilities.ConvertStringToInt(txtIndex.Text);
            }
        }

        private void txtAlias_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Alias = txtAlias.Text;
            }
        }

        private void chkIsServer_Checked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].IsServer = chkIsServer.IsChecked == true;
                chkIsServer.Content = chkIsServer.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void chkIsServer_Unchecked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].IsServer = chkIsServer.IsChecked == true;
                chkIsServer.Content = chkIsServer.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void chkIsClient_Checked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].IsClient = chkIsClient.IsChecked == true;
                chkIsClient.Content = chkIsClient.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void chkIsClient_Unchecked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].IsClient = chkIsClient.IsChecked == true;
                chkIsClient.Content = chkIsClient.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void cmbModelName_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].ModelName = (ModelName)selectedIndex;
            }
        }

        private void txtServerHost_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Server.Host = txtServerHost.Text;
            }
        }

        private void txtServerPort_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Server.Port = Utilities.ConvertStringToInt(txtServerPort.Text);
            }
        }

        private void txtClientHost_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Client.Host = txtClientHost.Text;
            }
        }

        private void txtClientPort_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Client.Port = Utilities.ConvertStringToInt(txtClientPort.Text);
            }
        }

        private void txtPlcMovePickup_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Plc.MovePickup = txtPlcMovePickup.Text;
            }
        }

        private void txtPlcSavePickup_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Plc.SavePickup = txtPlcSavePickup.Text;
            }
        }

        private void txtPlcMoveDropdown_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Plc.MoveDropdown = txtPlcMoveDropdown.Text;
            }
        }

        private void txtPlcSaveDropdown_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Plc.SaveDropdown = txtPlcSaveDropdown.Text;
            }
        }

        private void txtPlcInit_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Plc.Status.Init = txtPlcInit.Text;
            }
        }

        private void txtPlcPass_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Plc.Status.Pass = txtPlcPass.Text;
            }
        }

        private void txtPlcFail_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Plc.Status.Fail = txtPlcFail.Text;
            }
        }

        private void txtPlcRepair_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Plc.Status.Repair = txtPlcRepair.Text;
            }
        }

        private void txtPlcInitInit_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Plc.Status.InitInit = txtPlcInitInit.Text;
            }
        }

        private void txtPlcPassPass_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Plc.Status.PassPass = txtPlcPassPass.Text;
            }
        }

        private void txtPlcFailFail_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Plc.Status.FailFail = txtPlcFailFail.Text;
            }
        }

        private void txtPlcPassFail_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Plc.Status.PassFail = txtPlcPassFail.Text;
            }
        }

        private void txtPlcFailPass_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Plc.Status.FailPass = txtPlcFailPass.Text;
            }
        }

        private void txtPlcReady_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Plc.Status.Ready = txtPlcReady.Text;
            }
        }

        private void txtRobotMovePickup_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Robot.MovePickup = txtRobotMovePickup.Text;
            }
        }

        private void txtRobotSavePickup_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Robot.SavePickup = txtRobotSavePickup.Text;
            }
        }

        private void txtRobotMoveDropdown_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Robot.SaveDropdown = txtRobotMoveDropdown.Text;
            }
        }

        private void txtRobotSaveDropdown_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Robot.SaveDropdown = txtRobotSaveDropdown.Text;
            }
        }

        private void txtRobotInit_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Robot.Status.Init = txtRobotInit.Text;
            }
        }

        private void txtRobotPass_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Robot.Status.Pass = txtRobotPass.Text;
            }
        }

        private void txtRobotFail_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Robot.Status.Fail = txtRobotFail.Text;
            }
        }

        private void txtRobotRepair_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Robot.Status.Repair = txtRobotRepair.Text;
            }
        }

        private void txtRobotReady_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Robot.Status.Ready = txtRobotReady.Text;
            }
        }

        private void txtRobotInitInit_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Robot.Status.InitInit = txtRobotInitInit.Text;
            }
        }

        private void txtRobotPassPass_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Robot.Status.PassPass = txtRobotPassPass.Text;
            }
        }

        private void txtRobotFailFail_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Robot.Status.FailFail = txtRobotFailFail.Text;
            }
        }

        private void txtRobotPassFail_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Robot.Status.PassFail = txtRobotPassFail.Text;
            }
        }

        private void txtRobotFailPass_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex].Robot.Status.FailPass = txtRobotFailPass.Text;
            }
        }

        private void txtPlcHost_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void txtPlcPort_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void txtDevice_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void txtValue_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void btnPing_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IPAddress.TryParse(txtPlcHost.Text, out IPAddress ip) && int.TryParse(txtPlcPort.Text, out int port))
                {
                    using (var client = new TcpClient())
                    {
                        client.SendTimeout = 500;
                        client.ReceiveTimeout = 500;
                        var result = client.ConnectAsync(ip, port).Wait(500);
                        if (result)
                            AppUi.ShowMessage($"Ping is successful ({ip}:{port}).", MessageBoxImage.Information);
                        else
                            AppUi.ShowMessage($"Cannot ping to ({ip}:{port}).", MessageBoxImage.Error);
                        AppUi.ShowButton(this, btnConnect, result);
                    }
                }
                else
                {
                    AppUi.ShowMessage($"Host or port is wrong format.", MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                AppUi.ShowMessage(ex.Message, MessageBoxImage.Error);
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IPAddress.TryParse(txtPlcHost.Text, out IPAddress ip) && int.TryParse(txtPlcPort.Text, out int port))
                {
                    if (_plc != null)
                        _plc = null;
                    _plc = new Plc()
                    {
                        Index = 0,
                        Alias = "",
                        Host = ip.ToString(),
                        Port = port,
                    };
                    if (_plc.StartPlc())
                    {
                        AppUi.ShowMessage($"Connected ({ip}:{port})");
                    }
                    else
                    {
                        AppUi.ShowMessage($"Cannot ping to ({ip}:{port})", MessageBoxImage.Error);
                    }
                }
                else
                {
                    AppUi.ShowMessage($"Host or port is wrong format.", MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                AppUi.ShowMessage(ex.Message, MessageBoxImage.Error);
            }
        }

        private void btnGetDevice_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        if (_plc == null)
                            AppUi.ShowMessage("Cannot get device.", MessageBoxImage.Error);

                        var device = txtDevice.Text;
                        var value = -1;
                        _plc.GetDevice(device, ref value);
                        txtValue.Text = value.ToString();
                    });
                }
                catch (Exception ex)
                {
                    AppUi.ShowMessage(ex.Message, MessageBoxImage.Error);
                }
            });
        }

        private void btnSetDevice_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        if (_plc == null)
                            AppUi.ShowMessage("Cannot set device.", MessageBoxImage.Error);

                        var value = Utilities.ConvertStringToInt(txtValue.Text);
                        var device = txtDevice.Text;
                        if (value > -1)
                            _plc.SetDevice(device, value);
                    });
                }
                catch (Exception ex)
                {
                    AppUi.ShowMessage(ex.Message, MessageBoxImage.Error);
                }
            });
        }

        private void txtPlcAllowJogging_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null)
                return;
            var message = "Do you want to allow jogging?";
            var result = AppUi.ShowMessage(message, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.AllowJogging, 1);
            }
            else if (result == MessageBoxResult.No)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.AllowJogging, 0);
            }
        }

        private void txtPlcDecreaseAxisX_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null) return;
            _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.DecreaseAxisX, 1);
        }

        private void txtPlcIncreaseAxisX_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null) return;
            _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.IncreaseAxisX, 1);
        }

        private void txtPlcDecreaseAxisY_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null) return;
            _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.DecreaseAxisY, 1);
        }

        private void txtPlcIncreaseAxisY_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null) return;
            _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.IncreaseAxisY, 1);
        }

        private void txtPlcDecreaseAxisZ_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null) return;
            _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.DecreaseAxisZ, 1);
        }

        private void txtPlcIncreaseAxisZ_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null) return;
            _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.IncreaseAxisZ, 1);
        }

        private void txtPlcDecreaseAxisR_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null) return;
            _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.ClockwiseAxisR, 1);
        }

        private void txtPlcIncreaseAxisR_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null) return;
            _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.CounterClockwiseAxisR, 1);
        }

        private void txtPlcCylinder1_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null) return;
            var message = "Do you want to control cylinder 1?\r\nYes: Suck air\r\nNo: Release air";
            var result = AppUi.ShowMessage(message, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.SuckAirCylinder0, 1);
            }
            else if (result == MessageBoxResult.No)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.ReleaseAirCylinder0, 1);
            }
        }

        private void txtPlcCylinder2_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null) return;
            var message = "Do you want to control cylinder 2?\r\nYes: Suck air\r\nNo: Release air";
            var result = AppUi.ShowMessage(message, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.SuckAirCylinder1, 1);
            }
            else if (result == MessageBoxResult.No)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.ReleaseAirCylinder1, 1);
            }
        }

        private void txtPlcVacuumPads1_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null) return;
            var message = "Do you want to control vacuum pads 12?\r\nYes: Suck air\r\nNo: Release air";
            var result = AppUi.ShowMessage(message, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.SuckAirVacuumPads0, 1);
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.SuckAirVacuumPads1, 1);
            }
            else if (result == MessageBoxResult.No)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.ReleaseAirVacuumPads0, 1);
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.ReleaseAirVacuumPads1, 1);
            }
        }

        private void txtPlcVacuumPads2_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null) return;
            var message = "Do you want to control vacuum pads 34?\r\nYes: Suck air\r\nNo: Release air";
            var result = AppUi.ShowMessage(message, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.SuckAirVacuumPads2, 1);
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.SuckAirVacuumPads3, 1);
            }
            else if (result == MessageBoxResult.No)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.ReleaseAirVacuumPads2, 1);
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.ReleaseAirVacuumPads3, 1);
            }
        }

        private void txtLight_Click(object sender, RoutedEventArgs e)
        {
            if (_plc == null) return;
            var message = "Do you want to control light?\r\nYes: Turn on\r\nNo: Turn off";
            var result = AppUi.ShowMessage(message, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.Light, 1);
            }
            else if (result == MessageBoxResult.No)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.Light, 0);
            }
        }

        private void txtPlcCommand_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbPositions.SelectedIndex;
            object selectedItem = tbcmbPositions.SelectedItem;
            if (selectedIndex > -1)
            {
                PlcCommand(Root.AppManager.DatabaseManager.Runtime.Positions[selectedIndex]);
            }
        }

        private void txtPlcSpeedJogAxisX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_plc == null) return;
            var value = Utilities.ConvertStringToInt(txtPlcSpeedJogAxisX.Text);
            if (value > -1)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.SpeedJogAxisX, value);
            }
        }

        private void txtPlcSpeedJogAxisY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_plc == null) return;
            var value = Utilities.ConvertStringToInt(txtPlcSpeedJogAxisY.Text);
            if (value > -1)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.SpeedJogAxisY, value);
            }
        }

        private void txtPlcSpeedJogAxisZ_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_plc == null) return;
            var value = Utilities.ConvertStringToInt(txtPlcSpeedJogAxisZ.Text);
            if (value > -1)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.SpeedJogAxisZ, value);
            }
        }

        private void txtPlcSpeedJogAxisR_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_plc == null) return;
            var value = Utilities.ConvertStringToInt(txtPlcSpeedJogAxisR.Text);
            if (value > -1)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.SpeedJogAxisR, value);
            }
        }

        private void txtPlcSpeedAxisX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_plc == null) return;
            var value = Utilities.ConvertStringToInt(txtPlcSpeedAxisX.Text);
            if (value > -1)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.SpeedAxisX, value);
            }
        }

        private void txtPlcSpeedAxisY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_plc == null) return;
            var value = Utilities.ConvertStringToInt(txtPlcSpeedAxisY.Text);
            if (value > -1)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.SpeedAxisY, value);
            }
        }

        private void txtPlcSpeedAxisZ_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_plc == null) return;
            var value = Utilities.ConvertStringToInt(txtPlcSpeedAxisZ.Text);
            if (value > -1)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.SpeedAxisZ, value);
            }
        }

        private void txtPlcSpeedAxisR_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_plc == null) return;
            var value = Utilities.ConvertStringToInt(txtPlcSpeedAxisR.Text);
            if (value > -1)
            {
                _plc.SetDevice(Root.AppManager.DatabaseManager.Basic.Plc.SpeedAxisR, value);
            }
        }

        private void ShowPosition()
        {
            Dispatcher.Invoke(() =>
            {
                tbcmbPositions.Items.Clear();
                //var positions = Root.AppManager.DatabaseManager.Runtime.Positions;
                var positions = Root.AppManager.DatabaseManager.Runtime.Positions.OrderBy(x => x.Alias).ToList();
                Root.AppManager.DatabaseManager.Runtime.Positions = positions;
                for (int i = 0; i < positions.Count; i++)
                {
                    positions[i].Index = i;
                    tbcmbPositions.Items.Add($"[{i}] {positions[i].Alias}");
                }

                if (positions.Count > 0)
                {
                    tbcmbPositions.SelectedIndex = positions.Count - 1;
                }
            });
        }

        private void ShowParameter(int index)
        {
            ShowBasicParameter(Root.AppManager.DatabaseManager.Runtime.Positions[index]);
            ShowServerParameter(Root.AppManager.DatabaseManager.Runtime.Positions[index].Server);
            ShowClientParameter(Root.AppManager.DatabaseManager.Runtime.Positions[index].Client);
            ShowPlcParameter(Root.AppManager.DatabaseManager.Runtime.Positions[index].Plc);
            ShowRobotParameter(Root.AppManager.DatabaseManager.Runtime.Positions[index].Robot);
        }

        private void ShowBasicParameter(Position position)
        {
            chkStatus.Checked -= chkStatus_Checked;
            txtIndex.TextChanged -= txtIndex_TextChanged;
            txtAlias.TextChanged -= txtAlias_TextChanged;
            chkIsServer.Checked -= chkIsServer_Checked;
            chkIsClient.Checked -= chkIsClient_Checked;
            cmbModelName.SelectionChanged -= cmbModelName_SelectionChanged;

            chkStatus.IsChecked = position.Enable;
            chkStatus.Content = position.Enable ? "Enable" : "Disable";
            txtIndex.Text = position.Index.ToString();
            txtAlias.Text = position.Alias;
            chkIsServer.IsChecked = position.IsServer;
            chkIsServer.Content = position.IsServer ? "Enable" : "Disable";
            chkIsClient.IsChecked = position.IsClient;
            chkIsClient.Content = position.IsClient ? "Enable" : "Disable";
            cmbModelName.SelectedIndex = (int)position.ModelName;

            chkStatus.Checked += chkStatus_Checked;
            txtIndex.TextChanged += txtIndex_TextChanged;
            txtAlias.TextChanged += txtAlias_TextChanged;
            chkIsServer.Checked += chkIsServer_Checked;
            chkIsClient.Checked += chkIsClient_Checked;
            cmbModelName.SelectionChanged += cmbModelName_SelectionChanged;
        }

        private void ShowServerParameter(ServerConfiguration server)
        {
            txtServerHost.TextChanged -= txtServerHost_TextChanged;
            txtServerPort.TextChanged -= txtServerPort_TextChanged;

            txtServerHost.Text = server.Host;
            txtServerPort.Text = server.Port.ToString();

            txtServerHost.TextChanged += txtServerHost_TextChanged;
            txtServerPort.TextChanged += txtServerPort_TextChanged;
        }

        private void ShowClientParameter(ClientConfiguration client)
        {
            txtClientHost.TextChanged -= txtClientHost_TextChanged;
            txtClientPort.TextChanged -= txtClientPort_TextChanged;

            txtClientHost.Text = client.Host;
            txtClientPort.Text = client.Port.ToString();

            txtClientHost.TextChanged += txtClientHost_TextChanged;
            txtClientPort.TextChanged += txtClientPort_TextChanged;
        }

        private void ShowPlcParameter(PlcConfiguration plc)
        {
            txtPlcMovePickup.TextChanged -= txtPlcMovePickup_TextChanged;
            txtPlcSavePickup.TextChanged -= txtPlcSavePickup_TextChanged;
            txtPlcMoveDropdown.TextChanged -= txtPlcMoveDropdown_TextChanged;
            txtPlcSaveDropdown.TextChanged -= txtPlcSaveDropdown_TextChanged;
            txtPlcInit.TextChanged -= txtPlcInit_TextChanged;
            txtPlcPass.TextChanged -= txtPlcPass_TextChanged;
            txtPlcFail.TextChanged -= txtPlcFail_TextChanged;
            txtPlcRepair.TextChanged -= txtPlcRepair_TextChanged;
            txtPlcInitInit.TextChanged -= txtPlcInitInit_TextChanged;
            txtPlcPassPass.TextChanged -= txtPlcPassPass_TextChanged;
            txtPlcFailFail.TextChanged -= txtPlcFailFail_TextChanged;
            txtPlcPassFail.TextChanged -= txtPlcPassFail_TextChanged;
            txtPlcFailPass.TextChanged -= txtPlcFailPass_TextChanged;
            txtPlcReady.TextChanged -= txtPlcReady_TextChanged;

            txtPlcMovePickup.Text = plc.MovePickup;
            txtPlcSavePickup.Text = plc.SavePickup;
            txtPlcMoveDropdown.Text = plc.MoveDropdown;
            txtPlcSaveDropdown.Text = plc.SaveDropdown;
            txtPlcInit.Text = plc.Status.Init;
            txtPlcPass.Text = plc.Status.Pass;
            txtPlcFail.Text = plc.Status.Fail;
            txtPlcRepair.Text = plc.Status.Repair;
            txtPlcInitInit.Text = plc.Status.InitInit;
            txtPlcPassPass.Text = plc.Status.PassPass;
            txtPlcFailFail.Text = plc.Status.FailFail;
            txtPlcPassFail.Text = plc.Status.PassFail;
            txtPlcFailPass.Text = plc.Status.FailPass;
            txtPlcReady.Text = plc.Status.Ready;

            txtPlcMovePickup.TextChanged += txtPlcMovePickup_TextChanged;
            txtPlcSavePickup.TextChanged += txtPlcSavePickup_TextChanged;
            txtPlcMoveDropdown.TextChanged += txtPlcMoveDropdown_TextChanged;
            txtPlcSaveDropdown.TextChanged += txtPlcSaveDropdown_TextChanged;
            txtPlcInit.TextChanged += txtPlcInit_TextChanged;
            txtPlcPass.TextChanged += txtPlcPass_TextChanged;
            txtPlcFail.TextChanged += txtPlcFail_TextChanged;
            txtPlcRepair.TextChanged += txtPlcRepair_TextChanged;
            txtPlcInitInit.TextChanged += txtPlcInitInit_TextChanged;
            txtPlcPassPass.TextChanged += txtPlcPassPass_TextChanged;
            txtPlcFailFail.TextChanged += txtPlcFailFail_TextChanged;
            txtPlcPassFail.TextChanged += txtPlcPassFail_TextChanged;
            txtPlcFailPass.TextChanged += txtPlcFailPass_TextChanged;
            txtPlcReady.TextChanged += txtPlcReady_TextChanged;
        }

        private void ShowRobotParameter(RobotConfiguration robot)
        {
            txtRobotMovePickup.TextChanged -= txtRobotMovePickup_TextChanged;
            txtRobotSavePickup.TextChanged -= txtRobotSavePickup_TextChanged;
            txtRobotMoveDropdown.TextChanged -= txtRobotMoveDropdown_TextChanged;
            txtRobotSaveDropdown.TextChanged -= txtRobotSaveDropdown_TextChanged;
            txtRobotInit.TextChanged -= txtRobotInit_TextChanged;
            txtRobotPass.TextChanged -= txtRobotPass_TextChanged;
            txtRobotFail.TextChanged -= txtRobotFail_TextChanged;
            txtRobotRepair.TextChanged -= txtRobotRepair_TextChanged;
            txtRobotInitInit.TextChanged -= txtRobotInitInit_TextChanged;
            txtRobotPassPass.TextChanged -= txtRobotPassPass_TextChanged;
            txtRobotFailFail.TextChanged -= txtRobotFailFail_TextChanged;
            txtRobotPassFail.TextChanged -= txtRobotPassFail_TextChanged;
            txtRobotFailPass.TextChanged -= txtRobotFailPass_TextChanged;
            txtRobotReady.TextChanged -= txtRobotReady_TextChanged;

            txtRobotMovePickup.Text = robot.MovePickup;
            txtRobotSavePickup.Text = robot.SavePickup;
            txtRobotMoveDropdown.Text = robot.MoveDropdown;
            txtRobotSaveDropdown.Text = robot.SaveDropdown;
            txtRobotInit.Text = robot.Status.Init;
            txtRobotPass.Text = robot.Status.Pass;
            txtRobotFail.Text = robot.Status.Fail;
            txtRobotRepair.Text = robot.Status.Repair;
            txtRobotInitInit.Text = robot.Status.InitInit;
            txtRobotPassPass.Text = robot.Status.PassPass;
            txtRobotFailFail.Text = robot.Status.FailFail;
            txtRobotPassFail.Text = robot.Status.PassFail;
            txtRobotFailPass.Text = robot.Status.FailPass;
            txtRobotReady.Text = robot.Status.Ready;

            txtRobotMovePickup.TextChanged += txtRobotMovePickup_TextChanged;
            txtRobotSavePickup.TextChanged += txtRobotSavePickup_TextChanged;
            txtRobotMoveDropdown.TextChanged += txtRobotMoveDropdown_TextChanged;
            txtRobotSaveDropdown.TextChanged += txtRobotSaveDropdown_TextChanged;
            txtRobotInit.TextChanged += txtRobotInit_TextChanged;
            txtRobotPass.TextChanged += txtRobotPass_TextChanged;
            txtRobotFail.TextChanged += txtRobotFail_TextChanged;
            txtRobotRepair.TextChanged += txtRobotRepair_TextChanged;
            txtRobotInitInit.TextChanged += txtRobotInitInit_TextChanged;
            txtRobotPassPass.TextChanged += txtRobotPassPass_TextChanged;
            txtRobotFailFail.TextChanged += txtRobotFailFail_TextChanged;
            txtRobotPassFail.TextChanged += txtRobotPassFail_TextChanged;
            txtRobotFailPass.TextChanged += txtRobotFailPass_TextChanged;
            txtRobotReady.TextChanged += txtRobotReady_TextChanged;
        }

        private void LoadPlcSpeed()
        {
            if (_plc == null) return;
            Task.Run(() =>
              {
                  try
                  {
                      int speedJogAxisX = -1;
                      int speedJogAxisY = -1;
                      int speedJogAxisZ = -1;
                      int speedJogAxisR = -1;
                      int speedAxisX = -1;
                      int speedAxisY = -1;
                      int speedAxisZ = -1;
                      int speedAxisR = -1;
                      var plc = Root.AppManager.DatabaseManager.Basic.Plc;
                      _plc.GetDevice(plc.SpeedJogAxisX, ref speedJogAxisX);
                      _plc.GetDevice(plc.SpeedJogAxisY, ref speedJogAxisY);
                      _plc.GetDevice(plc.SpeedJogAxisZ, ref speedJogAxisZ);
                      _plc.GetDevice(plc.SpeedJogAxisR, ref speedJogAxisR);
                      _plc.GetDevice(plc.SpeedAxisX, ref speedAxisX);
                      _plc.GetDevice(plc.SpeedAxisY, ref speedAxisY);
                      _plc.GetDevice(plc.SpeedAxisZ, ref speedAxisZ);
                      _plc.GetDevice(plc.SpeedAxisR, ref speedAxisR);

                      Dispatcher.Invoke(() =>
                      {
                          txtPlcSpeedJogAxisX.TextChanged -= txtPlcSpeedJogAxisX_TextChanged;
                          txtPlcSpeedJogAxisY.TextChanged -= txtPlcSpeedJogAxisY_TextChanged;
                          txtPlcSpeedJogAxisZ.TextChanged -= txtPlcSpeedJogAxisZ_TextChanged;
                          txtPlcSpeedJogAxisR.TextChanged -= txtPlcSpeedJogAxisR_TextChanged;
                          txtPlcSpeedAxisX.TextChanged -= txtPlcSpeedAxisX_TextChanged;
                          txtPlcSpeedAxisY.TextChanged -= txtPlcSpeedAxisY_TextChanged;
                          txtPlcSpeedAxisZ.TextChanged -= txtPlcSpeedAxisZ_TextChanged;
                          txtPlcSpeedAxisR.TextChanged -= txtPlcSpeedAxisR_TextChanged;

                          txtPlcSpeedJogAxisX.Text = speedJogAxisX.ToString();
                          txtPlcSpeedJogAxisY.Text = speedJogAxisY.ToString();
                          txtPlcSpeedJogAxisZ.Text = speedJogAxisZ.ToString();
                          txtPlcSpeedJogAxisR.Text = speedJogAxisR.ToString();
                          txtPlcSpeedAxisX.Text = speedAxisX.ToString();
                          txtPlcSpeedAxisY.Text = speedAxisY.ToString();
                          txtPlcSpeedAxisZ.Text = speedAxisZ.ToString();
                          txtPlcSpeedAxisR.Text = speedAxisR.ToString();

                          txtPlcSpeedJogAxisX.TextChanged += txtPlcSpeedJogAxisX_TextChanged;
                          txtPlcSpeedJogAxisY.TextChanged += txtPlcSpeedJogAxisY_TextChanged;
                          txtPlcSpeedJogAxisZ.TextChanged += txtPlcSpeedJogAxisZ_TextChanged;
                          txtPlcSpeedJogAxisR.TextChanged += txtPlcSpeedJogAxisR_TextChanged;
                          txtPlcSpeedAxisX.TextChanged += txtPlcSpeedAxisX_TextChanged;
                          txtPlcSpeedAxisY.TextChanged += txtPlcSpeedAxisY_TextChanged;
                          txtPlcSpeedAxisZ.TextChanged += txtPlcSpeedAxisZ_TextChanged;
                          txtPlcSpeedAxisR.TextChanged += txtPlcSpeedAxisR_TextChanged;
                      });
                  }
                  catch (Exception ex)
                  {
                      Logger.Instance.Write(ex.StackTrace);
                  }
              });
        }

        private void PlcCommand(Position position)
        {
            if (_plc == null) return;
            var step1 = AppUi.ShowMessage("Do you want to select pickup or dropdown?\r\nYes: Pickup\r\nNo: Dropdown", MessageBoxImage.Question);
            if (step1 == MessageBoxResult.Yes)
            {
                var step2 = AppUi.ShowMessage("Do you want to move or save position pickup?\r\nYes: Move\r\nNo: Save", MessageBoxImage.Question);
                if (step2 == MessageBoxResult.Yes)
                {
                    _plc.SetDevice(position.Plc.MovePickup, 1);
                }
                else if (step2 == MessageBoxResult.No)
                {
                    if (AppUi.ShowMessage($"Save pickup: {position.Alias}", MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        _plc.SetDevice(position.Plc.SavePickup, 1);
                    }
                }
            }
            else if (step1 == MessageBoxResult.No)
            {
                var step2 = AppUi.ShowMessage("Do you want to move or save position dropdown?\r\nYes: Move\r\nNo: Save", MessageBoxImage.Question);
                if (step2 == MessageBoxResult.Yes)
                {
                    _plc.SetDevice(position.Plc.MoveDropdown, 1);
                }
                else if (step2 == MessageBoxResult.No)
                {
                    if (AppUi.ShowMessage($"Save dropdown: {position.Alias}", MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        _plc.SetDevice(position.Plc.SaveDropdown, 1);
                    }
                }
            }
        }
    }
}
