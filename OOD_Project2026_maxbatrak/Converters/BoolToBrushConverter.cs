using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace OOD_Project2026_maxbatrak
{
    // Converts a bool (IsPaid) to a colour brush.
    // true  = Paid  = Green
    // false = Owed  = Red
    public class BoolToBrushConverter : IValueConverter
    {
        public static readonly BoolToBrushConverter Instance = new BoolToBrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool isPaid && isPaid)
                ? new SolidColorBrush(Color.FromRgb(56, 142, 60))   // green
                : new SolidColorBrush(Color.FromRgb(198, 40, 40));  // red
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
