using System;

namespace Foxconn.Threading
{
    public interface IProducerConsumer
    {
        void BeginInvoke(Delegate @delegate);

        void BeginInvoke(Delegate @delegate, object[] arguments);
    }
}
