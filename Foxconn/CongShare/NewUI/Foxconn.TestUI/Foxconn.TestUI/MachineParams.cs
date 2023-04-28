using Foxconn.TestUI.Enums;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Foxconn.TestUI
{
    public class MachineParams 
    {
        private string _filePath = @"Params\MachineParams.json";
        private CameraParams _camera1 = new CameraParams();
        private CameraParams _camera2 = new CameraParams();
        private LightParams _light1 = new LightParams();
        private LightParams _light2 = new LightParams();
        private SocketParams _plc1 = new SocketParams();
        private SocketParams _plc2 = new SocketParams();
        private SocketParams _robot1 = new SocketParams();
        private SocketParams _robot2 = new SocketParams();

        private TerminalParams _terminal = new TerminalParams();
        public CameraParams Camera1 => _camera1;
        public CameraParams Camera2 => _camera2;
        public LightParams Light1 => _light1;
        public LightParams Light2 => _light2;
        public SocketParams PLC1 => _plc1;
        public SocketParams PLC2 => _plc2;
        public SocketParams Robot1 => _robot1;
        public SocketParams Robot2 => _robot2;

        public TerminalParams Terminal => _terminal;
        //singleton
        public static MachineParams Current => __current;
        private static MachineParams __current = new MachineParams();
        private MachineParams() { }
        static MachineParams() { }
        //
        public static void Reload()
        {
            MachineParams machineParams = new MachineParams();
            MachineParams loaded = machineParams.Load();
            if (loaded != null)
            {
                __current = loaded;
            }
            else
            {
                machineParams.Save();
            }
        }

        public MachineParams Load()
        {
            MachineParams machineParams = null;
            if (File.Exists(_filePath))
            {
                string contents = File.ReadAllText(_filePath);
                machineParams = JsonConvert.DeserializeObject<MachineParams>(contents);
            }
            return machineParams;
        }

        public void Save()
        {
            string contents = JsonConvert.SerializeObject(__current, Formatting.Indented);
            File.WriteAllText(_filePath, contents);
        }


        public class CameraParams
        {
            private bool _isEnabled = false;
            private string _userDefinedName = string.Empty;
            private CameraType _type = CameraType.Unknow;

            public bool IsEnabled
            {
                get => _isEnabled;
                set => _isEnabled = value;
            }

            public string UserDefinedName
            {
                get => _userDefinedName;
                set => _userDefinedName = value;
            }

            public CameraType Type
            {
                get => _type;
                set => _type = value;
            }
        }
        public class LightParams
        {
            private bool _isEnabled = false;
            private string _portName = string.Empty;

            public bool IsEnabled
            {
                get => _isEnabled;
                set => _isEnabled = value;
            }

            public string PortName
            {
                get => _portName;
                set => _portName = value;
            }
        }
        public class SocketParams
        {
            private bool _isEnabled = false;
            private string _host = string.Empty;
            private int _port = 0;

            public bool IsEnabled
            {
                get => _isEnabled;
                set => _isEnabled = value;
            }

            public string Host
            {
                get => _host;
                set => _host = value;
            }

            public int Port
            {
                get => _port;
                set => _port = value;
            }
        }
        public class TerminalParams
        {
            private bool _isEnabled = false;
            private string _portName = string.Empty;
            private string _undo = "UNDO";
            private string _user = "125H";

            public bool IsEnabled
            {
                get => _isEnabled;
                set => _isEnabled = value;
            }

            public string PortName
            {
                get => _portName;
                set => _portName = value;
            }

            public string Undo
            {
                get => _undo;
                set => _undo = value;
            }

            public string User
            {
                get => _user;
                set => _user = value;
            }

        }


    }
}
