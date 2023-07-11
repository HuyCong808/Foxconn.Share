using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;

namespace Foxconn.Editor
{
    public class LC
    {
        private string key = string.Empty;
        private string filename = string.Empty;
        private Timer timer = new Timer();
        private static int userRightLevel = 0;
        private int secondCount = 0;
        private int hourCount = 24;

        public Action deadlineAction = null;

        public static bool CanOffline => userRightLevel > 0;

        public static bool CanOnline => userRightLevel > 1;

        public static bool IsDemo => userRightLevel > 2;

        public static bool CanTune => userRightLevel > 2;

        public LC()
        {
            key = ComputerInfo.GetComputerInfo();
            filename = "license.lic";
            timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            timer.Interval = 60000.0;
            timer.Start();
            Trace.WriteLine(key);
            Test();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            secondCount += 60;
            Application.Current.Dispatcher.Invoke((Delegate)(() =>
            {
                try
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    if (secondCount == 3600)
                    {
                        secondCount = 0;
                        ++hourCount;
                    }

                    if (hourCount == 24)
                        hourCount = 0;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }));
        }

        public bool Verify()
        {
            try
            {
                if (File.Exists(filename))
                {
                    var contents = File.ReadAllText(filename);
                    JsonSerializerSettings formatting = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        ContractResolver = new EncryptedStringPropertyResolver("NGUYENQUANGTIEP")
                    };
                    var license = JsonConvert.DeserializeObject<LCInfo>(contents, formatting);
                    if (license.Key == key)
                    {
                        if (license.Type == "Lifetime")
                        {
                            return true;
                        }
                        else if (license.Type == "Trial")
                        {
                            var licenseTime = DateTime.ParseExact(license.Time, "yyyy/MM/dd/ HH:mm:ss.ffff", System.Globalization.CultureInfo.InvariantCulture);
                            var totalMinutes = (DateTime.Now - licenseTime).TotalMinutes;
                            var totalDays = (DateTime.Now - licenseTime).Days;
                            if (totalMinutes > 0)
                            {
                                deadlineAction();
                                return false;
                            }
                            else
                            {
                                if (Math.Abs(totalDays) <= 7)
                                {
                                    string message = $"Trial license left {Math.Abs(totalDays) + 1} day.\r\nContact: Nguyen Quang Tiep, (+84)90 29 65789, nguyenquangtiep222@gmail.com";
                                    MessageBox.Show(message, "License", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                                }
                            }
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
        }

        public void Close()
        {
            try
            {
                timer.Stop();
                timer.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        private void Test()
        {
            try
            {
                var license = new LCInfo
                {
                    Name = "FOXCONN",
                    Version = "1.0.0.0",
                    Description = "Not Available",
                    Key = key,
                    Type = "Lifetime",
                    Time = DateTime.Now.ToString("yyyy/MM/dd/ HH:mm:ss.ffff")
                };
                //var license = new LCInfo
                //{
                //    Name = "FOXCONN",
                //    Version = "1.0.0.0",
                //    Description = "Not Available",
                //    Key = key,
                //    Type = "Trial",
                //    Time = DateTime.Now.AddDays(30).ToString("yyyy/MM/dd/ HH:mm:ss.ffff")
                //};
                JsonSerializerSettings formatting = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ContractResolver = new EncryptedStringPropertyResolver("NGUYENQUANGTIEP")
                };
                var contents = JsonConvert.SerializeObject(license, formatting);
                File.WriteAllText(filename, contents);

                //var contents = File.ReadAllText(filename);
                //var license = JsonConvert.DeserializeObject<LCInfo>(contents, formatting);
                //MessageBox.Show(license.HardwareInfo);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
