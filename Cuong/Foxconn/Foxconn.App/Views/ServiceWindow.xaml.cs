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
    /// Interaction logic for ServiceWindow.xaml
    /// </summary>
    public partial class ServiceWindow : Window
    {
        private readonly MainWindow Root = MainWindow.Current;

        public ServiceWindow()
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
                Root.CodeFlow("SERVICE");

                #region Image Icon
                AppUi.ShowImage(this, imgAddService, @"Assets/Plus.png");
                AppUi.ShowImage(this, imgRemoveService, @"Assets/Subtract.png");
                #endregion

                ShowService();
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void tbcmbServices_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = tbcmbServices.SelectedIndex;
            object selectedItem = tbcmbServices.SelectedItem;
            if (selectedIndex > -1)
            {
                ShowParameter(Root.AppManager.DatabaseManager.Basic.Services[selectedIndex]);
            }
        }

        private void tbbtnAddService_Click(object sender, RoutedEventArgs e)
        {
            int count = Root.AppManager.DatabaseManager.Basic.Services.Count;
            Root.AppManager.DatabaseManager.Basic.Services.Add(new ServiceConfiguration { Index = count });
            Root.ShowMessage($"[Service] Add: {count}");
            ShowService();
        }

        private void tbbtnRemoveService_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbServices.SelectedIndex;
            object selectedItem = tbcmbServices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Services.RemoveAt(selectedIndex);
                Root.ShowMessage($"[Service] Remove {selectedIndex}");
                ShowService();
            }
            else
            {
                Root.ShowMessage($"[Service] Cannot remove service.", AppColor.Red);
                AppUi.ShowMessage("Cannot remove service.", MessageBoxImage.Error);
            }
        }

        private void chkStatus_Checked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbServices.SelectedIndex;
            object selectedItem = tbcmbServices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Services[selectedIndex].Enable = chkStatus.IsChecked == true;
                chkStatus.Content = chkStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void chkStatus_Unchecked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbServices.SelectedIndex;
            object selectedItem = tbcmbServices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Services[selectedIndex].Enable = chkStatus.IsChecked == true;
                chkStatus.Content = chkStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void txtIndex_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbServices.SelectedIndex;
            object selectedItem = tbcmbServices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Services[selectedIndex].Index = Utilities.ConvertStringToInt(txtIndex.Text);
            }
        }

        private void txtAlias_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbServices.SelectedIndex;
            object selectedItem = tbcmbServices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Services[selectedIndex].Alias = txtAlias.Text;
            }
        }

        private void txtUserId_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbServices.SelectedIndex;
            object selectedItem = tbcmbServices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Services[selectedIndex].UserId = txtUserId.Text;
            }
        }

        private void txtPassword_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbServices.SelectedIndex;
            object selectedItem = tbcmbServices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Services[selectedIndex].Password = txtPassword.Text;
            }
        }

        private void txtHost_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbServices.SelectedIndex;
            object selectedItem = tbcmbServices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Services[selectedIndex].Host = txtHost.Text;
            }
        }

        private void txtPort_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbServices.SelectedIndex;
            object selectedItem = tbcmbServices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Services[selectedIndex].Port = Utilities.ConvertStringToInt(txtPort.Text);
            }
        }

        private void txtServiceName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbServices.SelectedIndex;
            object selectedItem = tbcmbServices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Services[selectedIndex].ServiceName = txtServiceName.Text;
            }
        }

        private void ShowService()
        {
            Dispatcher.Invoke(() =>
            {
                tbcmbServices.Items.Clear();
                //var services = Root.AppManager.DatabaseManager.Basic.Services;
                var services = Root.AppManager.DatabaseManager.Basic.Services.OrderBy(x => x.Alias).ToList();
                Root.AppManager.DatabaseManager.Basic.Services = services;
                for (int i = 0; i < services.Count; i++)
                {
                    services[i].Index = i;
                    tbcmbServices.Items.Add($"[{i}] {services[i].Alias}");
                }

                if (services.Count > 0)
                {
                    tbcmbServices.SelectedIndex = services.Count - 1;
                }
            });
        }

        private void ShowParameter(ServiceConfiguration service)
        {
            chkStatus.Checked -= chkStatus_Checked;
            txtIndex.TextChanged -= txtIndex_TextChanged;
            txtAlias.TextChanged -= txtAlias_TextChanged;
            txtUserId.TextChanged -= txtUserId_TextChanged;
            txtPassword.TextChanged -= txtPassword_TextChanged;
            txtHost.TextChanged -= txtHost_TextChanged;
            txtPort.TextChanged -= txtPort_TextChanged;
            txtServiceName.TextChanged -= txtServiceName_TextChanged;

            chkStatus.IsChecked = service.Enable;
            chkStatus.Content = service.Enable ? "Enable" : "Disable";
            txtIndex.Text = service.Index.ToString();
            txtAlias.Text = service.Alias;
            txtUserId.Text = service.UserId;
            txtPassword.Text = service.Password;
            txtHost.Text = service.Host;
            txtPort.Text = service.Port.ToString();
            txtServiceName.Text = service.ServiceName;

            chkStatus.Checked += chkStatus_Checked;
            txtIndex.TextChanged += txtIndex_TextChanged;
            txtAlias.TextChanged += txtAlias_TextChanged;
            txtUserId.TextChanged += txtUserId_TextChanged;
            txtPassword.TextChanged += txtPassword_TextChanged;
            txtHost.TextChanged += txtHost_TextChanged;
            txtPort.TextChanged += txtPort_TextChanged;
            txtServiceName.TextChanged += txtServiceName_TextChanged;
        }
    }
}
