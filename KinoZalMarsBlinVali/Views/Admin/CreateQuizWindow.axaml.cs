using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System;

namespace KinoZalMarsBlinVali.Views
{
    public partial class CreateQuizWindow : Window
    {
        public event EventHandler<Quiz>? QuizCreated;

        public CreateQuizWindow()
        {
            InitializeComponent();
        }

        private async void Create_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
                {
                    await ShowError("Please enter quiz title");
                    return;
                }

                if (!int.TryParse(BonusPointsTextBox.Text, out int bonusPoints) || bonusPoints <= 0)
                {
                    await ShowError("Please enter valid bonus points amount");
                    return;
                }

                if (!int.TryParse(PassingScoreTextBox.Text, out int passingScore) || passingScore < 0 || passingScore > 100)
                {
                    await ShowError("Passing score must be between 0 and 100");
                    return;
                }

                // Create quiz
                var quiz = new Quiz
                {
                    QuizTitle = TitleTextBox.Text.Trim(),
                    QuizDescription = DescriptionTextBox.Text?.Trim(),
                    BonusPoints = bonusPoints,
                    PassingScore = passingScore,
                    IsActive = IsActiveCheckBox.IsChecked ?? true,
                    CreatedAt = DateTime.Now
                };

                AppDataContext.DbContext.Quizzes.Add(quiz);
                await AppDataContext.DbContext.SaveChangesAsync();

                QuizCreated?.Invoke(this, quiz);
                Close();

                await ShowMessage("Success", "Quiz created successfully!");
            }
            catch (Exception ex)
            {
                await ShowError($"Error creating quiz: {ex.Message}");
            }
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private async System.Threading.Tasks.Task ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            await dialog.ShowDialog(this);
        }

        private async System.Threading.Tasks.Task ShowMessage(string title, string message)
        {
            var dialog = new MessageWindow(title, message);
            await dialog.ShowDialog(this);
        }
    }
}