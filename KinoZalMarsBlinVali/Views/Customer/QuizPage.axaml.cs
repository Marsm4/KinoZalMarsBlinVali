using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System.Collections.Generic;
using System.Linq;

namespace KinoZalMarsBlinVali.Views
{
    public partial class QuizPage : UserControl
    {
        private Quiz _quiz;
        private List<QuizQuestion> _questions;
        private List<QuizAnswer> _answers;
        private int _currentQuestionIndex = 0;
        private Dictionary<int, int> _userAnswers = new Dictionary<int, int>();

        public QuizPage(Quiz quiz)
        {
            InitializeComponent();
            _quiz = quiz;
            LoadQuizData();
            DisplayCurrentQuestion();
        }

        private void LoadQuizData()
        {
            _questions = AppDataContext.DbContext.QuizQuestions
                .Where(q => q.QuizId == _quiz.QuizId)
                .OrderBy(q => q.QuestionOrder)
                .ToList();

            var questionIds = _questions.Select(q => q.QuestionId).ToList();
            _answers = AppDataContext.DbContext.QuizAnswers
                .Where(a => questionIds.Contains(a.QuestionId))
                .OrderBy(a => a.AnswerOrder)
                .ToList();

            QuizTitleText.Text = _quiz.QuizTitle;
            UpdateProgress();
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

                    if (selectedAnswer?.IsCorrect == true)
                    {
                        correctAnswers++;
                    }
                }
            }

            double scorePercent = (double)correctAnswers / _questions.Count * 100;
            bool passed = scorePercent >= _quiz.PassingScore;
            int earnedPoints = passed ? _quiz.BonusPoints ?? 0 : 0;

            // Сохраняем попытку
            var attempt = new QuizAttempt
            {
                CustomerId = AppDataContext.CurrentUser.EmployeeId,
                QuizId = _quiz.QuizId,
                CompletedAt = System.DateTime.Now,
                ScorePercent = (decimal)scorePercent,
                EarnedPoints = earnedPoints
            };

            AppDataContext.DbContext.QuizAttempts.Add(attempt);

            // Начисляем бонусные баллы
            if (passed && earnedPoints > 0)
            {
                var customer = AppDataContext.DbContext.Customers
                    .FirstOrDefault(c => c.CustomerId == AppDataContext.CurrentUser.EmployeeId);

                if (customer != null)
                {
                    customer.BonusPoints = (customer.BonusPoints ?? 0) + earnedPoints;
                    customer.TotalQuizPoints = (customer.TotalQuizPoints ?? 0) + earnedPoints;
                    customer.QuizzesCompleted = (customer.QuizzesCompleted ?? 0) + 1;
                    customer.LastQuizAttempt = System.DateTime.Now;
                }
            }

            await AppDataContext.DbContext.SaveChangesAsync();

            // Показываем результаты
            var resultWindow = new QuizResultWindow(scorePercent, correctAnswers, _questions.Count, earnedPoints, passed);
            await resultWindow.ShowDialog((Window)this.VisualRoot);

            // Возвращаемся к списку викторин
            if (this.VisualRoot is MainWindow mainWindow)
            {
                mainWindow.NavigateTo(new CustomerQuizzesPage());
            }
        }
    }
}