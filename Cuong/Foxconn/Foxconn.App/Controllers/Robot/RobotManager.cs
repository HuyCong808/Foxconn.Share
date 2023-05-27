using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Foxconn.App.Controllers.Robot
{
    public class RobotManager : IDisposable
    {
        private readonly MainWindow Root = MainWindow.Current;
        public List<Robot> RobotList { get; set; }
        public bool Scan { get; set; }
        public bool OutOfTray { get; set; }
        public bool CheckRepair { get; set; }
        public bool CheckInput { get; set; }
        public bool ReadyInput1OK { get; set; }
        public bool ReadyInput2OK { get; set; }
        public bool ReadyInput3OK { get; set; }
        public bool CheckOutput { get; set; }
        public bool ReadyOutput1OK { get; set; }
        public bool ReadyOutput2OK { get; set; }
        public bool ReadyOutput3OK { get; set; }
        private bool _disposed { get; set; }

        public RobotManager()
        {
            RobotList = new List<Robot>();
        }

        #region Disposable
        ~RobotManager()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        // Public implementation of Dispose pattern callable by consumers.
        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.
            _disposed = true;
        }
        #endregion

        public void Init()
        {
            try
            {
                _disposed = false;
                Scan = false;
                OutOfTray = false;
                CheckRepair = false;
                CheckOutput = false;
                ReadyInput1OK = false;
                ReadyInput2OK = false;
                ReadyInput3OK = false;
                CheckOutput = false;
                ReadyOutput1OK = false;
                ReadyOutput2OK = false;
                ReadyOutput3OK = false;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        public void Start()
        {
            var robots = Root.AppManager.DatabaseManager.Basic.Robot.Devices;
            foreach (var item in robots)
            {
                if (item.Enable)
                {
                    Root.ShowMessage($"[Robot {item.Index}] Enable");
                    var robot = new Robot
                    {
                        Index = item.Index,
                        Alias = item.Alias,
                        Host = item.Host,
                        Port = item.Port
                    };
                    robot.StartRobot();
                    robot.InvokeDataReceived += DataReceivedEventHandler;
                    RobotList.Add(robot);
                }
                else
                {
                    Root.ShowMessage($"[Robot {item.Index}] Disable");
                }
                Thread.Sleep(10);
            }
        }

        public void CheckConnection(int index = 0)
        {
            var robot = RobotList.Find(x => x.Index == index);
            if (robot != null)
            {
                robot.CheckConnection();
            }
        }

        public async Task<bool> Send(string data, int index = 0)
        {
            var robot = RobotList.Find(x => x.Index == index);
            if (robot != null)
            {
                if (robot.Status != ConnectionStatus.Connected)
                    return false;
                await robot.Send(data);
                return true;
            }
            return false;
        }

        public async Task<bool> SendMessage(string data, string dataReceived = "", int index = 0)
        {
            var robot = RobotList.Find(x => x.Index == index);
            if (robot != null)
            {
                if (robot.Status != ConnectionStatus.Connected)
                    return false;

                robot.DataReceived = string.Empty;
                await robot.Send(data);
                while (true)
                {
                    if (robot.DataReceived == dataReceived)
                        return true;

                    await Task.Delay(100);
                }
            }
            return false;
        }

        private void DataReceivedEventHandler(string data)
        {
            switch (data.Trim())
            {
                case "SCAN":
                    Scan = true;
                    break;
                case "OUT_OF_TRAY":
                    OutOfTray = true;
                    break;
                case "CHECK_REPAIR":
                    CheckRepair = true;
                    break;
                case "CHECK_INPUT":
                    CheckInput = true;
                    break;
                case "READY_INPUT1OK":
                    ReadyInput1OK = true;
                    break;
                case "READY_INPUT2OK":
                    ReadyInput2OK = true;
                    break;
                case "READY_INPUT3OK":
                    ReadyInput3OK = true;
                    break;
                case "CHECK_OUTPUT":
                    CheckOutput = true;
                    break;
                case "READY_OUTPUT1OK":
                    ReadyOutput1OK = true;
                    break;
                case "READY_OUTPUT2OK":
                    ReadyOutput2OK = true;
                    break;
                case "READY_OUTPUT3OK":
                    ReadyOutput3OK = true;
                    break;
                default:
                    break;
            }
        }

        public string GettingDataReceived(int index = 0)
        {
            var robot = RobotList.Find(x => x.Index == index);
            if (robot != null)
            {
                return robot.DataReceived;
            }
            return string.Empty;
        }

        public bool ClearDataReceived(string data = "", int index = 0)
        {
            var robot = RobotList.Find(x => x.Index == index);
            if (robot != null)
            {
                if (data == "")
                {
                    robot.DataReceived = string.Empty;
                    return true;
                }
                if (robot.DataReceived == data)
                {
                    robot.DataReceived = string.Empty;
                    return true;
                }
            }
            return false;
        }
    }
}
