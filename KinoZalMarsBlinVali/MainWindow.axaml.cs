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
                // Показываем главную страницу для авторизованного пользователя
                MainContentControl.Content = new MainPage();
            }
            else
            {
                // Если пользователь не авторизован, показываем страницу авторизации
                MainContentControl.Content = new AuthPage();
            }
        }

        // Метод для смены страниц из других контролов
        public void NavigateTo(UserControl page)
        {
            MainContentControl.Content = page;
        }
    }
}