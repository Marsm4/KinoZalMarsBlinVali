using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Interactivity;
using System;
using System.IO;

namespace KinoZalMarsBlinVali.Views
{
    public partial class QrCodeWindow : Window
    {
        public QrCodeWindow(string title, string ticketInfo, string qrCodeBase64)
        {
            InitializeComponent();
            InitializeWindow(title, ticketInfo, qrCodeBase64);
        }

        private void InitializeWindow(string title, string ticketInfo, string qrCodeBase64)
        {
            Title = title;

            if (!string.IsNullOrEmpty(qrCodeBase64))
            {
                try
                {
                    var imageBytes = Convert.FromBase64String(qrCodeBase64);
                    using (var memoryStream = new MemoryStream(imageBytes))
                    {
                        QrCodeImage.Source = new Bitmap(memoryStream);
                    }
                }
                catch (Exception ex)
                {
                    // Можно добавить обработку ошибок
                    QrCodeImage.IsVisible = false;
                }
            }

            TicketInfoText.Text = ticketInfo;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}