using System;
using System.IO;

namespace second_task.WPF.Services
{
    public static class LoggingService
    {
        private static readonly string LogDirectory;
        private static readonly string LogFilePath;

        static LoggingService()
        {
            LogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "second_task.WPF");
            LogFilePath = Path.Combine(LogDirectory, "application.log");
            
            // Створити директорію, якщо вона не існує
            Directory.CreateDirectory(LogDirectory);
        }

        public static void LogInfo(string message)
        {
            WriteLog("ІНФО", message, null);
        }

        public static void LogError(string message, Exception ex = null)
        {
            WriteLog("ПОМИЛКА", message, ex);
        }

        private static void WriteLog(string level, string message, Exception ex)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logEntry = $"[{timestamp}] [{level}] {message}";
                
                if (ex != null)
                {
                    logEntry += $"\nДеталі помилки: {ex.Message}";
                    if (ex.StackTrace != null)
                    {
                        logEntry += $"\nСтек викликів: {ex.StackTrace}";
                    }
                }
                
                logEntry += Environment.NewLine;
                
                // Записати в файл (додати до існуючого вмісту)
                File.AppendAllText(LogFilePath, logEntry);
            }
            catch
            {
                // Ігнорувати помилки логування, щоб не порушити роботу програми
            }
        }

        public static string GetLogFilePath()
        {
            return LogFilePath;
        }
    }
}
