using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Foxconn.App.Models.BasicConfiguration.CameraConfiguration;

namespace Foxconn.App.Views
{
    /// <summary>
    /// Interaction logic for CameraSignalWindow.xaml
    /// </summary>
    public partial class CameraSignalWindow : Window
    {
        private readonly MainWindow Root = MainWindow.Current;

        public CameraSignalWindow()
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
                Root.CodeFlow("CAMERA SIGNAL");

                #region Image Icon
                AppUi.ShowImage(this, imgAddSignal, @"Assets/Plus.png");
                AppUi.ShowImage(this, imgRemoveSignal, @"Assets/Subtract.png");
                #endregion

                ShowSignal();
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void tbcmbSignals_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = tbcmbSignals.SelectedIndex;
            object selectedItem = tbcmbSignals.SelectedItem;
            if (selectedIndex > -1)
            {
                ShowParameter(Root.AppManager.DatabaseManager.Basic.Camera.Signals[selectedIndex]);
            }
        }

        private void tbbtnAddSignal_Click(object sender, RoutedEventArgs e)
        {
            int count = Root.AppManager.DatabaseManager.Basic.Camera.Signals.Count;
            Root.AppManager.DatabaseManager.Basic.Camera.Signals.Add(new CameraSignal { Index = count });
            Root.ShowMessage($"[Signal] Add: {count}");
            ShowSignal();
        }

        private void tbbtnRemoveSignal_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbSignals.SelectedIndex;
            object selectedItem = tbcmbSignals.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Camera.Signals.RemoveAt(selectedIndex);
                Root.ShowMessage($"[Signal] Remove {selectedIndex}");
                ShowSignal();
            }
            else
            {
                Root.ShowMessage($"[Signal] Cannot remove signal.", AppColor.Red);
                AppUi.ShowMessage("Cannot remove signal.", MessageBoxImage.Error);
            }
        }

        private void chkStatus_Checked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbSignals.SelectedIndex;
            object selectedItem = tbcmbSignals.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Camera.Signals[selectedIndex].Enable = chkStatus.IsChecked == true;
                chkStatus.Content = chkStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void chkStatus_Unchecked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = tbcmbSignals.SelectedIndex;
            object selectedItem = tbcmbSignals.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Camera.Signals[selectedIndex].Enable = chkStatus.IsChecked == true;
                chkStatus.Content = chkStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void txtIndex_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbSignals.SelectedIndex;
            object selectedItem = tbcmbSignals.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Camera.Signals[selectedIndex].Index = Utilities.ConvertStringToInt(txtIndex.Text);
            }
        }

        private void txtAlias_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbSignals.SelectedIndex;
            object selectedItem = tbcmbSignals.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Camera.Signals[selectedIndex].Alias = txtAlias.Text;
            }
        }

        private void txtCanCheck_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbSignals.SelectedIndex;
            object selectedItem = tbcmbSignals.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Camera.Signals[selectedIndex].CanCheck = txtCanCheck.Text;
            }
        }

        private void txtPassed_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbSignals.SelectedIndex;
            object selectedItem = tbcmbSignals.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Camera.Signals[selectedIndex].Passed = txtPassed.Text;
            }
        }

        private void txtFailed_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = tbcmbSignals.SelectedIndex;
            object selectedItem = tbcmbSignals.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Camera.Signals[selectedIndex].Failed = txtFailed.Text;
            }
        }

        private void ShowSignal()
        {
            Dispatcher.Invoke(() =>
            {
                tbcmbSignals.Items.Clear();
                //var commands = Root.AppManager.DatabaseManager.Basic.Camera.Signals;
                var signals = Root.AppManager.DatabaseManager.Basic.Camera.Signals.OrderBy(x => x.Alias).ToList();
                Root.AppManager.DatabaseManager.Basic.Camera.Signals = signals;
                for (int i = 0; i < signals.Count; i++)
                {
                    signals[i].Index = i;
                    tbcmbSignals.Items.Add($"[{i}] {signals[i].Alias}");
                }

                if (signals.Count > 0)
                {
                    tbcmbSignals.SelectedIndex = signals.Count - 1;
                }
            });
        }

        private void ShowParameter(CameraSignal signal)
        {
            chkStatus.Checked -= chkStatus_Checked;
            txtIndex.TextChanged -= txtIndex_TextChanged;
            txtAlias.TextChanged -= txtAlias_TextChanged;
            txtCanCheck.TextChanged -= txtCanCheck_TextChanged;
            txtPassed.TextChanged -= txtPassed_TextChanged;
            txtFailed.TextChanged -= txtFailed_TextChanged;

            chkStatus.IsChecked = signal.Enable;
            chkStatus.Content = signal.Enable ? "Enable" : "Disable";
            txtIndex.Text = signal.Index.ToString();
            txtAlias.Text = signal.Alias;
            txtCanCheck.Text = signal.CanCheck;
            txtPassed.Text = signal.Passed;
            txtFailed.Text = signal.Failed;

            chkStatus.Checked += chkStatus_Checked;
            txtIndex.TextChanged += txtIndex_TextChanged;
            txtAlias.TextChanged += txtAlias_TextChanged;
            txtCanCheck.TextChanged += txtCanCheck_TextChanged;
            txtPassed.TextChanged += txtPassed_TextChanged;
            txtFailed.TextChanged += txtFailed_TextChanged;
        }
    }
}
