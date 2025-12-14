using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DuckNet.UI.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOnline && isOnline)
                return new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Зелений (#10B981)

            return new SolidColorBrush(Color.FromRgb(239, 68, 68));   // Червоний (#EF4444)
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}