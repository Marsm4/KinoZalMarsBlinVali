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
    public partial class CashierBonusPage : UserControl
    {
        private Customer _selectedCustomer;
        private List<Customer> _allCustomers = new List<Customer>();

        public CashierBonusPage()
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
                                          $"- Бонусы: {_selectedCustomer.BonusPoints ?? 0}";
            }
            else
            {
                SelectedCustomerText.Text = "Выберите клиента из списка";
            }
        }

        private async void AddBonus_Click(object? sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                await ShowError("Сначала выберите клиента из списка");
                return;
            }

            if (int.TryParse(BonusPointsTextBox.Text, out int points) && points > 0)
            {
                try
                {
                    _selectedCustomer.BonusPoints = (_selectedCustomer.BonusPoints ?? 0) + points;
                    await AppDataContext.DbContext.SaveChangesAsync();
                    var transaction = new FinancialTransaction
                    {
                        TransactionType = "bonus_add",
                        Amount = 0,
                        PaymentMethod = "bonus",
                        Description = $"Начисление бонусных баллов: {points}",
                        EmployeeId = AppDataContext.CurrentUser?.EmployeeId,
                        TransactionTime = DateTime.Now
                    };
                    AppDataContext.DbContext.FinancialTransactions.Add(transaction);
                    await AppDataContext.DbContext.SaveChangesAsync();

                    UpdateSelectedCustomerInfo();
                    await ShowSuccess($"Начислено {points} бонусных баллов клиенту {_selectedCustomer.FirstName} {_selectedCustomer.LastName}");

                    LoadAllCustomers();
                }
                catch (Exception ex)
                {
                    await ShowError($"Ошибка начисления бонусов: {ex.Message}");
                }
            }
            else
            {
                await ShowError("Введите корректное положительное количество баллов");
            }
        }

        private async void RemoveBonus_Click(object? sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                await ShowError("Сначала выберите клиента из списка");
                return;
            }

            if (int.TryParse(BonusPointsTextBox.Text, out int points) && points > 0)
            {
                try
                {
                    var currentBonus = _selectedCustomer.BonusPoints ?? 0;
                    if (points <= currentBonus)
                    {
                        _selectedCustomer.BonusPoints = currentBonus - points;


                        var transaction = new FinancialTransaction
                        {
                            TransactionType = "bonus_remove",
                            Amount = 0,
                            PaymentMethod = "bonus",
                            Description = $"Списание бонусных баллов: {points}",
                            EmployeeId = AppDataContext.CurrentUser?.EmployeeId,
                            TransactionTime = DateTime.Now
                        };
                        AppDataContext.DbContext.FinancialTransactions.Add(transaction);

                        await AppDataContext.DbContext.SaveChangesAsync();
                        UpdateSelectedCustomerInfo();
                        await ShowSuccess($"Списано {points} бонусных баллов у клиента {_selectedCustomer.FirstName} {_selectedCustomer.LastName}");

                        LoadAllCustomers();
                    }
                    else
                    {
                        await ShowError($"Недостаточно бонусных баллов для списания. Доступно: {currentBonus}");
                    }
                }
                catch (Exception ex)
                {
                    await ShowError($"Ошибка списания бонусов: {ex.Message}");
                }
            }
            else
            {
                await ShowError("Введите корректное положительное количество баллов");
            }
        }

        private async void ShowBonusHistory_Click(object? sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                await ShowError("Сначала выберите клиента из списка");
                return;
            }

            try
            {
                var currentUserId = AppDataContext.CurrentUser?.EmployeeId;

                if (currentUserId.HasValue)
                {
                    var history = AppDataContext.DbContext.FinancialTransactions
                        .Where(t => t.Description.Contains("бонус") &&
                                   t.EmployeeId == currentUserId.Value)
                        .OrderByDescending(t => t.TransactionTime)
                        .Take(10)
                        .ToList();

                    var historyText = "Последние операции с бонусами:\n";
                    foreach (var transaction in history)
                    {
                        historyText += $"{transaction.TransactionTime:dd.MM.yyyy HH:mm} - {transaction.Description}\n";
                    }

                    var dialog = new MessageWindow("История операций", historyText);
                    await dialog.ShowDialog((Window)this.VisualRoot);
                }
                else
                {
                    await ShowError("Не удалось определить пользователя");
                }
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