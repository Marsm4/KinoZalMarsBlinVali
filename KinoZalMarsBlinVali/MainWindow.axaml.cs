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

            // ѕровер€ем авторизацию пользовател€
            if (AppDataContext.CurrentUser != null)
            {
                // ≈сли пользователь администратор или менеджер - открываем админ панель
                if (AppDataContext.CurrentUser.Role == "admin" || AppDataContext.CurrentUser.Role == "manager")
                {
                    var adminWindow = new AdminWindow();
                    adminWindow.Show();
                    this.Close();
                }
                else
                {
                    // ƒл€ зрителей и кассиров показываем обычную главную страницу
                    MainContentControl.Content = new MainPage();
                }
            }
            else
            {
                // ≈сли пользователь не авторизован, показываем страницу авторизации
                MainContentControl.Content = new AuthPage();
            }
        }

        // ћетод дл€ смены страниц из других контролов
        public void NavigateTo(UserControl page)
        {
            MainContentControl.Content = page;
        }
    }
}