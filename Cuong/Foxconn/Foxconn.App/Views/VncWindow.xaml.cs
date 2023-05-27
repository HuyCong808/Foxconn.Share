using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Foxconn.App.Models.BasicConfiguration;

namespace Foxconn.App.Views
{
    /// <summary>
    /// Interaction logic for VncWindow.xaml
    /// </summary>
    public partial class VncWindow : Window
    {
        private readonly MainWindow Root = MainWindow.Current;

        public VncWindow()
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
                Root.CodeFlow("VNC");

                #region Image Icon
                AppUi.ShowImage(this, imgAddVnc, @"Assets/Plus.png");
                AppUi.ShowImage(this, imgRemoveVnc, @"Assets/Subtract.png");
                #endregion

                ShowVnc();
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void tbcmbVncs_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = tbcmbVncs.SelectedIndex;
            object selectedItem = tbcmbVncs.SelectedItem;
            if (selectedIndex > -1)
            {
                ShowParameter(Root.AppManager.DatabaseManager.Basic.Vncs[selectedIndex]);
            }
        }

        private void tbbtnAddVnc_Click(object sender, RoutedEventArgs e)
        {
            int count = Root.AppManager.DatabaseManager.Basic.Vncs.Count;
            Root.AppManager.DatabaseManager.Basic.Vncs.Add(new VncConfiguration { Index = count });
            Root.ShowMessage($"[VNC] Add: {count}");
            ShowVnc();
        }

        private void tbbtnRemoveVnc_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbVncs.SelectedIndex;
            object selectedItem = tbcmbVncs.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Vncs.RemoveAt(selectedIndex);
                Root.ShowMessage($"[Service] Remove {selectedIndex}");
                ShowVnc();
            }
            else
            {
                Root.ShowMessage($"[Service] Cannot remove vnc.", AppColor.Red);
                AppUi.ShowMessage("Cannot remove vnc.", MessageBoxImage.Error);
            }
        }

        private void chkStatus_Checked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbVncs.SelectedIndex;
            object selectedItem = tbcmbVncs.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Vncs[selectedIndex].Enable = chkStatus.IsChecked == true;
                chkStatus.Content = chkStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void chkStatus_Unchecked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbVncs.SelectedIndex;
            object selectedItem = tbcmbVncs.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Vncs[selectedIndex].Enable = chkStatus.IsChecked == true;
                chkStatus.Content = chkStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void txtIndex_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbVncs.SelectedIndex;
            object selectedItem = tbcmbVncs.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Vncs[selectedIndex].Index = Utilities.ConvertStringToInt(txtIndex.Text);
            }
        }

        private void txtAlias_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbVncs.SelectedIndex;
            object selectedItem = tbcmbVncs.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Vncs[selectedIndex].Alias = txtAlias.Text;
            }
        }

        private void txtHost_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbVncs.SelectedIndex;
            object selectedItem = tbcmbVncs.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Vncs[selectedIndex].Host = txtHost.Text;
            }
        }

        private void txtPassword_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbVncs.SelectedIndex;
            object selectedItem = tbcmbVncs.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Vncs[selectedIndex].Password = txtPassword.Text;
            }
        }

        private void ShowVnc()
        {
            Dispatcher.Invoke(() =>
            {
                tbcmbVncs.Items.Clear();
                //var vncs = Root.AppManager.DatabaseManager.Basic.Vncs;
                var vncs = Root.AppManager.DatabaseManager.Basic.Vncs.OrderBy(x => x.Alias).ToList();
                Root.AppManager.DatabaseManager.Basic.Vncs = vncs;
                for (int i = 0; i < vncs.Count; i++)
                {
                    vncs[i].Index = i;
                    tbcmbVncs.Items.Add($"[{i}] {vncs[i].Alias}");
                }

                if (vncs.Count > 0)
                {
                    tbcmbVncs.SelectedIndex = vncs.Count - 1;
                }
            });
        }

        private void ShowParameter(VncConfiguration vnc)
        {
            chkStatus.Checked -= chkStatus_Checked;
            txtIndex.TextChanged -= txtIndex_TextChanged;
            txtAlias.TextChanged -= txtAlias_TextChanged;
            txtHost.TextChanged -= txtHost_TextChanged;
            txtPassword.TextChanged -= txtPassword_TextChanged;

            chkStatus.IsChecked = vnc.Enable;
            chkStatus.Content = vnc.Enable ? "Enable" : "Disable";
            txtIndex.Text = vnc.Index.ToString();
            txtAlias.Text = vnc.Alias;
            txtHost.Text = vnc.Host;
            txtPassword.Text = vnc.Password;

            chkStatus.Checked += chkStatus_Checked;
            txtIndex.TextChanged += txtIndex_TextChanged;
            txtAlias.TextChanged += txtAlias_TextChanged;
            txtHost.TextChanged += txtHost_TextChanged;
            txtPassword.TextChanged += txtPassword_TextChanged;
        }
    }
}
