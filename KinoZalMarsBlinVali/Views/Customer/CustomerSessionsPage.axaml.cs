using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KinoZalMarsBlinVali.Views
{
    public partial class CustomerSessionsPage : UserControl
    {
        private List<Session> _sessions = new List<Session>();

        public CustomerSessionsPage()
        {
            InitializeComponent();
            LoadSessions();
            LoadGenreFilter();
        }

        private void LoadSessions()
        {
            try
            {
                var now = DateTime.Now;

                _sessions = AppDataContext.DbContext.Sessions
                    .Include(s => s.Movie)
                    .Include(s => s.Hall)
                    .Include(s => s.Tickets)
                    .Where(s => s.StartTime > now && s.Movie.IsActive == true)
                    .OrderBy(s => s.StartTime)
                    .ToList();

                ApplyFilters();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки сеансов: {ex.Message}");
            }
        }

        private void LoadGenreFilter()
        {
            try
            {
                var genres = AppDataContext.DbContext.Movies
                    .Where(m => m.IsActive == true && m.Genre != null)
                    .Select(m => m.Genre)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();

                // Очищаем и добавляем "Все жанры" первым элементом
                GenreFilterComboBox.Items.Clear();
                GenreFilterComboBox.Items.Add("Все жанры");
                foreach (var genre in genres)
                {
                    GenreFilterComboBox.Items.Add(genre);
                }
                GenreFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки загрузки жанров
            }
        }

        private void ApplyFilters()
        {
            var filtered = _sessions.AsEnumerable();

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
                    case "На неделе":
                        var endOfWeek = today.AddDays(7 - (int)today.DayOfWeek);
                        filtered = filtered.Where(s => s.StartTime.Date >= today && s.StartTime.Date <= endOfWeek);
                        break;
                }
            }

            // Фильтр по жанру
            var selectedGenre = GenreFilterComboBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedGenre) && selectedGenre != "Все жанры")
            {
                filtered = filtered.Where(s => s.Movie.Genre == selectedGenre);
            }

            // Фильтр по поиску
            var searchText = SearchTextBox.Text?.ToLower() ?? "";
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(s => s.Movie.Title.ToLower().Contains(searchText));
            }

            SessionsItemsControl.ItemsSource = filtered.ToList();
        }

        private void BookTicket_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int sessionId)
            {
                var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
                if (session != null)
                {
                    // ИСПРАВЛЕНИЕ: Правильная навигация к странице бронирования
                    var customerMainPage = this.FindAncestorOfType<CustomerMainPage>();
                    if (customerMainPage != null)
                    {
                        customerMainPage.MainContentControl.Content = new BookingPage(session);
                    }
                    else
                    {
                        // Альтернативный способ навигации
                        if (this.Parent is ContentControl contentControl &&
                            contentControl.Parent is CustomerMainPage mainPage)
                        {
                            mainPage.MainContentControl.Content = new BookingPage(session);
                        }
                    }
                }
            }
        }

        private void Search_Click(object? sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void Refresh_Click(object? sender, RoutedEventArgs e)
        {
            LoadSessions();
        }

        private async void ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            await dialog.ShowDialog((Window)this.VisualRoot);
        }
    }
}