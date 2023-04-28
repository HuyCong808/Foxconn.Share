using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Foxconn.AOI.Editor.Converters
{
    public class BoolToVisibilityHiddenConverter : IValueConverter
    {
        private bool invertBoolean;

        public bool InvertBoolean
        {
            get => invertBoolean;
            set => invertBoolean = value;
        }

        public object ConvertBack(object o, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = (Visibility)o == Visibility.Visible;
            if (invertBoolean)
                flag = !flag;
            return flag;
        }

        public object Convert(object o, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = (bool)o;
            if (invertBoolean)
                flag = !flag;
            return (Visibility)(flag ? 0 : 1);
        }
    }
}
