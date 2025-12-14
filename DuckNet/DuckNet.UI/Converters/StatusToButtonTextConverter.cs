using System;
using System.Globalization;
using System.Windows.Data;

namespace DuckNet.UI.Converters
{
    public class StatusToButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value - це властивість IsEnabled (bool)
            if (value is bool isEnabled && isEnabled)
            {
                // Якщо адаптер працює, пропонуємо його вимкнути
                return "❌ Вимкнути";
            }

            // Якщо адаптер вимкнений, пропонуємо його увімкнути
            return "✅ Увімкнути";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}