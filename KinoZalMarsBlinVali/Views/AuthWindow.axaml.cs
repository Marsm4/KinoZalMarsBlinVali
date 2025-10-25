using Avalonia.Controls;

namespace KinoZalMarsBlinVali.Views
{
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
            MainContentControl.Content = new AuthPage();
        }
    }
}