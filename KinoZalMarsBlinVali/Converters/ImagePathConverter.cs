using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Globalization;
using System.IO;

namespace KinoZalMarsBlinVali.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string imagePath && !string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    string fullPath = GetFullImagePath(imagePath);

                    Console.WriteLine($"🖼️ Загружаем изображение: {imagePath}");
                    Console.WriteLine($"🖼️ Полный путь: {fullPath}");
                    Console.WriteLine($"🖼️ Файл существует: {File.Exists(fullPath)}");

                    if (File.Exists(fullPath))
                    {
                        var bitmap = new Bitmap(fullPath);
                        Console.WriteLine($"✅ Изображение успешно загружено: {bitmap.Size.Width}x{bitmap.Size.Height}");
                        return bitmap;
                    }
                    else
                    {
                        Console.WriteLine($"❌ Файл не найден: {fullPath}");
                        // Возвращаем заглушку или null
                        return CreatePlaceholderImage();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка загрузки изображения: {ex.Message}");
                    return CreatePlaceholderImage();
                }
            }
            else
            {
                Console.WriteLine($"❌ Путь к изображению пустой или null: {value}");
                return CreatePlaceholderImage();
            }
        }

        private string GetFullImagePath(string imagePath)
        {
            // Если путь уже абсолютный
            if (Path.IsPathRooted(imagePath))
            {
                return imagePath;
            }

            // Убираем начальный слеш если есть (для совместимости со старыми записями)
            var cleanPath = imagePath.TrimStart('/');

            var currentDir = Directory.GetCurrentDirectory();
            string projectRoot;

            // Определяем корень проекта
            if (currentDir.Contains("bin\\Debug") || currentDir.Contains("bin\\Release"))
            {
                projectRoot = Path.GetFullPath(Path.Combine(currentDir, "..", "..", ".."));
            }
            else
            {
                projectRoot = Path.GetFullPath(Path.Combine(currentDir, "..", ".."));
            }

            var fullPath = Path.Combine(projectRoot, cleanPath);

            Console.WriteLine($"📁 Корень проекта: {projectRoot}");
            Console.WriteLine($"📁 Чистый путь: {cleanPath}");
            Console.WriteLine($"📁 Итоговый путь: {fullPath}");

            return fullPath;
        }

        private Bitmap? CreatePlaceholderImage()
        {
            try
            {
                // Пытаемся найти заглушку
                var placeholderPath = GetFullImagePath("Assets/placeholder.jpg");
                if (File.Exists(placeholderPath))
                {
                    return new Bitmap(placeholderPath);
                }
            }
            catch
            {
                // Игнорируем ошибки создания заглушки
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}