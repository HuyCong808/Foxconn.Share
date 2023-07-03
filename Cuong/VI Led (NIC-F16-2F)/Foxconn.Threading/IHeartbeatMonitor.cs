namespace Foxconn.Threading
{
    public interface IHeartbeatMonitor
    {
        void Wait();

        bool Wait(int millisecondsTimeout);

        int Count { get; }
    }
}
