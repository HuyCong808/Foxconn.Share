using System;
using System.Globalization;
using System.Windows.Data;

namespace Foxconn.UI.Controls
{
    public class NullToValueConverter : IValueConverter
    {
        public object NullValue { get; set; }

        public object NotNullValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value != null ? NotNullValue : NullValue;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
