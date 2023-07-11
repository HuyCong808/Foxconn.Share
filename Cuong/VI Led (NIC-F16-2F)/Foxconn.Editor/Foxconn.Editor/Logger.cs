using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Globalization;
using System.IO;

namespace Foxconn.Editor
{
    public class Logger
    {
        private static readonly object _lock = new object();
        private NLog.Logger _logger { get; set; }

        public static Logger Current => __current;
        private static Logger __current = new Logger();
        static Logger() { }
        private Logger() { }

        public void Create()
        {
            try
            {
                _logger = LogManager.GetLogger("debug");

                //var layout = "[${longdate}] [${level:lowercase=true}] [${callsite:methodName=true}] [${callsite-linenumber:skipFrames=0}] ${message} ${exception}";
                //var layout = "[${longdate}] [${level:lowercase=true}] ${message} ${callsite:className=false:fileName=true:includeSourcePath=false:methodName=false} ${exception}";
                var layout = "[${longdate}] [${level:lowercase=true}] ${message} ${exception}";

                var consoleTarget = new ConsoleTarget("logConsole")
                {
                    Layout = layout
                };

                var fileTarget = new FileTarget("logFile")
                {
                    Layout = layout,
                    FileName = "${basedir}/logs/${date:format=yyyy-MM-dd}.log",
                    ArchiveFileName = "${basedir}/logs/{##}.log",
                    ArchiveDateFormat = "yyyy-MM-dd",
                    ArchiveEvery = FileArchivePeriod.None,
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                    MaxArchiveFiles = 50,
                    ArchiveAboveSize = 10000000
                };

                var config = new LoggingConfiguration();
                config.AddTarget(consoleTarget);
                config.AddTarget(fileTarget);
                config.AddRuleForAllLevels(consoleTarget);
                config.AddRuleForAllLevels(fileTarget);
                LogManager.Configuration = config;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Clear(int days = 31)
        {
            try
            {
                var path = $"{AppDomain.CurrentDomain.BaseDirectory}logs";
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
                Console.WriteLine(ex.Message);
            }
        }

        public void Debug(string message)
        {
            lock (_lock)
            {
                _logger.Debug(message);
            }
        }

        public void Error(string message)
        {
            lock (_lock)
            {
                _logger.Error(message);
            }
        }

        public void Fatal(string message)
        {
            lock (_lock)
            {
                _logger.Fatal(message);
            }
        }

        public void Info(string message)
        {
            lock (_lock)
            {
                _logger.Info(message);
            }
        }

        public void Trace(string message)
        {
            lock (_lock)
            {
                _logger.Trace(message);
            }
        }

        public void Warn(string message)
        {
            lock (_lock)
            {
                _logger.Warn(message);
            }
        }
    }
}
