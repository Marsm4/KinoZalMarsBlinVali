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
                    (t.Customer.Email != null && t.Customer.Email.ToLower().Contains(searchText)));
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
                    var paymentWindow = new PaymentProcessingWindow(reservation);
                    var result = await paymentWindow.ShowDialog<bool>((Window)this.VisualRoot);

                    if (result)
                    {
                        LoadReservations();
                         ShowSuccess("Оплата прошла успешно!");
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
                    var confirm = new ConfirmWindow("Отмена бронирования",
                        "Вы уверены, что хотите отменить это бронирование?");

                    var result = await confirm.ShowDialog<bool>((Window)this.VisualRoot);

                    if (result)
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

    // Переименуем класс, чтобы избежать конфликта
    public class CashierReservationViewModel
    {
        public Ticket Ticket { get; set; }
        public string SeatInfo => $"Ряд {Ticket.Seat.RowNumber}, Место {Ticket.Seat.SeatNumber}";
        public bool CanProcessPayment => Ticket.Status == "reserved" && Ticket.ReservationExpires > DateTime.Now;
        public bool CanCancel => Ticket.Status == "reserved";

        public CashierReservationViewModel(Ticket ticket)
        {
            Ticket = ticket;
        }
    }
}