using QRCoder;
using System;
using System.Drawing;
using System.IO;

namespace KinoZalMarsBlinVali.Models
{
    public partial class Ticket
    {
        public string GenerateQrCodeData()
        {
            var ticketData = $"KINO_TICKET#{TicketId}#{SessionId}#{SeatId}#{CustomerId}";

            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(ticketData, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrCodeData))
            {
                var qrCodeBytes = qrCode.GetGraphic(20);
                return Convert.ToBase64String(qrCodeBytes);
            }
        }

        public string GetQrCodeDisplayData()
        {
            return $"Билет #{TicketId}\n\n" +
                   $"🎬 {Session?.Movie?.Title ?? "Неизвестно"}\n" +
                   $"📅 {Session?.StartTime:dd.MM.yyyy HH:mm}\n" +
                   $"🎭 Зал: {Session?.Hall?.HallName ?? "Неизвестно"}\n" +
                   $"💺 Ряд {Seat?.RowNumber}, Место {Seat?.SeatNumber}\n" +
                   $"💰 {FinalPrice}₽\n" +
                   $"{(Status == "sold" ? "✅ ОПЛАЧЕН" : Status == "reserved" ? "⏳ ЗАБРОНИРОВАН" : "❓")}";
        }
    }
}