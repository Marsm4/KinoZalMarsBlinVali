using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace KinoZalMarsBlinVali.Views
{
    public class SeatTypeToClassesConverter : IValueConverter
    {
        public static SeatTypeToClassesConverter Instance { get; } = new SeatTypeToClassesConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string seatType)
            {
                return seatType; 
            }
            return "standard";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}