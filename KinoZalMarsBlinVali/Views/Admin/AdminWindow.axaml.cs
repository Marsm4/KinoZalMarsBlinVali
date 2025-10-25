using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using KinoZalMarsBlinVali.Data;

namespace KinoZalMarsBlinVali.Views
{
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();

            var currentUser = AppDataContext.CurrentUser;
            if (currentUser != null)
            {
                UserInfoText.Text = $"{currentUser.FirstName} {currentUser.LastName} ({currentUser.Position})";
            }

            // ѕо умолчанию показываем страницу фильмов
            Movies_Click(null, null);
        }

        private void Movies_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new AdminMoviesPage();
        }

        private void Sessions_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new AdminSessionsPage();
        }

        private void Halls_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new AdminHallsPage();
        }

        private void Employees_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new AdminEmployeesPage();
        }

        private void Reports_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new AdminReportsPage();
        }

        private void Logout_Click(object? sender, RoutedEventArgs e)
        {
            AppDataContext.CurrentUser = null;
            var authWindow = new AuthWindow();
            authWindow.Show();
            this.Close();
        }
    }
}