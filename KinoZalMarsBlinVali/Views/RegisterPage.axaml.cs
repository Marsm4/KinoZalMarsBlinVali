using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using KinoZalMarsBlinVali.Views;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace KinoZalMarsBlinVali.Views
{
    public partial class RegisterPage : UserControl
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void Register_Click(object? sender, RoutedEventArgs e)
        {
            string firstName = tbFirstName.Text ?? string.Empty;
            string lastName = tbLastName.Text ?? string.Empty;
            string email = tbEmail.Text ?? string.Empty;
            string phone = tbPhone.Text ?? string.Empty;
            string password = tbPassword.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(password))
            {
                ShowError("Пожалуйста, заполните все поля");
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Пожалуйста, введите корректный email с доменом mail.ru или gmail.com");
                return;
            }

            if (!IsValidPhone(phone))
            {
                ShowError("Телефон должен быть в формате: +7XXXXXXXXXX или 8XXXXXXXXXX (11 цифр)");
                return;
            }

            if (password.Length < 4)
            {
                ShowError("Пароль должен содержать минимум 4 символа");
                return;
            }

            try
            {
                var existingCustomer = AppDataContext.DbContext.Customers
                    .FirstOrDefault(c => c.Email == email);

                if (existingCustomer != null)
                {
                    ShowError("Пользователь с таким email уже существует");
                    return;
                }

                var newCustomer = new Customer
                {
                    FirstName = firstName.Trim(),
                    LastName = lastName.Trim(),
                    Email = email.Trim().ToLower(),
                    Phone = FormatPhone(phone.Trim()),
                    Password = password,
                    BonusPoints = 0,
                    CreatedAt = DateTime.Now
                };

                AppDataContext.DbContext.Customers.Add(newCustomer);
                AppDataContext.DbContext.SaveChanges();

                ShowSuccess("Регистрация прошла успешно! Теперь вы можете войти в систему как зритель.");

                BackToAuth();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при регистрации: {ex.Message}");
            }
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            BackToAuth();
        }

        private void BackToAuth()
        {
            if (this.VisualRoot is MainWindow mainWindow)
            {
                mainWindow.NavigateTo(new AuthPage());
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                // Базовая проверка формата email
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                    return false;

                // Проверка домена
                string domain = email.Split('@')[1].ToLower();
                return domain == "mail.ru" || domain == "gmail.com";
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            // Удаляем все пробелы, скобки и дефисы
            string cleanPhone = Regex.Replace(phone, @"[\s\-\(\)]", "");

            // Проверяем форматы: +7XXXXXXXXXX или 8XXXXXXXXXX
            if (cleanPhone.StartsWith("+7") && cleanPhone.Length == 12)
            {
                // Проверяем, что после +7 идут только цифры
                return cleanPhone.Substring(2).All(char.IsDigit);
            }
            else if (cleanPhone.StartsWith("8") && cleanPhone.Length == 11)
            {
                // Проверяем, что после 8 идут только цифры
                return cleanPhone.Substring(1).All(char.IsDigit);
            }

            return false;
        }

        private string FormatPhone(string phone)
        {
            // Удаляем все пробелы, скобки и дефисы
            string cleanPhone = Regex.Replace(phone, @"[\s\-\(\)]", "");

            // Приводим к единому формату +7XXXXXXXXXX
            if (cleanPhone.StartsWith("8") && cleanPhone.Length == 11)
            {
                return "+7" + cleanPhone.Substring(1);
            }

            return cleanPhone;
        }

        private void ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            dialog.ShowDialog((Window)this.VisualRoot);
        }

        private void ShowSuccess(string message)
        {
            var dialog = new MessageWindow("Успех", message);
            dialog.ShowDialog((Window)this.VisualRoot);
        }
    }
}