using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Views;
using System.Linq;

namespace KinoZalMarsBlinVali.Views
{
    public partial class CashierMainPage : UserControl
    {
        public CashierMainPage()
        {
            InitializeComponent();
            // По умолчанию показываем страницу оплаты
            NavigateToPayment_Click(null, null);
        }

        private void NavigateToPayment_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new CashierPaymentPage();
            UpdateActiveButton(sender as Button);
        }

        private void NavigateToReservations_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new CashierReservationsPage();
            UpdateActiveButton(sender as Button);
        }

        private void NavigateToBonus_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new CashierBonusPage();
            UpdateActiveButton(sender as Button);
        }

        private void NavigateToReports_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new CashierReportsPage();
            UpdateActiveButton(sender as Button);
        }
        // В CashierMainPage.xaml.cs добавляем метод
        private void NavigateToBalance_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new CashierBalancePage();
            UpdateActiveButton(sender as Button);
        }
        private void UpdateActiveButton(Button? activeButton)
        {
            try
            {
                var border = this.GetVisualChildren().FirstOrDefault() as Border;
                if (border?.Child != null)
                {
                    foreach (var child in border.Child.GetVisualChildren())
                    {
                        if (child is Button button && button.Classes.Contains("nav-button"))
                        {
                            button.Background = Avalonia.Media.Brushes.Transparent;
                        }
                    }

                    if (activeButton != null)
                    {
                        activeButton.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 86, 179));
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки обновления стилей
            }
        }

        private void Logout_Click(object? sender, RoutedEventArgs e)
        {
            AppDataContext.CurrentUser = null;
            if (this.VisualRoot is MainWindow mainWindow)
            {
                mainWindow.NavigateTo(new AuthPage());
            }
        }
    }
}