using Foxconn.Editor.Camera;
using System;
using System.Windows;

namespace Foxconn.Editor
{
    public class DeviceManager
    {
        private ICamera _camera1 = null;
        private ICamera _camera2 = null;
        private SLMP _plc1 = null;
        private SLMP _plc2 = null;
        private SerialLight _light1;
        private SerialLight _light2;
        private TCPClient _robot = null;
        private SerialPortClient _scanner1 = null;
        private SerialPortClient _scanner2 = null;
        private SerialPortClient _terminalIT = null;
        private SerialPortClient _terminalBoard = null;

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
        public TCPClient Robot
        {
            get => _robot;
            set => _robot = value;
        }

        public SerialPortClient Scanner1
        {
            get => _scanner1;
            set => _scanner1 = value;
        }

        public SerialPortClient Scanner2
        {
            get => _scanner2;
            set => _scanner2 = value;
        }

        public SerialPortClient TerminalIT
        {
            get => _terminalIT;
            set => _terminalIT = value;
        }

        public SerialPortClient TerminalBoard
        {
            get => _terminalBoard;
            set => _terminalBoard = value;
        }

        public static DeviceManager Current => __current;
        private static DeviceManager __current = new DeviceManager();
        static DeviceManager() { }
        private DeviceManager() { }

        public void LogInfo(string message)
        {
            Logger.Current.Info(message);
        }
        public void Open()
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
                        LogInfo("DeviceManager.Open: Camera1 Opened");
                    }
                    else
                    {
                        LogInfo("DeviceManager.Open: Camera1 can not Open");
                    }
                }

                if (param.Camera2.IsEnabled)
                {
                    device.Camera2 = CameraFactory.GetCamera(param.Camera2.Type);
                    device.Camera2.DeviceListAcq();
                    int nRet = device.Camera2.Open(param.Camera2.UserDefinedName);
                    if (nRet == 1)
                    {
                        device.Camera2.SetParameter(KeyName.TriggerMode, 1);
                        device.Camera2.SetParameter(KeyName.TriggerSource, 7);
                        device.Camera2.StartGrabbing();
                        LogInfo("DeviceManager.Open: Camera2 Opened");
                    }
                    else
                    {
                        LogInfo("DeviceManager.Open: Camera2 can not Open");
                    }
                }

                if (param.Light1.IsEnabled)
                {
                    int nRet = device.Light1.Open(param.Light1.PortName);
                    if (nRet == 1)
                    {
                        LogInfo("DeviceManager.Open: Light1 Opend ");
                    }
                    else
                    {
                        LogInfo("DeviceManager.Open: Light1 can not Open ");
                    }
                }

                if (param.Light2.IsEnabled)
                {
                    int nRet = device.Light2.Open(param.Light2.PortName);
                    if (nRet == 1)
                    {
                        LogInfo("DeviceManager.Open: Light2 is Opened");
                    }
                    else
                    {
                        LogInfo("DeviceManage.Open: Light2 can not Open");
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
                        LogInfo("DeviceManager.Open: PLC1 is Opened");
                    }
                    else
                    {
                        LogInfo("DeviceManager.Open: PLC1 can not Open");
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
                        LogInfo("DeviceManager.Open: PLC2 is Opened");
                    }
                    else
                    {
                        LogInfo("DeviceManager.Open: PLC2 can not Open");
                    }
                }

                if (param.Robot.IsEnabled)
                {
                    device.Robot = new TCPClient();
                    int nRet = device.Robot.Open(param.Robot.Host, param.Robot.Port);

                    if (nRet == 1)
                    {
                        LogInfo("DeviceManager.Open: Robot Opend ");
                        if (param.Robot.Host != "")
                        {
                            nRet = device.Robot.SocketWriteData(param.Robot.Host);
                        }
                        if (param.Robot.Port != -1)
                        {
                            nRet = device.Robot.SocketWriteData(param.Robot.Port.ToString());
                        }
                    }
                    else
                    {
                        LogInfo("DeviceManager.Open: Robot can not opend ");
                    }
                }

                if (param.Scanner1.IsEnabled)
                {
                    device.Scanner1 = new SerialPortClient();
                    int nRet = device.Scanner1.Open(param.Scanner1.PortName);
                    if (nRet == 1)
                    {
                        LogInfo("DeviceManager.Open: Scanner1 opened");
                    }
                    else
                    {
                        LogInfo("DeviceManager.Open: Scanner1 can not open");
                    }
                }

                if (param.Scanner2.IsEnabled)
                {
                    device.Scanner2 = new SerialPortClient();
                    int nRet = device.Scanner2.Open(param.Scanner2.PortName);
                    if (nRet == 1)
                    {
                        LogInfo("DeviceManager.Open: Scanner2 Opened");
                    }
                    else
                    {
                        LogInfo("DeviceManager.Open: Scanner2 can not Opened");
                    }
                }

                if (param.TerminalIT.IsEnabled)
                {
                    device.TerminalIT = new SerialPortClient();
                    int nRet = device.TerminalIT.Open(param.TerminalIT.PortName);
                    if (nRet == 1)
                    {
                        LogInfo("DeviceManager.Open: TerminalIT opened");
                        if (param.TerminalIT.Undo != "")
                        {
                            nRet = device.TerminalIT.SerialWriteData(param.TerminalIT.Undo + "\r\n");
                        }
                        if (param.TerminalIT.User != "")
                        {
                            nRet = device.TerminalIT.SerialWriteData(param.TerminalIT.User + "\r\n");
                        }
                    }
                    else
                    {
                        LogInfo("DeviceManager.Open: TerminalIT can not open");
                    }
                }

                if (param.TerminalBoard.IsEnabled)
                {
                    device.TerminalBoard = new SerialPortClient();
                    int nRet = device.TerminalBoard.Open(param.TerminalBoard.PortName);
                    if (nRet == 1)
                    {
                        LogInfo("DeviceManager.Open: TerminalBoard opened");
                    }
                    else
                    {
                        LogInfo("DeviceManager.Open: TerminalBoard can not open");
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Current.Error(ex.Message);
            }
        }
        public void Close()
        {
            try
            {

                _camera1?.Close();
                _camera2?.Close();
                _light1?.Close();
                _light2?.Close();
                _robot?.Close();
            }
            catch (Exception ex)
            {
                Logger.Current.Error(ex.Message);
            }
        }

        public int Ping()
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

                if (param.PLC1.IsEnabled)
                {
                    int nRet = device.PLC1.Ping(param.PLC1.Host, param.PLC1.Port);
                    if (nRet != 1)
                    {
                        fRet = -1;
                        MessageBox.Show($"PLC1 disconnected ({param.PLC1.Host},{param.PLC1.Port} ).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

                if (param.PLC2.IsEnabled)
                {
                    int nRet = device.PLC2.Ping(param.PLC2.Host, param.PLC2.Port);

                    if (nRet != 1)
                    {
                        fRet = -1;
                        MessageBox.Show($"PLC2 disconnected ({param.PLC2.Host},{param.PLC2.Port}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
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
                        MessageShow.Error($"Light2 disconnected ({param.Light2.PortName}).", "Error");

                    }
                }

                if (param.Scanner1.IsEnabled)
                {
                    if (!device.Scanner1.IsConnected)
                    {
                        fRet = -1;
                        MessageShow.Error($"Scanner1 disconnected ({param.Scanner1.PortName}).", "Error");
                    }
                }

                if (param.TerminalIT.IsEnabled)
                {
                    if (!device.TerminalIT.IsConnected)
                    {
                        fRet = -1;
                        MessageShow.Error($"TerminalIT disconnected ({param.TerminalIT.PortName}).", "Error");
                    }
                }

                if (param.TerminalBoard.IsEnabled)
                {
                    if (!device.TerminalBoard.IsConnected)
                    {
                        fRet = -1;
                        MessageShow.Error($"TerminalBoard disconnected ({param.TerminalBoard.PortName}).", "Error");
                    }
                }

                if (param.Robot.IsEnabled)
                {
                    int nRet = device.Robot.Ping(param.Robot.Host, param.Robot.Port);
                    if (nRet != 1)
                    {
                        fRet = -1;
                        MessageBox.Show($"Robot disconnected ({param.Robot.Host}, {param.Robot.Port}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error(ex.Message);
            }
            return fRet;
        }

    }
}
