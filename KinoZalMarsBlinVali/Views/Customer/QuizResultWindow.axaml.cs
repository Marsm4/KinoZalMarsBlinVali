using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System.Linq;

namespace KinoZalMarsBlinVali.Views
{
    public partial class QuizResultWindow : Window
    {
        public QuizResultWindow(double scorePercent, int correctAnswers, int totalQuestions, int earnedPoints, bool passed)
        {
            InitializeComponent();

            // Получаем информацию о попытках
            var customerId = AppDataContext.CurrentUser?.EmployeeId ?? 0;
            var attemptsCount = AppDataContext.DbContext.QuizAttempts
                .Count(a => a.CustomerId == customerId);

            var remainingAttempts = 3 - attemptsCount;

            if (passed)
            {
                ResultTitle.Text = "🎉 Поздравляем!";
                ResultTitle.Foreground = new SolidColorBrush(Color.Parse("#00A651"));
            }
            else
            {
                ResultTitle.Text = "😔 Попробуйте еще раз";
                ResultTitle.Foreground = new SolidColorBrush(Color.Parse("#DC3545"));
            }

            ScoreText.Text = $"Правильных ответов: {correctAnswers}/{totalQuestions} ({scorePercent:F1}%)";

            if (earnedPoints > 0)
            {
                BonusText.Text = $"🎁 Вам начислено: {earnedPoints} бонусных баллов!";
                BonusText.Foreground = new SolidColorBrush(Color.Parse("#00A651"));
            }
            else
            {
                BonusText.Text = $"Для получения баллов нужно набрать более 80%";
                BonusText.Foreground = new SolidColorBrush(Color.Parse("#6C757D"));
            }

            // Добавляем информацию о попытках
            AttemptsText.Text = $"Осталось попыток: {remainingAttempts}/3";
        }

        private void OKButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}