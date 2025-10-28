using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.VisualTree;
namespace KinoZalMarsBlinVali.Views
{
    public partial class AddBalancePage : UserControl
    {
        public AddBalancePage()
        {
            InitializeComponent();
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            // Возврат на страницу профиля через родительский CustomerMainPage
            if (this.FindAncestorOfType<CustomerMainPage>() is CustomerMainPage mainPage)
            {
                mainPage.NavigateToProfile_Click(null, null);
            }
        }

        private async void ProcessPayment_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                // Базовая валидация
                if (string.IsNullOrWhiteSpace(CardNumberTextBox.Text) ||
                    string.IsNullOrWhiteSpace(ExpiryDateTextBox.Text) ||
                    string.IsNullOrWhiteSpace(CvvTextBox.Text) ||
                    string.IsNullOrWhiteSpace(AmountTextBox.Text))
                {
                    await ShowError("Пожалуйста, заполните все поля");
                    return;
                }

                if (!decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount < 100)
                {
                    await ShowError("Сумма пополнения должна быть не менее 100 рублей");
                    return;
                }

                // Имитация обработки платежа
                var processingDialog = new MessageWindow("Обработка", "Идет обработка платежа...");
                await processingDialog.ShowDialog((Window)this.VisualRoot);

                // Обновляем баланс пользователя
                var customerId = AppDataContext.CurrentUser?.EmployeeId;
                var customer = AppDataContext.DbContext.Customers
                    .FirstOrDefault(c => c.CustomerId == customerId);

                if (customer != null)
                {
                    customer.Balance += amount;

                    // Создаем финансовую транзакцию
                    var transaction = new FinancialTransaction
                    {
                        TransactionType = "balance_topup",
                        Amount = amount,
                        PaymentMethod = "bank_card",
                        Description = $"Пополнение баланса через карту ****{CardNumberTextBox.Text[^4..]}",
                        TransactionTime = DateTime.Now
                    };

                    AppDataContext.DbContext.FinancialTransactions.Add(transaction);
                    AppDataContext.DbContext.SaveChanges();

                    await ShowSuccess($"Баланс успешно пополнен на {amount}₽!\n\nНовый баланс: {customer.Balance}₽");

                    // Возвращаемся на страницу профиля
                    if (this.VisualRoot is MainWindow mainWindow)
                    {
                        mainWindow.NavigateTo(new CustomerProfilePage());
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Ошибка при пополнении баланса: {ex.Message}");
            }
        }

        private async Task ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            await dialog.ShowDialog((Window)this.VisualRoot);
        }

        private async Task ShowSuccess(string message)
        {
            var dialog = new MessageWindow("Успех", message);
            await dialog.ShowDialog((Window)this.VisualRoot);
        }
    }
}