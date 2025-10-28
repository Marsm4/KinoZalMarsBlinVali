using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class EmployeeEditPage : UserControl
    {
        private Employee _employee;
        private bool _isEditMode;

        public string WindowTitle => _isEditMode ? "Редактирование сотрудника" : "Добавление сотрудника";

        public EmployeeEditPage()
        {
            _employee = new Employee();
            _isEditMode = false;
            InitializeComponent();
            DataContext = this;
        }

        public EmployeeEditPage(Employee employee)
        {
            _employee = employee;
            _isEditMode = true;
            InitializeComponent();
            DataContext = this;
            LoadEmployeeData();
        }

        private void LoadEmployeeData()
        {
            UsernameTextBox.Text = _employee.Username;
            PasswordTextBox.Text = ""; 
            FirstNameTextBox.Text = _employee.FirstName;
            LastNameTextBox.Text = _employee.LastName;

           
            if (!string.IsNullOrEmpty(_employee.Position))
            {
                foreach (ComboBoxItem item in PositionComboBox.Items)
                {
                    if (item.Content?.ToString() == _employee.Position)
                    {
                        PositionComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

 
            if (!string.IsNullOrEmpty(_employee.Role))
            {
                foreach (ComboBoxItem item in RoleComboBox.Items)
                {
                    if (item.Content?.ToString() == _employee.Role)
                    {
                        RoleComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            IsActiveCheckBox.IsChecked = _employee.IsActive ?? true;
        }

        private async void Save_Click(object? sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                _employee.Username = UsernameTextBox.Text?.Trim() ?? "";

                if (!string.IsNullOrEmpty(PasswordTextBox.Text))
                {
                    _employee.Password = PasswordTextBox.Text; 
                }

                _employee.FirstName = FirstNameTextBox.Text?.Trim() ?? "";
                _employee.LastName = LastNameTextBox.Text?.Trim() ?? "";
                _employee.Position = (PositionComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                _employee.Role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                _employee.IsActive = IsActiveCheckBox.IsChecked ?? true;

                if (!_isEditMode)
                {
                    _employee.CreatedAt = DateTime.Now;
                    AppDataContext.DbContext.Employees.Add(_employee);
                }

                await AppDataContext.DbContext.SaveChangesAsync();

                
                var successDialog = new MessageWindow("Успех",
                    _isEditMode ? "Сотрудник успешно обновлен!" : "Сотрудник успешно добавлен!");
                await successDialog.ShowDialog((Window)this.VisualRoot);

                
                Back_Click(sender, e);
            }
            catch (Exception ex)
            {
                var dialog = new MessageWindow("Ошибка", $"Ошибка сохранения: {ex.Message}");
                await dialog.ShowDialog((Window)this.VisualRoot);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                ShowError("Введите логин сотрудника");
                return false;
            }

            if (!_isEditMode && string.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                ShowError("Введите пароль сотрудника");
                return false;
            }

            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                ShowError("Введите имя сотрудника");
                return false;
            }

            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                ShowError("Введите фамилию сотрудника");
                return false;
            }

            if (PositionComboBox.SelectedItem == null)
            {
                ShowError("Выберите должность");
                return false;
            }

            if (RoleComboBox.SelectedItem == null)
            {
                ShowError("Выберите роль в системе");
                return false;
            }

            var existingEmployee = AppDataContext.DbContext.Employees
                .FirstOrDefault(emp => emp.Username == UsernameTextBox.Text.Trim() &&
                                      emp.EmployeeId != _employee.EmployeeId);
            if (existingEmployee != null)
            {
                ShowError("Сотрудник с таким логином уже существует");
                return false;
            }

            return true;
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentControl &&
                contentControl.Parent is Grid grid &&
                grid.Parent is AdminPanelPage adminPanel)
            {
                adminPanel.MainContentControl.Content = new AdminEmployeesPage();
            }
        }

        private async void ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            await dialog.ShowDialog((Window)this.VisualRoot);
        }
    }
}