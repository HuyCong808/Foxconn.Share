namespace Foxconn.Threading
{
    public interface IProgressListener<TProgress, TStatus>
    {
        void ReportProgress(TProgress completed, TProgress total, TStatus status);

        void Complete();

        void Cancel();

        void Cancel(TStatus status);
    }
}
