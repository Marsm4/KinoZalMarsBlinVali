using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System.Collections.Generic;
using System.Linq;

namespace KinoZalMarsBlinVali.Views
{
    public partial class CustomerQuizzesPage : UserControl
    {
        public CustomerQuizzesPage()
        {
            InitializeComponent();
            LoadQuizzes();
        }

        private void LoadQuizzes()
        {
            try
            {
                var quizzes = AppDataContext.DbContext.Quizzes
                    .Where(q => q.IsActive == true)
                    .ToList();

                foreach (var quiz in quizzes)
                {
                    var quizCard = CreateQuizCard(quiz);
                    QuizzesContainer.Children.Add(quizCard);
                }
            }
            catch (System.Exception ex)
            {
                // Обработка ошибок
            }
        }

        private Border CreateQuizCard(Quiz quiz)
        {
            var card = new Border
            {
                Padding = new Avalonia.Thickness(20),
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            card.Classes.Add("card");

            var stackPanel = new StackPanel { Spacing = 10 };

            // Заголовок викторины
            var titleText = new TextBlock
            {
                Text = quiz.QuizTitle,
                FontSize = 18,
                FontWeight = Avalonia.Media.FontWeight.Bold
            };

            // Описание
            var descText = new TextBlock
            {
                Text = quiz.QuizDescription,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };

            // Информация о награде
            var rewardText = new TextBlock
            {
                Text = $"🎁 Награда: {quiz.BonusPoints} бонусных баллов",
                FontSize = 14,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 102, 0))
            };

            // Кнопка начала
            var startButton = new Button
            {
                Content = "🚀 Начать викторину",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            startButton.Classes.Add("success");

            startButton.Click += (s, e) => StartQuiz(quiz);

            stackPanel.Children.Add(titleText);
            stackPanel.Children.Add(descText);
            stackPanel.Children.Add(rewardText);
            stackPanel.Children.Add(startButton);

            card.Child = stackPanel;
            return card;
        }

        private void StartQuiz(Quiz quiz)
        {
            if (this.VisualRoot is MainWindow mainWindow)
            {
                mainWindow.NavigateTo(new QuizPage(quiz));
            }
        }
    }
}