using Foxconn.Editor.Camera;
using Foxconn.Editor.Controls;
using Foxconn.Editor.Dialogs;
using Foxconn.Editor.Enums;
using Foxconn.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using static Foxconn.Editor.TestParams;

namespace Foxconn.Editor
{
    public class DeviceManager
    {
        private ICamera _camera1 = null;
        private ICamera _camera2 = null;
        private SerialLight _light1 = null;
        private SerialLight _light2 = null;
        private SLMP _plc1 = null;
        private SLMP _plc2 = null;
        private RobotTest _robot1 = null;
        private RobotTest _robot2 = null;
        private ProgramTest _programTest = null;
        private SerialClient _scanner = null;
        private SerialClient _terminal = null;
        private SCPClient _scp = null;
        private List<SocketTest> _socketTests = null;
        private List<VNCClient> _VNCs = null;

        public ICamera Camera1
        {
            get => _camera1;
            set => _camera1 = value;
        }

        public ICamera Camera2
        {
            get => _camera2;
            set => _camera2 = value;
        }

        public SerialLight Light1
        {
            get => _light1;
            set => _light1 = value;
        }

        public SerialLight Light2
        {
            get => _light2;
            set => _light2 = value;
        }

        public SLMP PLC1
        {
            get => _plc1;
            set => _plc1 = value;
        }

        public SLMP PLC2
        {
            get => _plc2;
            set => _plc2 = value;
        }

        public RobotTest Robot1
        {
            get => _robot1;
            set => _robot1 = value;
        }

        public RobotTest Robot2
        {
            get => _robot2;
            set => _robot2 = value;
        }

        public ProgramTest ProgramTest
        {
            get => _programTest;
            set => _programTest = value;
        }

        public SerialClient Scanner
        {
            get => _scanner;
            set => _scanner = value;
        }

        public SerialClient Terminal
        {
            get => _terminal;
            set => _terminal = value;
        }

        public SCPClient SCP
        {
            get => _scp;
            set => _scp = value;
        }

        public List<SocketTest> Sockets
        {
            get => _socketTests;
            set => _socketTests = value;
        }

        public List<VNCClient> VNCs
        {
            get => _VNCs;
            set => _VNCs = value;
        }

        public static DeviceManager Current => __current;
        private static DeviceManager __current = new DeviceManager();
        static DeviceManager() { }
        private DeviceManager() { }

        public void OpenDevices()
        {
            try
            {
                MachineParams param = MachineParams.Current;
                DeviceManager device = Current;

                if (param.Camera1.IsEnabled)
                {
                    device.Camera1 = CameraFactory.GetCamera(param.Camera1.Type);
                    device.Camera1.DeviceListAcq();
                    int nRet = device.Camera1.Open(param.Camera1.UserDefinedName);
                    if (nRet == 1)
                    {
                        device.Camera1.SetParameter(KeyName.TriggerMode, 1);
                        device.Camera1.SetParameter(KeyName.TriggerSource, 7);
                        device.Camera1.StartGrabbing();
                        Trace.WriteLine("DeviceManager.Open: Camera1 opened");
                    }
                    else
                    {
                        Trace.WriteLine("DeviceManager.Open: Camera1 cannot open");
                    }
                }

                if (param.Camera2.IsEnabled)
                {
                    device.Camera2 = CameraFactory.GetCamera(param.Camera2.Type);
                    device.Camera2.DeviceListAcq();
                    int nRet = _camera2.Open(param.Camera2.UserDefinedName);
                    if (nRet == 1)
                    {
                        device.Camera2.SetParameter(KeyName.TriggerMode, 1);
                        device.Camera2.SetParameter(KeyName.TriggerSource, 7);
                        device.Camera2.StartGrabbing();
                        Trace.WriteLine("DeviceManager.Open: Camera2 opened");
                    }
                    else
                    {
                        Trace.WriteLine("DeviceManager.Open: Camera2 cannot open");
                    }
                }

                if (param.Light1.IsEnabled)
                {
                    int nRet = device.Light1.Open(param.Light1.PortName);
                    if (nRet == 1)
                    {
                        Trace.WriteLine("DeviceManager.Open: Light1 opened");
                    }
                    else
                    {
                        Trace.WriteLine("DeviceManager.Open: Light1 cannot open");
                    }
                }

                if (param.Light2.IsEnabled)
                {
                    int nRet = device.Light2.Open(param.Light2.PortName);
                    if (nRet == 1)
                    {
                        Trace.WriteLine("DeviceManager.Open: Light2 opened");
                    }
                    else
                    {
                        Trace.WriteLine("DeviceManager.Open: Light2 cannot open");
                    }
                }

                if (param.PLC1.IsEnabled)
                {
                    device.PLC1 = new SLMP();
                    int nRet = device.PLC1.Ping(param.PLC1.Host, param.PLC1.Port);
                    if (nRet == 1)
                    {
                        device.PLC1.Host = param.PLC1.Host;
                        device.PLC1.Port = param.PLC1.Port;
                        Trace.WriteLine("DeviceManager.Open: PLC1 opened");
                    }
                    else
                    {
                        Trace.WriteLine("DeviceManager.Open: PLC1 cannot open");
                    }
                }

                if (param.PLC2.IsEnabled)
                {
                    device.PLC2 = new SLMP();
                    int nRet = device.PLC2.Ping(param.PLC2.Host, param.PLC2.Port);
                    if (nRet == 1)
                    {
                        device.PLC2.Host = param.PLC2.Host;
                        device.PLC2.Port = param.PLC2.Port;
                        Trace.WriteLine("DeviceManager.Open: PLC2 opened");
                    }
                    else
                    {
                        Trace.WriteLine("DeviceManager.Open: PLC2 cannot open");
                    }
                }

                if (param.Robot1.IsEnabled)
                {
                    device.Robot1 = new RobotTest();
                    int nRet = device.Robot1.Open(param.Robot1.Host, param.Robot1.Port);
                    if (nRet == 1)
                    {
                        Trace.WriteLine("DeviceManager.Open: Robot1 opened");
                    }
                    else
                    {
                        Trace.WriteLine("DeviceManager.Open: Robot1 cannot open");
                    }
                }

                if (param.Robot2.IsEnabled)
                {
                    device.Robot2 = new RobotTest();
                    int nRet = device.Robot2.Open(param.Robot2.Host, param.Robot2.Port);
                    if (nRet == 1)
                    {
                        Trace.WriteLine("DeviceManager.Open: Robot2 opened");
                    }
                    else
                    {
                        Trace.WriteLine("DeviceManager.Open: Robot2 cannot open");
                    }
                }

                if (param.ProgramTest.IsEnabled)
                {
                    device.ProgramTest = new ProgramTest();
                    int nRet = device.ProgramTest.Open(param.ProgramTest.Host, param.ProgramTest.Port);
                    if (nRet == 1)
                    {
                        Trace.WriteLine("DeviceManager.Open: ProgramTest opened");
                    }
                    else
                    {
                        Trace.WriteLine("DeviceManager.Open: ProgramTest cannot open");
                    }
                }

                if (param.Scanner.IsEnabled)
                {
                    //device.Scanner = new SerialClient();
                    //int nRet = device.Scanner.Open(param.Scanner.PortName);
                    //if (nRet == 1)
                    //{
                    //    Trace.WriteLine("DeviceManager.Open: Scanner opened");
                    //}
                    //else
                    //{
                    //    Trace.WriteLine("DeviceManager.Open: Scanner cannot open");
                    //}
                }

                if (param.Terminal.IsEnabled)
                {
                    device.Terminal = new SerialClient();
                    int nRet = device.Terminal.Open(param.Terminal.PortName);
                    if (nRet == 1)
                    {
                        Trace.WriteLine("DeviceManager.Open: Terminal opened");
                        if (param.Terminal.Undo != "")
                        {
                            nRet = device.Terminal.SerialWriteData(param.Terminal.Undo);
                        }
                        if (param.Terminal.User != "")
                        {
                            nRet = device.Terminal.SerialWriteData(param.Terminal.User, param.Terminal.User + "PASS", timeout: 10000);
                        }
                    }
                    else
                    {
                        Trace.WriteLine("DeviceManager.Open: Terminal cannot open");
                    }
                }

                if (param.SCP.IsEnabled)
                {
                    device.SCP = new SCPClient
                    {
                        Protocol = (WinSCP.Protocol)param.SCP.Protocol,
                        Host = param.SCP.Host,
                        Port = param.SCP.Port,
                        User = param.SCP.User,
                        Password = param.SCP.Password,
                        Key = param.SCP.Key
                    };
                    int nRet = device.SCP.Ping();
                    if (nRet == 1)
                    {
                        Trace.WriteLine("DeviceManager.Open: SCP opened");
                    }
                    else
                    {
                        Trace.WriteLine("DeviceManager.Open: SCP cannot open");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void CloseDevices()
        {
            try
            {
                _camera1?.Close();
                _camera2?.Close();
                _light1?.Close();
                _light2?.Close();
                _robot1?.Close();
                _robot2?.Close();
                _programTest?.Close();
                CloseSockets();
                CloseVNCs();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public int PingDevices()
        {
            int fRet = 1;
            try
            {
                MachineParams param = MachineParams.Current;
                DeviceManager device = Current;

                if (param.Camera1.IsEnabled)
                {
                    if (!device.Camera1.IsConnected)
                    {
                        fRet = -1;
                        MessageBox.Show($"Camera1 disconnected ({param.Camera1.UserDefinedName}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

                if (param.Camera2.IsEnabled)
                {
                    if (!device.Camera2.IsConnected)
                    {
                        fRet = -1;
                        MessageBox.Show($"Camera2 disconnected ({param.Camera2.UserDefinedName}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

                if (param.Light1.IsEnabled)
                {
                    if (!device.Light1.IsConnected)
                    {
                        fRet = -1;
                        MessageBox.Show($"Light1 disconnected ({param.Light1.PortName}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

                if (param.Light2.IsEnabled)
                {
                    if (!device.Light2.IsConnected)
                    {
                        fRet = -1;
                        MessageBox.Show($"Light2 disconnected ({param.Light2.PortName}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

                if (param.PLC1.IsEnabled)
                {
                    int nRet = device.PLC1.Ping(param.PLC1.Host, param.PLC1.Port);
                    if (nRet != 1)
                    {
                        fRet = -1;
                        MessageBox.Show($"PLC1 disconnected ({param.PLC1.Host}, {param.PLC1.Port}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

                if (param.PLC2.IsEnabled)
                {
                    int nRet = device.PLC2.Ping(param.PLC2.Host, param.PLC2.Port);
                    if (nRet != 1)
                    {
                        fRet = -1;
                        MessageBox.Show($"PLC2 disconnected ({param.PLC2.Host}, {param.PLC2.Port}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

                if (param.Robot1.IsEnabled)
                {
                    //int nRet = device.Robot.Ping(param.Robot.Host, param.Robot.Port);
                    int nRet = device.Robot1.IsConnected ? 1 : -1;
                    if (nRet != 1)
                    {
                        fRet = -1;
                        MessageBox.Show($"Robot1 disconnected ({param.Robot1.Host}, {param.Robot1.Port}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

                if (param.Robot2.IsEnabled)
                {
                    //int nRet = device.Robot.Ping(param.Robot.Host, param.Robot.Port);
                    int nRet = device.Robot2.IsConnected ? 1 : -1;
                    if (nRet != 1)
                    {
                        fRet = -1;
                        MessageBox.Show($"Robot2 disconnected ({param.Robot2.Host}, {param.Robot2.Port}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

                if (param.ProgramTest.IsEnabled)
                {
                    //int nRet = device.ProgramTest.Ping(param.Robot.Host, param.Robot.Port);
                    int nRet = device.ProgramTest.IsConnected ? 1 : -1;
                    if (nRet != 1)
                    {
                        fRet = -1;
                        MessageBox.Show($"ProgramTest disconnected ({param.ProgramTest.Host}, {param.ProgramTest.Port}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

                if (param.Scanner.IsEnabled)
                {
                    if (!device.Scanner.IsConnected)
                    {
                        //fRet = -1;
                        //MessageBox.Show($"Scanner disconnected ({param.Scanner.PortName}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

                if (param.Terminal.IsEnabled)
                {
                    if (!device.Terminal.IsConnected)
                    {
                        fRet = -1;
                        MessageBox.Show($"Terminal disconnected ({param.Terminal.PortName}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

                if (param.SCP.IsEnabled)
                {
                    int nRet = device.SCP.Ping();
                    if (nRet != 1)
                    {
                        fRet = -1;
                        MessageBox.Show($"SCP disconnected ({param.SCP.Host}:{param.SCP.Port}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            }
            catch (Exception ex)
            {
                fRet = -1;
                Trace.WriteLine(ex);
            }
            return fRet;
        }

        public int OpenSockets(List<FixtureParams> sockets, List<SocketTestUI> socketUIs)
        {
            try
            {
                int nRet = 1;
                if (sockets.Count > 0)
                {
                    _socketTests = new List<SocketTest>();
                    foreach (FixtureParams f in sockets)
                    {
                        Trace.WriteLine($"SocketServer.Listen ({f.Socket.Host}, {f.Socket.Port}): IsEnabled = {f.IsEnabled}");
                        SocketTestUI testUI = socketUIs.Find(x => x.Id == f.Id);
                        if (f.Id < socketUIs.Count)
                        {
                            testUI = socketUIs[f.Id];
                        }
                        if (f.IsEnabled)
                        {
                            SocketTest socketTest = new SocketTest
                            {
                                Id = f.Id,
                                Model = TestModel.Unknow,
                                SocketUI = testUI
                            };
                            if (socketTest.Open(f.Socket.Host, f.Socket.Port) == 1)
                            {
                                _socketTests.Add(socketTest);
                                if (testUI != null)
                                {
                                    testUI.Box.Visibility = Visibility.Visible;
                                }
                            }
                            else
                            {
                                nRet = -1;
                                if (testUI != null)
                                {
                                    testUI.Box.Visibility = Visibility.Hidden;
                                }
                            }
                        }
                        else
                        {
                            if (testUI != null)
                            {
                                testUI.Box.Visibility = Visibility.Hidden;
                            }
                        }
                    }
                }
                return nRet;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }

        public void CloseSockets()
        {
            try
            {
                if (_socketTests != null)
                {
                    foreach (SocketTest socket in _socketTests)
                    {
                        socket.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void SocketForceClose(int id)
        {
            SocketTest socket = _socketTests.Find(x => x.Id == id);
            if (socket != null)
            {
                string message = $"Are you sure want to close client {id} ({"localhost"},{socket.Port})?";
                if (MessageBox.Show(message, "Force Close Connection", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                {
                    socket.SocketForceClose();
                }
            }
        }

        public void OpenFixture(int id)
        {
            SocketTest socket = _socketTests.Find(x => x.Id == id);
            if (socket != null)
            {
                string message = $"Are you sure want to open fixture {id} ({"localhost"},{socket.Port})?";
                if (MessageBox.Show(message, "Open Fixture", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                {
                    socket.ClearDataReceived();
                    socket.SocketWriteData("OPEN_FIXTURE");
                    WaitingDialog.DoWork(AutoRunManagerDialog.Current, "Opening the fixture...", Task.Create(queryCanncel =>
                    {
                        while (!queryCanncel())
                        {
                            if (socket.DataReceived == "OPEN_FIXTUREOK")
                            {
                                break;
                            }
                            Thread.Sleep(25);
                        }
                    }), true);
                }
            }
        }

        public void CloseFixture(int id)
        {
            SocketTest socket = _socketTests.Find(x => x.Id == id);
            if (socket != null)
            {
                string message = $"Are you sure want to close fixture {id} ({"localhost"},{socket.Port})?";
                if (MessageBox.Show(message, "Close Fixture", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                {
                    socket.ClearDataReceived();
                    socket.SocketWriteData("CLOSE_FIXTURE");
                    WaitingDialog.DoWork(AutoRunManagerDialog.Current, "Closing the fixture...", Task.Create(queryCanncel =>
                    {
                        while (!queryCanncel())
                        {
                            if (socket.DataReceived == "CLOSE_FIXTUREOK")
                            {
                                break;
                            }
                            Thread.Sleep(25);
                        }
                    }), true);
                }
            }
        }

        public int CreateVNCs(List<FixtureParams> sockets)
        {
            try
            {
                VNCClient.RegistryKeyVNC();
                _VNCs = new List<VNCClient>();
                foreach (FixtureParams f in sockets)
                {
                    Trace.WriteLine($"VNCClient ({f.VNC.Host}:{f.VNC.Password}): IsEnabled = {f.IsEnabled}");
                    string filename = IntPtr.Size == 4 ? "VNC-Viewer-6.0.0-Windows-32bit.exe" : "VNC-Viewer-6.0.0-Windows-64bit.exe";
                    string configFile = @$"params\VNC{f.Id}.ini";
                    int maxClient = 0;
                    foreach (var item in sockets)
                    {
                        if (item.IsEnabled)
                        {
                            ++maxClient;
                        }
                    }
                    VNCClient client = new VNCClient
                    {
                        Id = f.Id,
                        Host = f.VNC.Host,
                        Password = f.VNC.Password,
                        Filename = filename,
                        ConfigFile = configFile,
                        MaxClient = maxClient
                    };
                    client.CreatConfigurationFile(f.VNC.Host, f.VNC.Password, configFile);
                    if (f.IsEnabled)
                    {
                        _VNCs.Add(client);
                    }
                    else
                    {
                        continue;
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return -1;
            }
        }

        public void OpenVNCs()
        {
            if (_VNCs != null)
            {
                foreach (VNCClient client in _VNCs)
                {
                    System.Threading.Tasks.Task.Run(() => client.Open());
                }
            }
        }

        public void CloseVNCs()
        {
            if (_VNCs != null)
            {
                foreach (VNCClient client in _VNCs)
                {
                    System.Threading.Tasks.Task.Run(() => client.Close());
                }
            }
        }

        public void OpenVNC(int id)
        {
            VNCClient client = _VNCs.Find(x => x.Id == id);
            if (client != null)
            {
                client.Open(centerScreen: true);
            }
        }

        public void CloseVNC(int id)
        {
            VNCClient client = _VNCs.Find(x => x.Id == id);
            if (client != null)
            {
                client.Close();
            }
        }
    }
}
