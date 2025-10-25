using Avalonia.Controls;
using Avalonia.Interactivity;

namespace KinoZalMarsBlinVali.Views
{
    public partial class ConfirmationDialog : Window
    {
        public bool DialogResult { get; private set; } = false;
        public ConfirmationDialog()
        {
            InitializeComponent();
         
        }

        public ConfirmationDialog(string title, string message)
        {
            InitializeComponent();
            Title = title;
            MessageText.Text = message;
        }

        private void YesButton_Click(object? sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object? sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}