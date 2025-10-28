using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class SessionTicketsPage : UserControl
    {
        private Session _session;
        private List<Ticket> _tickets = new List<Ticket>();

        public SessionTicketsPage(Session session)
        {
            _session = session;
            InitializeComponent();
            TitleText.Text = $"Билеты сеанса: {session.Movie.Title} - {session.StartTime:dd.MM.yyyy HH:mm}";
            LoadTickets();
        }

        private void LoadTickets()
        {
            try
            {
                _tickets = AppDataContext.DbContext.Tickets
                    .Where(t => t.SessionId == _session.SessionId)
                    .Include(t => t.Seat)
                    .Include(t => t.TicketType)
                    .Include(t => t.Customer)
                    .OrderBy(t => t.Seat.RowNumber)
                    .ThenBy(t => t.Seat.SeatNumber)
                    .ToList();

                UpdateTicketsDataGrid();
                UpdateStatistics();
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка загрузки билетов: {ex.Message}");
            }
        }

        private void UpdateTicketsDataGrid()
        {
            TicketsDataGrid.ItemsSource = _tickets;
        }

        private void UpdateStatistics()
        {
            try
            {

                var totalSeats = AppDataContext.DbContext.HallSeats
                    .Count(s => s.HallId == _session.HallId && s.IsActive == true);


                var soldTickets = _tickets.Count(t => t.Status == "sold" || t.Status == "used");
                var availableSeats = totalSeats - soldTickets;


                var revenue = _tickets
                    .Where(t => t.Status == "sold" || t.Status == "used")
                    .Sum(t => t.FinalPrice);

                TotalSeatsText.Text = totalSeats.ToString();
                SoldTicketsText.Text = soldTickets.ToString();
                AvailableSeatsText.Text = availableSeats.ToString();
                RevenueText.Text = $"{revenue}₽";
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка расчета статистики: {ex.Message}");
            }
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentControl &&
                contentControl.Parent is Grid grid &&
                grid.Parent is AdminPanelPage adminPanel)
            {
                adminPanel.MainContentControl.Content = new AdminSessionsPage();
            }
        }

        private async Task ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            await dialog.ShowDialog((Window)this.VisualRoot);
        }
    }
}