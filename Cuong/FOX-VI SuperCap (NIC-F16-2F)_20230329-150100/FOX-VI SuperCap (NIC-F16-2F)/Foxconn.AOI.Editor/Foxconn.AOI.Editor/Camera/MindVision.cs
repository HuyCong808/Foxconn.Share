namespace Foxconn.AOI.Editor.Camera
{
    public class MindVision : ICamera
    {
        private readonly object _lockObj = new object();
        private string _userDefinedName = string.Empty;
        private bool _isConnected = false;
        private bool _isGrabbing = false;
        private bool _isStreaming = false;

        public bool IsConnected => _isConnected;

        public bool IsGrabbing => _isGrabbing;

        public bool IsStreaming => _isStreaming;

        public void DeviceListAcq()
        {

        }

        public int Open(string userDefinedName = "")
        {
            return -1;
        }

        public int Close()
        {
            return -1;
        }

        public int StartGrabbing()
        {
            return -1;
        }

        public int StopGrabbing()
        {
            return -1;
        }

        public int StartStreaming()
        {
            return -1;
        }

        public int StopStreaming()
        {
            return -1;
        }

        public int GetParameter(KeyName key, ref object value)
        {
            return 1;
        }

        public int SetParameter(KeyName key, object value)
        {
            return 1;
        }

        public void ClearImageBuffer()
        {

        }

        public System.Drawing.Bitmap GrabFrame(int timeout = 1000)
        {
            return null;
        }
    }
}
