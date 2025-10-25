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
    public partial class AdminMoviesPage : UserControl
    {
        private List<Movie> _movies = new List<Movie>();

        public AdminMoviesPage()
        {
            InitializeComponent();
            LoadMovies();
        }

        private void LoadMovies()
        {
            try
            {
                _movies = AppDataContext.DbContext.Movies.ToList();
                MoviesDataGrid.ItemsSource = _movies;
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка загрузки фильмов: {ex.Message}");
            }
        }

        private void AddMovie_Click(object? sender, RoutedEventArgs e)
        {
            // Переходим на страницу добавления фильма через родительский AdminPanelPage
            if (this.Parent is ContentControl contentControl &&
                contentControl.Parent is Grid grid &&
                grid.Parent is AdminPanelPage adminPanel)
            {
                adminPanel.MainContentControl.Content = new MovieEditPage();
            }
        }

        private void EditMovie_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int movieId)
            {
                var movie = _movies.FirstOrDefault(m => m.MovieId == movieId);
                if (movie != null)
                {
                    // Переходим на страницу редактирования фильма через родительский AdminPanelPage
                    if (this.Parent is ContentControl contentControl &&
                        contentControl.Parent is Grid grid &&
                        grid.Parent is AdminPanelPage adminPanel)
                    {
                        adminPanel.MainContentControl.Content = new MovieEditPage(movie);
                    }
                }
            }
        }

        private async void ViewMovie_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int movieId)
            {
                var movie = _movies.FirstOrDefault(m => m.MovieId == movieId);
                if (movie != null)
                {
                    var info = $"Название: {movie.Title}\n" +
                              $"Жанр: {movie.Genre}\n" +
                              $"Продолжительность: {movie.DurationMinutes} мин\n" +
                              $"Возрастное ограничение: {movie.AgeRating}\n" +
                              $"Режиссер: {movie.Director}\n" +
                              $"Актерский состав: {movie.CastText}\n" +
                              $"Описание: {movie.Description}";

                    var dialog = new MessageWindow("Информация о фильме", info);
                    await dialog.ShowDialog((Window)this.VisualRoot);
                }
            }
        }

        private async void DeleteMovie_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int movieId)
            {
                var movie = _movies.FirstOrDefault(m => m.MovieId == movieId);
                if (movie != null)
                {
                    var confirmWindow = new ConfirmWindow("Подтверждение удаления",
                        $"Вы уверены, что хотите удалить фильм \"{movie.Title}\"?");

                    var result = await confirmWindow.ShowDialog<bool>((Window)this.VisualRoot);

                    if (result)
                    {
                        try
                        {
                            AppDataContext.DbContext.Movies.Remove(movie);
                            AppDataContext.DbContext.SaveChanges();
                            LoadMovies();
                            ShowSuccess("Фильм успешно удален");
                        }
                        catch (System.Exception ex)
                        {
                            ShowError($"Ошибка удаления фильма: {ex.Message}");
                        }
                    }
                }
            }
        }

        private void Refresh_Click(object? sender, RoutedEventArgs e)
        {
            LoadMovies();
        }

        private void Search_Click(object? sender, RoutedEventArgs e)
        {
            var searchText = SearchTextBox.Text?.ToLower() ?? "";
            if (string.IsNullOrWhiteSpace(searchText))
            {
                MoviesDataGrid.ItemsSource = _movies;
            }
            else
            {
                var filtered = _movies.Where(m => m.Title.ToLower().Contains(searchText)).ToList();
                MoviesDataGrid.ItemsSource = filtered;
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