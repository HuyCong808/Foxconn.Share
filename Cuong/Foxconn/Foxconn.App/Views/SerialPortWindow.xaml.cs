using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using System;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Foxconn.App.Models.BasicConfiguration;

namespace Foxconn.App.Views
{
    /// <summary>
    /// Interaction logic for SerialPortWindow.xaml
    /// </summary>
    public partial class SerialPortWindow : Window
    {
        private readonly MainWindow Root = MainWindow.Current;

        public SerialPortWindow()
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
                Root.CodeFlow("SERIAL PORT");

                #region Image Icon
                AppUi.ShowImage(this, imgAddSerialPort, @"Assets/Plus.png");
                AppUi.ShowImage(this, imgRemoveSerialPort, @"Assets/Subtract.png");
                #endregion

                #region ComboBox
                Dispatcher.Invoke(() =>
                {
                    cmbParity.ItemsSource = Enum.GetValues(typeof(Parity));
                    cmbParity.Items.Refresh();

                    cmbStopBits.ItemsSource = Enum.GetValues(typeof(StopBits));
                    cmbStopBits.Items.Refresh();

                    cmbHandshake.ItemsSource = Enum.GetValues(typeof(Handshake));
                    cmbHandshake.Items.Refresh();
                });
                #endregion

                AppUi.ShowListToComboBox(this, cmbPortName, SerialPort.GetPortNames().ToList());

                ShowSerialPort();
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void tbcmbSerialPorts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = tbcmbSerialPorts.SelectedIndex;
            object selectedItem = tbcmbSerialPorts.SelectedItem;
            if (selectedIndex > -1)
            {
                ShowParameter(Root.AppManager.DatabaseManager.Basic.SerialPorts[selectedIndex]);
            }
        }

        private void tbbtnAddSerialPort_Click(object sender, RoutedEventArgs e)
        {
            int count = Root.AppManager.DatabaseManager.Basic.SerialPorts.Count;
            Root.AppManager.DatabaseManager.Basic.SerialPorts.Add(new SerialPortConfiguration { Index = count });
            Root.ShowMessage($"[Serial Port] Add: {count}");
            ShowSerialPort();
        }

        private void tbbtnRemoveSerialPort_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbSerialPorts.SelectedIndex;
            object selectedItem = tbcmbSerialPorts.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.SerialPorts.RemoveAt(selectedIndex);
                Root.ShowMessage($"[Serial Port] Remove {selectedIndex}");
                ShowSerialPort();
            }
            else
            {
                Root.ShowMessage($"[Serial Port] Cannot remove serial port.", AppColor.Red);
                AppUi.ShowMessage("Cannot remove serial port.", MessageBoxImage.Error);
            }
        }

        private void chkStatus_Checked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbSerialPorts.SelectedIndex;
            object selectedItem = tbcmbSerialPorts.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.SerialPorts[selectedIndex].Enable = chkStatus.IsChecked == true;
                chkStatus.Content = chkStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void chkStatus_Unchecked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbSerialPorts.SelectedIndex;
            object selectedItem = tbcmbSerialPorts.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.SerialPorts[selectedIndex].Enable = chkStatus.IsChecked == true;
                chkStatus.Content = chkStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void txtIndex_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbSerialPorts.SelectedIndex;
            object selectedItem = tbcmbSerialPorts.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.SerialPorts[selectedIndex].Index = Utilities.ConvertStringToInt(txtIndex.Text);
            }
        }

        private void txtAlias_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbSerialPorts.SelectedIndex;
            object selectedItem = tbcmbSerialPorts.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.SerialPorts[selectedIndex].Alias = txtAlias.Text;
            }
        }

        private void cmbPortName_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = tbcmbSerialPorts.SelectedIndex;
            object selectedItem = tbcmbSerialPorts.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.SerialPorts[selectedIndex].PortName = cmbPortName.SelectedItem.ToString();
            }
        }

        private void txtBaudRate_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbSerialPorts.SelectedIndex;
            object selectedItem = tbcmbSerialPorts.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.SerialPorts[selectedIndex].BaudRate = Utilities.ConvertStringToInt(txtBaudRate.Text);
            }
        }

        private void cmbParity_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = tbcmbSerialPorts.SelectedIndex;
            object selectedItem = tbcmbSerialPorts.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.SerialPorts[selectedIndex].Parity = (Parity)selectedIndex;
            }
        }

        private void txtDataBits_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbSerialPorts.SelectedIndex;
            object selectedItem = tbcmbSerialPorts.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.SerialPorts[selectedIndex].DataBits = Utilities.ConvertStringToInt(txtDataBits.Text);
            }
        }

        private void cmbStopBits_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = tbcmbSerialPorts.SelectedIndex;
            object selectedItem = tbcmbSerialPorts.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.SerialPorts[selectedIndex].StopBits = (StopBits)selectedIndex;
            }
        }

        private void cmbHandshake_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = tbcmbSerialPorts.SelectedIndex;
            object selectedItem = tbcmbSerialPorts.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.SerialPorts[selectedIndex].Handshake = (Handshake)selectedIndex;
            }
        }

        private void txtReadTimeout_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbSerialPorts.SelectedIndex;
            object selectedItem = tbcmbSerialPorts.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.SerialPorts[selectedIndex].ReadTimeout = Utilities.ConvertStringToInt(txtReadTimeout.Text);
            }
        }

        private void txtWriteTimeout_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbSerialPorts.SelectedIndex;
            object selectedItem = tbcmbSerialPorts.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.SerialPorts[selectedIndex].WriteTimeout = Utilities.ConvertStringToInt(txtWriteTimeout.Text);
            }
        }

        private void ShowSerialPort()
        {
            Dispatcher.Invoke(() =>
            {
                tbcmbSerialPorts.Items.Clear();
                //var serialPorts = Root.AppManager.DatabaseManager.Basic.SerialPorts;
                var serialPorts = Root.AppManager.DatabaseManager.Basic.SerialPorts.OrderBy(x => x.Alias).ToList();
                Root.AppManager.DatabaseManager.Basic.SerialPorts = serialPorts;
                for (int i = 0; i < serialPorts.Count; i++)
                {
                    serialPorts[i].Index = i;
                    tbcmbSerialPorts.Items.Add($"[{i}] {serialPorts[i].Alias}");
                }

                if (serialPorts.Count > 0)
                {
                    tbcmbSerialPorts.SelectedIndex = serialPorts.Count - 1;
                }
            });
        }

        private void ShowParameter(SerialPortConfiguration serialPort)
        {
            chkStatus.Checked -= chkStatus_Checked;
            txtIndex.TextChanged -= txtIndex_TextChanged;
            txtAlias.TextChanged -= txtAlias_TextChanged;
            cmbPortName.SelectionChanged -= cmbPortName_SelectionChanged;
            txtBaudRate.TextChanged -= txtBaudRate_TextChanged;
            cmbParity.SelectionChanged -= cmbParity_SelectionChanged;
            txtDataBits.TextChanged -= txtDataBits_TextChanged;
            cmbStopBits.SelectionChanged -= cmbStopBits_SelectionChanged;
            cmbHandshake.SelectionChanged -= cmbHandshake_SelectionChanged;
            txtReadTimeout.TextChanged -= txtReadTimeout_TextChanged;
            txtWriteTimeout.TextChanged -= txtWriteTimeout_TextChanged;

            chkStatus.IsChecked = serialPort.Enable;
            chkStatus.Content = serialPort.Enable ? "Enable" : "Disable";
            txtIndex.Text = serialPort.Index.ToString();
            txtAlias.Text = serialPort.Alias;
            cmbPortName.Text = serialPort.PortName;
            txtBaudRate.Text = serialPort.BaudRate.ToString();
            cmbParity.SelectedIndex = (int)serialPort.Parity;
            txtDataBits.Text = serialPort.DataBits.ToString();
            cmbStopBits.SelectedIndex = (int)serialPort.StopBits;
            cmbHandshake.SelectedIndex = (int)serialPort.Handshake;
            txtReadTimeout.Text = serialPort.ReadTimeout.ToString();
            txtWriteTimeout.Text = serialPort.WriteTimeout.ToString();

            chkStatus.Checked += chkStatus_Checked;
            txtIndex.TextChanged += txtIndex_TextChanged;
            txtAlias.TextChanged += txtAlias_TextChanged;
            cmbPortName.SelectionChanged += cmbPortName_SelectionChanged;
            txtBaudRate.TextChanged += txtBaudRate_TextChanged;
            cmbParity.SelectionChanged += cmbParity_SelectionChanged;
            txtDataBits.TextChanged += txtDataBits_TextChanged;
            cmbStopBits.SelectionChanged += cmbStopBits_SelectionChanged;
            cmbHandshake.SelectionChanged += cmbHandshake_SelectionChanged;
            txtReadTimeout.TextChanged += txtReadTimeout_TextChanged;
            txtWriteTimeout.TextChanged += txtWriteTimeout_TextChanged;
        }
    }
}
