using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using KinoZalMarsBlinVali.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class AdminSessionsPage : UserControl
    {
        private List<Session> _sessions = new List<Session>();

        public AdminSessionsPage()
        {
            InitializeComponent();
            LoadSessions();
        }

        private void LoadSessions()
        {
            try
            {
                _sessions = AppDataContext.DbContext.Sessions
                    .Include(s => s.Movie)
                    .Include(s => s.Hall)
                    .Include(s => s.Tickets)
                    .OrderBy(s => s.StartTime)
                    .ToList();

                UpdateSessionsDataGrid();
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка загрузки сеансов: {ex.Message}");
            }
        }

        private void UpdateSessionsDataGrid()
        {
            var filteredSessions = ApplyFilters(_sessions);
            SessionsDataGrid.ItemsSource = filteredSessions;
        }

        private List<Session> ApplyFilters(List<Session> sessions)
        {
            var filtered = sessions.AsEnumerable();

            // Фильтр по дате
            var dateFilter = (DateFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (!string.IsNullOrEmpty(dateFilter))
            {
                var today = DateTime.Today;
                switch (dateFilter)
                {
                    case "Сегодня":
                        filtered = filtered.Where(s => s.StartTime.Date == today);
                        break;
                    case "Завтра":
                        filtered = filtered.Where(s => s.StartTime.Date == today.AddDays(1));
                        break;
                    case "На этой неделе":
                        var endOfWeek = today.AddDays(7 - (int)today.DayOfWeek);
                        filtered = filtered.Where(s => s.StartTime.Date >= today && s.StartTime.Date <= endOfWeek);
                        break;
                }
            }

            // Фильтр по поиску
            var searchText = SearchTextBox.Text?.ToLower() ?? "";
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(s => s.Movie.Title.ToLower().Contains(searchText));
            }

            return filtered.ToList();
        }

        private void AddSession_Click(object? sender, RoutedEventArgs e)
        {
            // Переходим на страницу добавления сеанса
            if (this.Parent is ContentControl contentControl &&
                contentControl.Parent is Grid grid &&
                grid.Parent is AdminPanelPage adminPanel)
            {
                adminPanel.MainContentControl.Content = new SessionEditPage();
            }
        }

        private void EditSession_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int sessionId)
            {
                var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
                if (session != null)
                {
                    // Переходим на страницу редактирования сеанса
                    if (this.Parent is ContentControl contentControl &&
                        contentControl.Parent is Grid grid &&
                        grid.Parent is AdminPanelPage adminPanel)
                    {
                        adminPanel.MainContentControl.Content = new SessionEditPage(session);
                    }
                }
            }
        }

        private async void ViewTickets_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int sessionId)
            {
                var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
                if (session != null)
                {
                    // Переходим на страницу просмотра билетов
                    if (this.Parent is ContentControl contentControl &&
                        contentControl.Parent is Grid grid &&
                        grid.Parent is AdminPanelPage adminPanel)
                    {
                        adminPanel.MainContentControl.Content = new SessionTicketsPage(session);
                    }
                }
            }
        }

        private async void DeleteSession_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int sessionId)
            {
                var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
                if (session != null)
                {
                    var confirmWindow = new ConfirmWindow("Подтверждение удаления",
                        $"Вы уверены, что хотите удалить сеанс фильма \"{session.Movie.Title}\" от {session.StartTime:dd.MM.yyyy HH:mm}?");

                    var result = await confirmWindow.ShowDialog<bool>((Window)this.VisualRoot);

                    if (result)
                    {
                        try
                        {
                            AppDataContext.DbContext.Sessions.Remove(session);
                            AppDataContext.DbContext.SaveChanges();
                            LoadSessions();
                            ShowSuccess("Сеанс успешно удален");
                        }
                        catch (System.Exception ex)
                        {
                            ShowError($"Ошибка удаления сеанса: {ex.Message}");
                        }
                    }
                }
            }
        }

        private void Refresh_Click(object? sender, RoutedEventArgs e)
        {
            LoadSessions();
        }

        private void Search_Click(object? sender, RoutedEventArgs e)
        {
            UpdateSessionsDataGrid();
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