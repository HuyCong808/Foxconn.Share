using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Foxconn.AOI.Editor.Converters
{
    public class BoolToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? (SolidColorBrush)new BrushConverter().ConvertFromString("#FFEFEFEF") : (SolidColorBrush)new BrushConverter().ConvertFromString("#666666");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
