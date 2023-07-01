using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Foxconn.AOI.Editor
{
    internal static class IdentityManager
    {
        private static UserIdentity _userIdentity = null;

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool CloseHandle(IntPtr handle);

        public static event EventHandler UserChanged;

        private static void RaiseUserChanged()
        {
            if (UserChanged == null)
                return;
            UserChanged(null, EventArgs.Empty);
        }

        static IdentityManager() => _userIdentity = new UserIdentity();

        public static bool LogonUser(string username, string password, string operatorName, string shiftName)
        {
            IntPtr zero = IntPtr.Zero;
            if (!LogonUser(username, ".", password, 2, 0, ref zero))
            {
                Trace.WriteLine("IdentityManager.LoginUser: Failure username = " + username + ", pwd = " + password);
                return false;
            }
            try
            {
                Logout(false);
                _userIdentity = new UserIdentity(new WindowsIdentity(zero), operatorName, shiftName);
                RaiseUserChanged();
            }
            finally
            {
                CloseHandle(zero);
            }
            return true;
        }

        private static void Logout(bool isRaiseEvent)
        {
            if (_userIdentity == null)
                return;
            _userIdentity = new UserIdentity();
            if (!isRaiseEvent)
                return;
            RaiseUserChanged();
        }

        public static void Logoff() => Logout(true);

        public static bool HasUserLogon => _userIdentity.IsExplicitLogin;

        public static IUserIdentity CurrentIdentity => _userIdentity;

        public static void Init() => AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

        public static bool IsProgrammer() => _userIdentity.IsProgrammer;

        public static bool IsOperator() => _userIdentity.IsOperator;

        public static bool IsHighProgrammer() => _userIdentity.IsHighProgrammer;

        public static bool IsFoxconnAOI()
        {
            string loginName = _userIdentity.LoginName;
            int startIndex = loginName.LastIndexOf("\\") + 1;
            return loginName.Substring(startIndex, loginName.Length - startIndex).ToLower().Equals("admin");
        }

        private static void Demand(string role)
        {
        }

        public static void Impersonate(Action action)
        {
            //if (!_userIdentity.IsExplicitLogon)
            //{
            //    action();
            //}
            //else
            //{
            //    WindowsImpersonationContext impersonationContext = _userIdentity.LoginID.Impersonate();
            //    try
            //    {
            //        action();
            //    }
            //    finally
            //    {
            //        impersonationContext.Undo();
            //        impersonationContext.Dispose();
            //    }
            //}
        }

        public static void DemandOperator() => Demand("Foxconn.AOI.Operators");

        public static void DemandProgrammer() => Demand("Foxconn.AOI.Programmers");

        private class UserIdentity : IUserIdentity
        {
            private WindowsIdentity _loginID;
            private string _operatorName;
            private string _shift;
            private bool _isExplicitLogin;

            public UserIdentity()
            {
                _loginID = WindowsIdentity.GetCurrent();
                _operatorName = _loginID.Name;
                _shift = string.Empty;
                _isExplicitLogin = false;
            }

            public UserIdentity(WindowsIdentity logonID, string operatorName, string shift)
            {
                if (logonID == null)
                    throw new ArgumentNullException(nameof(logonID));
                if (operatorName == null)
                    throw new ArgumentNullException(nameof(operatorName));
                if (shift == null)
                    throw new ArgumentNullException(nameof(shift));
                _loginID = logonID;
                _operatorName = operatorName.Trim();
                _shift = shift.Trim();
                _isExplicitLogin = true;
            }

            public bool IsInRole(string role) => new WindowsPrincipal(_loginID).IsInRole(role);

            public bool IsProgrammer => IsInRole("Foxconn.AOI.Programmers");

            public bool IsHighProgrammer => IsInRole("Foxconn.AOI.Programmers") && !GetIdentityName().Equals("admin");

            private string GetIdentityName()
            {
                string identityName = "";
                string[] strArray = _loginID.Name.Split('\\');
                if ((uint)strArray.Length > 0U)
                    identityName = strArray[strArray.Length - 1];
                return identityName;
            }

            public bool IsOperator => IsInRole("Foxconn.AOI.Operators");

            public bool IsExplicitLogin => _isExplicitLogin;

            public string LoginName => _loginID.Name;

            public WindowsIdentity LoginID => _loginID;

            public string OperatorName => _operatorName;

            public string Shift => _shift;
        }
    }
}
