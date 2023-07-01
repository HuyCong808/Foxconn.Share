namespace Foxconn.Threading
{
    public interface IHeartbeat
    {
        int Count { get; }

        IHeartbeatMonitor CreateMonitor();
    }
}
