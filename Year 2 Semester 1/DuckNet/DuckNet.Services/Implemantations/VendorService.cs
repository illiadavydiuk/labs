using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuckNet.Services.Implementations
{
    public class VendorService
    {
        private const string DatabaseFileName = "oui.txt";

        // Словник для швидкого пошуку: "00:00:0C" -> "Cisco Systems, Inc"
        private readonly Dictionary<string, string> _vendors;

        public VendorService()
        {
            _vendors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            LoadDatabase();
        }

        private void LoadDatabase()
        {
            // Перевіряємо, чи існує файл поруч з .exe
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DatabaseFileName);

            if (!File.Exists(path))
            {
                // Якщо файлу немає, програма не впаде, але не буде знати вендорів
                return;
            }

            try
            {
                // Читаємо всі рядки
                var lines = File.ReadAllLines(path);

                foreach (var line in lines)
                {
                    // Шукаємо рядки, що містять "(hex)". Формат файлу IEEE:
                    // 00-00-0C   (hex)		Cisco Systems, Inc
                    if (line.Contains("(hex)"))
                    {
                        // Розділяємо рядок по маркеру "(hex)"
                        // part[0] буде "00-00-0C   "
                        // part[1] буде "		Cisco Systems, Inc"
                        var parts = line.Split(new[] { "(hex)" }, StringSplitOptions.None);

                        if (parts.Length >= 2)
                        {
                            // Очищаємо MAC: "00-00-0C   " -> "00:00:0C"
                            string prefix = parts[0].Trim().Replace("-", ":");

                            // Очищаємо назву: "		Cisco Systems, Inc" -> "Cisco Systems, Inc"
                            string vendorName = parts[1].Trim();

                            // Додаємо в словник (якщо такого ключа ще немає)
                            if (prefix.Length == 8 && !_vendors.ContainsKey(prefix))
                            {
                                _vendors[prefix] = vendorName;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ігноруємо помилки читання файлу, щоб програма не вилітала
            }
        }

        public string GetVendor(string macAddress)
        {
            if (string.IsNullOrEmpty(macAddress) || macAddress.Length < 8)
                return "Unknown";

            // Беремо перші 8 символів (наприклад "00:1A:2B")
            // і замінюємо тире на двокрапку для сумісності з форматом OUI
            string prefix = macAddress.Substring(0, 8).ToUpper().Replace("-", ":");

            if (_vendors.TryGetValue(prefix, out string vendor))
            {
                return vendor;
            }

            return "Unknown";
        }
    }
}