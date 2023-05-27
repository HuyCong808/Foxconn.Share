using System;
using System.Resources;
using System.Threading;

namespace Foxconn.App.Helper
{
    public class Resources
    {
        public static string GetString(string cultureName, string key = "")
        {
            try
            {
                if (cultureName != null)
                {
                    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
                }

                var currentCulture = Thread.CurrentThread.CurrentCulture.ToString();
                var uriString = string.Empty;
                uriString = currentCulture switch
                {
                    "vi-VN" => System.IO.Path.GetFullPath("Resources\\Vietnamese.resx"),
                    "en-US" => System.IO.Path.GetFullPath("Resources\\English.resx"),
                    _ => System.IO.Path.GetFullPath("Resources\\English.resx"),
                };

                if (System.IO.File.Exists(uriString))
                {
                    using (var resSet = new ResXResourceSet(uriString))
                    {
                        Console.WriteLine("Resources file exists.");
                        var value = resSet.GetString(key);
                        return value;
                    }
                }
                else
                {
                    Console.WriteLine("Resources file does not exist.");
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }
    }
}
