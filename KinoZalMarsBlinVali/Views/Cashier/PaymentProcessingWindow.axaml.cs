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
        public bool PaymentSuccess { get; private set; } = false;

        public string MovieTitle => _ticket.Session?.Movie?.Title ?? "Неизвестно";
        public string SessionTime => _ticket.Session?.StartTime.ToString("dd.MM.yyyy HH:mm") ?? "Неизвестно";
        public string SeatInfo => $"Ряд {_ticket.Seat?.RowNumber}, Место {_ticket.Seat?.SeatNumber}";
        public string CustomerInfo => $"{_ticket.Customer?.FirstName} {_ticket.Customer?.LastName} ({_ticket.Customer?.Email})";
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
            CurrentBonusText.Text = $"Текущие бонусы клиента: {_customer?.BonusPoints ?? 0}";
        }

        private string GetPaymentMethod()
        {
            var selectedItem = PaymentMethodComboBox.SelectedItem as ComboBoxItem;
            return selectedItem?.Content?.ToString() switch
            {
                "💳 Банковская карта" => "card",
                "💵 Наличные" => "cash",
                "📱 Онлайн-оплата" => "online",
                _ => "card"
            };
        }

        private async void ConfirmPayment_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(BonusPointsTextBox.Text, out int bonusPoints) || bonusPoints < 0)
                {
                    await ShowError("Введите корректное количество бонусных баллов");
                    return;
                }

                _ticket.Status = "sold";
                _ticket.PurchaseTime = DateTime.Now;
                _ticket.ReservationExpires = null;

 
                if (bonusPoints > 0 && _customer != null)
                {
                    _customer.BonusPoints = (_customer.BonusPoints ?? 0) + bonusPoints;
                }

                var paymentMethod = GetPaymentMethod();

                var transaction = new FinancialTransaction
                {
                    TransactionType = "ticket_sale",
                    Amount = _ticket.FinalPrice,
                    PaymentMethod = paymentMethod,
                    Description = $"Оплата билета на {MovieTitle} ({SeatInfo})",
                    EmployeeId = AppDataContext.CurrentUser?.EmployeeId,
                    TicketId = _ticket.TicketId,
                    TransactionTime = DateTime.Now
                };

                AppDataContext.DbContext.FinancialTransactions.Add(transaction);
                await AppDataContext.DbContext.SaveChangesAsync();

                PaymentSuccess = true;
                Close();
            }
            catch (Exception ex)
            {
                await ShowError($"Ошибка обработки оплаты: {ex.Message}");
            }
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            PaymentSuccess = false;
            Close();
        }

        private async Task ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            await dialog.ShowDialog(this);
        }
    }
}