using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

                Console.WriteLine($"=== LOADED SESSIONS ===");
                foreach (var session in _sessions)
                {
                    Console.WriteLine($"🎬 {session.Movie.Title}");
                    Console.WriteLine($"🖼️ PosterPath: {session.Movie.PosterPath}");
                    Console.WriteLine($"📅 Start: {session.StartTime}");
                    Console.WriteLine($"---");
                }

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
                Console.WriteLine($"Ошибка загрузки жанров: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            var filtered = _sessions.AsEnumerable();

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

            var selectedGenre = GenreFilterComboBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedGenre) && selectedGenre != "Все жанры")
            {
                filtered = filtered.Where(s => s.Movie.Genre == selectedGenre);
            }

            var searchText = SearchTextBox.Text?.ToLower() ?? "";
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(s => s.Movie.Title.ToLower().Contains(searchText));
            }

            var result = filtered.ToList();
            Console.WriteLine($"🔍 Отфильтровано сеансов: {result.Count}");

            SessionsItemsControl.ItemsSource = result;
        }

        private void BookTicket_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int sessionId)
            {
                var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
                if (session != null)
                {
                    Console.WriteLine($"🎫 Бронирование сеанса: {session.Movie.Title}");
                    Console.WriteLine($"🖼️ PosterPath для бронирования: {session.Movie.PosterPath}");

                    var customerMainPage = this.FindAncestorOfType<CustomerMainPage>();
                    if (customerMainPage != null)
                    {
                        customerMainPage.MainContentControl.Content = new BookingPage(session);
                    }
                    else
                    {
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