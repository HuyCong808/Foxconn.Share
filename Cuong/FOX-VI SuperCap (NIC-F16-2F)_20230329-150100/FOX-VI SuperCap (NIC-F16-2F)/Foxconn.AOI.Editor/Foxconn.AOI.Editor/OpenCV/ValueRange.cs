namespace Foxconn.AOI.Editor.OpenCV
{
    public class ValueRange : NotifyProperty
    {
        private double _lower { get; set; }
        private double _upper { get; set; }
        private double _minimum { get; set; }
        private double _maximum { get; set; }

        public double Lower
        {
            get => _lower;
            set
            {
                _lower = value;
                _lower = _lower < Minimum ? Minimum : _lower > Maximum ? Maximum : _lower;
                NotifyPropertyChanged(nameof(Lower));
            }
        }

        public double Upper
        {
            get => _upper;
            set
            {
                _upper = value;
                _upper = _upper < Minimum ? Minimum : _upper > Maximum ? Maximum : _upper;
                NotifyPropertyChanged(nameof(Upper));
            }
        }

        public double Minimum
        {
            get => _minimum;
            set => _minimum = value;
        }

        public double Maximum
        {
            get => _maximum;
            set => _maximum = value;
        }

        public ValueRange()
        {
            _lower = 80;
            _upper = 100;
            _minimum = 0;
            _maximum = 100;
        }

        public ValueRange(double lower, double upper, double minimum, double maximum)
        {
            _minimum = minimum;
            _maximum = maximum;
            _lower = lower < minimum ? minimum : lower > maximum ? maximum : lower;
            _upper = upper < minimum ? minimum : upper > maximum ? maximum : upper;
        }
    }
}
