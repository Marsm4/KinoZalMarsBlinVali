using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class AdminEmployeesPage : UserControl
    {
        private List<Employee> _employees = new List<Employee>();

        public AdminEmployeesPage()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            try
            {
                _employees = AppDataContext.DbContext.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .ToList();
                EmployeesDataGrid.ItemsSource = _employees;
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка загрузки сотрудников: {ex.Message}");
            }
        }

        private void AddEmployee_Click(object? sender, RoutedEventArgs e)
        {
            // Переходим на страницу добавления сотрудника
            if (this.Parent is ContentControl contentControl &&
                contentControl.Parent is Grid grid &&
                grid.Parent is AdminPanelPage adminPanel)
            {
                adminPanel.MainContentControl.Content = new EmployeeEditPage();
            }
        }

        private void EditEmployee_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int employeeId)
            {
                var employee = _employees.FirstOrDefault(emp => emp.EmployeeId == employeeId);
                if (employee != null)
                {
                    // Переходим на страницу редактирования сотрудника
                    if (this.Parent is ContentControl contentControl &&
                        contentControl.Parent is Grid grid &&
                        grid.Parent is AdminPanelPage adminPanel)
                    {
                        adminPanel.MainContentControl.Content = new EmployeeEditPage(employee);
                    }
                }
            }
        }

        private async void ViewEmployee_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int employeeId)
            {
                var employee = _employees.FirstOrDefault(emp => emp.EmployeeId == employeeId);
                if (employee != null)
                {
                    var info = $"Логин: {employee.Username}\n" +
                              $"Имя: {employee.FirstName}\n" +
                              $"Фамилия: {employee.LastName}\n" +
                              $"Должность: {employee.Position}\n" +
                              $"Роль: {employee.Role}\n" +
                              $"Статус: {(employee.IsActive == true ? "Активен" : "Неактивен")}\n" +
                              $"Дата создания: {employee.CreatedAt:dd.MM.yyyy}";

                    var dialog = new MessageWindow("Информация о сотруднике", info);
                    await dialog.ShowDialog((Window)this.VisualRoot);
                }
            }
        }

        private async void DeleteEmployee_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int employeeId)
            {
                var employee = _employees.FirstOrDefault(emp => emp.EmployeeId == employeeId);
                if (employee != null)
                {
                    // Нельзя удалить самого себя
                    if (employee.EmployeeId == AppDataContext.CurrentUser?.EmployeeId)
                    {
                         ShowError("Нельзя удалить свой собственный аккаунт");
                        return;
                    }

                    var confirmWindow = new ConfirmWindow("Подтверждение удаления",
                        $"Вы уверены, что хотите удалить сотрудника \"{employee.FirstName} {employee.LastName}\"?");

                    var result = await confirmWindow.ShowDialog<bool>((Window)this.VisualRoot);

                    if (result)
                    {
                        try
                        {
                            // Мягкое удаление - деактивация
                            employee.IsActive = false;
                            AppDataContext.DbContext.SaveChanges();
                            LoadEmployees();
                            ShowSuccess("Сотрудник успешно деактивирован");
                        }
                        catch (System.Exception ex)
                        {
                            ShowError($"Ошибка удаления сотрудника: {ex.Message}");
                        }
                    }
                }
            }
        }

        private void Refresh_Click(object? sender, RoutedEventArgs e)
        {
            LoadEmployees();
        }

        private void Search_Click(object? sender, RoutedEventArgs e)
        {
            var searchText = SearchTextBox.Text?.ToLower() ?? "";
            if (string.IsNullOrWhiteSpace(searchText))
            {
                EmployeesDataGrid.ItemsSource = _employees;
            }
            else
            {
                var filtered = _employees.Where(emp =>
                    emp.FirstName.ToLower().Contains(searchText) ||
                    emp.LastName.ToLower().Contains(searchText) ||
                    emp.Username.ToLower().Contains(searchText) ||
                    emp.Position.ToLower().Contains(searchText)
                ).ToList();
                EmployeesDataGrid.ItemsSource = filtered;
            }
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