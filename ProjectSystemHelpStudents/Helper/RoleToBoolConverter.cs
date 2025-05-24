using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ProjectSystemHelpStudents.Helper
{
    public class RoleToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int role)
                return role == 1; // 1 = админ, 2 = обычный

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAdmin)
                return isAdmin ? 1 : 2;

            return 2;
        }
    }
}
