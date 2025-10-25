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

            // Показываем информацию о текущем пользователе
            var currentUser = AppDataContext.CurrentUser;
            if (currentUser != null)
            {
                UserInfoText.Text = $"{currentUser.FirstName} {currentUser.LastName} ({currentUser.Position})";

                // Показываем/скрываем админские кнопки в зависимости от роли
                bool isAdmin = currentUser.Role == "admin";
                bool isManager = currentUser.Role == "manager";
                bool isCashier = currentUser.Role == "cashier";

                AdminMoviesBtn.IsVisible = isAdmin || isManager;
                AdminHallsBtn.IsVisible = isAdmin || isManager;
                AdminEmployeesBtn.IsVisible = isAdmin;
                AdminReportsBtn.IsVisible = isAdmin || isManager;
            }

            // По умолчанию показываем страницу сеансов
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
            MainContentControl.Content = new MoviesPage();
        }

        private void Halls_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new HallsPage();
        }

        private void Employees_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new EmployeesPage();
        }

        private void Reports_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new ReportsPage();
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