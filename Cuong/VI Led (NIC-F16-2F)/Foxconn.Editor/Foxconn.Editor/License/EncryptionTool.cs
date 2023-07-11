using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Foxconn.Editor
{
    internal class EncryptionTool
    {
        public static string Encrypt(string text, string key = null)
        {
            try
            {
                byte[] data = Encoding.Default.GetBytes(text);
                byte[] privateKey = !string.IsNullOrEmpty(key) ? Encoding.Default.GetBytes(key) : Array.Empty<byte>();
                byte[] bytes = ProtectedData.Protect(data, privateKey, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return string.Empty;
            }
        }

        public static string Decrypt(string text, string key = null)
        {
            try
            {
                byte[] encryptedData = Convert.FromBase64String(text);
                byte[] privateKey = !string.IsNullOrEmpty(key) ? Encoding.Default.GetBytes(key) : Array.Empty<byte>();
                byte[] bytes = ProtectedData.Unprotect(encryptedData, privateKey, DataProtectionScope.CurrentUser);
                return Encoding.Default.GetString(bytes);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return string.Empty;
            }
        }

        public static string AES256_Encrypt(string encryptStr, string key)
        {
            byte[] bytes1 = Encoding.UTF8.GetBytes(key);
            byte[] bytes2 = Encoding.UTF8.GetBytes(encryptStr);
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Key = bytes1;
            rijndaelManaged.Mode = CipherMode.ECB;
            rijndaelManaged.Padding = PaddingMode.PKCS7;
            byte[] inArray = rijndaelManaged.CreateEncryptor().TransformFinalBlock(bytes2, 0, bytes2.Length);
            return Convert.ToBase64String(inArray, 0, inArray.Length);
        }

        public static string AES256_Decrypt(string decryptStr, string key)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBuffer = Convert.FromBase64String(decryptStr);
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Key = bytes;
            rijndaelManaged.Mode = CipherMode.ECB;
            rijndaelManaged.Padding = PaddingMode.PKCS7;
            return Encoding.UTF8.GetString(rijndaelManaged.CreateDecryptor().TransformFinalBlock(inputBuffer, 0, inputBuffer.Length));
        }
    }
}
