using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace Foxconn.App.Helper
{
    public class Language
    {
        public static void Apply(Window window, string cultureName = null)
        {
            try
            {

                window.Dispatcher.Invoke(() =>
                {
                    if (cultureName != null)
                    {
                        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
                    }

                    var dict = new ResourceDictionary();
                    string currentCulture = Thread.CurrentThread.CurrentCulture.ToString();
                    string uriString = string.Empty;
                    uriString = currentCulture switch
                    {
                        "vi-VN" => "Languages\\Vietnamese.xaml",
                        "en-US" => "Languages\\English.xaml",
                        _ => "Languages\\English.xaml",
                    };
                    if (File.Exists(uriString))
                    {
                        Console.WriteLine("Language file exists.");
                        dict.Source = new Uri(Path.GetFullPath(uriString), UriKind.RelativeOrAbsolute);
                        window.Resources.MergedDictionaries.Clear();
                        window.Resources.MergedDictionaries.Add(dict);
                    }
                    else
                    {
                        Console.WriteLine("Language file does not exist.");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
