using System;
using System.IO;

namespace DuckNet.Services.Helpers
{
    public static class FileLogger
    {
        private static string LogPath = "network_events.txt";
        private static object _lock = new object();

        public static void Log(string message)
        {
            try
            {
                string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                lock (_lock) // Блокування, щоб потоки не сварилися за файл
                {
                    File.AppendAllText(LogPath, logLine);
                }
            }
            catch { /* Ігноруємо помилки доступу до диску */ }
        }
    }
}