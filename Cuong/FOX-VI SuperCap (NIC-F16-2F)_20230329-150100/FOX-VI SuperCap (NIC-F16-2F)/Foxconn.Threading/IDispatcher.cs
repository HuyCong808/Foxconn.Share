using System;

namespace Foxconn.Threading
{
    public interface IDispatcher
    {
        void BeginInvoke(Delegate @delegate, params object[] args);

        void Invoke(Delegate @delegate, params object[] args);
    }
}
