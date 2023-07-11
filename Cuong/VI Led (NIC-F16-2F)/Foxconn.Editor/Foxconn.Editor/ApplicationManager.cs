using Foxconn.Editor.Dialogs;
using System;
using System.Diagnostics;

namespace Foxconn.Editor
{
    internal class ApplicationManager
    {
        public static ApplicationManager Current => __current;
        private static ApplicationManager __current = new ApplicationManager();
        static ApplicationManager() { }
        private ApplicationManager() { }

        public void Init()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void Run()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void Stop()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public bool IsAdmin()
        {
            LoginDialog loginDialog = new LoginDialog();
            loginDialog.ShowDialog();
            if (loginDialog.Username.Text == "admin" && loginDialog.Password.Password == "admin")
            {
                return true;
            }
            return false;
        }
    }
}
