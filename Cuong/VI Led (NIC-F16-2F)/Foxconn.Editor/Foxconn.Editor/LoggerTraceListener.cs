using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Foxconn.Editor
{
    public class LoggerTraceListener : TraceListener
    {
        private int _maxFileSize = 20;
        private int _maxFileCount = 10;
        private string _outputDir = string.Empty;
        private string _filename = string.Empty;

        public LoggerTraceListener()
        {
            _outputDir = $"{AppDomain.CurrentDomain.BaseDirectory}logs";
            if (!Directory.Exists(_outputDir))
                Directory.CreateDirectory(_outputDir);
            InitOutputLogSetting();
        }

        private void InitOutputLogSetting()
        {
            try
            {
                List<string> fileContent = FileExplorer.GetFileContent(Environment.ExpandEnvironmentVariables("%FOXCONNAOI_ROOT%"), "OutputLogSetting.txt");
                if (fileContent.Count > 0)
                {
                    string str = fileContent.Where(s => s.Contains("_maxFileCount")).FirstOrDefault();
                    if (str != null)
                        _maxFileCount = int.Parse(str.Split(':')[1]);
                }
                Trace.WriteLine($"LoggerTraceListener.GetOutputLogSetting: _maxFileCount = {_maxFileCount}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"LoggerTraceListener.GetOutputLogSetting: Exception = {ex}");
            }
        }

        private string GetOutputFileName()
        {
            string tempFilename = string.Format("{0}.log", DateTime.Now.ToString("yyyy-MM-dd_HHmmssffff"));
            List<string> filePath = FileExplorer.GetFilePath(_outputDir);
            if (filePath.Count > 0)
                filePath.Sort();
            if (filePath.Count == 0)
            {
                _filename = tempFilename;
                filePath.Add(tempFilename);
            }
            else if (FileExplorer.GetFileSize(_outputDir, filePath[filePath.Count - 1]) >= _maxFileSize)
            {
                _filename = tempFilename;
                filePath.Add(tempFilename);
            }
            else
            {
                _filename = filePath[filePath.Count - 1];
            }
            if (filePath.Count > _maxFileCount)
            {
                int num = filePath.Count - _maxFileCount;
                for (int i = 0; i < num; ++i)
                    FileExplorer.DeleteFile(_outputDir, filePath[i]);
            }
            return Path.Combine(_outputDir, _filename);
        }

        public override void Write(string message)
        {
            message = Format(message, "");
            File.AppendAllText(GetOutputFileName(), message);
            Console.WriteLine(message);
        }

        public override void Write(object obj)
        {
            string contents = Format(obj, "");
            File.AppendAllText(GetOutputFileName(), contents);
            Console.WriteLine(contents);
        }

        public override void WriteLine(object obj)
        {
            string contents = Format(obj, "");
            File.AppendAllText(GetOutputFileName(), contents);
            Console.WriteLine(contents);
        }

        public override void WriteLine(string message)
        {
            message = Format(message, "");
            File.AppendAllText(GetOutputFileName(), message);
            Console.WriteLine(message);
        }

        public override void WriteLine(object obj, string category)
        {
            string contents = Format(obj, category);
            File.AppendAllText(GetOutputFileName(), contents);
            Console.WriteLine(contents);
        }

        public override void WriteLine(string message, string category)
        {
            message = Format(message, category);
            File.AppendAllText(GetOutputFileName(), message);
            Console.WriteLine(message);
        }

        private string Format(object obj, string category)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("[{0}] ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
            if (!string.IsNullOrEmpty(category))
                stringBuilder.AppendFormat("[{0}] ", category);
            if (obj is Exception)
            {
                Exception exception = (Exception)obj;
                stringBuilder.Append(exception.Message + "\r\n");
                stringBuilder.Append(exception.StackTrace + "\r\n");
            }
            else
            {
                stringBuilder.Append(obj.ToString() + "\r\n");
            }
            return stringBuilder.ToString();
        }
    }
}
