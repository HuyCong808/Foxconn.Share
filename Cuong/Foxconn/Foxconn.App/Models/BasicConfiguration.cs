using Foxconn.App.Helper.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace Foxconn.App.Models
{
    public class BasicConfiguration
    {
        #region Camera
        public class CameraConfiguration
        {
            public class DeviceInformation
            {
                public bool Enable { get; set; }
                public int Index { get; set; }
                public string Alias { get; set; }
                public CameraType CameraType { get; set; }
                public string UserDefinedName { get; set; }
                public string ModelName { get; set; }
                public string SerialNumber { get; set; }

                public DeviceInformation()
                {
                    Enable = false;
                    Index = 0;
                    Alias = null;
                    CameraType = CameraType.None;
                    UserDefinedName = null;
                    ModelName = null;
                    SerialNumber = null;
                }

                public DeviceInformation Clone()
                {
                    return new DeviceInformation()
                    {
                        Enable = Enable,
                        Index = Index,
                        Alias = Alias,
                        CameraType = CameraType,
                        UserDefinedName = UserDefinedName,
                        ModelName = ModelName,
                        SerialNumber = SerialNumber,
                    };
                }

                public DeviceInformation Clone0()
                {
                    return new DeviceInformation()
                    {
                        Enable = Enable,
                        Index = 0,
                        Alias = Alias,
                        CameraType = CameraType,
                        UserDefinedName = UserDefinedName,
                        ModelName = ModelName,
                        SerialNumber = SerialNumber,
                    };
                }

                public DeviceInformation Clone1()
                {
                    return new DeviceInformation()
                    {
                        Enable = Enable,
                        Index = 1,
                        Alias = Alias,
                        CameraType = CameraType,
                        UserDefinedName = UserDefinedName,
                        ModelName = ModelName,
                        SerialNumber = SerialNumber,
                    };
                }

                public DeviceInformation Clone2()
                {
                    return new DeviceInformation()
                    {
                        Enable = Enable,
                        Index = 2,
                        Alias = Alias,
                        CameraType = CameraType,
                        UserDefinedName = UserDefinedName,
                        ModelName = ModelName,
                        SerialNumber = SerialNumber,
                    };
                }
            }

            public class CameraSignal
            {
                public bool Enable { get; set; }
                public int Index { get; set; }
                public string Alias { get; set; }
                public string CanCheck { get; set; }
                public string Passed { get; set; }
                public string Failed { get; set; }

                public CameraSignal()
                {
                    Enable = false;
                    Index = 0;
                    Alias = null;
                    CanCheck = null;
                    Passed = null;
                    Failed = null;
                }

                public CameraSignal Clone()
                {
                    return new CameraSignal()
                    {
                        Enable = Enable,
                        Index = Index,
                        Alias = Alias,
                        CanCheck = CanCheck,
                        Passed = Passed,
                        Failed = Failed,
                    };
                }

                public CameraSignal Clone0()
                {
                    return new CameraSignal()
                    {
                        Enable = Enable,
                        Index = 0,
                        Alias = Alias,
                        CanCheck = CanCheck,
                        Passed = Passed,
                        Failed = Failed,
                    };
                }

                public CameraSignal Clone1()
                {
                    return new CameraSignal()
                    {
                        Enable = Enable,
                        Index = 1,
                        Alias = Alias,
                        CanCheck = CanCheck,
                        Passed = Passed,
                        Failed = Failed,
                    };
                }

                public CameraSignal Clone2()
                {
                    return new CameraSignal()
                    {
                        Enable = Enable,
                        Index = 2,
                        Alias = Alias,
                        CanCheck = CanCheck,
                        Passed = Passed,
                        Failed = Failed,
                    };
                }

                public CameraSignal Clone3()
                {
                    return new CameraSignal()
                    {
                        Enable = Enable,
                        Index = 3,
                        Alias = Alias,
                        CanCheck = CanCheck,
                        Passed = Passed,
                        Failed = Failed,
                    };
                }

                public CameraSignal Clone4()
                {
                    return new CameraSignal()
                    {
                        Enable = Enable,
                        Index = 4,
                        Alias = Alias,
                        CanCheck = CanCheck,
                        Passed = Passed,
                        Failed = Failed,
                    };
                }
            }

            public List<DeviceInformation> Devices { get; set; }
            public List<CameraSignal> Signals { get; set; }

            public CameraConfiguration()
            {
                Devices = new List<DeviceInformation>();
                Signals = new List<CameraSignal>();
            }

            public CameraConfiguration Clone()
            {
                return new CameraConfiguration()
                {
                    Devices = Devices != null ? new List<DeviceInformation>() {
                        new DeviceInformation().Clone0(),
                        new DeviceInformation().Clone1(),
                        new DeviceInformation().Clone2()
                    } : null,
                    Signals = Signals != null ? new List<CameraSignal>() {
                        new CameraSignal().Clone0(),
                        new CameraSignal().Clone1(),
                        new CameraSignal().Clone2(),
                        new CameraSignal().Clone3(),
                        new CameraSignal().Clone4()
                    } : null,
                };
            }
        }
        #endregion

        #region PLC
        public class PlcConfiguration
        {
            public class DeviceInformation
            {
                public bool Enable { get; set; }
                public int Index { get; set; }
                public string Alias { get; set; }
                public string Host { get; set; }
                public int Port { get; set; }

                public DeviceInformation()
                {
                    Enable = false;
                    Index = 0;
                    Alias = null;
                    Host = null;
                    Port = 8888;
                }

                public DeviceInformation Clone()
                {
                    return new DeviceInformation()
                    {
                        Enable = Enable,
                        Index = Index,
                        Alias = Alias,
                        Host = Host,
                        Port = Port,
                    };
                }

                public DeviceInformation Clone0()
                {
                    return new DeviceInformation()
                    {
                        Enable = Enable,
                        Index = 0,
                        Alias = Alias,
                        Host = Host,
                        Port = Port,
                    };
                }

                public DeviceInformation Clone1()
                {
                    return new DeviceInformation()
                    {
                        Enable = Enable,
                        Index = 1,
                        Alias = Alias,
                        Host = Host,
                        Port = Port,
                    };
                }
            }

            public class PlcStatus
            {
                public string Start { get; set; }
                public string Pause { get; set; }
                public string Reset { get; set; }
                public string ResetOK { get; set; }
                public string Stop { get; set; }
                public string Settings { get; set; }
                public string HasStarted { get; set; }
                public string HasPaused { get; set; }
                public string HasReset { get; set; }
                public string HasResetOK { get; set; }
                public string HasStopped { get; set; }
                public string HasSet { get; set; }

                public PlcStatus()
                {
                    Start = "M1";
                    Pause = "M1";
                    Reset = "M1";
                    ResetOK = "M1";
                    Stop = "M1";
                    Settings = "M1";
                    HasStarted = "M1";
                    HasPaused = "M1";
                    HasReset = "M1";
                    HasResetOK = "M1";
                    HasStopped = "M1";
                    HasSet = "M1";
                }

                public PlcStatus Clone()
                {
                    return new PlcStatus()
                    {
                        Start = Start,
                        Pause = Pause,
                        Reset = Reset,
                        ResetOK = ResetOK,
                        Stop = Stop,
                        Settings = Settings,
                        HasStarted = HasStarted,
                        HasPaused = HasPaused,
                        HasReset = HasReset,
                        HasResetOK = HasResetOK,
                        HasStopped = HasStopped,
                        HasSet = HasSet,
                    };
                }
            }

            public List<DeviceInformation> Devices { get; set; }
            public PlcStatus Status { get; set; }
            public string Home { get; set; }
            public string PlcState { get; set; }
            public string SoftwareState { get; set; }
            public string AllowJogging { get; set; }
            public string IncreaseAxisX { get; set; }
            public string DecreaseAxisX { get; set; }
            public string IncreaseAxisY { get; set; }
            public string DecreaseAxisY { get; set; }
            public string IncreaseAxisZ { get; set; }
            public string DecreaseAxisZ { get; set; }
            public string ClockwiseAxisR { get; set; }
            public string CounterClockwiseAxisR { get; set; }
            // Speed
            public string SpeedAxisX { get; set; }
            public string SpeedAxisY { get; set; }
            public string SpeedAxisZ { get; set; }
            public string SpeedAxisR { get; set; }
            public string SpeedJogAxisX { get; set; }
            public string SpeedJogAxisY { get; set; }
            public string SpeedJogAxisZ { get; set; }
            public string SpeedJogAxisR { get; set; }
            public string SpeedBasic { get; set; }
            public string SpeedAngle { get; set; }
            // Sensor
            public string CoorX0 { get; set; }
            public string CoorX1 { get; set; }
            public string CoorY0 { get; set; }
            public string CoorY1 { get; set; }
            public string CoorZ0 { get; set; }
            public string CoorZ1 { get; set; }
            public string CoorR0 { get; set; }
            public string CoorR1 { get; set; }
            // Arm and Vaccuum Pads
            public string SuckAirCylinder0 { get; set; }
            public string ReleaseAirCylinder0 { get; set; }
            public string SuckAirCylinder1 { get; set; }
            public string ReleaseAirCylinder1 { get; set; }
            public string SuckAirVacuumPads0 { get; set; }
            public string ReleaseAirVacuumPads0 { get; set; }
            public string SuckAirVacuumPads1 { get; set; }
            public string ReleaseAirVacuumPads1 { get; set; }
            public string SuckAirVacuumPads2 { get; set; }
            public string ReleaseAirVacuumPads2 { get; set; }
            public string SuckAirVacuumPads3 { get; set; }
            public string ReleaseAirVacuumPads3 { get; set; }
            // Light, Door, Sensor Safety
            public string Light { get; set; }
            public string Door { get; set; }
            public string Safety { get; set; }

            public PlcConfiguration()
            {
                Devices = new List<DeviceInformation>();
                Status = new PlcStatus();
                Home = "M1";
                PlcState = "M1";
                SoftwareState = "M1";
                AllowJogging = "M1";
                IncreaseAxisX = "M1";
                DecreaseAxisX = "M1";
                IncreaseAxisY = "M1";
                DecreaseAxisY = "M1";
                IncreaseAxisZ = "M1";
                DecreaseAxisZ = "M1";
                ClockwiseAxisR = "M1";
                CounterClockwiseAxisR = "M1";
                SpeedAxisX = "D1";
                SpeedAxisY = "D1";
                SpeedAxisZ = "D1";
                SpeedAxisR = "D1";
                SpeedJogAxisX = "D1";
                SpeedJogAxisY = "D1";
                SpeedJogAxisZ = "D1";
                SpeedJogAxisR = "D1";
                SpeedBasic = "D1";
                SpeedAngle = "D1";
                CoorX0 = "M1";
                CoorX1 = "M1";
                CoorY0 = "M1";
                CoorY1 = "M1";
                CoorZ0 = "M1";
                CoorZ1 = "M1";
                CoorR0 = "M1";
                CoorR1 = "M1";
                SuckAirCylinder0 = "M1";
                ReleaseAirCylinder0 = "M1";
                SuckAirCylinder1 = "M1";
                ReleaseAirCylinder1 = "M1";
                SuckAirVacuumPads0 = "M1";
                ReleaseAirVacuumPads0 = "M1";
                SuckAirVacuumPads1 = "M1";
                ReleaseAirVacuumPads1 = "M1";
                SuckAirVacuumPads2 = "M1";
                ReleaseAirVacuumPads2 = "M1";
                SuckAirVacuumPads3 = "M1";
                ReleaseAirVacuumPads3 = "M1";
                Light = "M1";
                Door = "M1";
                Safety = "M1";
            }

            public PlcConfiguration Clone()
            {
                return new PlcConfiguration()
                {
                    Devices = Devices != null ? new List<DeviceInformation>() {
                        new DeviceInformation().Clone0(),
                        new DeviceInformation().Clone1()
                    } : null,
                    Status = Status?.Clone(),
                    Home = Home,
                    PlcState = PlcState,
                    SoftwareState = SoftwareState,
                    AllowJogging = AllowJogging,
                    IncreaseAxisX = IncreaseAxisX,
                    DecreaseAxisX = DecreaseAxisX,
                    IncreaseAxisY = IncreaseAxisY,
                    DecreaseAxisY = DecreaseAxisY,
                    IncreaseAxisZ = IncreaseAxisZ,
                    DecreaseAxisZ = DecreaseAxisZ,
                    ClockwiseAxisR = ClockwiseAxisR,
                    CounterClockwiseAxisR = CounterClockwiseAxisR,
                    SpeedAxisX = SpeedAxisX,
                    SpeedAxisY = SpeedAxisY,
                    SpeedAxisZ = SpeedAxisZ,
                    SpeedAxisR = SpeedAxisR,
                    SpeedJogAxisX = SpeedJogAxisX,
                    SpeedJogAxisY = SpeedJogAxisY,
                    SpeedJogAxisZ = SpeedJogAxisZ,
                    SpeedJogAxisR = SpeedJogAxisR,
                    SpeedBasic = SpeedBasic,
                    SpeedAngle = SpeedAngle,
                    CoorX0 = CoorX0,
                    CoorX1 = CoorX1,
                    CoorY0 = CoorY0,
                    CoorY1 = CoorY1,
                    CoorZ0 = CoorZ0,
                    CoorZ1 = CoorZ1,
                    CoorR0 = CoorR0,
                    CoorR1 = CoorR1,
                    SuckAirCylinder0 = SuckAirCylinder0,
                    ReleaseAirCylinder0 = ReleaseAirCylinder0,
                    SuckAirCylinder1 = SuckAirCylinder1,
                    ReleaseAirCylinder1 = ReleaseAirCylinder1,
                    SuckAirVacuumPads0 = SuckAirVacuumPads0,
                    ReleaseAirVacuumPads0 = ReleaseAirVacuumPads0,
                    SuckAirVacuumPads1 = SuckAirVacuumPads1,
                    ReleaseAirVacuumPads1 = ReleaseAirVacuumPads1,
                    SuckAirVacuumPads2 = SuckAirVacuumPads2,
                    ReleaseAirVacuumPads2 = ReleaseAirVacuumPads2,
                    SuckAirVacuumPads3 = SuckAirVacuumPads3,
                    ReleaseAirVacuumPads3 = ReleaseAirVacuumPads3,
                    Light = Light,
                    Door = Door,
                    Safety = Safety,
                };
            }
        }
        #endregion

        #region Robot
        public class RobotConfiguration
        {

            public class DeviceInformation
            {
                public bool Enable { get; set; }
                public int Index { get; set; }
                public string Alias { get; set; }
                public string Host { get; set; }
                public int Port { get; set; }

                public DeviceInformation()
                {
                    Enable = false;
                    Index = 0;
                    Alias = null;
                    Host = null;
                    Port = 9999;
                }

                public DeviceInformation Clone()
                {
                    return new DeviceInformation()
                    {
                        Enable = Enable,
                        Index = Index,
                        Alias = Alias,
                        Host = Host,
                        Port = Port,
                    };
                }

                public DeviceInformation Clone0()
                {
                    return new DeviceInformation()
                    {
                        Enable = Enable,
                        Index = 0,
                        Alias = Alias,
                        Host = Host,
                        Port = Port,
                    };
                }

                public DeviceInformation Clone1()
                {
                    return new DeviceInformation()
                    {
                        Enable = Enable,
                        Index = 1,
                        Alias = Alias,
                        Host = Host,
                        Port = Port,
                    };
                }
            }

            public class RobotStatus
            {
                public string Start { get; set; }
                public string Pause { get; set; }
                public string Reset { get; set; }
                public string ResetOK { get; set; }
                public string Stop { get; set; }
                public string Settings { get; set; }
                public string HasStarted { get; set; }
                public string HasPaused { get; set; }
                public string HasReset { get; set; }
                public string HasResetOK { get; set; }
                public string HasStopped { get; set; }
                public string HasSet { get; set; }

                public RobotStatus()
                {
                    Start = "START";
                    Pause = "PAUSE";
                    Reset = "RESET";
                    ResetOK = "RESETOK";
                    Stop = "STOP";
                    Settings = "SETTINGS";
                    HasStarted = "HAS_STARTED";
                    HasPaused = "HAS_PAUSED";
                    HasReset = "HAS_RESET";
                    HasResetOK = "HAS_RESETOK";
                    HasStopped = "HAS_STOPPED";
                    HasSet = "HAS_SET";
                }

                public RobotStatus Clone()
                {
                    return new RobotStatus()
                    {
                        Start = Start,
                        Pause = Pause,
                        Reset = Reset,
                        Stop = Stop,
                        Settings = Settings,
                        ResetOK = ResetOK,
                        HasStarted = HasStarted,
                        HasPaused = HasPaused,
                        HasReset = HasReset,
                        HasResetOK = HasResetOK,
                        HasStopped = HasStopped,
                        HasSet = HasSet,
                    };
                }
            }

            public List<DeviceInformation> Devices { get; set; }
            public RobotStatus Status { get; set; }
            public string LightOn { get; set; }
            public string LightOff { get; set; }
            public string DoorOpened { get; set; }
            public string DoorClosed { get; set; }

            public RobotConfiguration()
            {
                Devices = new List<DeviceInformation>();
                Status = new RobotStatus();
                LightOn = "LIGHT_ON";
                LightOff = "LIGHT_OFF";
                DoorOpened = "DOOR_OPENED";
                DoorClosed = "DOOR_CLOSED";
            }

            public RobotConfiguration Clone()
            {
                return new RobotConfiguration()
                {
                    Devices = Devices != null ? new List<DeviceInformation>() {
                        new DeviceInformation().Clone0(),
                        new DeviceInformation().Clone1()
                    } : null,
                    Status = Status?.Clone(),
                    LightOn = LightOn,
                    LightOff = LightOff,
                    DoorOpened = DoorOpened,
                    DoorClosed = DoorClosed,
                };
            }
        }
        #endregion

        #region Serial Port
        public class SerialPortConfiguration
        {
            public bool Enable { get; set; }
            public int Index { get; set; }
            public string Alias { get; set; }
            public string PortName { get; set; }
            public int BaudRate { get; set; }
            public Parity Parity { get; set; }
            public int DataBits { get; set; }
            public StopBits StopBits { get; set; }
            public Handshake Handshake { get; set; }
            public int ReadTimeout { get; set; }
            public int WriteTimeout { get; set; }

            public SerialPortConfiguration()
            {
                Enable = false;
                Index = 0;
                Alias = null;
                PortName = null;
                BaudRate = 9600;
                Parity = Parity.None;
                DataBits = 8;
                StopBits = StopBits.One;
                Handshake = Handshake.None;
                ReadTimeout = 500;
                WriteTimeout = 500;
            }

            public SerialPortConfiguration Clone()
            {
                return new SerialPortConfiguration()
                {
                    Enable = Enable,
                    Index = Index,
                    Alias = Alias,
                    PortName = PortName,
                    BaudRate = BaudRate,
                    Parity = Parity,
                    DataBits = DataBits,
                    StopBits = StopBits,
                    Handshake = Handshake,
                    ReadTimeout = ReadTimeout,
                    WriteTimeout = WriteTimeout,
                };
            }
        }
        #endregion

        #region Service
        public class ServiceConfiguration
        {
            public bool Enable { get; set; }
            public int Index { get; set; }
            public string Alias { get; set; }
            public string UserId { get; set; }
            public string Password { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public string ServiceName { get; set; }


            public ServiceConfiguration()
            {
                Enable = false;
                Index = 0;
                Alias = null;
                UserId = null;
                Password = null;
                Host = null;
                Port = 5000;
                ServiceName = null;
            }

            public ServiceConfiguration Clone()
            {
                return new ServiceConfiguration()
                {
                    Enable = Enable,
                    Index = Index,
                    Alias = Alias,
                    UserId = UserId,
                    Password = Password,
                    Host = Host,
                    Port = Port,
                    ServiceName = ServiceName,
                };
            }
        }
        #endregion

        #region Delay
        public class DelayConfiguration
        {
            public int Basic { get; set; }
            public int Advanced { get; set; }
            public int Trigger0 { get; set; }
            public int Trigger1 { get; set; }
            public int Trigger2 { get; set; }

            public DelayConfiguration()
            {
                Basic = 100;
                Advanced = 500;
                Trigger0 = 500;
                Trigger1 = 500;
                Trigger2 = 500;
            }

            public DelayConfiguration Clone()
            {
                return new DelayConfiguration()
                {
                    Basic = Basic,
                    Advanced = Advanced,
                    Trigger0 = Trigger0,
                    Trigger1 = Trigger1,
                    Trigger2 = Trigger2,
                };
            }
        }
        #endregion

        #region Timer
        public class TimerConfiguration
        {
            public int Basic { get; set; }
            public int Advanced { get; set; }
            public int Open0 { get; set; }
            public int Open1 { get; set; }
            public int Open2 { get; set; }
            public int Processing0 { get; set; }
            public int Processing1 { get; set; }
            public int Processing2 { get; set; }
            public int Resetting { get; set; }

            public TimerConfiguration()
            {
                Basic = 5000;
                Advanced = 60000;
                Open0 = 5000;
                Open1 = 5000;
                Open2 = 5000;
                Processing0 = 5000;
                Processing1 = 5000;
                Processing2 = 5000;
                Resetting = 60000;
            }

            public TimerConfiguration Clone()
            {
                return new TimerConfiguration()
                {
                    Basic = Basic,
                    Advanced = Advanced,
                    Open0 = Open0,
                    Open1 = Open1,
                    Open2 = Open2,
                    Processing0 = Processing0,
                    Processing1 = Processing1,
                    Processing2 = Processing2,
                    Resetting = Resetting,
                };
            }
        }
        #endregion

        #region VNC
        public class VncConfiguration
        {
            public bool Enable { get; set; }
            public int Index { get; set; }
            public string Alias { get; set; }
            public string Host { get; set; }
            public string Password { get; set; }

            public VncConfiguration()
            {
                Enable = false;
                Index = 0;
                Alias = null;
                Host = null;
                Password = null;
            }

            public VncConfiguration Clone()
            {
                return new VncConfiguration()
                {
                    Enable = Enable,
                    Index = Index,
                    Alias = Alias,
                    Host = Host,
                    Password = Password,
                };
            }
        }
        #endregion

        public BsonObjectId _id { get; set; }
        public string ModelName { get; set; }
        public CameraConfiguration Camera { get; set; }
        public PlcConfiguration Plc { get; set; }
        public RobotConfiguration Robot { get; set; }
        public List<SerialPortConfiguration> SerialPorts { get; set; }
        public List<ServiceConfiguration> Services { get; set; }
        public DelayConfiguration Delay { get; set; }
        public TimerConfiguration Timer { get; set; }
        public List<VncConfiguration> Vncs { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime DateCreated { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime DateModified { get; set; }

        public BasicConfiguration()
        {
            ModelName = string.Empty;
            Camera = new CameraConfiguration();
            Plc = new PlcConfiguration();
            Robot = new RobotConfiguration();
            SerialPorts = new List<SerialPortConfiguration>();
            Services = new List<ServiceConfiguration>();
            Delay = new DelayConfiguration();
            Timer = new TimerConfiguration();
            Vncs = new List<VncConfiguration>();
            DateCreated = DateTime.Now.Date;
            DateModified = DateTime.Now;
        }

        public BasicConfiguration Clone()
        {
            return new BasicConfiguration()
            {
                ModelName = ModelName,
                Camera = Camera?.Clone(),
                Plc = Plc?.Clone(),
                Robot = Robot?.Clone(),
                SerialPorts = SerialPorts != null ? new List<SerialPortConfiguration>() { new SerialPortConfiguration().Clone(), } : null,
                Services = Services != null ? new List<ServiceConfiguration>() { new ServiceConfiguration().Clone(), } : null,
                Delay = Delay?.Clone(),
                Timer = Timer?.Clone(),
                Vncs = Vncs != null ? new List<VncConfiguration>() { new VncConfiguration().Clone(), } : null,
                DateCreated = DateCreated,
                DateModified = DateModified,
            };
        }
    }
}
