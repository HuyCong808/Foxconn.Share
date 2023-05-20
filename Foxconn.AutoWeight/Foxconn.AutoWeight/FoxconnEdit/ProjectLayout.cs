using System;
using System.Collections.Generic;
using System.IO;

namespace Foxconn.AutoWeight.FoxconnEdit
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

}

