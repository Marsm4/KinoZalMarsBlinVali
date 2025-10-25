using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class PaymentProcessingWindow : Window
    {
        private Ticket _ticket;
        private Customer _customer;

        public string MovieTitle => _ticket.Session.Movie.Title;
        public string SessionTime => _ticket.Session.StartTime.ToString("dd.MM.yyyy HH:mm");
        public string SeatInfo => $"Ряд {_ticket.Seat.RowNumber}, Место {_ticket.Seat.SeatNumber}";
        public string CustomerInfo => $"{_ticket.Customer.FirstName} {_ticket.Customer.LastName} ({_ticket.Customer.Email})";
        public decimal TotalPrice => _ticket.FinalPrice;

        public PaymentProcessingWindow(Ticket ticket)
        {
            _ticket = ticket;
            _customer = ticket.Customer;
            InitializeComponent();
            DataContext = this;
            LoadCustomerBonusInfo();
        }

        private void LoadCustomerBonusInfo()
        {
            CurrentBonusText.Text = $"Текущие бонусы клиента: {_customer.BonusPoints ?? 0}";
        }

        private async void ConfirmPayment_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                // Обновляем статус билета
                _ticket.Status = "sold";
                _ticket.PurchaseTime = DateTime.Now;
                _ticket.ReservationExpires = null;

                // Начисляем бонусные баллы
                if (int.TryParse(BonusPointsTextBox.Text, out int bonusPoints) && bonusPoints > 0)
                {
                    _customer.BonusPoints = (_customer.BonusPoints ?? 0) + bonusPoints;
                }

                // Создаем финансовую транзакцию
                var transaction = new FinancialTransaction
                {
                    TransactionType = "ticket_sale",
                    Amount = _ticket.FinalPrice,
                    PaymentMethod = "card", // или cash, в зависимости от выбора
                    Description = $"Оплата билета на {_ticket.Session.Movie.Title}",
                    EmployeeId = AppDataContext.CurrentUser?.EmployeeId,
                    TicketId = _ticket.TicketId,
                    TransactionTime = DateTime.Now
                };

                AppDataContext.DbContext.FinancialTransactions.Add(transaction);
                await AppDataContext.DbContext.SaveChangesAsync();

                Close(true);
            }
            catch (Exception ex)
            {
                var dialog = new MessageWindow("Ошибка", $"Ошибка обработки оплаты: {ex.Message}");
                await dialog.ShowDialog(this);
            }
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}