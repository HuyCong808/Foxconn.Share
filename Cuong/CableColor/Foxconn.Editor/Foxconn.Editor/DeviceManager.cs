using Foxconn.Editor.Camera;
using System;
using System.Diagnostics;
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
        private TCPClient _tcpClient = null;
        private SerialPortClient _scanner = null;
        private SerialPortClient _terminal = null;

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
        public TCPClient TCPClient
        {
            get => _tcpClient;
            set => _tcpClient = value;
        }

        public SerialPortClient Scanner
        {
            get => _scanner;
            set => _scanner = value;
        }

        public SerialPortClient Terminal
        {
            get => _terminal;
            set => _terminal = value;
        }

        public static DeviceManager Current => __current;
        private static DeviceManager __current = new DeviceManager();
        static DeviceManager() { }
        private DeviceManager() { }

        public void LogInfoMessage(string message)
        {
            Logger.Current.Info(message);
            Console.WriteLine(message);
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
                        LogInfoMessage("DeviceManager.Open: Camera1 Opened");
                    }
                    else
                    {
                        LogInfoMessage("DeviceManager.Open: Camera1 can not Open");
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
                        LogInfoMessage("DeviceManager.Open: Camera2 Opened");
                    }
                    else
                    {
                        LogInfoMessage("DeviceManager.Open: Camera2 can not Open");
                    }
                }

                if (param.Light1.IsEnabled)
                {
                    int nRet = device.Light1.Open(param.Light1.PortName);
                    if (nRet == 1)
                    {
                        LogInfoMessage("DeviceManager.Open: Light1 Opend ");
                    }
                    else
                    {
                        LogInfoMessage("DeviceManager.Open: Light1 can not Open ");
                    }
                }

                if (param.Light2.IsEnabled)
                {
                    int nRet = device.Light2.Open(param.Light2.PortName);
                    if (nRet == 1)
                    {
                        LogInfoMessage("DeviceManager.Open: Light2 is Opened");
                    }
                    else
                    {
                        LogInfoMessage("DeviceManage.Open: Light2 can not Open");
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
                        LogInfoMessage("DeviceManager.Open: PLC1 is Opened");
                    }
                    else
                    {
                        LogInfoMessage("DeviceManager.Open: PLC1 can not Open");
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
                        LogInfoMessage("DeviceManager.Open: PLC2 is Opened");
                    }
                    else
                    {
                        LogInfoMessage("DeviceManager.Open: PLC2 can not Open");
                    }
                }

                if (param.Robot.IsEnabled)
                {
                    device.TCPClient = new TCPClient();
                    int nRet = device.TCPClient.Open(param.Robot.Host, param.Robot.Port);

                    if (nRet == 1)
                    {
                        LogInfoMessage("DeviceManager.Open: Robot Opend ");
                        if (param.Robot.Host != "")
                        {
                            nRet = device.TCPClient.SocketWriteData(param.Robot.Host);
                        }
                        if (param.Robot.Port != -1)
                        {
                            nRet = device.TCPClient.SocketWriteData(param.Robot.Port.ToString());
                        }
                    }
                    else
                    {
                        LogInfoMessage("DeviceManager.Open: Robot can not opend ");
                    }
                }

                if (param.Scanner.IsEnabled)
                {
                    device.Scanner = new SerialPortClient();
                    int nRet = device.Scanner.Open(param.Scanner.PortName);
                    if (nRet == 1)
                    {
                        LogInfoMessage("DeviceManager.Open: Scanner opened");
                    }
                    else
                    {
                        LogInfoMessage("DeviceManager.Open: Scanner can not open");
                    }
                }

                if (param.Terminal.IsEnabled)
                {
                    Console.WriteLine(param.Terminal.PortName);
                    device.Terminal = new SerialPortClient();
                    int nRet = device.Terminal.Open(param.Terminal.PortName);
                    if (nRet == 1)
                    {
                        Console.WriteLine("DeviceManager.Open: Terminal opened");
                        if (param.Terminal.Undo != "")
                        {
                            nRet = device.Terminal.SerialWriteData(param.Terminal.Undo);
                            Console.WriteLine(param.Terminal.Undo);
                        }
                        if (param.Terminal.User != "")
                        {
                            nRet = device.Terminal.SerialWriteData(param.Terminal.User);
                            Console.WriteLine(param.Terminal.User);
                        }
                    }
                    else
                    {
                        LogInfoMessage("DeviceManager.Open: Terminal can not open");
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

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
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
                        MessageBox.Show($"Light2 disconnected ({param.Light2.PortName}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);

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


                if (param.Robot.IsEnabled)
                {
                    int nRet = device.TCPClient.Ping(param.Robot.Host, param.Robot.Port);
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
