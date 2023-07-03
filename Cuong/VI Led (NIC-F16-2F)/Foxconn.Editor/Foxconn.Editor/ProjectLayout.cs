using System;
using System.Collections.Generic;
using System.IO;

namespace Foxconn.Editor
{
    internal static class ProjectLayout
    {
        public static void Init()
        {
            try
            {
                var directories = new List<string>()
                {
                    "data",
                    "params",
                    "docs",
                    "temp"
                };
                var files = new List<string>();

                foreach (var item in directories)
                {
                    if (!Directory.Exists(item))
                        Directory.CreateDirectory(item);
                }

                foreach (var item in files)
                {
                    if (!File.Exists(item))
                        Directory.CreateDirectory(item);
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
