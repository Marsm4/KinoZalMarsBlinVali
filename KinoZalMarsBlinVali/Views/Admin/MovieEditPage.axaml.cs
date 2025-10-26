using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System;
using System.IO;
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

        private async void SelectImage_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                // Настройка диалога выбора файла
                var fileType = new FilePickerFileType("Изображения")
                {
                    Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" },
                    MimeTypes = new[] { "image/jpeg", "image/png", "image/bmp", "image/gif" }
                };

                var options = new FilePickerOpenOptions
                {
                    Title = "Выберите постер фильма",
                    FileTypeFilter = new[] { fileType },
                    AllowMultiple = false
                };

                // Получаем TopLevel для показа диалога
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel != null)
                {
                    var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);

                    if (files.Count > 0)
                    {
                        var selectedFile = files[0];

                        // Копируем файл в папку проекта
                        var imagePath = await CopyImageToProject(selectedFile);
                        PosterPathTextBox.Text = imagePath;
                    }
                }
            }
            catch (Exception ex)
            {
                 ShowError($"Ошибка при выборе файла: {ex.Message}");
            }
        }

        private async Task<string> CopyImageToProject(IStorageFile sourceFile)
        {
            try
            {
                // Определяем правильный путь к папке проекта
                var currentDir = Directory.GetCurrentDirectory();
                string projectRoot;

                // Если мы в bin/Debug или bin/Release
                if (currentDir.Contains("bin\\Debug") || currentDir.Contains("bin\\Release"))
                {
                    projectRoot = Path.GetFullPath(Path.Combine(currentDir, "..", "..", ".."));
                }
                else
                {
                    projectRoot = Path.GetFullPath(Path.Combine(currentDir, "..", ".."));
                }

                var postersDir = Path.Combine(projectRoot, "Assets", "Posters");

                // Создаем директорию, если не существует
                if (!Directory.Exists(postersDir))
                {
                    Directory.CreateDirectory(postersDir);
                    Console.WriteLine($"✅ Создана папка: {postersDir}");
                }

                // Генерируем уникальное имя файла
                var fileName = $"poster_{Guid.NewGuid():N}{Path.GetExtension(sourceFile.Name)}";
                var destinationPath = Path.Combine(postersDir, fileName);

                // Копируем файл
                using var sourceStream = await sourceFile.OpenReadAsync();
                using var destinationStream = File.Create(destinationPath);
                await sourceStream.CopyToAsync(destinationStream);

                // Возвращаем относительный путь для БД - БЕЗ начального слеша!
                var relativePath = $"Assets/Posters/{fileName}";

                Console.WriteLine($"✅ Файл сохранен: {destinationPath}");
                Console.WriteLine($"✅ Относительный путь для БД: {relativePath}");
                Console.WriteLine($"✅ Файл существует: {File.Exists(destinationPath)}");
                Console.WriteLine($"✅ Проект корень: {projectRoot}");

                return relativePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка копирования файла: {ex.Message}");
                throw;
            }
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

                await AppDataContext.DbContext.SaveChangesAsync();

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