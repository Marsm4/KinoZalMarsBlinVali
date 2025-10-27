using Avalonia.Controls;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Views;

namespace KinoZalMarsBlinVali
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Проверяем авторизацию пользователя
            if (AppDataContext.CurrentUser != null)
            {
                // Если пользователь администратор или менеджер - открываем админ панель
                if (AppDataContext.CurrentUser.Role == "admin" || AppDataContext.CurrentUser.Role == "manager")
                {
                    var adminWindow = new AdminWindow();
                    adminWindow.Show();
                    this.Close();
                }
                else
                {
                    // Для зрителей и кассиров показываем обычную главную страницу
                    MainContentControl.Content = new MainPage();
                }
            }
            else
            {
                // Если пользователь не авторизован, показываем страницу авторизации
                MainContentControl.Content = new AuthPage();
            }
        }

        public void NavigateTo(UserControl page)
        {
            MainContentControl.Content = page;
        }
    }
}