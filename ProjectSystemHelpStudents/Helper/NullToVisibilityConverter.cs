using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProjectSystemHelpStudents.Helper
{
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Если value == null → Collapsed, иначе → Visible.
        /// Если ConverterParameter="true" → наоборот.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter?.ToString().ToLower() == "true";
            bool isNull = value == null;
            if (invert) isNull = !isNull;
            return isNull
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}