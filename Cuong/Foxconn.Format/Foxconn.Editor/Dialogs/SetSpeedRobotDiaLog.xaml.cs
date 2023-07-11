using System;
using System.Windows;
namespace Foxconn.Editor.Dialogs
{
    public partial class SetSpeedRobotDiaLog : Window
    {
        public SetSpeedRobotDiaLog()
        {
            InitializeComponent();
            CheckInput();

        }

        private void btnSetSpeedRobot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //AutoRun autoRun = new AutoRun();
                DeviceManager _device = DeviceManager.Current;
                string str = txtSetSpeedRobot.Text;
                if (int.TryParse(str, out int value) && str != string.Empty)
                {
                    if (value > 0 && value <= 100)
                    {
                        string data = $"Set Speed: {value}".Trim();
                       // _device.TCPClient.SocketWriteData(data);
                        MessageShow.Info(data, "Set Speed");
                        LogInfo(data);
                    }
                    else
                    {
                        MessageBox.Show("Out of range value: 1-100", "Set Speed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Input error!", "Set Speed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Close();
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        private void btnCancelSpeedRobot_Click(object sender, RoutedEventArgs e)
        { 
            Close();
        }

        public void CheckInput()
        {
            if (txtSetSpeedRobot.Text.Length > 0)
            {
                btnSetSpeedRobot.IsEnabled = true;
            }
            else
            {
                btnSetSpeedRobot.IsEnabled = false;
            }
        }

        public void LogInfo(string message)
        {
            Logger.Current.Info(message);
        }
        public void LogError(string message)
        {
            Logger.Current.Error(message);
        }

        private void txtSetSpeedRobot_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CheckInput();
        }
    }
}
