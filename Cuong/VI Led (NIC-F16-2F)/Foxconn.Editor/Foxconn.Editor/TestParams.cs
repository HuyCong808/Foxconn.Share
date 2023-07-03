using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Foxconn.Editor
{
    public class TestParams
    {
        private string _filePath = @$"{AppDomain.CurrentDomain.BaseDirectory}params\TestParams.json";
        private bool _readBarcode = true;
        private bool _openFixture = true;
        private bool _closeFixture = true;
        private GoldenSampleParams _goldenSample = new GoldenSampleParams();
        private List<FixtureParams> _fixtures = new List<FixtureParams>();

        public bool ReadBarcode
        {
            get => _readBarcode;
            set => _readBarcode = value;
        }

        public bool OpenFixture
        {
            get => _openFixture;
            set => _openFixture = value;
        }

        public bool CloseFixture
        {
            get => _closeFixture;
            set => _closeFixture = value;
        }

        public GoldenSampleParams GoldenSample
        {
            get => _goldenSample;
            set => _goldenSample = value;
        }

        public List<FixtureParams> Fixtures
        {
            get => _fixtures;
            set => _fixtures = value;
        }

        public static TestParams Current => __current;
        private static TestParams __current = new TestParams();
        static TestParams() { }
        private TestParams() { }

        public static void Reload()
        {
            TestParams testParams = new TestParams();
            TestParams loaded = testParams.Load();
            if (loaded != null)
            {
                __current = loaded;
            }
            else
            {
                testParams.Save();
            }
        }

        public TestParams Load()
        {
            TestParams testParams = null;
            if (File.Exists(_filePath))
            {
                string contents = File.ReadAllText(_filePath);
                testParams = JsonConvert.DeserializeObject<TestParams>(contents);
            }
            return testParams;
        }

        public void Save()
        {
            string contents = JsonConvert.SerializeObject(__current, Formatting.Indented);
            File.WriteAllText(_filePath, contents);
        }

        public class SignalParams
        {
            private string _1init = "M1";
            private string _1pass = "M1";
            private string _1fail = "M1";
            private string _1repa = "M1";
            private string _1Init2Init = "M1";
            private string _1Pass2Pass = "M1";
            private string _1Pass2Fail = "M1";
            private string _1Fail2Pass = "M1";
            private string _1Fail2Fail = "M1";
            private string _ready = "M1";

            public string S_1Init
            {
                get => _1init;
                set => _1init = value;
            }

            public string S_1Pass
            {
                get => _1pass;
                set => _1pass = value;
            }

            public string S_1Fail
            {
                get => _1fail;
                set => _1fail = value;
            }

            public string S_1Repa
            {
                get => _1repa;
                set => _1repa = value;
            }

            public string D_1Init2Init
            {
                get => _1Init2Init;
                set => _1Init2Init = value;
            }

            public string D_1Pass2Pass
            {
                get => _1Pass2Pass;
                set => _1Pass2Pass = value;
            }

            public string D_1Pass2Fail
            {
                get => _1Pass2Fail;
                set => _1Pass2Fail = value;
            }

            public string D_1Fail2Pass
            {
                get => _1Fail2Pass;
                set => _1Fail2Pass = value;
            }

            public string D_1Fail2Fail
            {
                get => _1Fail2Fail;
                set => _1Fail2Fail = value;
            }

            public string Ready
            {
                get => _ready;
                set => _ready = value;
            }
        }

        public class SocketParams
        {
            private string _host = string.Empty;
            private int _port = 0;

            public string Host
            {
                get => _host;
                set => _host = value;
            }

            public int Port
            {
                get => _port;
                set => _port = value;
            }
        }

        public class VNCParams
        {
            private string _host = "127.0.0.1";
            private string _password = "123456";

            public string Host
            {
                get => _host;
                set => _host = value;
            }

            public string Password
            {
                get => _password;
                set => _password = value;
            }
        }

        public class FixtureParams
        {
            private int _id = -1;
            private string _name = "F";
            private bool _isEnabled = false;
            private SocketParams _socket = new SocketParams();
            private VNCParams _vnc = new VNCParams();
            private SignalParams _signal = new SignalParams();

            public int Id
            {
                get => _id;
                set => _id = value;
            }

            public string Name
            {
                get => _name;
                set => _name = value;
            }

            public bool IsEnabled
            {
                get => _isEnabled;
                set => _isEnabled = value;
            }

            public SocketParams Socket
            {
                get => _socket;
                set => _socket = value;
            }

            public VNCParams VNC
            {
                get => _vnc;
                set => _vnc = value;
            }

            public SignalParams Signal
            {
                get => _signal;
                set => _signal = value;
            }
        }

        public class GoldenSampleParams
        {
            private bool _isEnabled = false;
            private string _labelOK = string.Empty;
            private string _labelNG = string.Empty;

            public bool IsEnabled
            {
                get => _isEnabled;
                set => _isEnabled = value;
            }

            public string LabelOK
            {
                get => _labelOK;
                set => _labelOK = value;
            }

            public string LabelNG
            {
                get => _labelNG;
                set => _labelNG = value;
            }
        }
    }
}
