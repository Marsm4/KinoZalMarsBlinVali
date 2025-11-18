using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KinoZalMarsBlinVali.Views
{
    public partial class AdminQuizzesPage : UserControl
    {
        public AdminQuizzesPage()
        {
            InitializeComponent();
            LoadQuizzes();
        }

        private void LoadQuizzes()
        {
            try
            {
                QuizzesContainer.Children.Clear();

                var quizzes = AppDataContext.DbContext.Quizzes
                    .OrderByDescending(q => q.CreatedAt)
                    .ToList();

                if (!quizzes.Any())
                {
                    var noQuizzesText = new TextBlock
                    {
                        Text = "Пока нет созданных викторин",
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
                    Margin = new Avalonia.Thickness(0, 50, 0, 0)
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

            // Заголовок и статус
            var headerPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };

            var titleText = new TextBlock
            {
                Text = quiz.QuizTitle,
                FontSize = 18,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#333333")),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            var statusBadge = new Border
            {
                Background = new SolidColorBrush(quiz.IsActive ? Color.Parse("#28A745") : Color.Parse("#6C757D")),
                Padding = new Avalonia.Thickness(8, 4),
                CornerRadius = new Avalonia.CornerRadius(12),
                Margin = new Avalonia.Thickness(10, 0, 0, 0),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            statusBadge.Child = new TextBlock
            {
                Text = quiz.IsActive ? "Активна" : "Неактивна",
                Foreground = new SolidColorBrush(Color.Parse("#FFFFFF")),
                FontSize = 12,
                FontWeight = Avalonia.Media.FontWeight.SemiBold
            };

            headerPanel.Children.Add(titleText);
            headerPanel.Children.Add(statusBadge);

            // Описание
            var descText = new TextBlock
            {
                Text = quiz.QuizDescription ?? "Описание отсутствует",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.Parse("#666666"))
            };

            // Информация
            var infoPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 20 };

            var bonusText = new TextBlock
            {
                Text = $"🏆 Баллы: {quiz.BonusPoints}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#007BFF"))
            };

            var passingText = new TextBlock
            {
                Text = $"🎯 Проходной балл: {quiz.PassingScore}%",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#6C757D"))
            };

            var questionsCount = AppDataContext.DbContext.QuizQuestions.Count(q => q.QuizId == quiz.QuizId);
            var questionsText = new TextBlock
            {
                Text = $"❓ Вопросов: {questionsCount}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#6C757D"))
            };

            infoPanel.Children.Add(bonusText);
            infoPanel.Children.Add(passingText);
            infoPanel.Children.Add(questionsText);

            // Кнопки управления
            var buttonsPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10 };

            var editButton = new Button
            {
                Content = "✏️ Редактировать",
                Background = new SolidColorBrush(Color.Parse("#FFC107")),
                Foreground = new SolidColorBrush(Color.Parse("#212529")),
                Padding = new Avalonia.Thickness(12, 6),
                FontSize = 12
            };
            editButton.Click += (s, e) => EditQuiz(quiz);

            var toggleButton = new Button
            {
                Content = quiz.IsActive ? "⏸️ Деактивировать" : "▶️ Активировать",
                Background = new SolidColorBrush(quiz.IsActive ? Color.Parse("#DC3545") : Color.Parse("#28A745")),
                Foreground = new SolidColorBrush(Color.Parse("#FFFFFF")),
                Padding = new Avalonia.Thickness(12, 6),
                FontSize = 12
            };
            toggleButton.Click += (s, e) => ToggleQuiz(quiz);

            var deleteButton = new Button
            {
                Content = "🗑️ Удалить",
                Background = new SolidColorBrush(Color.Parse("#DC3545")),
                Foreground = new SolidColorBrush(Color.Parse("#FFFFFF")),
                Padding = new Avalonia.Thickness(12, 6),
                FontSize = 12
            };
            deleteButton.Click += (s, e) => DeleteQuiz(quiz);

            buttonsPanel.Children.Add(editButton);
            buttonsPanel.Children.Add(toggleButton);
            buttonsPanel.Children.Add(deleteButton);

            stackPanel.Children.Add(headerPanel);
            stackPanel.Children.Add(descText);
            stackPanel.Children.Add(infoPanel);
            stackPanel.Children.Add(buttonsPanel);

            card.Child = stackPanel;
            return card;
        }

        private void CreateQuiz_Click(object? sender, RoutedEventArgs e)
        {
            // Открываем окно создания викторины
            var createWindow = new CreateQuizWindow();
            createWindow.QuizCreated += (s, quiz) => LoadQuizzes();

            var visualRoot = this.VisualRoot as Window;
            if (visualRoot != null)
            {
                createWindow.ShowDialog(visualRoot);
            }
        }

        private async void EditQuiz(Quiz quiz)
        {
            // Открываем окно редактирования викторины
            var editWindow = new EditQuizWindow(quiz);
            editWindow.QuizUpdated += (s, e) => LoadQuizzes();

            var visualRoot = this.VisualRoot as Window;
            if (visualRoot != null)
            {
                await editWindow.ShowDialog(visualRoot);
            }
        }

        private async void ToggleQuiz(Quiz quiz)
        {
            try
            {
                quiz.IsActive = !quiz.IsActive;
                await AppDataContext.DbContext.SaveChangesAsync();
                LoadQuizzes();

                var message = quiz.IsActive ? "Викторина активирована" : "Викторина деактивирована";
                await ShowMessage("Успех", message);
            }
            catch (Exception ex)
            {
                await ShowMessage("Ошибка", $"Не удалось изменить статус викторины: {ex.Message}");
            }
        }

        private async void DeleteQuiz(Quiz quiz)
        {
            try
            {
                var confirmDialog = new MessageWindow("Подтверждение",
                    $"Вы уверены, что хотите удалить викторину \"{quiz.QuizTitle}\"?");

                var visualRoot = this.VisualRoot as Window;
                if (visualRoot != null)
                {
                    await confirmDialog.ShowDialog(visualRoot);

                    // MessageWindow не возвращает результат, поэтому просто удаляем после подтверждения
                    // Удаляем связанные ответы и вопросы
                    var questions = AppDataContext.DbContext.QuizQuestions
                        .Where(q => q.QuizId == quiz.QuizId).ToList();

                    foreach (var question in questions)
                    {
                        var answers = AppDataContext.DbContext.QuizAnswers
                            .Where(a => a.QuestionId == question.QuestionId).ToList();
                        AppDataContext.DbContext.QuizAnswers.RemoveRange(answers);
                    }

                    AppDataContext.DbContext.QuizQuestions.RemoveRange(questions);
                    AppDataContext.DbContext.Quizzes.Remove(quiz);
                    await AppDataContext.DbContext.SaveChangesAsync();

                    LoadQuizzes();
                    await ShowMessage("Успех", "Викторина удалена");
                }
            }
            catch (Exception ex)
            {
                await ShowMessage("Ошибка", $"Не удалось удалить викторину: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task ShowMessage(string title, string message)
        {
            var dialog = new MessageWindow(title, message);
            var visualRoot = this.VisualRoot as Window;
            if (visualRoot != null)
            {
                await dialog.ShowDialog(visualRoot);
            }
        }
    }
}