using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using KinoZalMarsBlinVali.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KinoZalMarsBlinVali.Views
{
    public partial class MovieDetailsPage : UserControl
    {
        private Movie _movie;
        private List<Session> _sessions;
        private DateTime _selectedDate;

        public class SessionViewModel
        {
            public int SessionId { get; set; }
            public string SessionInfo { get; set; } = string.Empty;
            public string HallInfo { get; set; } = string.Empty;
            public DateTime StartTime { get; set; }
        }

        public MovieDetailsPage()
        {
            InitializeComponent();
        }

        public MovieDetailsPage(Movie movie, List<Session> sessions, DateTime selectedDate)
        {
            InitializeComponent();
            _movie = movie;
            _sessions = sessions;
            _selectedDate = selectedDate;
            LoadMovieDetails();
            LoadSessions();
        }

        private void LoadMovieDetails()
        {
            if (_movie == null) return;

            TitleText.Text = _movie.Title;
            MovieTitleText.Text = _movie.Title;
            MovieGenreText.Text = _movie.Genre;
            MovieAgeRatingText.Text = $"{_movie.AgeRating}+";
            MovieDurationText.Text = $"{_movie.DurationMinutes} мин";
            MovieDirectorText.Text = $"Режиссер: {_movie.Director}";
            MovieCastText.Text = _movie.CastText ?? "Информация отсутствует";
            MovieDescriptionText.Text = _movie.Description ?? "Описание отсутствует";

            this.DataContext = _movie;
        }

        private void LoadSessions()
        {
            if (_sessions == null) return;

            var sessionViewModels = _sessions
                .OrderBy(s => s.StartTime)
                .Select(s => new SessionViewModel
                {
                    SessionId = s.SessionId,
                    SessionInfo = $"{s.StartTime:dd.MM.yyyy} в {s.StartTime:HH:mm} - {s.BasePrice}₽",
                    HallInfo = $"Зал: {s.Hall.HallName}",
                    StartTime = s.StartTime
                })
                .ToList();

            SessionsItemsControl.ItemsSource = sessionViewModels;
        }

        private void BookThisSession_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int sessionId)
            {
                var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
                if (session != null)
                {
                    NavigateToBooking(session);
                }
            }
        }

        private void SelectSession_Click(object? sender, RoutedEventArgs e)
        {
            // Возвращаемся к расписанию с сохранением даты
            Back_Click(sender, e);
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            var customerMainPage = this.FindAncestorOfType<CustomerMainPage>();
            if (customerMainPage != null)
            {
                // Передаем выбранную дату обратно
                customerMainPage.MainContentControl.Content = new CustomerSessionsPage(_selectedDate);
            }
        }

        private void NavigateToBooking(Session session)
        {
            var customerMainPage = this.FindAncestorOfType<CustomerMainPage>();
            if (customerMainPage != null)
            {
                customerMainPage.MainContentControl.Content = new BookingPage(session);
            }
        }
    }
}