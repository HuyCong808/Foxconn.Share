using System.Threading;

namespace Foxconn.App.Controllers
{
    public class Token
    {
        private static CancellationTokenSource _cts { get; set; }
        private static CancellationToken _token { get; set; }

        private Token() { }
        private static Token _instance { get; set; }
        public static Token Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Token();
                    Init();
                }
                return _instance;
            }
        }

        private static void Init()
        {
            _cts = new CancellationTokenSource();
            _token = _cts.Token;
        }

        public CancellationToken GetToken()
        {
            return _token;
        }

        public bool CanRun()
        {
            return !_token.IsCancellationRequested;
        }

        public void Dispose()
        {
            if (_cts == null)
                return;

            // Request cancellation on the token.
            _cts.Cancel();
            if (_token.CanBeCanceled)
            {
                // Call Dispose when we're done with the CancellationTokenSource.
                _cts.Dispose();
            }
        }
    }
}
