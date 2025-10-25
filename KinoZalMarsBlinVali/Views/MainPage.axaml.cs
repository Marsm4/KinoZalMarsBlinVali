using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Views;

namespace KinoZalMarsBlinVali.Views
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();

            var currentUser = AppDataContext.CurrentUser;
            if (currentUser != null)
            {
                UserInfoText.Text = $"{currentUser.FirstName} {currentUser.LastName} ({currentUser.Position})";

                // Показываем/скрываем кнопки в зависимости от роли
                bool isAdmin = currentUser.Role == "admin";
                bool isManager = currentUser.Role == "manager";
                bool isCashier = currentUser.Role == "cashier";
                bool isCustomer = currentUser.Role == "customer";

                // Админские кнопки показываем только кассирам (для продажи билетов)
                AdminMoviesBtn.IsVisible = isCashier;
                AdminHallsBtn.IsVisible = isCashier;
                AdminEmployeesBtn.IsVisible = false; // Только в админ-панели
                AdminReportsBtn.IsVisible = isCashier;

                // Для зрителей показываем кнопку профиля
                //CustomerProfileBtn.IsVisible = isCustomer;
            }

            Sessions_Click(null, null);
        }

        private void Sessions_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new SessionsPage();
        }

        private void Tickets_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new TicketsPage();
        }

        private void Movies_Click(object? sender, RoutedEventArgs e)
        {
            // Для кассиров - упрощенное управление фильмами
            MainContentControl.Content = new MoviesPage();
        }

        private void Halls_Click(object? sender, RoutedEventArgs e)
        {
            // Для кассиров - просмотр залов
            MainContentControl.Content = new HallsPage();
        }

        private void Employees_Click(object? sender, RoutedEventArgs e)
        {
            // Эта кнопка не должна быть видна на MainPage
        }

        private void Reports_Click(object? sender, RoutedEventArgs e)
        {
            // Для кассиров - упрощенные отчеты
            MainContentControl.Content = new ReportsPage();
        }

        private void CustomerProfile_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new CustomerProfilePage();
        }

        private void Logout_Click(object? sender, RoutedEventArgs e)
        {
            AppDataContext.CurrentUser = null;

            // Переходим на страницу авторизации
            if (this.VisualRoot is MainWindow mainWindow)
            {
                mainWindow.NavigateTo(new AuthPage());
            }
        }
    }
}