using System;

namespace Foxconn.Threading.Tasks
{
    public static class Future
    {
        public static Future<T> Create<T>(Func<T> func) => Future<T>.Create(func);
    }

    public class Future<T> : Task
    {
        private T _value;
        private Func<T> _func;

        public T Value
        {
            get
            {
                Wait();
                return _value;
            }
        }

        protected Future(Func<T> func) => _func = func;

        protected override void OnExecute() => _value = _func();

        public static Future<T> Create(Func<T> func) => func != null ? new Future<T>(func) : throw new ArgumentNullException(nameof(func));
    }
}
