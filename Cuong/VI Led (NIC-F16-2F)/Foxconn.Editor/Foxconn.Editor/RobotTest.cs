using Foxconn.Editor.Dialogs;

namespace Foxconn.Editor
{
    public class RobotTest : SocketClient
    {
        public RobotTest() : base()
        {
            InvokeDataReceived += DataReceivedEventHandler;
        }

        private void DataReceivedEventHandler(string data)
        {
            switch (data)
            {
                case "SCAN":
                    {
                        GlobalDataManager.Current.Scan = true;
                        break;
                    }
                default:
                    {
                        if (data.ToUpper().Contains("ERROR="))
                        {
                            AutoRunManagerDialog.Current.ShowErrorMessage(data.Replace("ERROR=", "").Trim());
                        }
                        break;
                    }
            }
        }
    }
}
