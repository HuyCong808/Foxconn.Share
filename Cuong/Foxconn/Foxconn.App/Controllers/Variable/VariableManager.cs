using Foxconn.App.Helper;
using System;

namespace Foxconn.App.Controllers.Variable
{
    public class VariableManager
    {
        private VariableManager() { }
        private static VariableManager _instance { get; set; }
        public static VariableManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VariableManager();
                    Init();
                }
                return _instance;
            }
        }

        private static void Init()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }
    }
}
