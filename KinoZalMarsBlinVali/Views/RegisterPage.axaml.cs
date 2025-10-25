using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using KinoZalMarsBlinVali.Views;
using System;
using System.Linq;

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

            // Валидация
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(password))
            {
                ShowError("Пожалуйста, заполните все поля");
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Пожалуйста, введите корректный email");
                return;
            }

            if (password.Length < 4)
            {
                ShowError("Пароль должен содержать минимум 4 символа");
                return;
            }

            try
            {
                // Проверяем, нет ли уже пользователя с таким email
                var existingCustomer = AppDataContext.DbContext.Customers
                    .FirstOrDefault(c => c.Email == email);

                if (existingCustomer != null)
                {
                    ShowError("Пользователь с таким email уже существует");
                    return;
                }

                // Создаем нового зрителя
                var newCustomer = new Customer
                {
                    FirstName = firstName.Trim(),
                    LastName = lastName.Trim(),
                    Email = email.Trim().ToLower(),
                    Phone = phone.Trim(),
                    Password = password, // Сохраняем пароль
                    BonusPoints = 0,
                    CreatedAt = DateTime.Now
                };

                // Сохраняем в базу данных
                AppDataContext.DbContext.Customers.Add(newCustomer);
                AppDataContext.DbContext.SaveChanges();

                ShowSuccess("Регистрация прошла успешно! Теперь вы можете войти в систему как зритель.");

                // Возвращаемся на страницу авторизации
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
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
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