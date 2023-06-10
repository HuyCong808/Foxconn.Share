using Foxconn.TestUI.Camera;
using System;
using System.Diagnostics;
using System.Windows;

namespace Foxconn.TestUI.Editor
{
    internal class DeviceManager
    {

        private ICamera _camera1 = null;
        private ICamera _camera2 = null;
        private SerialLight _light1 = null;
        private SerialLight _light2 = null;
        private SLMP _plc1 = null;
        private SLMP _plc2 = null;
        private SocketClient _robot1 = null;
        private SocketClient _robot2 = null;

        private SerialClient _terminal = null;
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

        public SocketClient Robot1
        {
            get => _robot1;
            set => _robot1 = value;
        }
        public SocketClient Robot2
        {
            get => _robot2;
            set => _robot2 = value;
        }

        public SerialClient Terminal
        {
            get => _terminal;
            set => _terminal = value;
        }
        public static DeviceManager Current => __current;
        private static DeviceManager __current = new DeviceManager();
        static DeviceManager() { }
        private DeviceManager() { }

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
                        Console.WriteLine("DeviceManager.Open: Camera1 opened");
                    }
                    else
                    {
                        Console.WriteLine("DeviceManager.Open: Camera1 can not open");
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
                        Trace.WriteLine("DeviceManager.Open: Camera2 can not open");
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
                        Trace.WriteLine("DeviceManager.Open: Light1 can not open");
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
                        Trace.WriteLine("DeviceManager.Open: Light2 can not open");
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
                        Trace.WriteLine("DeviceManager.Open: PLC1 can not open");
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
                        Trace.WriteLine("DeviceManager.Open: PLC2 can not open");
                    }
                }

                if (param.Robot1.IsEnabled)
                {
                    device.Robot1 = new SocketClient();
                    int nRet = device.Robot1.Open(param.Robot1.Host, param.Robot1.Port);
                    if (nRet == 1)
                    {
                        Console.WriteLine("DeviceManager.Open: Robot opened");
                    }
                    else
                    {
                        Console.WriteLine("DeviceManager.Open: Robot can not open");
                    }
                }
                if (param.Robot2.IsEnabled)
                {
                    device.Robot2 = new SocketClient();
                    int nRet = device.Robot2.Open(param.Robot2.Host, param.Robot2.Port);
                    if (nRet == 1)
                    {
                        Console.WriteLine("DeviceManager.Open: Robot opened");
                    }
                    else
                    {
                        Console.WriteLine("DeviceManager.Open: Robot can not open");
                    }
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
                        Trace.WriteLine("DeviceManager.Open: Terminal can not open");
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
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
                _robot1?.Close();
                _robot2?.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
        public int Ping()
        {
            int Pret = 1;
            try
            {
                MachineParams param = MachineParams.Current;
                DeviceManager device = Current;
                if (param.Camera1.IsEnabled)
                {
                    if (!device.Camera1.IsConnected)
                    {
                        Pret = -1;
                        MessageBox.Show($"Camera1 disconnected ({param.Camera1.UserDefinedName}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

                if (param.Camera2.IsEnabled)
                {
                    if (!device.Camera2.IsConnected)
                    {
                        Pret = -1;
                        MessageBox.Show($"Camera2 disconnected ({param.Camera2.UserDefinedName}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }

                if (param.Light1.IsEnabled)
                {
                    if (!device.Light1.IsConnected)
                    {
                        Pret = -1;
                        MessageBox.Show("Light 1 Disconnect");
                    }
                }

                if (param.Light2.IsEnabled)
                {
                    if (!device.Light2.IsConnected)
                    {
                        Pret = -1;
                        MessageBox.Show("light 2 Disconnect");
                    }
                }

                if (param.PLC1.IsEnabled)
                {
                    int nRet = device.PLC1.Ping(param.PLC1.Host, param.PLC1.Port);
                    if (nRet != 1)
                    {
                        Pret = -1;
                        MessageBox.Show("PLC1 Disconnect");
                    }
                }

                if (param.PLC2.IsEnabled)
                {
                    int nRet = device.PLC2.Ping(param.PLC2.Host, param.PLC2.Port);
                    if (nRet != 1)
                    {
                        Pret = -1;
                        MessageBox.Show("PLC2 Disconnect");

                    }
                }

                if (param.Robot1.IsEnabled)
                {
                    int nRet = device.Robot1.Ping(param.Robot1.Host, param.Robot1.Port);
                    if (nRet != 1)
                    {
                        MessageBox.Show("Robot1 Disconnect");
                        Pret = -1;
                    }
                }
                if (param.Robot2.IsEnabled)
                {
                    int nRet = device.Robot2.Ping(param.Robot2.Host, param.Robot2.Port);
                    if (nRet != 1)
                    {
                        MessageBox.Show("Robot1 Disconnect");
                        Pret = -1;
                    }
                }
                if (param.Terminal.IsEnabled)
                {
                    if (!device.Terminal.IsConnected)
                    {
                        Pret = -1;
                        MessageBox.Show("Terminal Disconnect");

                    }
                }
            }
            catch (Exception ex)
            {
                Pret = -1;
                Trace.WriteLine(ex);
            }
            return Pret;
        }
    }
}
