using Avalonia.Controls;
using Avalonia.Interactivity;

namespace KinoZalMarsBlinVali.Views
{
    public partial class QuizResultWindow : Window
    {
        public QuizResultWindow(double scorePercent, int correctAnswers, int totalQuestions, int earnedPoints, bool passed)
        {
            InitializeComponent();

            if (passed)
            {
                ResultTitle.Text = "🎉 Поздравляем!";
                ResultTitle.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 128, 0));
            }
            else
            {
                ResultTitle.Text = "😔 Попробуйте еще раз";
                ResultTitle.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(255, 0, 0));
            }

            ScoreText.Text = $"Правильных ответов: {correctAnswers}/{totalQuestions} ({scorePercent:F1}%)";

            if (earnedPoints > 0)
            {
                BonusText.Text = $"🎁 Вам начислено: {earnedPoints} бонусных баллов!";
                BonusText.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 128, 0));
            }
            else
            {
                BonusText.Text = "Для получения баллов нужно набрать более 80%";
                BonusText.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(128, 128, 128));
            }
        }

        private void OKButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}