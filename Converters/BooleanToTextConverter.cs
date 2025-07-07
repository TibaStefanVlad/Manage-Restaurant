using System;
using System.Globalization;
using System.Windows.Data;

namespace Restaurant.Converters
{
    public class BooleanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string param)
            {
                string[] options = param.Split('|');
                if (options.Length >= 2)
                {
                    return boolValue ? options[0] : options[1];
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}