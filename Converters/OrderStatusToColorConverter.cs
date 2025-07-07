using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Restaurant.Converters
{
    public class OrderStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status)
                {
                    case "Registered":
                        return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                    case "InPreparation":
                        return new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Blue
                    case "Shipped":
                        return new SolidColorBrush(Color.FromRgb(156, 39, 176)); // Purple
                    case "Delivered":
                        return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                    case "Cancelled":
                        return new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                    default:
                        return new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Grey
                }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
