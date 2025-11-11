using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class CustomerSessionsPage : UserControl
    {
        private List<Session> _allSessions = new List<Session>();
        private DateTime _selectedDate = DateTime.Today;
        private Dictionary<DateTime, Button> _dateButtons = new Dictionary<DateTime, Button>();

        public CustomerSessionsPage()
        {
            InitializeComponent();
            InitializeDateSelector();
            LoadSessions();
            LoadGenreFilter();
        }

        public CustomerSessionsPage(DateTime selectedDate)
        {
            InitializeComponent();
            _selectedDate = selectedDate;
            InitializeDateSelector();
            LoadSessions();
            LoadGenreFilter();
        }

        private void InitializeDateSelector()
        {
            DatesPanel.Children.Clear();
            _dateButtons.Clear();

            // Создаем кнопки на 14 дней вперед
            for (int i = 0; i < 14; i++)
            {
                var date = DateTime.Today.AddDays(i);
                var dateButton = CreateDateButton(date);
                DatesPanel.Children.Add(dateButton);
                _dateButtons[date] = dateButton;
            }

            // Выделяем выбранную дату
            if (_dateButtons.ContainsKey(_selectedDate))
            {
                SelectDateButton(_dateButtons[_selectedDate], _selectedDate);
            }
        }

        private Button CreateDateButton(DateTime date)
        {
            var dayOfWeek = GetRussianDayOfWeek(date);
            var isToday = date.Date == DateTime.Today.Date;
            var isTomorrow = date.Date == DateTime.Today.AddDays(1).Date;

            var button = new Button
            {
                Content = CreateDateContent(date, dayOfWeek, isToday, isTomorrow),
                Tag = date,
                Padding = new Avalonia.Thickness(20, 12),
                MinWidth = 120,
                Background = Avalonia.Media.Brushes.Transparent,
                BorderBrush = Avalonia.Media.Brushes.LightGray,
                BorderThickness = new Avalonia.Thickness(1)
            };

            button.Click += (sender, e) => DateButton_Click(sender, e, date);
            return button;
        }

        private StackPanel CreateDateContent(DateTime date, string dayOfWeek, bool isToday, bool isTomorrow)
        {
            var panel = new StackPanel
            {
                Spacing = 4,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            // Число и месяц
            panel.Children.Add(new TextBlock
            {
                Text = date.ToString("dd MMMM").ToLower(),
                FontSize = 14,
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            });

            // День недели
            var dayText = isToday ? "СЕГОДНЯ" :
                          isTomorrow ? "ЗАВТРА" :
                          dayOfWeek.ToUpper();

            panel.Children.Add(new TextBlock
            {
                Text = dayText,
                FontSize = 12,
                Foreground = isToday ? Avalonia.Media.Brushes.OrangeRed : Avalonia.Media.Brushes.Gray,
                FontWeight = isToday ? Avalonia.Media.FontWeight.Bold : Avalonia.Media.FontWeight.Normal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            });

            return panel;
        }

        private string GetRussianDayOfWeek(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => "Понедельник",
                DayOfWeek.Tuesday => "Вторник",
                DayOfWeek.Wednesday => "Среда",
                DayOfWeek.Thursday => "Четверг",
                DayOfWeek.Friday => "Пятница",
                DayOfWeek.Saturday => "Суббота",
                DayOfWeek.Sunday => "Воскресенье",
                _ => ""
            };
        }

        private void DateButton_Click(object? sender, RoutedEventArgs e, DateTime date)
        {
            if (sender is Button button)
            {
                _selectedDate = date;
                SelectDateButton(button, date);
                LoadSessions();
            }
        }

        private void SelectDateButton(Button selectedButton, DateTime date)
        {
            // Сбрасываем все кнопки
            foreach (var button in _dateButtons.Values)
            {
                button.Background = Avalonia.Media.Brushes.Transparent;
                button.Foreground = Avalonia.Media.Brushes.Black;
                button.BorderBrush = Avalonia.Media.Brushes.LightGray;
            }

            // Выделяем выбранную
            selectedButton.Background = Avalonia.Media.Brushes.Blue;
            selectedButton.Foreground = Avalonia.Media.Brushes.White;
            selectedButton.BorderBrush = Avalonia.Media.Brushes.Blue;
        }

        private void LoadSessions()
        {
            try
            {
                var startOfDay = _selectedDate.Date;
                var endOfDay = _selectedDate.Date.AddDays(1).AddSeconds(-1);

                _allSessions = AppDataContext.DbContext.Sessions
                    .Include(s => s.Movie)
                    .Include(s => s.Hall)
                    .Include(s => s.Tickets)
                    .Where(s => s.StartTime >= startOfDay &&
                               s.StartTime <= endOfDay &&
                               s.Movie.IsActive == true)
                    .OrderBy(s => s.Movie.Title)
                    .ThenBy(s => s.StartTime)
                    .ToList();

                Console.WriteLine($"=== LOADED SESSIONS FOR {_selectedDate:dd.MM.yyyy} ===");

                CreateMovieCards();
                UpdateDateTitle();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки сеансов: {ex.Message}");
            }
        }

        private void CreateMovieCards()
        {
            var movies = _allSessions
                .Select(s => s.Movie)
                .Distinct()
                .ToList();

            var movieCards = new List<Control>();

            foreach (var movie in movies)
            {
                var movieSessions = _allSessions
                    .Where(s => s.MovieId == movie.MovieId)
                    .OrderBy(s => s.StartTime)
                    .ToList();

                var movieCard = CreateMovieCard(movie, movieSessions);
                movieCards.Add(movieCard);
            }

            MoviesItemsControl.ItemsSource = movieCards;
        }

        private Border CreateMovieCard(Movie movie, List<Session> sessions)
        {
            var card = new Border
            {
                Background = Avalonia.Media.Brushes.White,
                CornerRadius = new Avalonia.CornerRadius(10),
                BorderBrush = Avalonia.Media.Brushes.LightGray,
                BorderThickness = new Avalonia.Thickness(1),
                Margin = new Avalonia.Thickness(0, 0, 0, 15),
                Padding = new Avalonia.Thickness(20),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
            };

            card.PointerPressed += (sender, e) => OpenMovieDetails(movie);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Постер
            var posterBorder = new Border
            {
                Width = 120,
                Height = 180,
                Background = Avalonia.Media.Brushes.LightGray,
                CornerRadius = new Avalonia.CornerRadius(5),
                Margin = new Avalonia.Thickness(0, 0, 20, 0)
            };

            var image = new Avalonia.Controls.Image
            {
                Width = 120,
                Height = 180,
                Stretch = Avalonia.Media.Stretch.UniformToFill
            };

            try
            {
                var converter = new KinoZalMarsBlinVali.Converters.ImagePathConverter();
                var bitmap = converter.Convert(movie.PosterPath, typeof(Avalonia.Media.Imaging.Bitmap), null, System.Globalization.CultureInfo.CurrentCulture);
                if (bitmap is Avalonia.Media.Imaging.Bitmap convertedBitmap)
                {
                    image.Source = convertedBitmap;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка загрузки изображения {movie.PosterPath}: {ex.Message}");
            }

            posterBorder.Child = image;
            Grid.SetColumn(posterBorder, 0);
            grid.Children.Add(posterBorder);

            // Информация о фильме
            var infoPanel = new StackPanel { Spacing = 8 };

            var titleText = new TextBlock
            {
                Text = movie.Title,
                FontSize = 18,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = Avalonia.Media.Brushes.Blue
            };
            infoPanel.Children.Add(titleText);

            var detailsPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 15
            };

            detailsPanel.Children.Add(new TextBlock
            {
                Text = movie.Genre,
                FontSize = 14,
                Foreground = Avalonia.Media.Brushes.Gray
            });

            detailsPanel.Children.Add(new TextBlock
            {
                Text = $"{movie.AgeRating}+",
                FontSize = 14,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = Avalonia.Media.Brushes.OrangeRed
            });

            detailsPanel.Children.Add(new TextBlock
            {
                Text = $"{movie.DurationMinutes} мин",
                FontSize = 14,
                Foreground = Avalonia.Media.Brushes.Gray
            });

            infoPanel.Children.Add(detailsPanel);

            infoPanel.Children.Add(new TextBlock
            {
                Text = $"Режиссер: {movie.Director}",
                FontSize = 12,
                Foreground = Avalonia.Media.Brushes.Gray
            });

            var shortDescription = movie.Description?.Length > 100
                ? movie.Description.Substring(0, 100) + "..."
                : movie.Description;

            infoPanel.Children.Add(new TextBlock
            {
                Text = shortDescription,
                FontSize = 12,
                Foreground = Avalonia.Media.Brushes.Gray,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                MaxWidth = 400,
                MaxHeight = 40,
                TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis
            });

            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            // Времена сеансов
            var sessionsPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 8
            };

            foreach (var session in sessions.Take(10))
            {
                var sessionButton = new Button
                {
                    Content = session.StartTime.ToString("HH:mm"),
                    Background = Avalonia.Media.Brushes.Blue,
                    Foreground = Avalonia.Media.Brushes.White,
                    Padding = new Avalonia.Thickness(12, 6),
                    FontSize = 11,
                    Tag = session.SessionId
                };

                sessionButton.Click += (sender, e) => BookSession(session.SessionId);
                sessionsPanel.Children.Add(sessionButton);
            }

            var sessionsScroll = new ScrollViewer
            {
                Content = sessionsPanel,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                MaxWidth = 300,
                MaxHeight = 120
            };

            Grid.SetColumn(sessionsScroll, 2);
            grid.Children.Add(sessionsScroll);

            card.Child = grid;
            return card;
        }

        private void UpdateDateTitle()
        {
            if (_selectedDate.Date == DateTime.Today.Date)
            {
                DateTitleText.Text = "Сегодня в кино";
            }
            else if (_selectedDate.Date == DateTime.Today.AddDays(1).Date)
            {
                DateTitleText.Text = "Завтра в кино";
            }
            else
            {
                DateTitleText.Text = $"Сеансы на {_selectedDate:dd.MM.yyyy}";
            }
        }

        private void OpenMovieDetails(Movie movie)
        {
            Console.WriteLine($"🎬 Открываем детали фильма: {movie.Title}");
            var movieSessions = _allSessions.Where(s => s.MovieId == movie.MovieId).ToList();

            var customerMainPage = this.FindAncestorOfType<CustomerMainPage>();
            if (customerMainPage != null)
            {
                customerMainPage.MainContentControl.Content = new MovieDetailsPage(movie, movieSessions, _selectedDate);
            }
        }

        private void BookSession(int sessionId)
        {
            var session = _allSessions.FirstOrDefault(s => s.SessionId == sessionId);
            if (session != null)
            {
                Console.WriteLine($"🎫 Бронирование сеанса: {session.Movie.Title} в {session.StartTime:HH:mm}");

                var customerMainPage = this.FindAncestorOfType<CustomerMainPage>();
                if (customerMainPage != null)
                {
                    customerMainPage.MainContentControl.Content = new BookingPage(session);
                }
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

        private void Search_Click(object? sender, RoutedEventArgs e)
        {
            LoadSessions();
        }

        private void Refresh_Click(object? sender, RoutedEventArgs e)
        {
            LoadSessions();
        }

        private void Today_Click(object? sender, RoutedEventArgs e)
        {
            _selectedDate = DateTime.Today;
            InitializeDateSelector();
            LoadSessions();
        }

        private async void ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            await dialog.ShowDialog((Window)this.VisualRoot);
        }
    }
}