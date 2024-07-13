using System;
using System.IO;

namespace MajorAppMVVM2.Logging
{
    public class StatusChangeLogger : ILogger
    {
        private readonly string _filePath;

        public StatusChangeLogger(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            _filePath = filePath;
        }

        public void Log(string message)
        {
            using (var writer = new StreamWriter(_filePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }
}
