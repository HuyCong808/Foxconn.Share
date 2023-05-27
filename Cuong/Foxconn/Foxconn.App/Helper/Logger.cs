using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Foxconn.App.Helper
{
    public enum LoggerLevel
    {
        Debug,
        Error,
        Fatal,
        Info,
        Trace,
        Warn,
    }

    public class Logger
    {
        private static NLog.Logger _logger { get; set; }
        private static NLog.Config.LoggingConfiguration _loggingConfiguration { get; set; }
        private static NLog.Layouts.CsvLayout _csvLayout { get; set; }
        private static NLog.Targets.FileTarget _fileTarget { get; set; }
        private static NLog.Targets.ConsoleTarget _consoleTarget { get; set; }
        private static DateTime _dateCreated { get; set; }
        private readonly object _syncObject = new object();

        private Logger() { }
        private static Logger _instance { get; set; }
        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Logger();
                    Init();
                }
                return _instance;
            }
        }

        private static void Init()
        {
            try
            {
                // Target
                _fileTarget = new NLog.Targets.FileTarget("systemfile") { FileName = $"Logs/{DateTime.Now:yyyy-MM-dd}.log" };
                _consoleTarget = new NLog.Targets.ConsoleTarget("logconsole");

                // Configuration
                _loggingConfiguration = new NLog.Config.LoggingConfiguration();
                _loggingConfiguration.LoggingRules.Clear();
                _loggingConfiguration.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Error, _fileTarget);
                _loggingConfiguration.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Error, _consoleTarget);
                NLog.LogManager.Configuration = _loggingConfiguration;

                // Layout
                _csvLayout = new NLog.Layouts.CsvLayout();
                _csvLayout.Columns.Add(new NLog.Layouts.CsvColumn("", "${longdate}|${level}|${callsite:methodName=true}[${callsite-linenumber:skipFrames=0}]: ${message}"));
                _fileTarget.Layout = _csvLayout;
                _consoleTarget.Layout = _csvLayout;

                // Logger
                _logger = NLog.LogManager.GetLogger("systemfile");  // fast-running method
                //_logger = NLog.LogManager.GetCurrentClassLogger();  // slow-running method

                // Date created
                _dateCreated = DateTime.Now;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void Write(string message, LoggerLevel loggerLevel = LoggerLevel.Error)
        {
            if (message == "" || _logger == null)
                return;
            lock (_syncObject)
            {
                IsNewDay();
                switch (loggerLevel)
                {
                    case LoggerLevel.Debug:
                        _logger.Debug(message);
                        break;
                    case LoggerLevel.Error:
                        _logger.Error(message);
                        break;
                    case LoggerLevel.Fatal:
                        _logger.Fatal(message);
                        break;
                    case LoggerLevel.Info:
                        _logger.Info(message);
                        break;
                    case LoggerLevel.Trace:
                        _logger.Trace(message);
                        break;
                    case LoggerLevel.Warn:
                        _logger.Warn(message);
                        break;

                    default:
                        break;
                }
            }
        }

        private void IsNewDay()
        {
            if ((DateTime.Now.Date - _dateCreated.Date).Days != 0)
            {
                Init();
            }
        }

        public static void ClearLog(int days = 31)
        {
            try
            {
                var path = $"{AppDomain.CurrentDomain.BaseDirectory}Logs";
                if (Directory.Exists(path))
                {
                    foreach (string item in Directory.GetFiles(path))
                    {
                        if (DateTime.TryParseExact(Path.GetFileNameWithoutExtension(item), "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out DateTime old))
                        {
                            var now = DateTime.Now;
                            if ((now.Date - old.Date).Days > days)
                            {
                                File.Delete(item);
                                Console.WriteLine($"Delete: {item}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        public static void SaveImage(Image<Bgr, byte> image, string modelName, int quality = 95)
        {
            try
            {
                var path = $@"{AppDomain.CurrentDomain.BaseDirectory}Logs\Images\{modelName}\{DateTime.Now:yyyy-MM-dd}";
                var fileName = $@"{path}\Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var saved = CvInvoke.Imwrite(fileName, image, new KeyValuePair<ImwriteFlags, int>(ImwriteFlags.JpegQuality, quality));
                if (saved)
                {
                    Console.WriteLine($"Saved: {fileName}");
                }
                else
                {
                    Console.WriteLine($"Cannot save: {fileName}");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
            finally
            {
                image?.Dispose();
            }
        }

        public static void SaveImage(Image<Bgr, byte> image, string modelName, string imageName, int quality = 95)
        {
            try
            {
                var path = $@"{AppDomain.CurrentDomain.BaseDirectory}Logs\Images\{modelName}\{DateTime.Now:yyyy-MM-dd}\{imageName}";
                var fileName = $@"{path}\Image_{DateTime.Now:yyyyMMddHHmmssffff}.jpeg";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var saved = CvInvoke.Imwrite(fileName, image, new KeyValuePair<ImwriteFlags, int>(ImwriteFlags.JpegQuality, quality));
                if (saved)
                {
                    Console.WriteLine($"Saved: {fileName}");
                }
                else
                {
                    Console.WriteLine($"Cannot save: {fileName}");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
            finally
            {
                image?.Dispose();
            }
        }

        public static void ClearImage(string modelName, int days = 31)
        {
            try
            {
                var path = $@"{AppDomain.CurrentDomain.BaseDirectory}Logs\Images\{modelName}";
                if (Directory.Exists(path))
                {
                    foreach (string item in Directory.GetDirectories(path))
                    {
                        if (DateTime.TryParseExact(new DirectoryInfo(item).Name, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out DateTime old))
                        {
                            var now = DateTime.Now;
                            if ((now.Date - old.Date).Days > days)
                            {
                                ClearFolder(item);
                                Directory.Delete(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        public static void ClearFolder(string path)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(path);

                foreach (var file in directoryInfo.GetFiles())
                {
                    file.Delete();
                    Console.WriteLine($"Delete file: {file}");
                }

                foreach (var directory in directoryInfo.GetDirectories())
                {
                    ClearFolder(directory.FullName);
                    directory.Delete();
                    Console.WriteLine($"Delete folder: {directory}");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }
    }
}
