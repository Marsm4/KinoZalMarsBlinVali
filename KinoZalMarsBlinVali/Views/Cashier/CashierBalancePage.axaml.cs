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
    public partial class CashierBalancePage : UserControl
    {
        private Customer _selectedCustomer;
        private List<Customer> _allCustomers = new List<Customer>();

        public CashierBalancePage()
        {
            InitializeComponent();
            LoadAllCustomers();
        }

        private void LoadAllCustomers()
        {
            try
            {
                _allCustomers = AppDataContext.DbContext.Customers
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToList();

                CustomersDataGrid.ItemsSource = _allCustomers;
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки клиентов: {ex.Message}");
            }
        }

        private void SearchCustomer_Click(object? sender, RoutedEventArgs e)
        {
            var searchText = SearchCustomerTextBox.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadAllCustomers();
                return;
            }

            try
            {
                var filteredCustomers = _allCustomers
                    .Where(c => (c.Email != null && c.Email.ToLower().Contains(searchText)) ||
                               (c.Phone != null && c.Phone.Contains(searchText)) ||
                               (c.FirstName != null && c.FirstName.ToLower().Contains(searchText)) ||
                               (c.LastName != null && c.LastName.ToLower().Contains(searchText)))
                    .ToList();

                CustomersDataGrid.ItemsSource = filteredCustomers;
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка поиска: {ex.Message}");
            }
        }

        private void ResetSearch_Click(object? sender, RoutedEventArgs e)
        {
            SearchCustomerTextBox.Text = "";
            LoadAllCustomers();
        }

        private void CustomerSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            _selectedCustomer = CustomersDataGrid.SelectedItem as Customer;
            UpdateSelectedCustomerInfo();
        }

        private void UpdateSelectedCustomerInfo()
        {
            if (_selectedCustomer != null)
            {
                SelectedCustomerText.Text = $"{_selectedCustomer.FirstName} {_selectedCustomer.LastName} " +
                                          $"(Email: {_selectedCustomer.Email}, " +
                                          $"Телефон: {_selectedCustomer.Phone}) " +
                                          $"- Баланс: {_selectedCustomer.Balance}₽, " +
                                          $"- Бонусы: {_selectedCustomer.BonusPoints ?? 0}";
            }
            else
            {
                SelectedCustomerText.Text = "Выберите клиента из списка";
            }
        }

        private async void AddBalance_Click(object? sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                await ShowError("Сначала выберите клиента из списка");
                return;
            }

            if (decimal.TryParse(BalanceAmountTextBox.Text, out decimal amount) && amount > 0)
            {
                try
                {
                    _selectedCustomer.Balance += amount;

                    // Создаем запись о пополнении баланса
                    var transaction = new FinancialTransaction
                    {
                        TransactionType = "balance_add",
                        Amount = amount,
                        PaymentMethod = "cash",
                        Description = $"Пополнение баланса клиента {_selectedCustomer.FirstName} {_selectedCustomer.LastName}",
                        EmployeeId = AppDataContext.CurrentUser?.EmployeeId,
                        TransactionTime = DateTime.Now
                    };

                    AppDataContext.DbContext.FinancialTransactions.Add(transaction);
                    await AppDataContext.DbContext.SaveChangesAsync();

                    UpdateSelectedCustomerInfo();
                    await ShowSuccess($"Баланс пополнен на {amount}₽. Новый баланс: {_selectedCustomer.Balance}₽");

                    // Обновляем список
                    LoadAllCustomers();
                }
                catch (Exception ex)
                {
                    await ShowError($"Ошибка пополнения баланса: {ex.Message}");
                }
            }
            else
            {
                await ShowError("Введите корректную положительную сумму");
            }
        }

        private async void RemoveBalance_Click(object? sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                await ShowError("Сначала выберите клиента из списка");
                return;
            }

            if (decimal.TryParse(BalanceAmountTextBox.Text, out decimal amount) && amount > 0)
            {
                try
                {
                    if (amount <= _selectedCustomer.Balance)
                    {
                        _selectedCustomer.Balance -= amount;

                        // Создаем запись о списании с баланса
                        var transaction = new FinancialTransaction
                        {
                            TransactionType = "balance_remove",
                            Amount = amount,
                            PaymentMethod = "balance",
                            Description = $"Списание с баланса клиента {_selectedCustomer.FirstName} {_selectedCustomer.LastName}",
                            EmployeeId = AppDataContext.CurrentUser?.EmployeeId,
                            TransactionTime = DateTime.Now
                        };

                        AppDataContext.DbContext.FinancialTransactions.Add(transaction);
                        await AppDataContext.DbContext.SaveChangesAsync();

                        UpdateSelectedCustomerInfo();
                        await ShowSuccess($"С баланса списано {amount}₽. Новый баланс: {_selectedCustomer.Balance}₽");

                        // Обновляем список
                        LoadAllCustomers();
                    }
                    else
                    {
                        await ShowError($"Недостаточно средств на балансе. Доступно: {_selectedCustomer.Balance}₽");
                    }
                }
                catch (Exception ex)
                {
                    await ShowError($"Ошибка списания с баланса: {ex.Message}");
                }
            }
            else
            {
                await ShowError("Введите корректную положительную сумму");
            }
        }

        private async void ShowBalanceHistory_Click(object? sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                await ShowError("Сначала выберите клиента из списка");
                return;
            }

            try
            {
                var currentUserId = AppDataContext.CurrentUser?.EmployeeId;

                var history = AppDataContext.DbContext.FinancialTransactions
                    .Where(t => t.Description.Contains("баланс") &&
                               t.EmployeeId.HasValue &&
                               t.EmployeeId.Value == currentUserId)
                    .OrderByDescending(t => t.TransactionTime)
                    .Take(10)
                    .ToList();

                var historyText = "Последние операции с балансами:\n";
                foreach (var transaction in history)
                {
                    historyText += $"{transaction.TransactionTime:dd.MM.yyyy HH:mm} - {transaction.Description} - {transaction.Amount}₽\n";
                }

                var dialog = new MessageWindow("История операций", historyText);
                await dialog.ShowDialog((Window)this.VisualRoot);
            }
            catch (Exception ex)
            {
                await ShowError($"Ошибка загрузки истории: {ex.Message}");
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