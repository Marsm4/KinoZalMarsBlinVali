using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace KinoZalMarsBlinVali.Converters
{
    public class StatusClassConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "active" => new SolidColorBrush(Color.Parse("#00A651")),
                    "pending" => new SolidColorBrush(Color.Parse("#FF9800")),
                    "used" => new SolidColorBrush(Color.Parse("#6C757D")),
                    _ => new SolidColorBrush(Color.Parse("#FF9800"))
                };
            }
            return new SolidColorBrush(Color.Parse("#FF9800"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}