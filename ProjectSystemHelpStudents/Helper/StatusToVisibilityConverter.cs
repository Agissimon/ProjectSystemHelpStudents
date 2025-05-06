using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProjectSystemHelpStudents.Helper
{
    public class StatusToVisibilityConverter : IValueConverter
    {
        // value — это строка Status ("Member", "Pending" и т.п.)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as string;
            return status == "Member"
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
