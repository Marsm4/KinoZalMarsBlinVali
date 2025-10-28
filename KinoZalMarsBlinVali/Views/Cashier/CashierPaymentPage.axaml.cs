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
    public partial class CashierPaymentPage : UserControl
    {
        private List<Ticket> _reservations = new List<Ticket>();

        public CashierPaymentPage()
        {
            InitializeComponent();
            LoadActiveReservations();
        }

        private void LoadActiveReservations()
        {
            try
            {
                var now = DateTime.Now;

                _reservations = AppDataContext.DbContext.Tickets
                    .Include(t => t.Session)
                        .ThenInclude(s => s.Movie)
                    .Include(t => t.Session.Hall)
                    .Include(t => t.Seat)
                    .Include(t => t.Customer)
                    .Include(t => t.TicketType)
                    .Where(t => t.Status == "reserved" &&
                               t.ReservationExpires > now)
                    .OrderBy(t => t.ReservationExpires)
                    .ToList();

                ReservationsItemsControl.ItemsSource = _reservations.Select(t => new PaymentReservationViewModel(t)).ToList();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки бронирований: {ex.Message}");
            }
        }

        private void SearchReservation_Click(object? sender, RoutedEventArgs e)
        {
            var searchText = SearchTextBox.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadActiveReservations();
                return;
            }

            try
            {
                var filtered = _reservations
                    .Where(t => (t.Customer.Email != null && t.Customer.Email.ToLower().Contains(searchText)) ||
                               t.TicketId.ToString().Contains(searchText))
                    .Select(t => new PaymentReservationViewModel(t))
                    .ToList();

                ReservationsItemsControl.ItemsSource = filtered;
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка поиска: {ex.Message}");
            }
        }

        private async void ProcessPayment_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int ticketId)
            {
                var reservation = _reservations.FirstOrDefault(t => t.TicketId == ticketId);
                if (reservation != null)
                {
                    try
                    {

                        var paymentWindow = new PaymentProcessingWindow(reservation);

                        await paymentWindow.ShowDialog((Window)this.VisualRoot);

                        if (paymentWindow.PaymentSuccess)
                        {
                            LoadActiveReservations();
                             ShowSuccess($"Оплата прошла успешно! Билет #{reservation.TicketId} подтвержден.");
                        }
                    }
                    catch (Exception ex)
                    {
                         ShowError($"Ошибка при обработке оплаты: {ex.Message}");
                    }
                }
                else
                {
                     ShowError("Бронирование не найдено");
                }
            }
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

    public class PaymentReservationViewModel
    {
        public Ticket Ticket { get; set; }
        public string SeatInfo => $"Ряд {Ticket.Seat.RowNumber}, Место {Ticket.Seat.SeatNumber}, Зал: {Ticket.Session.Hall.HallName}";

        public PaymentReservationViewModel(Ticket ticket)
        {
            Ticket = ticket;
        }
    }
}