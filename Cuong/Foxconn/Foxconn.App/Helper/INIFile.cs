using System.Runtime.InteropServices;
using System.Text;

namespace Foxconn.App.Helper
{
    public class INIFile
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public string FilePath
        {
            get => _filePath;
            set => _filePath = value;
        }
        private string _filePath;

        public INIFile(string filePath)
        {
            _filePath = filePath;
        }

        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, _filePath);
        }

        public string Read(string section, string key, string defaultData = "")
        {
            StringBuilder retVal = new StringBuilder(byte.MaxValue);
            GetPrivateProfileString(section, key, "", retVal, byte.MaxValue, _filePath);
            string str = retVal.ToString();
            return !(str != "") ? defaultData : str;
        }
    }
}
