using System;
using System.Globalization;
using System.Windows.Data;

namespace Foxconn.AOI.Editor.Converters
{
    public class StringNotNullToNextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEnable = true;
            for (int i = 0; i < values.Length; i++)
            {
                if (string.IsNullOrEmpty((string)values[i]))
                {
                    isEnable = false;
                }
            }
            return isEnable;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[0];
        }
    }
}
