using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json; // System.Text.Json

namespace DuckNet.Services.Helpers
{
    public class AdapterProfile
    {
        public string Name { get; set; }
        // Список назв адаптерів, які мають бути УВІМКНЕНІ. Всі інші будуть ВИМКНЕНІ.
        public List<string> ActiveAdapters { get; set; } = new List<string>();
    }

    public static class ProfileManager
    {
        private static string FilePath = "profiles.json";

        public static List<AdapterProfile> LoadProfiles()
        {
            if (!File.Exists(FilePath)) return GetDefaultProfiles();
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<AdapterProfile>>(json) ?? GetDefaultProfiles();
        }

        public static void SaveProfiles(List<AdapterProfile> profiles)
        {
            var json = JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        private static List<AdapterProfile> GetDefaultProfiles()
        {
            return new List<AdapterProfile>
            {
                new AdapterProfile { Name = "🏠 Стандартний", ActiveAdapters = new List<string> { "Wi-Fi", "Ethernet" } },
                new AdapterProfile { Name = "🎮 Ігровий (Тільки LAN)", ActiveAdapters = new List<string> { "Ethernet" } },
                new AdapterProfile { Name = "🎓 Навчання (VM)", ActiveAdapters = new List<string> { "Wi-Fi", "VirtualBox Host-Only Network" } }
            };
        }
    }
}