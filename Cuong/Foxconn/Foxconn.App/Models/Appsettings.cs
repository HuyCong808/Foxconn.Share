using Newtonsoft.Json;
using System;
using System.IO;

namespace Foxconn.App.Models
{
    public class Appsettings
    {
        public class Configuration
        {
            public class StringOptions
            {
                public bool Enable { get; set; }
                public int StartIndex { get; set; }
                public int Length { get; set; }

                public StringOptions()
                {
                    Enable = false;
                    StartIndex = 0;
                    Length = 63;
                }

                public StringOptions Clone()
                {
                    return new StringOptions()
                    {
                        Enable = Enable,
                        StartIndex = StartIndex,
                        Length = Length,
                    };
                }
            }

            public string Title { get; set; }
            public string Authors { get; set; }
            public string Company { get; set; }
            public string Product { get; set; }
            public string Version { get; set; }
            public string Description { get; set; }
            public string Copyright { get; set; }
            public string DateCreated { get; set; }
            public string DateModified { get; set; }
            public string FtpUrl { get; set; }
            public string FtpUser { get; set; }
            public string FtpPassword { get; set; }
            public string FtpUpdateFile { get; set; }
            public string FtpVersionFile { get; set; }
            public string DefaultDocumentName { get; set; }
            public StringOptions SubString { get; set; }

            public Configuration()
            {
                Title = "Foxconn.App";
                Authors = "Nguyen Quang Tiep";
                Company = "Foxconn Technology Group";
                Product = "Foxconn.App";
                Version = "1.0.0";
                Description = "";
                Copyright = "Nguyen Quang Tiep, (+84)90 29 65789";
                DateCreated = "2022/01/01";
                DateModified = "Monday, 10 January 2022 08:00:00 AM";
                FtpUrl = "ftp://127.0.0.1";
                FtpUser = "te";
                FtpPassword = "ubeeonly";
                FtpUpdateFile = "/Automation_Ubee/Nqt/Program/App/Update.rar";
                FtpVersionFile = "/Automation_Ubee/Nqt/Program/App/Version.txt";
                DefaultDocumentName = "APP";
                SubString = new StringOptions();
            }

            public Configuration Clone()
            {
                return new Configuration()
                {
                    Title = Title,
                    Authors = Authors,
                    Company = Company,
                    Product = Product,
                    Version = Version,
                    Description = Description,
                    Copyright = Copyright,
                    DateCreated = DateCreated,
                    DateModified = DateModified,
                    FtpUrl = FtpUrl,
                    FtpUser = FtpUser,
                    FtpPassword = FtpPassword,
                    FtpUpdateFile = FtpUpdateFile,
                    FtpVersionFile = FtpVersionFile,
                    DefaultDocumentName = DefaultDocumentName,

                    SubString = SubString?.Clone(),
                };
            }
        }

        private string _fileName = "appsettings.json";
        public static Configuration Config { get; set; }
        public static Appsettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Appsettings();
                    Config = new Configuration();
                }
                return _instance;
            }
        }   
        private static Appsettings _instance;
        public void Read()
        {
            try
            {
                if (File.Exists(_fileName))
                {
                    var configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(_fileName));
                    if (configuration != null)
                    {
                        Config = configuration.Clone();
                    }
                }
                else
                {
                    File.WriteAllText(_fileName, JsonConvert.SerializeObject(Config.Clone(), Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Write()
        {
            try
            {
                var configuration = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(_fileName, configuration);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
