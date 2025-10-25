using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class CashierReservationsPage : UserControl
    {
        private List<Ticket> _allReservations = new List<Ticket>();

        public CashierReservationsPage()
        {
            InitializeComponent();
            LoadReservations();
        }

        private void LoadReservations()
        {
            try
            {
                _allReservations = AppDataContext.DbContext.Tickets
                    .Include(t => t.Session)
                        .ThenInclude(s => s.Movie)
                    .Include(t => t.Session.Hall)
                    .Include(t => t.Seat)
                    .Include(t => t.Customer)
                    .Include(t => t.TicketType)
                    .OrderByDescending(t => t.PurchaseTime)
                    .ToList();

                ApplyFilters();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки бронирований: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allReservations.AsEnumerable();

            // Фильтр по статусу
            var statusFilter = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Все статусы")
            {
                filtered = filtered.Where(t => t.Status == GetStatusFromFilter(statusFilter));
            }

            // Фильтр по дате
            if (DateFilterPicker.SelectedDate.HasValue)
            {
                var selectedDate = DateFilterPicker.SelectedDate.Value.DateTime;
                filtered = filtered.Where(t => t.Session.StartTime.Date == selectedDate.Date);
            }

            // Фильтр по поиску
            var searchText = SearchTextBox.Text?.ToLower() ?? "";
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(t =>
                    (t.Session.Movie.Title != null && t.Session.Movie.Title.ToLower().Contains(searchText)) ||
                    (t.Customer != null && t.Customer.Email != null && t.Customer.Email.ToLower().Contains(searchText)));
            }

            ReservationsDataGrid.ItemsSource = filtered.Select(t => new CashierReservationViewModel(t)).ToList();
        }

        private string GetStatusFromFilter(string filter)
        {
            return filter switch
            {
                "Забронировано" => "reserved",
                "Оплачено" => "sold",
                "Отменено" => "cancelled",
                _ => ""
            };
        }

        private async void ProcessPayment_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int ticketId)
            {
                var reservation = _allReservations.FirstOrDefault(t => t.TicketId == ticketId);
                if (reservation != null && reservation.Status == "reserved")
                {
                    try
                    {
                        var paymentWindow = new PaymentProcessingWindow(reservation);
                        await paymentWindow.ShowDialog((Window)this.VisualRoot);

                        if (paymentWindow.PaymentSuccess)
                        {
                            LoadReservations();
                             ShowSuccess("Оплата прошла успешно!");
                        }
                    }
                    catch (Exception ex)
                    {
                         ShowError($"Ошибка при оплате: {ex.Message}");
                    }
                }
            }
        }

        private async void CancelReservation_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int ticketId)
            {
                var reservation = _allReservations.FirstOrDefault(t => t.TicketId == ticketId);
                if (reservation != null && reservation.Status == "reserved")
                {
                    // Создаем простое окно подтверждения
                    var dialog = new ConfirmationDialog(
                        "Отмена бронирования",
                        $"Вы уверены, что хотите отменить бронирование?\n\n" +
                        $"Фильм: {reservation.Session.Movie.Title}\n" +
                        $"Время: {reservation.Session.StartTime:dd.MM.yyyy HH:mm}\n" +
                        $"Место: Ряд {reservation.Seat.RowNumber}, Место {reservation.Seat.SeatNumber}");

                    await dialog.ShowDialog((Window)this.VisualRoot);

                    if (dialog.DialogResult == true)
                    {
                        try
                        {
                            reservation.Status = "cancelled";
                            await AppDataContext.DbContext.SaveChangesAsync();
                            LoadReservations();
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

        private void SearchReservations_Click(object? sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void Refresh_Click(object? sender, RoutedEventArgs e)
        {
            LoadReservations();
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

    public class CashierReservationViewModel
    {
        public Ticket Ticket { get; set; }

        // Свойства для отображения в DataGrid
        public int TicketId => Ticket.TicketId;
        public string MovieTitle => Ticket.Session?.Movie?.Title ?? "Неизвестно";
        public DateTime StartTime => Ticket.Session.StartTime;
        public string CustomerEmail => Ticket.Customer?.Email ?? "Неизвестно";
        public string SeatInfo => $"Ряд {Ticket.Seat.RowNumber}, Место {Ticket.Seat.SeatNumber}";
        public decimal FinalPrice => Ticket.FinalPrice;
        public string Status => Ticket.Status ?? "Неизвестно";
        public DateTime? ReservationExpires => Ticket.ReservationExpires;

        public bool CanProcessPayment => Ticket.Status == "reserved" &&
                                       Ticket.ReservationExpires > DateTime.Now;
        public bool CanCancel => Ticket.Status == "reserved";

        public CashierReservationViewModel(Ticket ticket)
        {
            Ticket = ticket;
        }
    }
}