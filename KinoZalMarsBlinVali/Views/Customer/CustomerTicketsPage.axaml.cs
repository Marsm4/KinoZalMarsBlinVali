using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class CustomerTicketsPage : UserControl
    {
        private List<Ticket> _tickets = new List<Ticket>();

        public CustomerTicketsPage()
        {
            InitializeComponent();
            LoadTickets();
        }

        private void LoadTickets()
        {
            try
            {
                var customerId = AppDataContext.CurrentUser?.EmployeeId; // CustomerId для зрителя

                _tickets = AppDataContext.DbContext.Tickets
                    .Include(t => t.Session)
                        .ThenInclude(s => s.Movie)
                    .Include(t => t.Session.Hall)
                    .Include(t => t.Seat)
                    .Include(t => t.TicketType)
                    .Where(t => t.CustomerId == customerId)
                    .OrderByDescending(t => t.PurchaseTime)
                    .ToList();

                ApplyFilters();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки билетов: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            var filtered = _tickets.AsEnumerable();

            var statusFilter = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (!string.IsNullOrEmpty(statusFilter))
            {
                switch (statusFilter)
                {
                    case "Активные":
                        filtered = filtered.Where(t => t.Status == "sold" && t.Session.StartTime > DateTime.Now);
                        break;
                    case "Забронированные":
                        filtered = filtered.Where(t => t.Status == "reserved" && t.ReservationExpires > DateTime.Now);
                        break;
                    case "Использованные":
                        filtered = filtered.Where(t => t.Status == "used" || t.Session.StartTime < DateTime.Now);
                        break;
                }
            }

            TicketsItemsControl.ItemsSource = filtered.Select(t => new DetailedTicketViewModel(t)).ToList();
        }

        private async void ShowQrCode_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int ticketId)
            {
                var ticket = _tickets.FirstOrDefault(t => t.TicketId == ticketId);
                if (ticket != null)
                {
                    var dialog = new MessageWindow("QR-код",
                        $"Билет #{ticket.TicketId}\n\n" +
                        $"🎬 {ticket.Session.Movie.Title}\n" +
                        $"📅 {ticket.Session.StartTime:dd.MM.yyyy HH:mm}\n" +
                        $"🎭 Зал: {ticket.Session.Hall.HallName}\n" +
                        $"💺 Ряд {ticket.Seat.RowNumber}, Место {ticket.Seat.SeatNumber}\n" +
                        $"💰 {ticket.FinalPrice}₽\n\n" +
                        $"QR-код будет сгенерирован позже");
                    await dialog.ShowDialog((Window)this.VisualRoot);
                }
            }
        }

        private async void CancelTicket_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int ticketId)
            {
                var ticket = _tickets.FirstOrDefault(t => t.TicketId == ticketId);
                if (ticket != null && ticket.Status == "reserved")
                {
                    var dialog = new ConfirmationDialog(
                        "Отмена бронирования",
                        $"Вы уверены, что хотите отменить бронирование?\n\n" +
                        $"🎬 {ticket.Session.Movie.Title}\n" +
                        $"📅 {ticket.Session.StartTime:dd.MM.yyyy HH:mm}\n" +
                        $"💺 Ряд {ticket.Seat.RowNumber}, Место {ticket.Seat.SeatNumber}");

                    await dialog.ShowDialog((Window)this.VisualRoot);

                    if (dialog.DialogResult == true)
                    {
                        try
                        {
                            ticket.Status = "cancelled";
                            await AppDataContext.DbContext.SaveChangesAsync();
                            LoadTickets();
                             ShowSuccess("Бронирование отменено");
                        }
                        catch (Exception ex)
                        {
                             ShowError($"Ошибка отмены: {ex.Message}");
                        }
                    }
                }
                else
                {
                     ShowError("Невозможно отменить оплаченный билет");
                }
            }
        }
        private async void PayTicket_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int ticketId)
            {
                var ticket = _tickets.FirstOrDefault(t => t.TicketId == ticketId);
                if (ticket != null && ticket.Status == "reserved")
                {
                    try
                    {
                        var customer = AppDataContext.DbContext.Customers
                            .FirstOrDefault(c => c.CustomerId == ticket.CustomerId);

                        if (customer == null)
                        {
                            ShowError("Ошибка: клиент не найден");
                            return;
                        }

                        var paymentWindow = new CustomerPaymentWindow(ticket, customer);
                        await paymentWindow.ShowDialog((Window)this.VisualRoot);

                        if (paymentWindow.PaymentSuccess)
                        {
                            int bonusPoints = (int)(ticket.FinalPrice * 0.05m);
                            customer.BonusPoints = (customer.BonusPoints ?? 0) + bonusPoints;

                       
                            AppDataContext.DbContext.SaveChanges();

                            LoadTickets();

                             ShowSuccess($"Билет успешно оплачен!\n" +
                                            $"Начислено бонусных баллов: {bonusPoints}");
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Ошибка при оплате: {ex.Message}");
                    }
                }
            }
        }

        private void Refresh_Click(object? sender, RoutedEventArgs e)
        {
            LoadTickets();
        }

        private async void ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            await dialog.ShowDialog((Window)this.VisualRoot);
        }

        private async void ShowSuccess(string message)
        {
            var dialog = new MessageWindow("Успех", message);
            await dialog.ShowDialog((Window)this.VisualRoot);
        }
    }

    public class DetailedTicketViewModel
    {
        public Ticket Ticket { get; set; }

        public string MovieTitle => Ticket.Session?.Movie?.Title ?? "Неизвестно";
        public string SessionDateTime => $"📅 {Ticket.Session?.StartTime:dd.MM.yyyy} ⏰ {Ticket.Session?.StartTime:HH:mm}";
        public string HallInfo => $"🎭 Зал: {Ticket.Session?.Hall?.HallName ?? "Неизвестно"}";
        public string SeatDetailedInfo => $"💺 Ряд {Ticket.Seat?.RowNumber}, Место {Ticket.Seat?.SeatNumber}";
        public string TicketTypeInfo => $"🎫 {Ticket.TicketType?.TypeName ?? "Стандарт"}";
        public string PriceInfo => $"💰 Стоимость: {Ticket.FinalPrice}₽";
        public bool CanPay => Ticket.Status == "reserved" &&
                      Ticket.ReservationExpires > DateTime.Now;


        public string BookingInfo
        {
            get
            {
                if (Ticket.Status == "reserved")
                    return $"⏳ Бронь действительна до: {Ticket.ReservationExpires:HH:mm}";
                else if (Ticket.Status == "sold")
                    return $"🛒 Куплен: {Ticket.PurchaseTime:dd.MM.yyyy HH:mm}";
                else if (Ticket.Status == "used")
                    return $"✅ Использован";
                else
                    return "";
            }
        }


        public string StatusText => Ticket.Status switch
        {
            "sold" => "ОПЛАЧЕН",
            "reserved" => "ЗАБРОНИРОВАН",
            "used" => "ИСПОЛЬЗОВАН",
            "cancelled" => "ОТМЕНЕН",
            _ => Ticket.Status?.ToUpper() ?? "НЕИЗВЕСТНО"
        };

        public string StatusIcon => Ticket.Status switch
        {
            "sold" => "✅",
            "reserved" => "⏳",
            "used" => "🎬",
            "cancelled" => "❌",
            _ => "❓"
        };

        public IBrush StatusColor => Ticket.Status switch
        {
            "sold" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),   
            "reserved" => new SolidColorBrush(Color.FromRgb(255, 152, 0)), 
            "used" => new SolidColorBrush(Color.FromRgb(158, 158, 158)),   
            "cancelled" => new SolidColorBrush(Color.FromRgb(244, 67, 54)), 
            _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))
        };

        public bool IsActive => (Ticket.Status == "sold" || Ticket.Status == "reserved") &&
                               Ticket.Session.StartTime > DateTime.Now;
        public bool CanCancel => Ticket.Status == "reserved" &&
                               Ticket.ReservationExpires > DateTime.Now;

        public int TicketId => Ticket.TicketId;

        public DetailedTicketViewModel(Ticket ticket)
        {
            Ticket = ticket;
        }
    }
}