using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Foxconn.Params
{
    public class FOVParams
    {
        public class FOVconfig
        {
            public class AppParamsconfig
            {
                public List<FOVsPara> FOVs { get; set; }
                public string ParamPatch { get; set; }
            }
            public class FOVsPara
            {
                public int ID { get; set; }
                public int ExposureTime { get; set; }
                public string Name { get; set; }
                public List<ComponentPara> Components { get; set; }
            }
            public class ComponentPara
            {
                public int ID { get; set; }
                public string Algorithm { get; set; }

                public Barcode Barcode { get; set; }
                //   public ROI ROI { get; set; }
                public string ImagePatch { get; set; }
                public BRectangle ROI { get; set; }
                public ComponentPara Clone()
                {
                    return (ComponentPara)MemberwiseClone();
                }
            }
            public class ROI
            {
                public int X { get; set; }
                public int Y { get; set; }
                public int Height { get; set; }
                public int Width { get; set; }
            }
            public class Barcode
            {
                public string Format { get; set; }
                public string Prefix { get; set; }
                public int Length { get; set; }
            }
            public AppParamsconfig AppParams { get; set; }
            public FOVconfig Clone()
            {
                return new FOVconfig()
                {
                    AppParams = AppParams,
                };
            }

        }
        private string _fileName = "Params\\FOVParams.json";
        public static FOVconfig Config { get; set; }
        public static FOVParams Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FOVParams();
                    Config = new FOVconfig();
                }
                return _instance;
            }
        }
        private static FOVParams _instance;

        public void Read()
        {
            try
            {
                if (File.Exists(_fileName))
                {
                    var configuration = JsonConvert.DeserializeObject<FOVconfig>(File.ReadAllText(_fileName));
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