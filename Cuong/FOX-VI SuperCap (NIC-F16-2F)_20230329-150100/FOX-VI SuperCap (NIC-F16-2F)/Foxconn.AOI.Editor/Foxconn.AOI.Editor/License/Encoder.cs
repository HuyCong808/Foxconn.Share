using System;
using System.IO;
using System.Text;

namespace Foxconn.AOI.Editor
{
    public class Encoder
    {
        public static void example()
        {
            string data = "test encode";
            string str = Decode(Encode(data));
            data.Equals(str);
        }

        public static string Encode(string data)
        {
            string str = data;
            if (!data.StartsWith("ALPHABETCODE@"))
                str = "ALPHABETCODE@" + EncodeAlphabet(data);
            return str;
        }

        public static string Decode(string data)
        {
            string str = data;
            if (data.StartsWith("ALPHABETCODE@"))
                str = DecodeAlphabet(data.Substring("ALPHABETCODE@".Length));
            return str;
        }

        public static string getFileData(string file) => ToStr(File2Bytes(file));

        public static byte[] File2Bytes(string path)
        {
            if (!File.Exists(path))
                return new byte[0];
            FileInfo fileInfo = new FileInfo(path);
            byte[] buffer = new byte[fileInfo.Length];
            FileStream fileStream = fileInfo.OpenRead();
            fileStream.Read(buffer, 0, Convert.ToInt32(fileStream.Length));
            fileStream.Close();
            return buffer;
        }

        private static string fileToString(string filePath) => Encoding.UTF8.GetString(File2Bytes(filePath));

        public static string EncodeAlphabet(string data) => ToStr(Encoding.UTF8.GetBytes(data));

        private static string ToStr(byte[] B)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in B)
                stringBuilder.Append(ToStr(b));
            return stringBuilder.ToString();
        }

        private static string ToStr(byte b)
        {
            char ch = ToChar(b / 16);
            string str1 = ch.ToString();
            ch = ToChar(b % 16);
            string str2 = ch.ToString();
            return str1 + str2;
        }

        private static char ToChar(int n) => (char)(97 + n);

        public static string DecodeAlphabet(string data) => Encoding.UTF8.GetString(ToBytes(data));

        public static byte[] ToBytes(string data)
        {
            byte[] bytes = new byte[data.Length / 2];
            char[] charArray = data.ToCharArray();
            for (int index = 0; index < charArray.Length; index += 2)
            {
                byte num = ToByte(charArray[index], charArray[index + 1]);
                bytes[index / 2] = num;
            }
            return bytes;
        }

        private static byte ToByte(char a1, char a2) => (byte)((a1 - 97) * 16 + (a2 - 97));
    }
}
