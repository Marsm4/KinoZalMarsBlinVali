using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class MovieEditPage : UserControl
    {
        private Movie _movie;
        private bool _isEditMode;

        public string WindowTitle => _isEditMode ? "Редактирование фильма" : "Добавление фильма";

        public MovieEditPage()
        {
            _movie = new Movie();
            _isEditMode = false;
            InitializeComponent();
            DataContext = this;
        }

        public MovieEditPage(Movie movie) : this()
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

        private async void Save_Click(object? sender, RoutedEventArgs e)
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
                    AppDataContext.DbContext.Movies.Add(_movie);
                }

                await AppDataContext.DbContext.SaveChangesAsync(); // Добавлен await

                // Показываем сообщение об успехе
                var successDialog = new MessageWindow("Успех",
                    _isEditMode ? "Фильм успешно обновлен!" : "Фильм успешно добавлен!");
                await successDialog.ShowDialog((Window)this.VisualRoot);

                // Возвращаемся назад через родительский AdminPanelPage
                Back_Click(sender, e);
            }
            catch (Exception ex)
            {
                var dialog = new MessageWindow("Ошибка", $"Ошибка сохранения: {ex.Message}");
                await dialog.ShowDialog((Window)this.VisualRoot);
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

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            // Возвращаемся на страницу управления фильмами через родительский AdminPanelPage
            if (this.Parent is ContentControl contentControl &&
                contentControl.Parent is Grid grid &&
                grid.Parent is AdminPanelPage adminPanel)
            {
                adminPanel.MainContentControl.Content = new AdminMoviesPage();
            }
        }

        private async void ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            await dialog.ShowDialog((Window)this.VisualRoot);
        }
    }
}