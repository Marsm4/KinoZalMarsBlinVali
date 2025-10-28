using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class HallEditPage : UserControl
    {
        private Hall _hall;
        private bool _isEditMode;

        public string WindowTitle => _isEditMode ? "Редактирование зала" : "Добавление зала";

        public HallEditPage()
        {
            _hall = new Hall();
            _isEditMode = false;
            InitializeComponent();
            DataContext = this;
            CalculateTotalSeats();
        }

        public HallEditPage(Hall hall) : this()
        {
            _hall = hall;
            _isEditMode = true;
            InitializeComponent();
            DataContext = this;
            LoadHallData();
        }

        private void LoadHallData()
        {
            HallNameTextBox.Text = _hall.HallName;

            // Устанавливаем тип зала
            foreach (ComboBoxItem item in HallTypeComboBox.Items)
            {
                if (item.Content?.ToString() == _hall.HallType)
                {
                    HallTypeComboBox.SelectedItem = item;
                    break;
                }
            }

            SeatRowsTextBox.Text = _hall.SeatRows.ToString();
            SeatColumnsTextBox.Text = _hall.SeatColumns.ToString();
            TotalSeatsText.Text = _hall.TotalSeats.ToString();
            LayoutSchemaPathTextBox.Text = _hall.LayoutSchemaPath ?? "";
        }

        private void CalculateTotalSeats()
        {
            if (int.TryParse(SeatRowsTextBox.Text, out int rows) &&
                int.TryParse(SeatColumnsTextBox.Text, out int columns))
            {
                TotalSeatsText.Text = (rows * columns).ToString();
            }
        }

        private void SeatRowsTextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            CalculateTotalSeats();
        }

        private void SeatColumnsTextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            CalculateTotalSeats();
        }

        private async void Save_Click(object? sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                _hall.HallName = HallNameTextBox.Text?.Trim() ?? "";
                _hall.HallType = (HallTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "standard";
                _hall.SeatRows = int.Parse(SeatRowsTextBox.Text);
                _hall.SeatColumns = int.Parse(SeatColumnsTextBox.Text);
                _hall.TotalSeats = int.Parse(TotalSeatsText.Text);
                _hall.LayoutSchemaPath = LayoutSchemaPathTextBox.Text?.Trim();

                if (!_isEditMode)
                {
                    AppDataContext.DbContext.Halls.Add(_hall);
                }

                AppDataContext.DbContext.SaveChanges();

               
                if (!_isEditMode)
                {
                    CreateHallSeats(_hall.HallId, _hall.SeatRows, _hall.SeatColumns);
                }

            
                var successDialog = new MessageWindow("Успех",
                    _isEditMode ? "Зал успешно обновлен!" : "Зал успешно добавлен!");
                await successDialog.ShowDialog((Window)this.VisualRoot);

     
                Back_Click(sender, e);
            }
            catch (Exception ex)
            {
                var dialog = new MessageWindow("Ошибка", $"Ошибка сохранения: {ex.Message}");
                await dialog.ShowDialog((Window)this.VisualRoot);
            }
        }

        private void CreateHallSeats(int hallId, int rows, int columns)
        {
            try
            {
 
                for (int row = 1; row <= rows; row++)
                {
                    for (int column = 1; column <= columns; column++)
                    {
                        var seat = new HallSeat
                        {
                            HallId = hallId,
                            RowNumber = row,
                            SeatNumber = column,
                            SeatType = row <= 2 ? "vip" : "standard", // Первые 2 ряда - VIP
                            PriceMultiplier = row <= 2 ? 1.5m : 1.0m,
                            IsActive = true
                        };
                        AppDataContext.DbContext.HallSeats.Add(seat);
                    }
                }
                AppDataContext.DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания мест: {ex.Message}");
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(HallNameTextBox.Text))
            {
                ShowError("Введите название зала");
                return false;
            }

            if (!int.TryParse(SeatRowsTextBox.Text, out int rows) || rows <= 0 || rows > 50)
            {
                ShowError("Введите корректное количество рядов (1-50)");
                return false;
            }

            if (!int.TryParse(SeatColumnsTextBox.Text, out int columns) || columns <= 0 || columns > 30)
            {
                ShowError("Введите корректное количество мест в ряду (1-30)");
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
                adminPanel.MainContentControl.Content = new AdminHallsPage();
            }
        }

        private async void ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            await dialog.ShowDialog((Window)this.VisualRoot);
        }
    }
}