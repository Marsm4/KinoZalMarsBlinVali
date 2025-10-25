using Avalonia;
using Avalonia.Controls;

namespace KinoZalMarsBlinVali.Views
{
    public static class SeatProperties
    {
        public static readonly AttachedProperty<string> SeatTypeProperty =
            AvaloniaProperty.RegisterAttached<Button, string>("SeatType", typeof(SeatProperties));

        public static void SetSeatType(Button element, string value)
        {
            element.SetValue(SeatTypeProperty, value);
        }

        public static string GetSeatType(Button element)
        {
            return element.GetValue(SeatTypeProperty);
        }
    }
}