using Foxconn.TestUI.Editor;
using System.Windows;

namespace Foxconn.TestUI.View
{
    /// <summary>
    /// Interaction logic for SetSpeedRobot.xaml
    /// </summary>
    public partial class SetSpeedRobot : Window
    {
        private DeviceManager _device = DeviceManager.Current;

        public SetSpeedRobot()
        {
            InitializeComponent();
        }

        private void Set_Click(object sender, RoutedEventArgs e)
        {
            string data = "SPEED:X" + "\r\n";
            string speed = txtSpeedRobot.Text;
            string data2 = data.Replace("X", speed);
          //  _device.Robot2.SocketWriteData(data2);
            System.Console.WriteLine(data2);
            Close();
        }
    }
}
