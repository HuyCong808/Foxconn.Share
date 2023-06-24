using Foxconn.Editor.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Foxconn.Editor.Controls
{
    /// <summary>
    /// Interaction logic for SetSpeedRobotControl.xaml
    /// </summary>
    public partial class SetSpeedRobotControl : UserControl
    {

        private DeviceManager _device = DeviceManager.Current;
        public SetSpeedRobotControl()
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
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
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



    }
}
