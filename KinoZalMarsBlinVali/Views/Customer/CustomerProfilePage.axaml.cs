using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class CustomerProfilePage : UserControl
    {
        private Customer _customer;

        public CustomerProfilePage()
        {
            InitializeComponent();
            LoadCustomerData();
        }

        private void LoadCustomerData()
        {
            try
            {
                var customerId = AppDataContext.CurrentUser?.EmployeeId;

                _customer = AppDataContext.DbContext.Customers
                    .FirstOrDefault(c => c.CustomerId == customerId);

                if (_customer != null)
                {
                    FirstNameTextBox.Text = _customer.FirstName ?? "";
                    LastNameTextBox.Text = _customer.LastName ?? "";
                    EmailTextBox.Text = _customer.Email ?? "";
                    PhoneTextBox.Text = _customer.Phone ?? "";
                    BalanceText.Text = $"{_customer.Balance}₽";
                    BonusPointsText.Text = (_customer.BonusPoints ?? 0).ToString();

                    // Устанавливаем контекст для привязки фотографии
                    this.DataContext = _customer;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private async void ChangePhoto_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var fileType = new FilePickerFileType("Изображения")
                {
                    Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" },
                    MimeTypes = new[] { "image/jpeg", "image/png", "image/bmp", "image/gif" }
                };

                var options = new FilePickerOpenOptions
                {
                    Title = "Выберите фотографию профиля",
                    FileTypeFilter = new[] { fileType },
                    AllowMultiple = false
                };

                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel != null)
                {
                    var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);

                    if (files.Count > 0)
                    {
                        var selectedFile = files[0];
                        var imagePath = await CopyImageToProfilePhotos(selectedFile);

                        _customer.ProfilePhotoPath = imagePath;
                        await AppDataContext.DbContext.SaveChangesAsync();

                        // Обновляем привязку
                        this.DataContext = null;
                        this.DataContext = _customer;

                        ShowSuccess("Фотография профиля успешно обновлена!");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при выборе файла: {ex.Message}");
            }
        }

        private async void RemovePhoto_Click(object? sender, RoutedEventArgs e)
        {
            if (_customer == null) return;

            try
            {
                _customer.ProfilePhotoPath = null;
                await AppDataContext.DbContext.SaveChangesAsync();

                // Обновляем привязку
                this.DataContext = null;
                this.DataContext = _customer;

                ShowSuccess("Фотография профиля удалена!");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка удаления фотографии: {ex.Message}");
            }
        }

        private async Task<string> CopyImageToProfilePhotos(IStorageFile sourceFile)
        {
            try
            {
                var currentDir = Directory.GetCurrentDirectory();
                string projectRoot;

                if (currentDir.Contains("bin\\Debug") || currentDir.Contains("bin\\Release"))
                {
                    projectRoot = Path.GetFullPath(Path.Combine(currentDir, "..", "..", ".."));
                }
                else
                {
                    projectRoot = Path.GetFullPath(Path.Combine(currentDir, "..", ".."));
                }

                var profilePhotosDir = Path.Combine(projectRoot, "Assets", "ProfilePhotos");

                if (!Directory.Exists(profilePhotosDir))
                {
                    Directory.CreateDirectory(profilePhotosDir);
                }

                var fileName = $"profile_{_customer.CustomerId}_{Guid.NewGuid():N}{Path.GetExtension(sourceFile.Name)}";
                var destinationPath = Path.Combine(profilePhotosDir, fileName);

                using var sourceStream = await sourceFile.OpenReadAsync();
                using var destinationStream = File.Create(destinationPath);
                await sourceStream.CopyToAsync(destinationStream);

                var relativePath = $"Assets/ProfilePhotos/{fileName}";

                Console.WriteLine($"✅ Фотография профиля сохранена: {destinationPath}");
                return relativePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка копирования фотографии: {ex.Message}");
                throw;
            }
        }

        private async void SaveProfile_Click(object? sender, RoutedEventArgs e)
        {
            if (_customer == null) return;

            try
            {
                _customer.FirstName = FirstNameTextBox.Text;
                _customer.LastName = LastNameTextBox.Text;
                _customer.Email = EmailTextBox.Text;
                _customer.Phone = PhoneTextBox.Text;

                await AppDataContext.DbContext.SaveChangesAsync();

                if (AppDataContext.CurrentUser != null)
                {
                    AppDataContext.CurrentUser.FirstName = _customer.FirstName;
                    AppDataContext.CurrentUser.LastName = _customer.LastName;
                    AppDataContext.CurrentUser.Username = _customer.Email;
                }

                ShowSuccess("Профиль успешно обновлен");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void ResetProfile_Click(object? sender, RoutedEventArgs e)
        {
            LoadCustomerData();
        }

        private void AddBalance_Click(object? sender, RoutedEventArgs e)
        {
            if (this.FindAncestorOfType<CustomerMainPage>() is CustomerMainPage mainPage)
            {
                mainPage.NavigateToAddBalance();
            }
        }

        private async void ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            await dialog.ShowDialog((Window)this.VisualRoot);
        }

        private async void ShowSuccess(string message)
        {
            var dialog = new MessageWindow("Успех", message);
            await dialog.ShowDialog((Window)this.VisualRoot);
        }
    }
}