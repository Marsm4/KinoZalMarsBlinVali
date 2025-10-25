using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KinoZalMarsBlinVali;

public partial class CashierReservationsPage : UserControl
{
    public CashierReservationsPage()
    {
        InitializeComponent();
        Content = new TextBlock { Text = "Страница управления бронированиями", FontSize = 20 };
    }
}