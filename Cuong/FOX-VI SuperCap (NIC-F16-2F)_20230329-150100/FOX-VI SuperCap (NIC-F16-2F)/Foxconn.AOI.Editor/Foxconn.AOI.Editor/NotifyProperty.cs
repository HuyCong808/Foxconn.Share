using System.ComponentModel;

namespace Foxconn.AOI.Editor
{
    public class NotifyProperty : INotifyPropertyChanged
    {
        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
