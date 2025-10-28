using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Views;
using System.Linq;

namespace KinoZalMarsBlinVali.Views
{
    public partial class CustomerMainPage : UserControl
    {
        public CustomerMainPage()
        {
            InitializeComponent();

            NavigateToSessions_Click(null, null);
        }

        private void NavigateToSessions_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new CustomerSessionsPage();
            UpdateActiveButton(sender as Button);
        }

        private void NavigateToMyTickets_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new CustomerTicketsPage();
            UpdateActiveButton(sender as Button);
        }

        public void NavigateToProfile_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new CustomerProfilePage();
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
    
            }
        }
        public void NavigateToAddBalance()
        {
            MainContentControl.Content = new AddBalancePage();
            ResetActiveButtons();
        }
        private void ResetActiveButtons()
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
                }
            }
            catch
            {
                // Игнорируем ошибки
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