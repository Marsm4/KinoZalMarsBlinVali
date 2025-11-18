using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class QuizPage : UserControl
    {
        private Quiz _quiz;
        private List<QuizQuestion> _questions;
        private List<QuizAnswer> _answers;
        private Dictionary<int, int> _userAnswers = new Dictionary<int, int>();
        private int _currentCustomerId;
        private bool _quizCompleted = false;

        public QuizPage(Quiz quiz)
        {
            InitializeComponent();
            _quiz = quiz;
            _currentCustomerId = AppDataContext.CurrentUser?.EmployeeId ?? 0;

            Console.WriteLine($"Создание QuizPage для викторины: {_quiz.QuizTitle} (ID: {_quiz.QuizId})");

            // Используем Loaded событие для гарантии инициализации
            this.Loaded += OnQuizPageLoaded;
        }

        private async void OnQuizPageLoaded(object? sender, RoutedEventArgs e)
        {
            this.Loaded -= OnQuizPageLoaded;

            try
            {
                // Проверяем возможность прохождения
                if (!CanAttemptQuiz())
                {
                    return;
                }

                // Загружаем данные
                LoadQuizData();

                // Проверяем что вопросы загружены
                if (_questions == null || !_questions.Any())
                {
                    await ShowErrorAndReturnAsync("В этой викторине пока нет вопросов");
                    return;
                }

                // Показываем все вопросы сразу
                DisplayAllQuestions();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации QuizPage: {ex.Message}");
                await ShowErrorAndReturnAsync("Ошибка загрузки викторины");
            }
        }

        private void BackToQuizzes_Click(object? sender, RoutedEventArgs e)
        {
            if (!_quizCompleted)
            {
                // Показываем подтверждение отмены викторины
                ShowCancelConfirmation();
            }
            else
            {
                ReturnToQuizzesPage();
            }
        }

        private void CancelQuiz_Click(object? sender, RoutedEventArgs e)
        {
            ShowCancelConfirmation();
        }

        private async void ShowCancelConfirmation()
        {
            try
            {
                var confirmDialog = new MessageWindow("Подтверждение",
                    "Вы уверены, что хотите отменить викторину? Прогресс будет потерян.");

                var visualRoot = this.VisualRoot as Window;
                if (visualRoot != null)
                {
                    // Показываем диалог и ждем результат
                    var result = await confirmDialog.ShowDialog<bool>(visualRoot);
                    if (result) // Если пользователь подтвердил отмену
                    {
                        await SaveFailedAttempt();
                        ReturnToQuizzesPage();
                    }
                    // Если отмена не подтверждена, остаемся на странице викторины
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подтверждения отмены: {ex.Message}");
                // При ошибке тоже остаемся на странице викторины
            }
        }

        private async Task SaveFailedAttempt()
        {
            try
            {
                // Сохраняем неудачную попытку
                var attempt = new QuizAttempt
                {
                    CustomerId = _currentCustomerId,
                    QuizId = _quiz.QuizId,
                    StartedAt = DateTime.Now,
                    CompletedAt = DateTime.Now,
                    TotalQuestions = _questions?.Count ?? 0,
                    CorrectAnswers = 0,
                    ScorePercent = 0,
                    EarnedPoints = 0,
                    PointsAwarded = false
                };

                AppDataContext.DbContext.QuizAttempts.Add(attempt);
                await AppDataContext.DbContext.SaveChangesAsync();

                Console.WriteLine("Сохранена неудачная попытка викторины");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения неудачной попытки: {ex.Message}");
            }
        }

        private async Task ShowErrorAndReturnAsync(string message)
        {
            try
            {
                var visualRoot = this.VisualRoot as Window;
                if (visualRoot != null)
                {
                    var dialog = new MessageWindow("Ошибка", message);
                    await dialog.ShowDialog(visualRoot);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка показа диалога: {ex.Message}");
            }

            ReturnToQuizzesPage();
        }

        private bool CanAttemptQuiz()
        {
            try
            {
                var userAttempts = AppDataContext.DbContext.QuizAttempts
                    .Where(a => a.CustomerId == _currentCustomerId && a.QuizId == _quiz.QuizId)
                    .ToList();

                if (userAttempts.Any(a => a.PointsAwarded))
                {
                    ShowErrorAndReturn("Вы уже получили баллы за эту викторину");
                    return false;
                }

                if (userAttempts.Count >= 3)
                {
                    ShowErrorAndReturn("Превышено максимальное количество попыток (3)");
                    return false;
                }

                if (userAttempts.Any(a => (a.ScorePercent ?? 0) >= (_quiz.PassingScore ?? 80)))
                {
                    ShowErrorAndReturn("Вы уже успешно прошли эту викторину");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в CanAttemptQuiz: {ex.Message}");
                ShowErrorAndReturn("Ошибка проверки возможности прохождения викторины");
                return false;
            }
        }

        private async void ShowErrorAndReturn(string message)
        {
            try
            {
                var visualRoot = this.VisualRoot as Window;
                if (visualRoot != null)
                {
                    var dialog = new MessageWindow("Ошибка", message);
                    await dialog.ShowDialog(visualRoot);
                }
                else
                {
                    Console.WriteLine($"VisualRoot is null. Message: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка показа диалога: {ex.Message}");
            }

            ReturnToQuizzesPage();
        }

        private void ReturnToQuizzesPage()
        {
            if (this.VisualRoot is MainWindow mainWindow)
            {
                // Просто возвращаемся на страницу викторин
                mainWindow.NavigateTo(new CustomerQuizzesPage());
            }
        }

        private void LoadQuizData()
        {
            try
            {
                Console.WriteLine($"Загрузка данных викторины ID: {_quiz.QuizId}");

                _questions = AppDataContext.DbContext.QuizQuestions
                    .Where(q => q.QuizId == _quiz.QuizId)
                    .OrderBy(q => q.QuestionOrder)
                    .ToList();

                Console.WriteLine($"Загружено вопросов: {_questions?.Count ?? 0}");

                if (_questions == null || !_questions.Any())
                {
                    ShowErrorAndReturn("В этой викторине нет вопросов");
                    return;
                }

                var questionIds = _questions.Select(q => q.QuestionId).ToList();
                _answers = AppDataContext.DbContext.QuizAnswers
                    .Where(a => questionIds.Contains(a.QuestionId))
                    .OrderBy(a => a.AnswerOrder)
                    .ToList();

                Console.WriteLine($"Загружено ответов: {_answers?.Count ?? 0}");

                QuizTitleText.Text = _quiz.QuizTitle;
                UpdateProgress();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки данных викторины: {ex.Message}");
                ShowErrorAndReturn("Ошибка загрузки викторины");
            }
        }

        private void DisplayAllQuestions()
        {
            QuestionsContainer.Children.Clear();

            if (_questions == null || !_questions.Any())
            {
                ShowErrorAndReturn("Вопросы не загружены");
                return;
            }

            foreach (var question in _questions)
            {
                var questionCard = CreateQuestionCard(question);
                QuestionsContainer.Children.Add(questionCard);
            }

            UpdateProgress();
        }

        private Border CreateQuestionCard(QuizQuestion question)
        {
            var card = new Border
            {
                Padding = new Avalonia.Thickness(15),
                Margin = new Avalonia.Thickness(0, 0, 0, 10),
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#F8F9FA")),
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#DEE2E6")),
                BorderThickness = new Avalonia.Thickness(1),
                CornerRadius = new Avalonia.CornerRadius(6)
            };

            var stackPanel = new StackPanel { Spacing = 10 };

            // Текст вопроса
            var questionText = new TextBlock
            {
                Text = $"{question.QuestionOrder}. {question.QuestionText}",
                FontSize = 16,
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            stackPanel.Children.Add(questionText);

            // Ответы
            var questionAnswers = _answers.Where(a => a.QuestionId == question.QuestionId).ToList();
            var answersStackPanel = new StackPanel { Spacing = 5, Margin = new Avalonia.Thickness(10, 0, 0, 0) };

            foreach (var answer in questionAnswers)
            {
                var radioButton = new RadioButton
                {
                    Content = answer.AnswerText,
                    Tag = answer.AnswerId,
                    GroupName = $"question_{question.QuestionId}",
                    Margin = new Avalonia.Thickness(0, 5, 0, 0)
                };

                if (_userAnswers.ContainsKey(question.QuestionId) &&
                    _userAnswers[question.QuestionId] == answer.AnswerId)
                {
                    radioButton.IsChecked = true;
                }

                radioButton.Checked += (s, e) =>
                {
                    _userAnswers[question.QuestionId] = answer.AnswerId;
                    UpdateProgress();
                };

                answersStackPanel.Children.Add(radioButton);
            }

            stackPanel.Children.Add(answersStackPanel);
            card.Child = stackPanel;

            return card;
        }

        private void UpdateProgress()
        {
            if (_questions == null) return;

            int answeredCount = _userAnswers.Count;
            int totalCount = _questions.Count;

            ProgressText.Text = $"Отвечено: {answeredCount} из {totalCount} вопросов";

            // Обновляем состояние кнопки завершения
            CompleteButton.IsEnabled = answeredCount == totalCount;
            CompleteButton.Content = answeredCount == totalCount ?
                "✅ Завершить викторину" :
                $"✅ Завершить ({answeredCount}/{totalCount})";
        }

        private void CompleteButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_userAnswers.Count < _questions.Count)
            {
                ShowCompletionWarning();
                return;
            }

            CompleteQuiz();
        }

        private async void ShowCompletionWarning()
        {
            try
            {
                var unansweredCount = _questions.Count - _userAnswers.Count;
                var confirmDialog = new MessageWindow("Предупреждение",
                    $"Вы ответили не на все вопросы. Осталось {unansweredCount} без ответа.\n\n" +
                    "Вы уверены, что хотите завершить викторину?");

                var visualRoot = this.VisualRoot as Window;
                if (visualRoot != null)
                {
                    var result = await confirmDialog.ShowDialog<bool>(visualRoot);
                    if (result)
                    {
                        CompleteQuiz();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка показа предупреждения: {ex.Message}");
            }
        }

        private async void CompleteQuiz()
        {
            _quizCompleted = true;

            int correctAnswers = 0;

            foreach (var question in _questions)
            {
                if (_userAnswers.ContainsKey(question.QuestionId))
                {
                    var selectedAnswerId = _userAnswers[question.QuestionId];
                    var selectedAnswer = _answers.FirstOrDefault(a => a.AnswerId == selectedAnswerId);

                    if (selectedAnswer?.IsCorrect == true)
                    {
                        correctAnswers++;
                    }
                }
            }

            double scorePercent = (double)correctAnswers / _questions.Count * 100;
            bool passed = scorePercent >= (_quiz.PassingScore ?? 80);
            int earnedPoints = passed ? (_quiz.BonusPoints ?? 0) : 0;
            bool pointsAwarded = passed && earnedPoints > 0;

            var attempt = new QuizAttempt
            {
                CustomerId = _currentCustomerId,
                QuizId = _quiz.QuizId,
                StartedAt = DateTime.Now,
                CompletedAt = DateTime.Now,
                TotalQuestions = _questions.Count,
                CorrectAnswers = correctAnswers,
                ScorePercent = (decimal)scorePercent,
                EarnedPoints = earnedPoints,
                PointsAwarded = pointsAwarded
            };

            AppDataContext.DbContext.QuizAttempts.Add(attempt);

            if (pointsAwarded)
            {
                var customer = AppDataContext.DbContext.Customers
                    .FirstOrDefault(c => c.CustomerId == _currentCustomerId);

                if (customer != null)
                {
                    customer.BonusPoints = (customer.BonusPoints ?? 0) + earnedPoints;
                    customer.TotalQuizPoints = (customer.TotalQuizPoints ?? 0) + earnedPoints;
                    customer.QuizzesCompleted = (customer.QuizzesCompleted ?? 0) + 1;
                    customer.LastQuizAttempt = DateTime.Now;
                }
            }

            await AppDataContext.DbContext.SaveChangesAsync();

            var resultWindow = new QuizResultWindow(scorePercent, correctAnswers, _questions.Count, earnedPoints, passed);

            var visualRoot = this.VisualRoot as Window;
            if (visualRoot != null)
            {
                await resultWindow.ShowDialog(visualRoot);
            }

            ReturnToQuizzesPage();
        }
    }
}