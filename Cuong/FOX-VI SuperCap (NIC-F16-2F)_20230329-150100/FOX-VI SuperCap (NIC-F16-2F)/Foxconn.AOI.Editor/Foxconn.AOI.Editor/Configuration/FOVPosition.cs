namespace Foxconn.AOI.Editor.Configuration
{
    public class FOVPosition : NotifyProperty
    {
        private int _x { get; set; }
        private int _y { get; set; }
        private int _z1 { get; set; }
        private int _r1 { get; set; }
        private int _z2 { get; set; }
        private int _r2 { get; set; }
        private int _z3 { get; set; }
        private int _r3 { get; set; }

        public FOVPosition()
        {
            _x = 0;
            _y = 0;
            _z1 = 0;
            _r1 = 0;
            _z2 = 0;
            _r2 = 0;
            _z3 = 0;
            _r3 = 0;
        }

        public int X
        {
            get => _x;
            set
            {
                _x = value;
                NotifyPropertyChanged(nameof(X));
            }
        }

        public int Y
        {
            get => _y;
            set
            {
                _y = value;
                NotifyPropertyChanged(nameof(Y));
            }
        }

        public int Z1
        {
            get => _z1;
            set
            {
                _z1 = value;
                NotifyPropertyChanged(nameof(Z1));
            }
        }

        public int R1
        {
            get => _r1;
            set
            {
                _r1 = value;
                NotifyPropertyChanged(nameof(R1));
            }
        }

        public int Z2
        {
            get => _z2;
            set
            {
                _z2 = value;
                NotifyPropertyChanged(nameof(Z2));
            }
        }

        public int R2
        {
            get => _r2;
            set
            {
                _r2 = value;
                NotifyPropertyChanged(nameof(R2));
            }
        }

        public int Z3
        {
            get => _z3;
            set
            {
                _z3 = value;
                NotifyPropertyChanged(nameof(Z3));
            }
        }

        public int R3
        {
            get => _r3;
            set
            {
                _r3 = value;
                NotifyPropertyChanged(nameof(R3));
            }
        }
    }
}
