using Foxconn.Editor.Enums;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Foxconn.Editor
{
    public class MachineParams
    {
        private string _filePath = @$"{AppDomain.CurrentDomain.BaseDirectory}params\MachineParams.json";
        private CameraParams _camera1 = new CameraParams();
        private CameraParams _camera2 = new CameraParams();
        private LightParams _light1 = new LightParams();
        private LightParams _light2 = new LightParams();
        private SocketParams _plc1 = new SocketParams();
        private SocketParams _plc2 = new SocketParams();
        private SocketParams _robot1 = new SocketParams();
        private SocketParams _robot2 = new SocketParams();
        private SocketParams _programTest = new SocketParams();
        private ScannerParams _scanner = new ScannerParams();
        private TerminalParams _terminal = new TerminalParams();
        private MachineCalibrationParams _machineCalibration = new MachineCalibrationParams();
        private AxisLimitParams _axislimit = new AxisLimitParams();
        private SCPParams _scp = new SCPParams();
        private int _loopCapture = 1;
        private int _delayCapture = 100;
        private int _numCheck = 50;
        private bool _webcamMonitor = false;
        private bool _debugMode = false;

        public CameraParams Camera1 => _camera1;
        public CameraParams Camera2 => _camera2;
        public LightParams Light1 => _light1;
        public LightParams Light2 => _light2;
        public SocketParams PLC1 => _plc1;
        public SocketParams PLC2 => _plc2;
        public SocketParams Robot1 => _robot1;
        public SocketParams Robot2 => _robot2;
        public SocketParams ProgramTest => _programTest;
        public ScannerParams Scanner => _scanner;
        public TerminalParams Terminal => _terminal;
        public MachineCalibrationParams MachineCalibration => _machineCalibration;
        public AxisLimitParams AxisLimit => _axislimit;
        public SCPParams SCP => _scp;
        public int LoopCapture
        {
            get => _loopCapture;
            set => _loopCapture = value;
        }
        public int DelayCapture
        {
            get => _delayCapture;
            set => _delayCapture = value;
        }
        public int NumCheck
        {
            get => _numCheck;
            set => _numCheck = value;
        }
        public bool WebcamMonitor
        {
            get => _webcamMonitor;
            set => _webcamMonitor = value;
        }
        public bool DebugMode
        {
            get => _debugMode;
            set => _debugMode = value;
        }

        public static MachineParams Current => __current;
        private static MachineParams __current = new MachineParams();
        private MachineParams() { }
        static MachineParams() { }

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

        public class ScannerParams
        {
            private bool _isEnabled = false;
            private string _portName = string.Empty;
            private int _dataLength = 0;

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

            public int DataLength
            {
                get => _dataLength;
                set => _dataLength = value;
            }
        }

        public class TerminalParams
        {
            private bool _isEnabled = false;
            private string _portName = string.Empty;
            private string _undo = "UNDO";
            private string _user = "125H";
            private string _format = string.Empty;

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

            public string Format
            {
                get => _format;
                set => _format = value;
            }
        }

        public class MachineCalibrationParams
        {
            private double _c1 = 0;
            private double _c2 = 0;
            private double _dx = 0;
            private double _dy = 0;
            private double _dz = 0;
            private double _dr = 0;

            public double C1
            {
                get => _c1;
                set => _c1 = value;
            }

            public double C2
            {
                get => _c2;
                set => _c2 = value;
            }

            public double X
            {
                get => _dx;
                set => _dx = value;
            }

            public double Y
            {
                get => _dy;
                set => _dy = value;
            }

            public double Z
            {
                get => _dz;
                set => _dz = value;
            }

            public double R
            {
                get => _dr;
                set => _dr = value;
            }
        }

        public class AxisLimitParams
        {
            private bool _isEnabled = false;
            private double _x = 0;
            private double _y = 0;
            private double _z = 0;
            private double _r = 0;

            public bool IsEnabled
            {
                get => _isEnabled;
                set => _isEnabled = value;
            }

            public double X
            {
                get => _x;
                set => _x = value;
            }

            public double Y
            {
                get => _y;
                set => _y = value;
            }

            public double Z
            {
                get => _z;
                set => _z = value;
            }

            public double R
            {
                get => _r;
                set => _r = value;
            }
        }

        public class SCPParams
        {
            private bool _isEnabled = false;
            private int _protocol = 0;
            private string _host = "10.220.145.202";
            private int _port = 4422;
            private string _user = "at";
            private string _password = "foxconnat";
            private string _key = "ssh-rsa 1024 8uZSgC2DMdG5JN5I0SJh0qb8/iGkqvnYT9Jz062zwEE=";
            private string _versionFile = @"/Software Update/version.txt";
            private string _updateFile = @"/Software Update/update.7z";
            private string _logsDirectory = @"/Logs/";

            public bool IsEnabled
            {
                get => _isEnabled;
                set => _isEnabled = value;
            }

            public int Protocol
            {
                get => _protocol;
                set => _protocol = value;
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

            public string User
            {
                get => _user;
                set => _user = value;
            }

            public string Password
            {
                get => _password;
                set => _password = value;
            }

            public string Key
            {
                get => _key;
                set => _key = value;
            }

            public string VersionFile
            {
                get => _versionFile;
                set => _versionFile = value;
            }

            public string UpdateFile
            {
                get => _updateFile;
                set => _updateFile = value;
            }

            public string LogsDirectory
            {
                get => _logsDirectory;
                set => _logsDirectory = value;
            }
        }
    }
}
