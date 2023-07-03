using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Foxconn.Editor.Converters
{
    public class BoolToShapeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return (bool)value ? (SolidColorBrush)new BrushConverter().ConvertFromString("#FF00FF00") : (SolidColorBrush)new BrushConverter().ConvertFromString("#FF666666");
            return (bool)value ? (SolidColorBrush)new BrushConverter().ConvertFromString("#FF00FF00") : (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFF0000");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
