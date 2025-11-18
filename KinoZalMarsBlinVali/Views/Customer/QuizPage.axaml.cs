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
        private int _currentQuestionIndex = 0;
        private Dictionary<int, int> _userAnswers = new Dictionary<int, int>();
        private int _currentCustomerId;

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

                // Показываем первый вопрос
                DisplayCurrentQuestion();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации QuizPage: {ex.Message}");
                await ShowErrorAndReturnAsync("Ошибка загрузки викторины");
            }
        }

        private async Task ShowErrorAndReturnAsync(string message)
        {
            // Асинхронная версия для использования в async методах
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
                // Явно загружаем данные без сложных LINQ преобразований
                var userAttempts = AppDataContext.DbContext.QuizAttempts
                    .Where(a => a.CustomerId == _currentCustomerId && a.QuizId == _quiz.QuizId)
                    .ToList();

                // Проверяем начисление баллов (используем GetValueOrDefault для nullable)
                if (userAttempts.Any(a => a.PointsAwarded))
                {
                    ShowErrorAndReturn("Вы уже получили баллы за эту викторину");
                    return false;
                }

                // Проверяем количество попыток
                if (userAttempts.Count >= 3)
                {
                    ShowErrorAndReturn("Превышено максимальное количество попыток (3)");
                    return false;
                }

                // Проверяем успешное прохождение (используем GetValueOrDefault для nullable)
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
                    // Если VisualRoot недоступен, просто возвращаемся
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
        private void DisplayCurrentQuestion()
        {
            QuestionContainer.Children.Clear();

            if (_currentQuestionIndex >= _questions.Count)
            {
                CompleteQuiz();
                return;
            }

            var currentQuestion = _questions[_currentQuestionIndex];

            // Текст вопроса
            var questionText = new TextBlock
            {
                Text = currentQuestion.QuestionText,
                FontSize = 16,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            QuestionContainer.Children.Add(questionText);

            // Варианты ответов
            var questionAnswers = _answers.Where(a => a.QuestionId == currentQuestion.QuestionId).ToList();

            foreach (var answer in questionAnswers)
            {
                var radioButton = new RadioButton
                {
                    Content = answer.AnswerText,
                    Tag = answer.AnswerId,
                    GroupName = $"question_{currentQuestion.QuestionId}",
                    Margin = new Avalonia.Thickness(0, 5, 0, 5)
                };

                // Восстанавливаем выбранный ответ, если он был
                if (_userAnswers.ContainsKey(currentQuestion.QuestionId) &&
                    _userAnswers[currentQuestion.QuestionId] == answer.AnswerId)
                {
                    radioButton.IsChecked = true;
                }

                radioButton.Checked += (s, e) =>
                {
                    _userAnswers[currentQuestion.QuestionId] = answer.AnswerId;
                };

                QuestionContainer.Children.Add(radioButton);
            }

            UpdateNavigationButtons();
        }

        private void UpdateProgress()
        {
            ProgressText.Text = $"Вопрос {_currentQuestionIndex + 1} из {_questions.Count}";
        }

        private void UpdateNavigationButtons()
        {
            PrevButton.IsEnabled = _currentQuestionIndex > 0;
            NextButton.Content = _currentQuestionIndex == _questions.Count - 1 ? "Завершить" : "Далее ➡️";

            // Убедимся, что классы установлены правильно
            if (!PrevButton.Classes.Contains("secondary"))
            {
                PrevButton.Classes.Add("secondary");
            }

            if (!NextButton.Classes.Contains("primary"))
            {
                NextButton.Classes.Add("primary");
            }
        }

        private void PrevButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentQuestionIndex > 0)
            {
                _currentQuestionIndex--;
                DisplayCurrentQuestion();
                UpdateProgress();
            }
        }

        private void NextButton_Click(object? sender, RoutedEventArgs e)
        {
            // Проверяем, что вопросы загружены
            if (_questions == null || !_questions.Any())
            {
                ShowErrorAndReturn("Вопросы не загружены");
                return;
            }

            if (_currentQuestionIndex < _questions.Count - 1)
            {
                _currentQuestionIndex++;
                DisplayCurrentQuestion();
                UpdateProgress();
            }
            else
            {
                CompleteQuiz();
            }
        }

        private async void CompleteQuiz()
        {
            // Подсчет результатов
            int correctAnswers = 0;

            foreach (var question in _questions)
            {
                if (_userAnswers.ContainsKey(question.QuestionId))
                {
                    var selectedAnswerId = _userAnswers[question.QuestionId];
                    var selectedAnswer = _answers.FirstOrDefault(a => a.AnswerId == selectedAnswerId);

                    if (selectedAnswer?.IsCorrect == true) // Теперь IsCorrect не nullable
                    {
                        correctAnswers++;
                    }
                }
            }

            double scorePercent = (double)correctAnswers / _questions.Count * 100;
            bool passed = scorePercent >= (_quiz.PassingScore ?? 80); // Используем GetValueOrDefault
            int earnedPoints = passed ? (_quiz.BonusPoints ?? 0) : 0;
            bool pointsAwarded = passed && earnedPoints > 0;

            // Сохраняем попытку
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

            // Начисляем бонусные баллы только если прошли успешно
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

            // Показываем результаты
            var resultWindow = new QuizResultWindow(scorePercent, correctAnswers, _questions.Count, earnedPoints, passed);

            var visualRoot = this.VisualRoot as Window;
            if (visualRoot != null)
            {
                await resultWindow.ShowDialog(visualRoot);
            }

            // Возвращаемся к списку викторин
            ReturnToQuizzesPage();
        }
    }
}