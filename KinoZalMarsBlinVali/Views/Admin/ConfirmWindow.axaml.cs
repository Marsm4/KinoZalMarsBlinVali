using Avalonia.Controls;
using Avalonia.Interactivity;

namespace KinoZalMarsBlinVali.Views
{
    public partial class ConfirmWindow : Window
    {
        public ConfirmWindow(string title, string message)
        {
            InitializeComponent();
            Title = title;
            MessageText.Text = message;
        }

        private void Yes_Click(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void No_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}