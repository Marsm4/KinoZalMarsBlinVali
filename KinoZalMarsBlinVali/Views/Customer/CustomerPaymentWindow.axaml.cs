using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class CustomerPaymentWindow : Window
    {
        private Ticket _ticket;
        private Customer _customer;
        public bool PaymentSuccess { get; private set; } = false;

        public string MovieTitle => _ticket.Session?.Movie?.Title ?? "Неизвестно";
        public string SessionTime => _ticket.Session?.StartTime.ToString("dd.MM.yyyy HH:mm") ?? "Неизвестно";
        public string SeatInfo => $"Ряд {_ticket.Seat?.RowNumber}, Место {_ticket.Seat?.SeatNumber}";
        public string PriceInfo => $"Сумма к оплате: {_ticket.FinalPrice}₽";

        public CustomerPaymentWindow(Ticket ticket, Customer customer)
        {
            _ticket = ticket;
            _customer = customer;
            InitializeComponent();
            DataContext = this;
            LoadCustomerInfo();
            UpdateFinalPrice();
        }

        private void LoadCustomerInfo()
        {
            BalanceText.Text = $"Баланс: {_customer.Balance}₽";
            BonusPointsText.Text = $"Бонусные баллы: {_customer.BonusPoints ?? 0}";
        }

        private void UpdateFinalPrice()
        {
            if (int.TryParse(BonusPointsUsedTextBox.Text, out int bonusPoints) && bonusPoints > 0)
            {
                var maxBonus = _customer.BonusPoints ?? 0;
                var actualBonus = Math.Min(bonusPoints, maxBonus);
                var discount = actualBonus; 
                var finalPrice = Math.Max(0, _ticket.FinalPrice - discount);

                FinalPriceText.Text = $"Итоговая сумма: {finalPrice}₽ (скидка {discount}₽)";
            }
            else
            {
                FinalPriceText.Text = $"Итоговая сумма: {_ticket.FinalPrice}₽";
            }
        }

        private async void ConfirmPayment_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                int bonusPointsUsed = 0;
                decimal discount = 0;

                if (int.TryParse(BonusPointsUsedTextBox.Text, out int requestedBonus) && requestedBonus > 0)
                {
                    var maxBonus = _customer.BonusPoints ?? 0;
                    bonusPointsUsed = Math.Min(requestedBonus, maxBonus);
                    discount = bonusPointsUsed; // 1 бонус = 1 рубль
                }

                var finalAmount = _ticket.FinalPrice - discount;

                if (_customer.Balance < finalAmount)
                {
                    await ShowError($"Недостаточно средств на балансе. Необходимо: {finalAmount}₽, доступно: {_customer.Balance}₽");
                    return;
                }

                _customer.Balance -= finalAmount;

                if (bonusPointsUsed > 0)
                {
                    _customer.BonusPoints = (_customer.BonusPoints ?? 0) - bonusPointsUsed;
                }

                _ticket.Status = "sold";
                _ticket.PurchaseTime = DateTime.Now;
                _ticket.ReservationExpires = null;
                _ticket.FinalPrice = finalAmount; 


                var transaction = new FinancialTransaction
                {
                    TransactionType = "ticket_sale",
                    Amount = finalAmount,
                    PaymentMethod = "balance",
                    Description = $"Оплата билета с баланса. Бонусы использовано: {bonusPointsUsed}",
                    EmployeeId = null,
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
                await ShowError($"Ошибка оплаты: {ex.Message}");
            }
        }

        private void BonusPointsUsedTextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            UpdateFinalPrice();
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