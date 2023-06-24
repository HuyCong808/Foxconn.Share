using System;
using System.Windows;
namespace Foxconn.Editor.Dialogs
{
    public partial class SetSpeedRobotDiaLog : Window
    {
        public SetSpeedRobotDiaLog()
        {
            InitializeComponent();
        }

        private void btnSetSpeedRobot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
            AutoRun autoRun = new AutoRun();
                string str = txtSetSpeedRobot.Text;
                if (int.TryParse(str, out int value) && str != string.Empty)
                {
                    if (value > 0 && value <= 100)
                    {
                        string data = $"Set Speed: {value}".Trim();
                        //_device.TCPClient.SocketWriteData(data);
                        MessageBox.Show(data, "Set Speed", MessageBoxButton.OK, MessageBoxImage.Information);
                        LogInfo(data);
                       // autoRun.LogsAutoRun($"SENT_TCPCLIENT: SPEED = {value}".Trim());
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

        public void LogInfo(string message)
        {
            Logger.Current.Info(message);
        }
        public void LogError(string message)
        {
            Logger.Current.Error(message);
        }


    }
}
