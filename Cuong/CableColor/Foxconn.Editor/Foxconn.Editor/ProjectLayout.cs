using System;
using System.Collections.Generic;
using System.IO;

namespace Foxconn.Editor
{
    public static class ProjectLayout
    {
        public static void Init()
        {
            try
            {
                var directories = new List<string>()
            {
                "data",
                "params",
                "logs",
                "docs",
                "temp"
            };

                foreach (var item in directories)
                {
                    if (!Directory.Exists(item))
                    {
                        Directory.CreateDirectory(item);
                    }
                }
                if (!Directory.Exists("data\\images"))
                {
                    Directory.CreateDirectory("data\\images");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

}

