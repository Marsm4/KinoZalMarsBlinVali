using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class CashierReportsPage : UserControl
    {
        public CashierReportsPage()
        {
            InitializeComponent();
            InitializeDefaultDates();
            LoadDefaultReport();
        }

        private void InitializeDefaultDates()
        {
            StartDatePicker.SelectedDate = DateTime.Today.AddDays(-30);
            EndDatePicker.SelectedDate = DateTime.Today.AddDays(1);
        }

        private void LoadDefaultReport()
        {
            try
            {
                GenerateReport(null, null);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки отчета: {ex.Message}");
            }
        }

        private void GenerateReport_Click(object? sender, RoutedEventArgs e)
        {
            GenerateReport(sender, e);
        }

        private void GenerateReport(object? sender, RoutedEventArgs e)
        {
            try
            {
                var currentUser = AppDataContext.CurrentUser;

                if (currentUser == null)
                {
                    ShowError("Пользователь не авторизован");
                    return;
                }

                DateTime? startDate = null;
                DateTime? endDate = null;

                // Определяем период на основе выбранного типа отчета
                if (AllTimeReport?.IsChecked == true)
                {
                    // За все время - без фильтрации по дате
                }
                else if (TodayReport?.IsChecked == true)
                {
                    startDate = DateTime.Today;
                    endDate = DateTime.Today.AddDays(1);
                }
                else if (WeekReport?.IsChecked == true)
                {
                    startDate = DateTime.Today.AddDays(-7);
                    endDate = DateTime.Today.AddDays(1);
                }
                else if (MonthReport?.IsChecked == true)
                {
                    startDate = DateTime.Today.AddDays(-30);
                    endDate = DateTime.Today.AddDays(1);
                }
                else if (PeriodReport?.IsChecked == true)
                {
                    startDate = StartDatePicker.SelectedDate?.DateTime;
                    endDate = EndDatePicker.SelectedDate?.DateTime;

                    if (startDate.HasValue && endDate.HasValue)
                    {
                        endDate = endDate.Value.AddDays(1); // Включаем конечную дату
                    }
                }

                // Основные запросы
                var ticketsQuery = AppDataContext.DbContext.Tickets
                    .Include(t => t.Seat)
                    .Include(t => t.Session)
                    .Where(t => t.EmployeeId == currentUser.EmployeeId)
                    .AsQueryable();

                var transactionsQuery = AppDataContext.DbContext.FinancialTransactions
                    .Where(t => t.EmployeeId == currentUser.EmployeeId)
                    .AsQueryable();

                var servicesQuery = AppDataContext.DbContext.TicketServices
                    .Include(ts => ts.Service)
                    .Include(ts => ts.Ticket)
                    .Where(ts => ts.Ticket.EmployeeId == currentUser.EmployeeId)
                    .AsQueryable();

                // Применяем фильтрацию по дате
                if (startDate.HasValue)
                {
                    ticketsQuery = ticketsQuery.Where(t => t.PurchaseTime >= startDate ||
                                                          t.ReservationTime >= startDate);
                    transactionsQuery = transactionsQuery.Where(t => t.TransactionTime >= startDate);
                    servicesQuery = servicesQuery.Where(ts => ts.Ticket.PurchaseTime >= startDate ||
                                                             ts.Ticket.ReservationTime >= startDate);
                }

                if (endDate.HasValue)
                {
                    ticketsQuery = ticketsQuery.Where(t => (t.PurchaseTime < endDate ||
                                                           t.ReservationTime < endDate) ||
                                                           t.PurchaseTime == null);
                    transactionsQuery = transactionsQuery.Where(t => t.TransactionTime < endDate);
                    servicesQuery = servicesQuery.Where(ts => (ts.Ticket.PurchaseTime < endDate ||
                                                              ts.Ticket.ReservationTime < endDate) ||
                                                              ts.Ticket.PurchaseTime == null);
                }

                // Применяем дополнительные фильтры
                if (IncludeSoldTickets?.IsChecked == false)
                {
                    ticketsQuery = ticketsQuery.Where(t => t.Status != "sold" && t.Status != "used");
                }

                if (IncludeReservations?.IsChecked == false)
                {
                    ticketsQuery = ticketsQuery.Where(t => t.Status != "reserved");
                }

                // Вычисляем статистику - исправленные строки
                var totalSales = ticketsQuery.Count(t => t.Status == "sold" || t.Status == "used");

                var totalRevenue = ticketsQuery
                    .Where(t => t.Status == "sold" || t.Status == "used")
                    .Sum(t => t.FinalPrice);

                var activeReservations = ticketsQuery
                    .Count(t => t.Status == "reserved" && t.ReservationExpires > DateTime.Now);

                var bonusTransactions = transactionsQuery
                    .Count(t => t.TransactionType == "bonus_add");

                var servicesRevenue = servicesQuery
                    .Where(ts => ts.Ticket.Status == "sold" || ts.Ticket.Status == "used")
                    .Sum(ts => (ts.Quantity ?? 1) * ts.Service.Price);

                // Детальная статистика
                var vipTickets = ticketsQuery
                    .Count(t => (t.Status == "sold" || t.Status == "used") &&
                               t.Seat.SeatType == "vip");

                var standardTickets = ticketsQuery
                    .Count(t => (t.Status == "sold" || t.Status == "used") &&
                               t.Seat.SeatType == "standard");

                var averageTicket = totalSales > 0 ? totalRevenue / totalSales : 0m;
                var cancelledTickets = ticketsQuery.Count(t => t.Status == "cancelled");

                var transactionsCount = transactionsQuery.Count();

                // Обновляем UI
                UpdateStatistics(totalSales, totalRevenue, activeReservations, bonusTransactions,
                               servicesRevenue, vipTickets, standardTickets, averageTicket,
                               cancelledTickets, transactionsCount);

                // Загружаем транзакции
                var filteredTransactions = transactionsQuery
                    .OrderByDescending(t => t.TransactionTime)
                    .Take(50)
                    .ToList();

                TransactionsDataGrid.ItemsSource = filteredTransactions;

                ShowSuccess("Отчет сформирован успешно");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка формирования отчета: {ex.Message}");
            }
        }

        private void UpdateStatistics(int totalSales, decimal totalRevenue, int activeReservations,
                                    int bonusTransactions, decimal servicesRevenue, int vipTickets,
                                    int standardTickets, decimal averageTicket, int cancelledTickets,
                                    int transactionsCount)
        {
            TotalSalesText.Text = totalSales.ToString("N0");
            TotalRevenueText.Text = $"{totalRevenue:N0}₽";
            ActiveReservationsText.Text = activeReservations.ToString("N0");
            BonusAddedText.Text = bonusTransactions.ToString("N0");
            ServicesRevenueText.Text = $"{servicesRevenue:N0}₽";
            VipTicketsText.Text = vipTickets.ToString("N0");
            StandardTicketsText.Text = standardTickets.ToString("N0");
            AverageTicketText.Text = $"{averageTicket:N0}₽";
            CancelledTicketsText.Text = cancelledTickets.ToString("N0");
            TransactionsCountText.Text = $"Всего: {transactionsCount:N0}";
        }

        private void ResetFilters_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                AllTimeReport.IsChecked = true;
                StartDatePicker.SelectedDate = DateTime.Today.AddDays(-30);
                EndDatePicker.SelectedDate = DateTime.Today.AddDays(1);
                IncludeSoldTickets.IsChecked = true;
                IncludeReservations.IsChecked = true;
                IncludeServices.IsChecked = true;

                LoadDefaultReport();
                ShowSuccess("Фильтры сброшены");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сброса фильтров: {ex.Message}");
            }
        }

        private async void PrintReport_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                // Здесь можно добавить логику экспорта в PDF или печати
                //var printDialog = new PrintDialog();
                // Реализация печати будет добавлена позже
                ShowSuccess("Функция печати будет реализована в будущем обновлении");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка печати: {ex.Message}");
            }
        }


        private async void ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            try
            {
                var owner = (Window)this.VisualRoot;
                if (owner != null && owner.IsVisible)
                {
                    await dialog.ShowDialog(owner);
                }
                else
                {
                    // Вместо ShowDialog с null, просто показываем окно
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                     dialog.Show();
                }
            }
            catch
            {
                // Если всё равно ошибка, просто создаем и показываем окно
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                 dialog.Show();
            }
        }

        private async void ShowSuccess(string message)
        {
            var dialog = new MessageWindow("Успех", message);
            try
            {
                var owner = (Window)this.VisualRoot;
                if (owner != null && owner.IsVisible)
                {
                    await dialog.ShowDialog(owner);
                }
                else
                {
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                     dialog.Show();
                }
            }
            catch
            {
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                 dialog.Show();
            }
        }


    }
}