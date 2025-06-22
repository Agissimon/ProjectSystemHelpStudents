using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProjectSystemHelpStudents.Helper
{
    public class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            {
                // убедимся, что строка начинается с #
                var c = hex.StartsWith("#") ? hex : "#" + hex;
                try
                {
                    return (SolidColorBrush)(new BrushConverter().ConvertFromString(c));
                }
                catch { }
            }
            // по умолчанию — прозрачный
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
