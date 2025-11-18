using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Media;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace KinoZalMarsBlinVali.Views
{
    public partial class CustomerQuizzesPage : UserControl
    {
        private int _currentCustomerId;

        public CustomerQuizzesPage()
        {
            InitializeComponent();
            _currentCustomerId = AppDataContext.CurrentUser?.EmployeeId ?? 0;
            LoadQuizzes();
        }

        private void LoadQuizzes()
        {
            try
            {
                QuizzesContainer.Children.Clear();

                var quizzes = AppDataContext.DbContext.Quizzes
                    .Where(q => q.IsActive)
                    .ToList();

                Console.WriteLine($"=== ДЕБАГ ИНФОРМАЦИЯ ===");
                Console.WriteLine($"Найдено викторин: {quizzes.Count}");
                foreach (var quiz in quizzes)
                {
                    Console.WriteLine($"Викторина: {quiz.QuizTitle}, ID: {quiz.QuizId}, Активна: {quiz.IsActive}");
                }
                Console.WriteLine($"======================");

                if (!quizzes.Any())
                {
                    var noQuizzesText = new TextBlock
                    {
                        Text = "На данный момент нет доступных викторин",
                        FontSize = 16,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Margin = new Avalonia.Thickness(0, 50, 0, 0)
                    };
                    QuizzesContainer.Children.Add(noQuizzesText);
                    return;
                }

                foreach (var quiz in quizzes)
                {
                    var quizCard = CreateQuizCard(quiz);
                    QuizzesContainer.Children.Add(quizCard);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки викторин: {ex.Message}");
                var errorText = new TextBlock
                {
                    Text = $"Ошибка загрузки викторин: {ex.Message}",
                    FontSize = 14,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Avalonia.Thickness(0, 50, 0, 0),
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                };
                QuizzesContainer.Children.Add(errorText);
            }
        }

        private Border CreateQuizCard(Quiz quiz)
        {
            var card = new Border
            {
                Padding = new Avalonia.Thickness(20),
                Margin = new Avalonia.Thickness(0, 0, 0, 15),
                Background = new SolidColorBrush(Color.Parse("#FFFFFF")),
                BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
                BorderThickness = new Avalonia.Thickness(1),
                CornerRadius = new Avalonia.CornerRadius(8)
            };

            var stackPanel = new StackPanel { Spacing = 12 };

            // Заголовок викторины
            var titleText = new TextBlock
            {
                Text = quiz.QuizTitle,
                FontSize = 18,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#333333"))
            };

            // Описание
            var descText = new TextBlock
            {
                Text = quiz.QuizDescription,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.Parse("#666666"))
            };

            // Информация о награде
            var rewardText = new TextBlock
            {
                Text = $"🎁 Награда: {quiz.BonusPoints} бонусных баллов",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#00A651"))
            };

            // Статус викторины
            var statusInfo = GetQuizStatusInfo(quiz);
            var statusText = new TextBlock
            {
                Text = statusInfo.Message,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.Parse(statusInfo.Color))
            };

            // Кнопка начала
            var startButton = new Button
            {
                Content = statusInfo.CanAttempt ? "🚀 Начать викторину" : "❌ Недоступно",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                Padding = new Avalonia.Thickness(15, 8),
                CornerRadius = new Avalonia.CornerRadius(4)
            };

            if (statusInfo.CanAttempt)
            {
                startButton.Background = new SolidColorBrush(Color.Parse("#28A745"));
                startButton.Foreground = new SolidColorBrush(Color.Parse("#FFFFFF"));
                startButton.Click += (s, e) => StartQuiz(quiz);
            }
            else
            {
                startButton.Background = new SolidColorBrush(Color.Parse("#6C757D"));
                startButton.Foreground = new SolidColorBrush(Color.Parse("#FFFFFF"));
                startButton.IsEnabled = false;
            }

            stackPanel.Children.Add(titleText);
            stackPanel.Children.Add(descText);
            stackPanel.Children.Add(rewardText);
            stackPanel.Children.Add(statusText);
            stackPanel.Children.Add(startButton);

            card.Child = stackPanel;
            return card;
        }

        private (string Message, string Color, bool CanAttempt) GetQuizStatusInfo(Quiz quiz)
        {
            try
            {
                // Получаем все попытки пользователя для этой викторины
                var userAttempts = AppDataContext.DbContext.QuizAttempts
                    .Where(a => a.CustomerId == _currentCustomerId && a.QuizId == quiz.QuizId)
                    .OrderByDescending(a => a.StartedAt)
                    .ToList();

                // Проверяем, были ли уже начислены баллы
                var hasAwardedPoints = userAttempts.Any(a => a.PointsAwarded);
                if (hasAwardedPoints)
                {
                    return ("✅ Вы уже получили баллы за эту викторину", "#00A651", false);
                }

                // Проверяем количество попыток
                var attemptCount = userAttempts.Count;
                var remainingAttempts = 3 - attemptCount;

                if (remainingAttempts <= 0)
                {
                    return ("❌ Превышено максимальное количество попыток (3)", "#DC3545", false);
                }

                // Проверяем, есть ли успешная попытка (более 80%)
                var hasSuccessfulAttempt = userAttempts.Any(a => (a.ScorePercent ?? 0) >= (quiz.PassingScore ?? 80));
                if (hasSuccessfulAttempt)
                {
                    return ("✅ Вы уже успешно прошли викторину", "#00A651", false);
                }

                return ($"🔄 Доступно попыток: {remainingAttempts}/3", "#007BFF", true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения статуса викторины: {ex.Message}");
                return ("🔄 Доступно для прохождения", "#007BFF", true);
            }
        }

        private void StartQuiz(Quiz quiz)
        {
            // Проверяем возможность прохождения перед стартом
            var statusInfo = GetQuizStatusInfo(quiz);
            if (!statusInfo.CanAttempt)
            {
                return;
            }

            // Находим главное окно и переходим на страницу викторины
            var mainWindow = this.FindAncestorOfType<MainWindow>();
            if (mainWindow != null)
            {
                mainWindow.NavigateTo(new QuizPage(quiz));
            }
            else
            {
                // Альтернативный способ найти главное окно
                var visualRoot = this.GetVisualRoot() as MainWindow;
                visualRoot?.NavigateTo(new QuizPage(quiz));
            }
        }
    }
}