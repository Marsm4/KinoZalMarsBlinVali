using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Models;
using System;

namespace KinoZalMarsBlinVali.Views
{
    public partial class MovieEditWindow : Window
    {
        private Movie _movie;
        private bool _isEditMode;

        public string WindowTitle => _isEditMode ? "Редактирование фильма" : "Добавление фильма";

        public MovieEditWindow()
        {
            _movie = new Movie();
            _isEditMode = false;
            InitializeComponent();
            DataContext = this;
        }

        public MovieEditWindow(Movie movie) : this()
        {
            _movie = movie;
            _isEditMode = true;
            InitializeComponent();
            DataContext = this;
            LoadMovieData();
        }

        private void LoadMovieData()
        {
            TitleTextBox.Text = _movie.Title;
            GenreTextBox.Text = _movie.Genre;
            DurationTextBox.Text = _movie.DurationMinutes.ToString();

            if (!string.IsNullOrEmpty(_movie.AgeRating))
            {
                foreach (ComboBoxItem item in AgeRatingComboBox.Items)
                {
                    if (item.Content?.ToString() == _movie.AgeRating)
                    {
                        AgeRatingComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            DirectorTextBox.Text = _movie.Director;
            CastTextBox.Text = _movie.CastText;
            DescriptionTextBox.Text = _movie.Description;
            PosterPathTextBox.Text = _movie.PosterPath;
            IsActiveCheckBox.IsChecked = _movie.IsActive ?? true;
        }

        private void Save_Click(object? sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                _movie.Title = TitleTextBox.Text?.Trim() ?? "";
                _movie.Genre = GenreTextBox.Text?.Trim();
                _movie.DurationMinutes = int.Parse(DurationTextBox.Text);
                _movie.AgeRating = (AgeRatingComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
                _movie.Director = DirectorTextBox.Text?.Trim();
                _movie.CastText = CastTextBox.Text?.Trim();
                _movie.Description = DescriptionTextBox.Text?.Trim();
                _movie.PosterPath = PosterPathTextBox.Text?.Trim();
                _movie.IsActive = IsActiveCheckBox.IsChecked ?? true;

                if (!_isEditMode)
                {
                    _movie.CreatedAt = DateTime.Now;
                    KinoZalMarsBlinVali.Data.AppDataContext.DbContext.Movies.Add(_movie);
                }

                KinoZalMarsBlinVali.Data.AppDataContext.DbContext.SaveChanges();
                Close();
            }
            catch (Exception ex)
            {
                var dialog = new MessageWindow("Ошибка", $"Ошибка сохранения: {ex.Message}");
                dialog.ShowDialog(this);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                ShowError("Введите название фильма");
                return false;
            }

            if (!int.TryParse(DurationTextBox.Text, out int duration) || duration <= 0)
            {
                ShowError("Введите корректную продолжительность");
                return false;
            }

            return true;
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            dialog.ShowDialog(this);
        }
    }
}