using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class CashierBonusPage : UserControl
    {
        private Customer _currentCustomer;

        public CashierBonusPage()
        {
            InitializeComponent();
        }

        private async void SearchCustomer_Click(object? sender, RoutedEventArgs e)
        {
            var searchText = SearchCustomerTextBox.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
            {
                await ShowError("Введите email или телефон клиента");
                return;
            }

            try
            {
                _currentCustomer = AppDataContext.DbContext.Customers
                    .FirstOrDefault(c => c.Email.ToLower().Contains(searchText) ||
                                       c.Phone.Contains(searchText));

                if (_currentCustomer != null)
                {
                    UpdateCustomerInfo();
                }
                else
                {
                    await ShowError("Клиент не найден");
                    ClearCustomerInfo();
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Ошибка поиска: {ex.Message}");
            }
        }

        private void UpdateCustomerInfo()
        {
            CustomerInfoText.Text = $"{_currentCustomer.FirstName} {_currentCustomer.LastName} ({_currentCustomer.Email})";
            BonusInfoText.Text = $"Текущие бонусные баллы: {_currentCustomer.BonusPoints ?? 0}";
        }

        private void ClearCustomerInfo()
        {
            CustomerInfoText.Text = "";
            BonusInfoText.Text = "";
            _currentCustomer = null;
        }

        private async void AddBonus_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentCustomer == null)
            {
                await ShowError("Сначала найдите клиента");
                return;
            }

            if (int.TryParse(BonusPointsTextBox.Text, out int points) && points > 0)
            {
                _currentCustomer.BonusPoints = (_currentCustomer.BonusPoints ?? 0) + points;
                await AppDataContext.DbContext.SaveChangesAsync();
                UpdateCustomerInfo();
                await ShowSuccess($"Начислено {points} бонусных баллов");
            }
            else
            {
                await ShowError("Введите корректное количество баллов");
            }
        }

        private async void RemoveBonus_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentCustomer == null)
            {
                await ShowError("Сначала найдите клиента");
                return;
            }

            if (int.TryParse(BonusPointsTextBox.Text, out int points) && points > 0)
            {
                var currentBonus = _currentCustomer.BonusPoints ?? 0;
                if (points <= currentBonus)
                {
                    _currentCustomer.BonusPoints = currentBonus - points;
                    await AppDataContext.DbContext.SaveChangesAsync();
                    UpdateCustomerInfo();
                    await ShowSuccess($"Списано {points} бонусных баллов");
                }
                else
                {
                    await ShowError("Недостаточно бонусных баллов для списания");
                }
            }
            else
            {
                await ShowError("Введите корректное количество баллов");
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