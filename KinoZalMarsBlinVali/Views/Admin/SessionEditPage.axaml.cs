using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace KinoZalMarsBlinVali.Views
{
    public partial class SessionEditPage : UserControl
    {
        private Session _session;
        private bool _isEditMode;

        public string WindowTitle => _isEditMode ? "Редактирование сеанса" : "Добавление сеанса";

        public SessionEditPage()
        {
            _session = new Session();
            _isEditMode = false;
            InitializeComponent();
            DataContext = this;
            LoadComboBoxData();
        }

        public SessionEditPage(Session session) : this()
        {
            _session = session;
            _isEditMode = true;
            InitializeComponent();
            DataContext = this;
            LoadSessionData();
        }

        private void LoadComboBoxData()
        {
            try
            {
                // Загружаем активные фильмы
                var movies = AppDataContext.DbContext.Movies
                    .Where(m => m.IsActive == true)
                    .OrderBy(m => m.Title)
                    .ToList();
                MovieComboBox.ItemsSource = movies;

                // Загружаем залы
                var halls = AppDataContext.DbContext.Halls
                    .OrderBy(h => h.HallName)
                    .ToList();
                HallComboBox.ItemsSource = halls;

                // Устанавливаем значения по умолчанию для нового сеанса
                if (!_isEditMode)
                {
                    StartDatePicker.SelectedDate = DateTimeOffset.Now;
                    StartTimePicker.SelectedTime = new TimeSpan(18, 0, 0); // 18:00 по умолчанию
                }
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadSessionData()
        {
            // Устанавливаем фильм
            var movie = AppDataContext.DbContext.Movies.FirstOrDefault(m => m.MovieId == _session.MovieId);
            if (movie != null)
            {
                foreach (var item in MovieComboBox.Items)
                {
                    if (item is Movie m && m.MovieId == movie.MovieId)
                    {
                        MovieComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            // Устанавливаем зал
            var hall = AppDataContext.DbContext.Halls.FirstOrDefault(h => h.HallId == _session.HallId);
            if (hall != null)
            {
                foreach (var item in HallComboBox.Items)
                {
                    if (item is Hall h && h.HallId == hall.HallId)
                    {
                        HallComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            // Устанавливаем дату и время
            StartDatePicker.SelectedDate = new DateTimeOffset(_session.StartTime);
            StartTimePicker.SelectedTime = _session.StartTime.TimeOfDay;

            // Устанавливаем цену
            BasePriceTextBox.Text = _session.BasePrice.ToString();

            // Обновляем время окончания
            UpdateEndTime();
        }

        private void UpdateEndTime()
        {
            if (MovieComboBox.SelectedItem is Movie selectedMovie &&
                StartDatePicker.SelectedDate.HasValue &&
                StartTimePicker.SelectedTime.HasValue)
            {
                var startDate = StartDatePicker.SelectedDate.Value.DateTime;
                var startTime = StartTimePicker.SelectedTime.Value;
                var startDateTime = startDate + startTime;

                var endTime = startDateTime.AddMinutes(selectedMovie.DurationMinutes + 30); // +30 минут на уборку

                EndTimeText.Text = endTime.ToString("dd.MM.yyyy HH:mm");
            }
        }

        private void MovieComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateEndTime();
        }

        private void StartDatePicker_SelectedDateChanged(object? sender, DatePickerSelectedValueChangedEventArgs e)
        {
            UpdateEndTime();
        }

        private void StartTimePicker_SelectedTimeChanged(object? sender, TimePickerSelectedValueChangedEventArgs e)
        {
            UpdateEndTime();
        }

        private async void Save_Click(object? sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var selectedMovie = MovieComboBox.SelectedItem as Movie;
                var selectedHall = HallComboBox.SelectedItem as Hall;

                if (selectedMovie == null || selectedHall == null)
                {
                    ShowError("Выберите фильм и зал");
                    return;
                }

                // Создаем DateTime начала сеанса
                var startDate = StartDatePicker.SelectedDate?.DateTime ?? DateTime.Today;
                var startTime = StartTimePicker.SelectedTime ?? new TimeSpan(18, 0, 0);
                var startDateTime = startDate + startTime;

                // Проверяем конфликты сеансов
                if (!_isEditMode && HasSessionConflict(selectedHall.HallId, startDateTime, selectedMovie.DurationMinutes))
                {
                    ShowError("В этом зале уже есть сеанс в указанное время");
                    return;
                }

                _session.MovieId = selectedMovie.MovieId;
                _session.HallId = selectedHall.HallId;
                _session.StartTime = startDateTime;
                _session.EndTime = startDateTime.AddMinutes(selectedMovie.DurationMinutes + 30); // +30 минут на уборку
                _session.BasePrice = decimal.Parse(BasePriceTextBox.Text);

                if (!_isEditMode)
                {
                    _session.CreatedAt = DateTime.Now;
                    AppDataContext.DbContext.Sessions.Add(_session);
                }

                AppDataContext.DbContext.SaveChanges();

                // Показываем сообщение об успехе
                var successDialog = new MessageWindow("Успех",
                    _isEditMode ? "Сеанс успешно обновлен!" : "Сеанс успешно добавлен!");
                await successDialog.ShowDialog((Window)this.VisualRoot);

                // Возвращаемся назад
                Back_Click(sender, e);
            }
            catch (Exception ex)
            {
                var dialog = new MessageWindow("Ошибка", $"Ошибка сохранения: {ex.Message}");
                await dialog.ShowDialog((Window)this.VisualRoot);
            }
        }

        private bool HasSessionConflict(int hallId, DateTime startTime, int movieDuration)
        {
            var endTime = startTime.AddMinutes(movieDuration + 30); // Фильм + уборка

            var conflictingSessions = AppDataContext.DbContext.Sessions
                .Where(s => s.HallId == hallId &&
                           s.StartTime < endTime &&
                           s.EndTime > startTime)
                .ToList();

            return conflictingSessions.Any();
        }

        private bool ValidateForm()
        {
            if (MovieComboBox.SelectedItem == null)
            {
                ShowError("Выберите фильм");
                return false;
            }

            if (HallComboBox.SelectedItem == null)
            {
                ShowError("Выберите зал");
                return false;
            }

            if (!StartDatePicker.SelectedDate.HasValue)
            {
                ShowError("Выберите дату начала");
                return false;
            }

            if (!StartTimePicker.SelectedTime.HasValue)
            {
                ShowError("Выберите время начала");
                return false;
            }

            if (!decimal.TryParse(BasePriceTextBox.Text, out decimal price) || price <= 0)
            {
                ShowError("Введите корректную цену");
                return false;
            }

            // Проверяем, что сеанс не в прошлом
            var startDate = StartDatePicker.SelectedDate?.DateTime ?? DateTime.Today;
            var startTime = StartTimePicker.SelectedTime ?? new TimeSpan(0, 0, 0);
            var startDateTime = startDate + startTime;

            if (startDateTime < DateTime.Now)
            {
                ShowError("Нельзя создавать сеансы в прошлом");
                return false;
            }

            return true;
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            // Возвращаемся на страницу управления сеансами
            if (this.Parent is ContentControl contentControl &&
                contentControl.Parent is Grid grid &&
                grid.Parent is AdminPanelPage adminPanel)
            {
                adminPanel.MainContentControl.Content = new AdminSessionsPage();
            }
        }

        private async void ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            await dialog.ShowDialog((Window)this.VisualRoot);
        }
    }
}