using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace KinoZalMarsBlinVali.Views
{
    public partial class MessageWindow : Window
    {
        public bool Result { get; private set; } = false;

        public MessageWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public MessageWindow(string title, string message) : this()
        {
            this.Title = title;
            // ”бедитесь, что у вас есть элементы с именами MessageText и OKButton в XAML
            var messageText = this.FindControl<TextBlock>("MessageText");
            if (messageText != null)
                messageText.Text = message;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }
}