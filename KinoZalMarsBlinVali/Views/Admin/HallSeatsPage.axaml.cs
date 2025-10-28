using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
            public ObservableCollection<SeatInfo> Seats { get; set; } = new ObservableCollection<SeatInfo>();
        }

        public class SeatInfo : INotifyPropertyChanged
        {
            public HallSeat Seat { get; set; }

            public IBrush SeatBackground
            {
                get
                {
                    return Seat.SeatType switch
                    {
                        "vip" => new SolidColorBrush(Colors.Orange),
                        "disabled" => new SolidColorBrush(Colors.Gray),
                        _ => new SolidColorBrush(Colors.Green)
                    };
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public void UpdateBackground()
            {
                OnPropertyChanged(nameof(SeatBackground));
            }
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

                    if (string.IsNullOrEmpty(seat.SeatType))
                    {
                        seat.SeatType = "standard";
                    }

                    var seatInfo = new SeatInfo
                    {
                        Seat = seat
                    };
                    seatRow.Seats.Add(seatInfo);
                }

                _seatRows.Add(seatRow);
            }

            SeatsItemsControl.ItemsSource = _seatRows;
        }

        private async void SeatButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is SeatInfo seatInfo)
            {
                var selectedType = (SeatTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "standard";

              
                seatInfo.Seat.SeatType = selectedType;

                seatInfo.Seat.PriceMultiplier = selectedType switch
                {
                    "vip" => 1.5m,
                    "disabled" => 0m,
                    _ => 1.0m
                };

                seatInfo.UpdateBackground();
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