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

            TicketsItemsControl.ItemsSource = filtered.Select(t => new TicketViewModel(t)).ToList();
        }

        private async void ShowQrCode_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int ticketId)
            {
                var ticket = _tickets.FirstOrDefault(t => t.TicketId == ticketId);
                if (ticket != null)
                {
                    // Показываем QR-код (заглушка)
                    var dialog = new MessageWindow("QR-код",
                        $"Билет на {ticket.Session.Movie.Title}\n" +
                        $"Место: Ряд {ticket.Seat.RowNumber}, Место {ticket.Seat.SeatNumber}\n" +
                        $"Время: {ticket.Session.StartTime:dd.MM.yyyy HH:mm}");
                    await dialog.ShowDialog((Window)this.VisualRoot);
                }
            }
        }

        private async void CancelTicket_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int ticketId)
            {
                var ticket = _tickets.FirstOrDefault(t => t.TicketId == ticketId);
                if (ticket != null)
                {
                    var confirm = new ConfirmWindow("Отмена бронирования",
                        "Вы уверены, что хотите отменить бронирование?");

                    var result = await confirm.ShowDialog<bool>((Window)this.VisualRoot);

                    if (result)
                    {
                        try
                        {
                            AppDataContext.DbContext.Tickets.Remove(ticket);
                            AppDataContext.DbContext.SaveChanges();
                            LoadTickets();
                             ShowSuccess("Бронирование отменено");
                        }
                        catch (Exception ex)
                        {
                             ShowError($"Ошибка отмены: {ex.Message}");
                        }
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

    public class TicketViewModel
    {
        public Ticket Ticket { get; set; }
        public string SeatInfo => $"Ряд {Ticket.Seat.RowNumber}, Место {Ticket.Seat.SeatNumber}";
        public string StatusText => Ticket.Status switch
        {
            "sold" => "Оплачен",
            "reserved" => "Забронирован",
            "used" => "Использован",
            "cancelled" => "Отменен",
            _ => Ticket.Status
        };

        public IBrush StatusColor => Ticket.Status switch
        {
            "sold" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
            "reserved" => new SolidColorBrush(Color.FromRgb(255, 152, 0)),
            "used" => new SolidColorBrush(Color.FromRgb(158, 158, 158)),
            "cancelled" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),
            _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))
        };

        public bool IsActive => Ticket.Status == "sold" && Ticket.Session.StartTime > DateTime.Now;
        public bool CanCancel => Ticket.Status == "reserved" && Ticket.ReservationExpires > DateTime.Now;

        public TicketViewModel(Ticket ticket)
        {
            Ticket = ticket;
        }
    }
}