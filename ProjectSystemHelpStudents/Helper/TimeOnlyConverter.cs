using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectSystemHelpStudents.Helper
{
    public class TimeOnlyConverter : IValueConverter
    {
        // From DateTime? to "HH:mm"
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
                return dt.ToString("HH:mm");
            return string.Empty;
        }

        // Если по какой-то причине WPF всё-таки вызовет ConvertBack,
        // просто не трогаем источник — возвращаем DoNothing
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
