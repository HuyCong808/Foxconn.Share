using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Foxconn.AOI.Editor
{
    public class FileExplorer
    {
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static void DeleteFiles(string path, bool subdirectories = false)
        {
            if (Directory.Exists(path))
            {
                string[] subPath = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path);
                int count = 0;
                foreach (string item in files)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(item);
                        fileInfo.Delete();
                        count++;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
                }
                if (count > 0)
                {
                    Trace.WriteLine($"FileExplorer.DeleteFiles: Delete successfully {count}/{files.Length} files at {path}");
                }
                foreach (string item in subPath)
                {
                    DeleteFiles(item, subdirectories: true);
                }
                if (subdirectories)
                {
                    Directory.Delete(path);
                    Trace.WriteLine($"FileExplorer.DeleteFiles: Delete successfully a directory at {path}");
                }
            }
        }

        public static bool IsHaveFiles(string directoryName, string partFileName)
        {
            bool flag = false;
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryName);
            if (!directoryInfo.Exists)
            {
                flag = false;
            }
            else
            {
                FileInfo[] files = directoryInfo.GetFiles();
                Trace.WriteLine("FileExplorer.IsHaveFiles: fileInfo.Length = " + files.Length);
                if (files.Length == 0)
                {
                    flag = false;
                }
                else
                {
                    string empty = string.Empty;
                    for (int index = 0; index < files.Length; ++index)
                    {
                        string name = files[index].Name;
                        Trace.WriteLine("FileExplorer.IsHaveFiles: fileInfo.name = " + name);
                        if (name.IndexOf(partFileName) > -1)
                        {
                            flag = true;
                            break;
                        }
                        flag = false;
                    }
                }
            }
            Trace.WriteLine("[FileExplorer.IsHaveFiles] ret = " + flag.ToString());
            return flag;
        }

        public static List<string> GetFileContent(string directoryName, string filename)
        {
            List<string> fileContent2 = new List<string>();
            string path = Path.Combine(directoryName, filename);
            if (IsHaveFiles(directoryName, filename))
            {
                StreamReader streamReader = new StreamReader(path);
                using (streamReader)
                {
                    for (string str = streamReader.ReadLine(); str != null; str = streamReader.ReadLine())
                        fileContent2.Add(str);
                }
                streamReader.Close();
            }
            return fileContent2;
        }

        public static List<string> GetFilePath(string directoryName)
        {
            List<string> filePath = new List<string>();
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryName);
            if (directoryInfo.Exists)
            {
                FileInfo[] files = directoryInfo.GetFiles();
                if (files.Length != 0)
                {
                    string empty = string.Empty;
                    for (int index = 0; index < files.Length; ++index)
                    {
                        string name = files[index].Name;
                        filePath.Add(name);
                    }
                }
            }
            return filePath;
        }

        public static List<string> GetFilePath(string directoryName, string partFilename)
        {
            List<string> filePath = new List<string>();
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryName);
            if (directoryInfo.Exists)
            {
                FileInfo[] files = directoryInfo.GetFiles();
                if (files.Length != 0)
                {
                    string empty = string.Empty;
                    for (int index = 0; index < files.Length; ++index)
                    {
                        string name = files[index].Name;
                        if (name.IndexOf(partFilename) > -1)
                            filePath.Add(name);
                    }
                }
            }
            return filePath;
        }

        public static long GetFileSize(string directoryName, string fileName)
        {
            long fileSize = 0;
            FileInfo fileInfo = null;
            try
            {
                fileInfo = new FileInfo(Path.Combine(directoryName, fileName));
            }
            catch
            {
            }
            if (fileInfo != null && fileInfo.Exists)
                fileSize = fileInfo.Length / 1024L / 1024L;
            return fileSize;
        }

        public static void DeleteFile(string directoryName, string filename) => File.Delete(Path.Combine(directoryName, filename));
    }
}
