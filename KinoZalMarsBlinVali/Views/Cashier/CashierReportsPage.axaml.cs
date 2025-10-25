using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KinoZalMarsBlinVali;

public partial class CashierReportsPage : UserControl
{
    public CashierReportsPage()
    {
        InitializeComponent();
        Content = new TextBlock { Text = "Страница отчетов", FontSize = 20 };
    }
}