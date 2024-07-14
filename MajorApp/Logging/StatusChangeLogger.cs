using System;
using System.IO;
using System.Linq;

namespace MajorAppMVVM2.Logging
{
    public class StatusChangeLogger : ILogger
    {
        private readonly string _baseDirectoryPath;

        public StatusChangeLogger(string baseDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(baseDirectoryPath))
                throw new ArgumentException("Путь к базовому каталогу не может быть нулевым или пустым.", nameof(baseDirectoryPath));

            _baseDirectoryPath = baseDirectoryPath;

            // Создание базовой директории, если она не существует
            if (!Directory.Exists(_baseDirectoryPath))
            {
                Directory.CreateDirectory(_baseDirectoryPath);
            }
        }

        public void Log(string message)
        {
            try
            {
                // Формирование имени файла на основе текущего дня и часа
                string fileName = GetLogFileName();
                string filePath = Path.Combine(_baseDirectoryPath, fileName);

                // Создание директории для текущего дня, если она не существует
                string dayDirectory = Path.Combine(_baseDirectoryPath, DateTime.Now.ToString("yyyy-MM-dd"));
                if (!Directory.Exists(dayDirectory))
                {
                    Directory.CreateDirectory(dayDirectory);
                }

                // Запись сообщения в файл
                using (var writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}");
                }

                // Ротация файлов
                RotateLogs(dayDirectory);
            }
            catch (Exception ex)
            {
                // Логирование ошибок в файл для ошибок
                string errorFilePath = Path.Combine(_baseDirectoryPath, "error.log");
                File.AppendAllText(errorFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: Error logging message: {message}. Exception: {ex.Message}\n");
            }
        }

        private string GetLogFileName()
        {
            // Формирование имени файла на основе текущего часа
            string hour = DateTime.Now.ToString("HH");
            string fileName = $"status_change_{DateTime.Now:yyyy-MM-dd}_{hour}.log";
            return Path.Combine(DateTime.Now.ToString("yyyy-MM-dd"), fileName);
        }

        private void RotateLogs(string dayDirectory)
        {
            var files = Directory.GetFiles(dayDirectory, "*.log");

            // Удаление старых файлов, если их больше 5
            if (files.Length > 5) // 
            {
                var oldestFile = files.OrderBy(f => new FileInfo(f).CreationTime).First();
                File.Delete(oldestFile);
            }
        }
    }
}
