using System.ComponentModel;

namespace Foxconn.Threading
{
    public class ProgressReporter<TProgress, TStatus> :
    IProgressListener<TProgress, TStatus>,
    INotifyPropertyChanged
    {
        private TProgress _completedProgress;
        private TProgress _totalProgress;
        private TStatus _status;
        private bool _isCompleted = true;
        private bool _isCancelled;
        private bool _isInProgress;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
                return;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            private set
            {
                if (_isCompleted == value)
                    return;
                _isCompleted = value;
                OnPropertyChanged(nameof(IsCompleted));
            }
        }

        public bool IsCancelled
        {
            get => _isCancelled;
            private set
            {
                if (_isCancelled == value)
                    return;
                _isCancelled = value;
                OnPropertyChanged(nameof(IsCancelled));
            }
        }

        public bool IsInProgress
        {
            get => _isInProgress;
            private set
            {
                if (_isInProgress == value)
                    return;
                _isInProgress = value;
                OnPropertyChanged(nameof(IsInProgress));
            }
        }

        public TProgress CompletedProgress
        {
            get => _completedProgress;
            private set
            {
                if (Equals(_completedProgress, value))
                    return;
                _completedProgress = value;
                OnPropertyChanged(nameof(CompletedProgress));
            }
        }

        public TProgress TotalProgress
        {
            get => _totalProgress;
            private set
            {
                if (Equals(_totalProgress, value))
                    return;
                _totalProgress = value;
                OnPropertyChanged(nameof(TotalProgress));
            }
        }

        public TStatus Status
        {
            get => _status;
            private set
            {
                if (Equals(_status, value))
                    return;
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public void ReportProgress(TProgress completed, TProgress total, TStatus status)
        {
            CompletedProgress = completed;
            TotalProgress = total;
            Status = status;
            IsCancelled = false;
            IsCompleted = false;
            IsInProgress = true;
        }

        public void Complete()
        {
            IsCancelled = false;
            IsCompleted = true;
            IsInProgress = false;
            Status = default(TStatus);
        }

        public void Cancel() => Cancel(default(TStatus));

        public void Cancel(TStatus status)
        {
            IsCompleted = false;
            IsCancelled = true;
            IsInProgress = false;
            Status = status;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
