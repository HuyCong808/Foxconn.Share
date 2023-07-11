using System;
using System.Globalization;
using System.Windows.Data;

namespace Foxconn.Editor.Controls
{
    internal class ObjectToStringConverter : IValueConverter
    {
        private string _format = string.Empty;

        public string Format
        {
            get => _format;
            set => _format = value;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format(culture, _format, new object[1] { value });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
