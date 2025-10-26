using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class CustomerProfilePage : UserControl
    {
        private Customer _customer;

        public CustomerProfilePage()
        {
            InitializeComponent();
            LoadCustomerData();
        }

        private void LoadCustomerData()
        {
            try
            {
                var customerId = AppDataContext.CurrentUser?.EmployeeId;

                _customer = AppDataContext.DbContext.Customers
                    .FirstOrDefault(c => c.CustomerId == customerId);

                if (_customer != null)
                {
                    FirstNameTextBox.Text = _customer.FirstName ?? "";
                    LastNameTextBox.Text = _customer.LastName ?? "";
                    EmailTextBox.Text = _customer.Email ?? "";
                    PhoneTextBox.Text = _customer.Phone ?? "";
                    BalanceText.Text = $"{_customer.Balance}₽";
                    BonusPointsText.Text = (_customer.BonusPoints ?? 0).ToString();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }
        private async void SaveProfile_Click(object? sender, RoutedEventArgs e)
        {
            if (_customer == null) return;

            try
            {
                _customer.FirstName = FirstNameTextBox.Text;
                _customer.LastName = LastNameTextBox.Text;
                _customer.Email = EmailTextBox.Text;
                _customer.Phone = PhoneTextBox.Text;

                AppDataContext.DbContext.SaveChanges();

                // Обновляем данные в CurrentUser
                if (AppDataContext.CurrentUser != null)
                {
                    AppDataContext.CurrentUser.FirstName = _customer.FirstName;
                    AppDataContext.CurrentUser.LastName = _customer.LastName;
                    AppDataContext.CurrentUser.Username = _customer.Email;
                }

                 ShowSuccess("Профиль успешно обновлен");
            }
            catch (Exception ex)
            {
                 ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void ResetProfile_Click(object? sender, RoutedEventArgs e)
        {
            LoadCustomerData();
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