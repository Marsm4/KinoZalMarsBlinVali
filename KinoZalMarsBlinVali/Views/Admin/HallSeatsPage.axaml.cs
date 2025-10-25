using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class HallSeatsPage : UserControl
    {
        private Hall _hall;
        private List<HallSeat> _seats = new List<HallSeat>();
        private ObservableCollection<SeatRow> _seatRows = new ObservableCollection<SeatRow>();

        public HallSeatsPage(Hall hall)
        {
            _hall = hall;
            InitializeComponent();
            TitleText.Text = $"Карта зала: {hall.HallName}";
            LoadSeats();
        }

        public class SeatRow
        {
            public int RowNumber { get; set; }
            public ObservableCollection<HallSeat> Seats { get; set; } = new ObservableCollection<HallSeat>();
        }

        private void LoadSeats()
        {
            try
            {
                _seats = AppDataContext.DbContext.HallSeats
                    .Where(s => s.HallId == _hall.HallId)
                    .OrderBy(s => s.RowNumber)
                    .ThenBy(s => s.SeatNumber)
                    .ToList();

                UpdateSeatRows();
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка загрузки мест: {ex.Message}");
            }
        }

        private void UpdateSeatRows()
        {
            _seatRows.Clear();

            var rows = _seats.GroupBy(s => s.RowNumber)
                            .OrderBy(g => g.Key);

            foreach (var rowGroup in rows)
            {
                var seatRow = new SeatRow
                {
                    RowNumber = rowGroup.Key
                };

                foreach (var seat in rowGroup.OrderBy(s => s.SeatNumber))
                {
                    seatRow.Seats.Add(seat);
                }

                _seatRows.Add(seatRow);
            }

            SeatsItemsControl.ItemsSource = _seatRows;
        }

        private async void SeatButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is HallSeat seat)
            {
                var selectedType = (SeatTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "standard";

                // Меняем тип места
                seat.SeatType = selectedType;

                // Обновляем множитель цены
                seat.PriceMultiplier = selectedType switch
                {
                    "vip" => 1.5m,
                    "disabled" => 0m,
                    _ => 1.0m
                };

                // Обновляем классы кнопки
                button.Classes.Clear();
                button.Classes.Add(seat.SeatType);
            }
        }

        private async void SaveSeats_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                AppDataContext.DbContext.SaveChanges();
                await ShowSuccess("Изменения сохранены успешно!");
            }
            catch (System.Exception ex)
            {
                await ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void RefreshSeats_Click(object? sender, RoutedEventArgs e)
        {
            LoadSeats();
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            // Возвращаемся на страницу управления залами
            if (this.Parent is ContentControl contentControl &&
                contentControl.Parent is Grid grid &&
                grid.Parent is AdminPanelPage adminPanel)
            {
                adminPanel.MainContentControl.Content = new AdminHallsPage();
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