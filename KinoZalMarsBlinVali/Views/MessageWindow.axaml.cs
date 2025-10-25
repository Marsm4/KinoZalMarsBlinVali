using Avalonia.Controls;
using Avalonia.Interactivity;

namespace KinoZalMarsBlinVali.Views
{
    public partial class MessageWindow : Window
    {
        public MessageWindow(string title, string message)
        {
            InitializeComponent();
            Title = title;
            MessageText.Text = message;
        }

        private void OK_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}