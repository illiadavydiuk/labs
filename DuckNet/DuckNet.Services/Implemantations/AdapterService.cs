using DuckNet.Data.Models; // 🔥 Тепер беремо з Data
using System.Collections.Generic;
using System.Management;

namespace DuckNet.Services.Implementations
{
    public class AdapterService
    {
        // Цей сервіс не потребує IRepository, бо працює з WMI напряму
        // Це відповідає правилу: не плодити репозиторії там, де немає БД

        public List<NetworkAdapterInfo> GetAdapters()
        {
            var adapters = new List<NetworkAdapterInfo>();
            string query = "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID != NULL";

            using (var searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    adapters.Add(new NetworkAdapterInfo
                    {
                        DeviceId = int.Parse(obj["DeviceID"].ToString()),
                        Name = obj["Name"]?.ToString(),
                        NetConnectionId = obj["NetConnectionID"]?.ToString(),
                        Status = ParseStatus(obj["NetConnectionStatus"]?.ToString()),
                        IsEnabled = obj["NetConnectionStatus"]?.ToString() != "0"
                    });
                }
            }
            return adapters;
        }

        public void ToggleAdapter(int deviceId, bool enable)
        {
            string query = $"SELECT * FROM Win32_NetworkAdapter WHERE DeviceID = '{deviceId}'";
            using (var searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string methodName = enable ? "Enable" : "Disable";
                    obj.InvokeMethod(methodName, null);
                }
            }
        }

        private string ParseStatus(string statusCode)
        {
            return statusCode switch
            {
                "2" => "Підключено",
                "7" => "Кабель відключено",
                "0" => "Вимкнено",
                _ => "Інше"
            };
        }
    }
}