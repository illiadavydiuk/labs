using System;
using System.Globalization;
using System.Windows.Data;

namespace DuckNet.UI.Converters
{
    // Цей клас перетворює bool (True/False) у рядок (🟢/🔴)
    public class StatusIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOnline && isOnline)
            {
                return "🟢"; // Онлайн
            }
            return "🔴"; // Офлайн
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}