using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using KinoZalMarsBlinVali.Views;
using System.Linq;

namespace KinoZalMarsBlinVali.Views
{
    public partial class AuthPage : UserControl
    {
        public AuthPage()
        {
            InitializeComponent();
        }

        private void Login_Click(object? sender, RoutedEventArgs e)
        {
            string username = tbUsername.Text ?? string.Empty;
            string password = tbPassword.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Пожалуйста, введите логин/email и пароль");
                return;
            }

            try
            {
                bool isCustomerLogin = rbCustomer.IsChecked == true;

                if (isCustomerLogin)
                {
                    // Авторизация зрителя
                    var customer = AppDataContext.DbContext.Customers
                        .FirstOrDefault(c => c.Email == username && c.Password == password);

                    if (customer != null)
                    {
                        // Создаем временный объект Employee для зрителя
                        AppDataContext.CurrentUser = new Employee
                        {
                            EmployeeId = customer.CustomerId,
                            FirstName = customer.FirstName,
                            LastName = customer.LastName,
                            Username = customer.Email,
                            Role = "customer",
                            Position = "Зритель",
                            Password = "", // Пустой пароль для зрителя
                            IsActive = true
                        };

                        // ИСПРАВЛЕНО: Переходим на главную страницу для зрителя
                        if (this.VisualRoot is MainWindow mainWindow)
                        {
                            mainWindow.NavigateTo(new CustomerMainPage()); // Вместо MainPage()
                        }
                    }
                    else
                    {
                        ShowError("Неверный email или пароль");
                    }
                }
                else
                {
                    // Авторизация сотрудника
                    var employee = AppDataContext.DbContext.Employees
                        .FirstOrDefault(u => u.Username == username &&
                                           u.Password == password &&
                                           u.IsActive == true);

                    if (employee != null)
                    {
                        AppDataContext.CurrentUser = employee;

                        // Проверяем роль пользователя
                        if (employee.Role == "admin" || employee.Role == "manager")
                        {
                            // Администратор или менеджер - переходим на админ-панель
                            if (this.VisualRoot is MainWindow mainWindow)
                            {
                                mainWindow.NavigateTo(new AdminPanelPage());
                            }
                        }
                        else
                        {
                            // Обычный сотрудник (кассир) - переходим на главную страницу
                            if (this.VisualRoot is MainWindow mainWindow)
                            {
                                mainWindow.NavigateTo(new MainPage());
                            }
                        }
                    }
                    else
                    {
                        ShowError("Неверный логин или пароль");
                    }
                }
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка при авторизации: {ex.Message}");
            }
        }

        private void Register_Click(object? sender, RoutedEventArgs e)
        {
            // Переходим на страницу регистрации (только для зрителей)
            if (this.VisualRoot is MainWindow mainWindow)
            {
                mainWindow.NavigateTo(new RegisterPage());
            }
        }

        private void ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            dialog.ShowDialog((Window)this.VisualRoot);
        }
    }
}