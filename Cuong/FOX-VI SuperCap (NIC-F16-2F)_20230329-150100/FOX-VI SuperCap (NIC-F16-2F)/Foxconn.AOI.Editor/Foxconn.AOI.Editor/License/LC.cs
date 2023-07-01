using Foxconn.AOI.Editor.License;
using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;

namespace Foxconn.AOI.Editor
{
    public class LC
    {
        private static string __licenseTo = "Unlicensed";
        private static string __sn = "Unlicensed";
        private string currentKey = string.Empty;
        private string dbPath = string.Empty;
        private Timer timer = new Timer();
        private TimeSpan ts = new TimeSpan();
        private NotificationWindow dialog = null;
        private SQLite sqlite = null;
        private static int userRightLevel = 0;
        public Action deadlineAction = null;
        private bool isOldLicenseFileName = false;
        private static int __features = 0;
        private const int OFFLINE_MASK = 1;
        private const int ONLINE_MASK = 2;
        private const int TUNE_MASK = 64;
        private const int DEMO_MASK = 128;
        private int sencondCount = 0;
        private int hourCount = 24;
        private bool isFirstLoad = true;
        private bool isDeadline = false;

        public static string LicenseTo => __licenseTo;

        public static string SN => __sn;

        public static bool CanOffline => userRightLevel > 0;

        public static bool CanOnline => userRightLevel > 1;

        public static bool IsDemo => userRightLevel > 2;

        public static bool CanTune => userRightLevel > 2;

        public LC()
        {
            currentKey = ComputerInfo.GetComputerInfo();
            timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            timer.Interval = 60000.0;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            sencondCount += 60;
            Application.Current.Dispatcher.Invoke((Delegate)(() =>
            {
                try
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    if (ts.TotalSeconds <= 0.0)
                    {
                        if (dialog != null)
                            dialog.Close();
                        if (deadlineAction != null && !isDeadline)
                        {
                            isDeadline = true;
                            deadlineAction();
                        }
                    }
                    if (sencondCount == 3600)
                    {
                        sencondCount = 0;
                        UpdateDB();
                        ++hourCount;
                    }
                    if (hourCount == 24)
                        hourCount = 0;
                    if (ts.TotalSeconds <= 604800.0 && (isFirstLoad || ts.TotalSeconds % 7200.0 == 0.0))
                    {
                        isFirstLoad = false;
                        if (dialog == null || !dialog.IsLoaded)
                        {
                            dialog = new NotificationWindow();
                            dialog.Show();
                        }
                    }
                    if (dialog == null || !dialog.IsLoaded)
                        return;
                    dialog.tbTime.Text = ts.ToString("g");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("FOXCONN AOI =====> License UpdateDB exception = " + ex.ToString());
                }
            }));
            ts = ts.Subtract(new TimeSpan(0, 1, 0));
        }

        public bool Verify()
        {
            try
            {
                dbPath = "license.db";
                if (File.Exists(dbPath))
                {
                    return VerifyByLicense();
                }
                return VerifyByCert();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                Trace.WriteLine("FOXCONN AOI =====> Verify Exception = " + ex);
                return false;
            }
        }

        public bool VerifyByLicense()
        {
            return true;
        }

        public bool VerifyByCert()
        {
            return true;
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
                Trace.WriteLine(ex.ToString());
            }
        }

        private void UpdateDB()
        {
            try
            {
                if (sqlite == null)
                    return;
                sqlite.ExecuteNonQuery(string.Format("update {0} set TrialTime='{1}' where ID=1", sqlite.tableName, ts.ToString("g")));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }

        private void UpdateTestDB()
        {
            try
            {
                if (sqlite == null)
                    return;
                sqlite.ExecuteNonQuery(string.Format("update {0} set TrialTime='{1}' where ID=1", sqlite.tableName, "00:00:01:00"));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}
