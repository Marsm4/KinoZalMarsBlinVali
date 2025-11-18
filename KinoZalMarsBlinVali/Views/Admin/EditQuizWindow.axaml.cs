using Avalonia.Controls;
using Avalonia.Interactivity;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System;

namespace KinoZalMarsBlinVali.Views
{
    public partial class EditQuizWindow : Window
    {
        private Quiz _quiz;
        public event EventHandler? QuizUpdated;

        public EditQuizWindow(Quiz quiz)
        {
            InitializeComponent();
            _quiz = quiz;
            LoadQuizData();
        }

        private void LoadQuizData()
        {
            TitleTextBox.Text = _quiz.QuizTitle;
            DescriptionTextBox.Text = _quiz.QuizDescription;
            BonusPointsTextBox.Text = _quiz.BonusPoints?.ToString();
            PassingScoreTextBox.Text = _quiz.PassingScore?.ToString();
            IsActiveCheckBox.IsChecked = _quiz.IsActive;
        }

        private async void Save_Click(object? sender, RoutedEventArgs e)
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

                // Update quiz
                _quiz.QuizTitle = TitleTextBox.Text.Trim();
                _quiz.QuizDescription = DescriptionTextBox.Text?.Trim();
                _quiz.BonusPoints = bonusPoints;
                _quiz.PassingScore = passingScore;
                _quiz.IsActive = IsActiveCheckBox.IsChecked ?? true;

                await AppDataContext.DbContext.SaveChangesAsync();

                QuizUpdated?.Invoke(this, EventArgs.Empty);
                Close();

                await ShowMessage("Success", "Quiz updated successfully!");
            }
            catch (Exception ex)
            {
                await ShowError($"Error updating quiz: {ex.Message}");
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