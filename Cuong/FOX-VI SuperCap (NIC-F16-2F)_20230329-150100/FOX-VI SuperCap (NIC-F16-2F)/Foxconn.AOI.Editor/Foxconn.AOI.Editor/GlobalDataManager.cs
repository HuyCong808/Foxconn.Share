namespace Foxconn.AOI.Editor
{
    internal class GlobalDataManager
    {
        private bool _isSettings = false;
        private bool _isAutoRun = false;
        private bool _IsDoingInspection = false;
        private string _barcode = string.Empty;
        private string _scannerData = string.Empty;
        private string _cycleTime = "00:00:00.0000000";
        private int _passCount = 0;
        private int _failCount = 0;
        private int _totalPCBs = 0;

        public bool IsSettings
        {
            get => _isSettings;
            set => _isSettings = value;
        }

        public bool IsAutoRun
        {
            get => _isAutoRun;
            set => _isAutoRun = value;
        }

        public bool IsDoingInspection
        {
            get => _IsDoingInspection;
            set => _IsDoingInspection = value;
        }

        public string Barcode
        {
            get => _barcode;
            set => _barcode = value;
        }

        public string ScannerData
        {
            get => _scannerData;
            set => _scannerData = value;
        }

        public string CycleTime
        {
            get => _cycleTime;
            set => _cycleTime = value;
        }

        public int PassCount
        {
            get => _passCount;
            set => _passCount = value;
        }

        public int FailCount
        {
            get => _failCount;
            set => _failCount = value;
        }

        public int TotalPCBs
        {
            get => _totalPCBs;
            set => _totalPCBs = value;
        }

        public static GlobalDataManager Current => __current;
        private static GlobalDataManager __current = new GlobalDataManager();
        private GlobalDataManager() { }
        static GlobalDataManager() { }

        public void Clear()
        {
            _isSettings = false;
            _isAutoRun = false;
            _IsDoingInspection = false;
            _barcode = string.Empty;
            _scannerData = string.Empty;
            _cycleTime = string.Empty;
            _passCount = 0;
            _failCount = 0;
            _totalPCBs = 0;
        }
    }
}
