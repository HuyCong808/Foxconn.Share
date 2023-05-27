using Foxconn.App.Helper;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Foxconn.App.Models.BasicConfiguration.RobotConfiguration;

namespace Foxconn.App.Views
{
    /// <summary>
    /// Interaction logic for RobotWindow.xaml
    /// </summary>
    public partial class RobotWindow : Window
    {
        private readonly MainWindow Root = MainWindow.Current;

        public RobotWindow()
        {
            InitializeComponent();
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
                Root.CodeFlow("ROBOT");

                #region Signal
                Dispatcher.Invoke(() =>
                {
                    var robot = Root.AppManager.DatabaseManager.Basic.Robot;

                    txtStart.TextChanged -= txtStart_TextChanged;
                    txtPause.TextChanged -= txtPause_TextChanged;
                    txtReset.TextChanged -= txtReset_TextChanged;
                    txtResetOK.TextChanged -= txtResetOK_TextChanged;
                    txtStop.TextChanged -= txtStop_TextChanged;
                    txtSettings.TextChanged -= txtSettings_TextChanged;
                    txtHasStarted.TextChanged -= txtHasStarted_TextChanged;
                    txtHasPaused.TextChanged -= txtHasPaused_TextChanged;
                    txtHasReset.TextChanged -= txtHasReset_TextChanged;
                    txtHasResetOK.TextChanged -= txtHasResetOK_TextChanged;
                    txtHasStopped.TextChanged -= txtHasStopped_TextChanged;
                    txtHasSet.TextChanged -= txtHasSet_TextChanged;

                    txtStart.Text = robot.Status.Start;
                    txtPause.Text = robot.Status.Pause;
                    txtReset.Text = robot.Status.Reset;
                    txtResetOK.Text = robot.Status.ResetOK;
                    txtStop.Text = robot.Status.Stop;
                    txtSettings.Text = robot.Status.Settings;
                    txtHasStarted.Text = robot.Status.HasStarted;
                    txtHasPaused.Text = robot.Status.HasPaused;
                    txtHasReset.Text = robot.Status.HasReset;
                    txtHasResetOK.Text = robot.Status.HasResetOK;
                    txtHasStopped.Text = robot.Status.HasStopped;
                    txtHasSet.Text = robot.Status.HasSet;

                    txtStart.TextChanged += txtStart_TextChanged;
                    txtPause.TextChanged += txtPause_TextChanged;
                    txtReset.TextChanged += txtReset_TextChanged;
                    txtResetOK.TextChanged += txtResetOK_TextChanged;
                    txtStop.TextChanged += txtStop_TextChanged;
                    txtSettings.TextChanged += txtSettings_TextChanged;
                    txtHasStarted.TextChanged += txtHasStarted_TextChanged;
                    txtHasPaused.TextChanged += txtHasPaused_TextChanged;
                    txtHasReset.TextChanged += txtHasReset_TextChanged;
                    txtHasResetOK.TextChanged += txtHasResetOK_TextChanged;
                    txtHasStopped.TextChanged += txtHasStopped_TextChanged;
                    txtHasSet.TextChanged += txtHasSet_TextChanged;
                });
                #endregion

                ShowDevice();
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void txtStart_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Robot.Status.Start = txtStart.Text;
        }

        private void txtPause_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Robot.Status.Pause = txtPause.Text;
        }

        private void txtReset_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Robot.Status.Reset = txtReset.Text;
        }

        private void txtResetOK_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Robot.Status.ResetOK = txtResetOK.Text;
        }

        private void txtStop_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Robot.Status.Stop = txtStop.Text;
        }

        private void txtSettings_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Robot.Status.Settings = txtSettings.Text;
        }

        private void txtHasStarted_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Robot.Status.HasStarted = txtHasStarted.Text;
        }

        private void txtHasPaused_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Robot.Status.HasPaused = txtHasPaused.Text;
        }

        private void txtHasReset_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Robot.Status.HasReset = txtHasReset.Text;
        }

        private void txtHasResetOK_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Robot.Status.HasResetOK = txtHasResetOK.Text;
        }

        private void txtHasStopped_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Robot.Status.HasStopped = txtHasStopped.Text;
        }

        private void txtHasSet_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Robot.Status.HasSet = txtHasSet.Text;
        }

        private void cmbDevices_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = cmbDevices.SelectedIndex;
            object selectedItem = cmbDevices.SelectedItem;
            if (selectedIndex > -1)
            {
                ShowParameter(Root.AppManager.DatabaseManager.Basic.Robot.Devices[selectedIndex]);
            }
        }

        private void chkStatus_Checked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = cmbDevices.SelectedIndex;
            object selectedItem = cmbDevices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Robot.Devices[selectedIndex].Enable = chkStatus.IsChecked == true;
                chkStatus.Content = chkStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void chkStatus_Unchecked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = cmbDevices.SelectedIndex;
            object selectedItem = cmbDevices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Robot.Devices[selectedIndex].Enable = chkStatus.IsChecked == true;
                chkStatus.Content = chkStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void txtIndex_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = cmbDevices.SelectedIndex;
            object selectedItem = cmbDevices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Robot.Devices[selectedIndex].Index = Utilities.ConvertStringToInt(txtIndex.Text);
            }
        }

        private void txtAlias_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = cmbDevices.SelectedIndex;
            object selectedItem = cmbDevices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Robot.Devices[selectedIndex].Alias = txtAlias.Text;
            }
        }

        private void txtHost_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = cmbDevices.SelectedIndex;
            object selectedItem = cmbDevices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Robot.Devices[selectedIndex].Host = txtHost.Text;
            }
        }

        private void txtPort_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = cmbDevices.SelectedIndex;
            object selectedItem = cmbDevices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Robot.Devices[selectedIndex].Port = Utilities.ConvertStringToInt(txtPort.Text);
            }
        }

        private void txtTx_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void txtRx_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void btnPing_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (IPAddress.TryParse(txtHost.Text, out IPAddress ip) && int.TryParse(txtPort.Text, out int port))
                        {
                            using (var client = new TcpClient())
                            {
                                client.SendTimeout = 500;
                                client.ReceiveTimeout = 500;
                                var result = client.ConnectAsync(ip, port).Wait(500);
                                if (result)
                                {
                                    AppUi.ShowMessage($"Ping is successful ({ip}:{port}).", MessageBoxImage.Information);
                                }
                                else
                                {
                                    AppUi.ShowMessage($"Cannot ping to ({ip}:{port}).", MessageBoxImage.Error);
                                }
                                AppUi.ShowButton(this, btnConnect, result);
                            }
                        }
                        else
                        {
                            AppUi.ShowMessage($"Host or port is wrong format.", MessageBoxImage.Error);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Logger.Instance.Write(ex.StackTrace);
                    AppUi.ShowMessage(ex.Message, MessageBoxImage.Error);
                }
            });
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IPAddress.TryParse(txtHost.Text, out IPAddress ip) && int.TryParse(txtPort.Text, out int port))
                {
                    btnSendCommand.IsEnabled = true;
                    AppUi.ShowMessage($"Connected ({ip}:{port})");
                }
                else
                {
                    AppUi.ShowMessage($"Host or port is wrong format.", MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                AppUi.ShowMessage(ex.Message, MessageBoxImage.Error);
            }
        }

        private void btnSendCommand_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (IPAddress.TryParse(txtHost.Text, out IPAddress ip) && int.TryParse(txtPort.Text, out int port))
                        {
                            txtRx.Text = string.Empty;
                            string message = txtTx.Text;

                            // Create a TcpClient.
                            var client = new TcpClient(ip.ToString(), port)
                            {
                                ReceiveTimeout = 5000,
                                SendTimeout = 500
                            };

                            // Translate the passed message into ASCII and store it as a Byte array.
                            byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                            // Get a client stream for reading and writing.
                            // Stream stream = client.GetStream();
                            NetworkStream stream = client.GetStream();

                            // Send the message to the connected TcpServer.
                            stream.Write(data, 0, data.Length);
                            Console.WriteLine("Sent: {0}", message);

                            // Receive the TcpServer.response.
                            // Buffer to store the response bytes.
                            data = new byte[1024];

                            // String to store the response ASCII representation.
                            string responseData = string.Empty;

                            // Read the first batch of the TcpServer response bytes.
                            int bytes = stream.Read(data, 0, data.Length);
                            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                            Console.WriteLine("Received: {0}", responseData);

                            // Close everything.
                            stream.Close();
                            client.Close();

                            txtRx.Text = responseData;
                        }
                        else
                        {
                            AppUi.ShowMessage($"Host or port is wrong format.", MessageBoxImage.Error);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Logger.Instance.Write(ex.StackTrace);
                    AppUi.ShowMessage(ex.Message, MessageBoxImage.Error);
                }
            });
        }

        private void ShowDevice()
        {
            Dispatcher.Invoke(() =>
            {
                cmbDevices.Items.Clear();
                //var devices = Root.AppManager.DatabaseManager.Basic.Robot.Devices;
                var devices = Root.AppManager.DatabaseManager.Basic.Robot.Devices.OrderBy(x => x.Alias).ToList();
                Root.AppManager.DatabaseManager.Basic.Robot.Devices = devices;
                for (int i = 0; i < devices.Count; i++)
                {
                    devices[i].Index = i;
                    cmbDevices.Items.Add($"[{i}] {devices[i].Alias}");
                }

                if (devices.Count > 0)
                {
                    cmbDevices.SelectedIndex = devices.Count - 1;
                }
            });
        }

        private void ShowParameter(DeviceInformation device)
        {
            chkStatus.Checked -= chkStatus_Checked;
            txtIndex.TextChanged -= txtIndex_TextChanged;
            txtAlias.TextChanged -= txtAlias_TextChanged;
            txtHost.TextChanged -= txtHost_TextChanged;
            txtPort.TextChanged -= txtPort_TextChanged;

            chkStatus.IsChecked = device.Enable;
            chkStatus.Content = device.Enable ? "Enable" : "Disable";
            txtIndex.Text = device.Index.ToString();
            txtAlias.Text = device.Alias;
            txtHost.Text = device.Host;
            txtPort.Text = device.Port.ToString();

            chkStatus.Checked += chkStatus_Checked;
            txtIndex.TextChanged += txtIndex_TextChanged;
            txtAlias.TextChanged += txtAlias_TextChanged;
            txtHost.TextChanged += txtHost_TextChanged;
            txtPort.TextChanged += txtPort_TextChanged;
        }
    }
}
