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
            LoadDefaultReport();
        }

        private void LoadDefaultReport()
        {
            try
            {
                var currentUser = AppDataContext.CurrentUser;

                if (currentUser == null)
                {
                    ShowError("Пользователь не авторизован");
                    return;
                }

                // Общая статистика
                var totalSales = AppDataContext.DbContext.Tickets
                    .Count(t => t.Status == "sold");

                var totalRevenue = AppDataContext.DbContext.Tickets
                    .Where(t => t.Status == "sold")
                    .Sum(t => t.FinalPrice);

                var activeReservations = AppDataContext.DbContext.Tickets
                    .Count(t => t.Status == "reserved" && t.ReservationExpires > DateTime.Now);

                // ИСПРАВЛЕНИЕ: Убираем оператор распространения NULL
                var bonusTransactions = AppDataContext.DbContext.FinancialTransactions
                    .Count(t => t.TransactionType == "bonus_add" &&
                               t.EmployeeId == currentUser.EmployeeId);

                TotalSalesText.Text = totalSales.ToString();
                TotalRevenueText.Text = $"{totalRevenue:0}₽";
                ActiveReservationsText.Text = activeReservations.ToString();
                BonusAddedText.Text = bonusTransactions.ToString();

                // Последние транзакции
                var recentTransactions = AppDataContext.DbContext.FinancialTransactions
                    .Where(t => t.EmployeeId == currentUser.EmployeeId)
                    .OrderByDescending(t => t.TransactionTime)
                    .Take(10)
                    .ToList();

                TransactionsDataGrid.ItemsSource = recentTransactions;
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки отчета: {ex.Message}");
            }
        }

        private void GenerateReport_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var startDate = StartDatePicker.SelectedDate?.DateTime;
                var endDate = EndDatePicker.SelectedDate?.DateTime;
                var currentUser = AppDataContext.CurrentUser;

                if (currentUser == null)
                {
                    ShowError("Пользователь не авторизован");
                    return;
                }

                // Фильтрация по дате
                var ticketsQuery = AppDataContext.DbContext.Tickets.AsQueryable();
                var transactionsQuery = AppDataContext.DbContext.FinancialTransactions
                    .Where(t => t.EmployeeId == currentUser.EmployeeId);

                if (startDate.HasValue)
                {
                    ticketsQuery = ticketsQuery.Where(t => t.PurchaseTime >= startDate);
                    transactionsQuery = transactionsQuery.Where(t => t.TransactionTime >= startDate);
                }

                if (endDate.HasValue)
                {
                    var end = endDate.Value.AddDays(1);
                    ticketsQuery = ticketsQuery.Where(t => t.PurchaseTime < end);
                    transactionsQuery = transactionsQuery.Where(t => t.TransactionTime < end);
                }

                // Обновляем статистику
                var totalSales = ticketsQuery.Count(t => t.Status == "sold");
                var totalRevenue = ticketsQuery.Where(t => t.Status == "sold").Sum(t => t.FinalPrice);
                var activeReservations = AppDataContext.DbContext.Tickets
                    .Count(t => t.Status == "reserved" && t.ReservationExpires > DateTime.Now);
                var bonusTransactions = transactionsQuery.Count(t => t.TransactionType == "bonus_add");

                TotalSalesText.Text = totalSales.ToString();
                TotalRevenueText.Text = $"{totalRevenue:0}₽";
                ActiveReservationsText.Text = activeReservations.ToString();
                BonusAddedText.Text = bonusTransactions.ToString();

                // Обновляем транзакции
                var filteredTransactions = transactionsQuery
                    .OrderByDescending(t => t.TransactionTime)
                    .Take(10)
                    .ToList();

                TransactionsDataGrid.ItemsSource = filteredTransactions;

                ShowSuccess("Отчет сформирован успешно");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка формирования отчета: {ex.Message}");
            }
        }

        private async void PrintReport_Click(object? sender, RoutedEventArgs e)
        {
            // Заглушка для печати
             ShowSuccess("Функция печати будет реализована в будущем");
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
}