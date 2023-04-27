using Foxconn.Enums;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Foxconn.Params
{
    internal class MachineParams
    {
        public MachineParams() { }
        private CameraType Type = CameraType.Unknow;
        public class MachineConfig
        {
            public class Camera1Config
            {
                public bool IsEnable { get; set; }
                public string Name { get; set; }
                public int Index { get; set; }
                public CameraType Type { get; set; }
            }
            public class Camera2Config
            {
                public bool IsEnable { get; set; }
                public string Name { get; set; }
                public int Index { get; set; }
            }
            public class Camera3Config
            {
                public bool IsEnable { get; set; }
                public string Name { get; set; }
                public int Index { get; set; }
            }

            public class Light1Config
            {
                public bool IsEnable { get; set; }
                public string PortName { get; set; }
            }
            public class Light2Config
            {
                public bool IsEnable { get; set; }
                public string PortName { get; set; }
            }
            public class PLC1config
            {
                public bool IsEnable { get; set; }
                public string Host { get; set; }
                public int Port { get; set; }
            }
            public class PLC2config
            {
                public bool IsEnable { get; set; }
                public string Host { get; set; }
                public int Port { get; set; }

            }
            public class PLC3config
            {
                public bool IsEnable { get; set; }
                public string Host { get; set; }
                public int Port { get; set; }
            }
            public class TerminalConfig
            {
                public string PortName { get; set; }
                public string Undo { get; set; }
                public string H125 { get; set; }
                public string Format { get; set; }
            }
            public class RobotConfig
            {
                public string IP { get; set; }
                public int Port { get; set; }
                public int Side { get; set; } 
            }
            public Camera1Config Camera1 { get; set; }
            public Camera2Config Camera2 { get; set; }
            public Camera3Config Camera3 { get; set; }
            public Light1Config Light1 { get; set; }
            public Light2Config Light2 { get; set; }
            public PLC1config PLC1 { get; set; }
            public PLC2config PLC2 { get; set; }
            public PLC3config PLC3 { get; set; }
            public TerminalConfig Terminal { get; set; }
            public RobotConfig Robot1 { get; set; }
            public RobotConfig Robot2 { get; set; }

            public MachineConfig Clone()
            {
                return new MachineConfig()
                {
                    Camera1 = Camera1,
                    Camera2 = Camera2,
                    Camera3 = Camera3,
                    Light1 = Light1,
                    Light2 = Light2,
                    PLC1 = PLC1,
                    PLC2 = PLC2,
                    PLC3 = PLC3,
                    Terminal = Terminal,
                    Robot1 = Robot1,
                    Robot2 = Robot2,
                };
            }
        }
        private string _fileName = "Params\\MachineParams.json";
        public static MachineConfig Config { get; set; }
        public static MachineParams Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MachineParams();
                    Config = new MachineConfig();
                }
                return _instance;
            }
        }
        private static MachineParams _instance;
        public void Read()
        {
            try
            {
                if (File.Exists(_fileName))
                {
                    var configuration = JsonConvert.DeserializeObject<MachineConfig>(File.ReadAllText(_fileName));
                    if (configuration != null)
                    {
                        Config = configuration.Clone();
                    }
                }
                else
                {
                    File.WriteAllText(_fileName, JsonConvert.SerializeObject(Config.Clone(), Newtonsoft.Json.Formatting.Indented));
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
                var configuration = JsonConvert.SerializeObject(Config, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_fileName, configuration);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }

}
