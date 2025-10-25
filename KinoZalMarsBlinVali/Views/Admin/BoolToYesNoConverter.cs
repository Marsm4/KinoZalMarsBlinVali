using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views.Admin
{
    public class BoolToYesNoConverter : IValueConverter
    {
        public static BoolToYesNoConverter Instance { get; } = new BoolToYesNoConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Да" : "Нет";
            }
            return "Нет";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue == "Да";
            }
            return false;
        }
    }
}
