using System;
using System.IO;
using System.Text.Json;

namespace DuckNet.Services.Implementations
{
    public class AppSettings
    {
        public bool IsAutoScanEnabled { get; set; } = false;
        public int ScanIntervalSeconds { get; set; } = 30;
    }

    public class SettingsService
    {
        private readonly string _filePath = "settings.json";
        public AppSettings CurrentSettings { get; private set; }

        public SettingsService()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    string json = File.ReadAllText(_filePath);
                    CurrentSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch
                {
                    CurrentSettings = new AppSettings();
                }
            }
            else
            {
                CurrentSettings = new AppSettings();
            }
        }

        public void SaveSettings(bool autoScan, int interval)
        {
            CurrentSettings.IsAutoScanEnabled = autoScan;
            CurrentSettings.ScanIntervalSeconds = interval;

            string json = JsonSerializer.Serialize(CurrentSettings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}