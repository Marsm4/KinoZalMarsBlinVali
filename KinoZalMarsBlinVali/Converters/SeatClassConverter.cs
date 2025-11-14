using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace KinoZalMarsBlinVali.Converters
{
    public class SeatClassConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string seatClass)
            {
                return seatClass;
            }
            return "seat-standard";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}