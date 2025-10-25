using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using KinoZalMarsBlinVali.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class AdminHallsPage : UserControl
    {
        private List<Hall> _halls = new List<Hall>();

        public AdminHallsPage()
        {
            InitializeComponent();
            LoadHalls();
        }

        private void LoadHalls()
        {
            try
            {
                _halls = AppDataContext.DbContext.Halls.ToList();
                HallsDataGrid.ItemsSource = _halls;
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка загрузки залов: {ex.Message}");
            }
        }

        private void AddHall_Click(object? sender, RoutedEventArgs e)
        {
            // Переходим на страницу добавления зала
            if (this.Parent is ContentControl contentControl &&
                contentControl.Parent is Grid grid &&
                grid.Parent is AdminPanelPage adminPanel)
            {
                adminPanel.MainContentControl.Content = new HallEditPage();
            }
        }

        private void EditHall_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int hallId)
            {
                var hall = _halls.FirstOrDefault(h => h.HallId == hallId);
                if (hall != null)
                {
                    // Переходим на страницу редактирования зала
                    if (this.Parent is ContentControl contentControl &&
                        contentControl.Parent is Grid grid &&
                        grid.Parent is AdminPanelPage adminPanel)
                    {
                        adminPanel.MainContentControl.Content = new HallEditPage(hall);
                    }
                }
            }
        }

        private void EditSeats_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int hallId)
            {
                var hall = _halls.FirstOrDefault(h => h.HallId == hallId);
                if (hall != null)
                {
                    // Переходим на страницу редактирования мест
                    if (this.Parent is ContentControl contentControl &&
                        contentControl.Parent is Grid grid &&
                        grid.Parent is AdminPanelPage adminPanel)
                    {
                        adminPanel.MainContentControl.Content = new HallSeatsPage(hall);
                    }
                }
            }
        }

        private async void ViewHall_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int hallId)
            {
                var hall = _halls.FirstOrDefault(h => h.HallId == hallId);
                if (hall != null)
                {
                    var info = $"Название: {hall.HallName}\n" +
                              $"Всего мест: {hall.TotalSeats}\n" +
                              $"Рядов: {hall.SeatRows}\n" +
                              $"Мест в ряду: {hall.SeatColumns}\n" +
                              $"Тип зала: {hall.HallType}";

                    var dialog = new MessageWindow("Информация о зале", info);
                    await dialog.ShowDialog((Window)this.VisualRoot);
                }
            }
        }

        private async void DeleteHall_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int hallId)
            {
                var hall = _halls.FirstOrDefault(h => h.HallId == hallId);
                if (hall != null)
                {
                    var confirmWindow = new ConfirmWindow("Подтверждение удаления",
                        $"Вы уверены, что хотите удалить зал \"{hall.HallName}\"? Все связанные сеансы и места будут также удалены.");

                    var result = await confirmWindow.ShowDialog<bool>((Window)this.VisualRoot);

                    if (result)
                    {
                        try
                        {
                            AppDataContext.DbContext.Halls.Remove(hall);
                            AppDataContext.DbContext.SaveChanges();
                            LoadHalls();
                            ShowSuccess("Зал успешно удален");
                        }
                        catch (System.Exception ex)
                        {
                            ShowError($"Ошибка удаления зала: {ex.Message}");
                        }
                    }
                }
            }
        }

        private void Refresh_Click(object? sender, RoutedEventArgs e)
        {
            LoadHalls();
        }

        private void Search_Click(object? sender, RoutedEventArgs e)
        {
            var searchText = SearchTextBox.Text?.ToLower() ?? "";
            if (string.IsNullOrWhiteSpace(searchText))
            {
                HallsDataGrid.ItemsSource = _halls;
            }
            else
            {
                var filtered = _halls.Where(h => h.HallName.ToLower().Contains(searchText)).ToList();
                HallsDataGrid.ItemsSource = filtered;
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