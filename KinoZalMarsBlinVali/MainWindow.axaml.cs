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

          
            if (AppDataContext.CurrentUser != null)
            {
               
                if (AppDataContext.CurrentUser.Role == "admin" || AppDataContext.CurrentUser.Role == "manager")
                {
                    var adminWindow = new AdminWindow();
                    adminWindow.Show();
                    this.Close();
                }
                else
                {
                    
                    MainContentControl.Content = new MainPage();
                }
            }
            else
            {
             
                MainContentControl.Content = new AuthPage();
            }
        }

        public void NavigateTo(UserControl page)
        {
            MainContentControl.Content = page;
        }
    }
}