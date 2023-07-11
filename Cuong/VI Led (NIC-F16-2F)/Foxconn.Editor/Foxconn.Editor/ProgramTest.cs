using System;

namespace Foxconn.Editor
{
    public class ProgramTest : SocketClient
    {
        public ProgramTest() : base()
        {
            InvokeDataReceived += DataReceivedEventHandler;
        }

        private void DataReceivedEventHandler(string data)
        {
            try
            {
                // Format: CHECK#1#SN
                if (data.Contains("CHECK"))
                {
                    GlobalDataManager.Current.Scan = false;
                    GlobalDataManager.Current.Num = 0;
                    GlobalDataManager.Current.SN = string.Empty;
                    string[] parts = data.Split("_");
                    if (parts.Length == 3)
                    {
                        if (int.TryParse(parts[1], out int num))
                        {
                            GlobalDataManager.Current.Scan = true;
                            GlobalDataManager.Current.Num = num;
                            GlobalDataManager.Current.SN = parts[2];
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
