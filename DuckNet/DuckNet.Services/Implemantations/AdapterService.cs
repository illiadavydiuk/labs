using DuckNet.Data.Models;
using System.Collections.Generic;
using System.Diagnostics; // Для Process
using System.Management;
using System.Threading.Tasks;

namespace DuckNet.Services.Implementations
{
    public class AdapterService
    {
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

        public void ToggleAdapter(string connectionName, bool enable)
        {
            string status = enable ? "enable" : "disable";
            RunNetsh($"interface set interface \"{connectionName}\" admin={status}");
        }

        // 🔥 НОВИЙ МЕТОД: Зміна профілю (IP/DNS)
        public void SetAdapterProfile(string adapterName, string profileType)
        {
            if (profileType == "DHCP")
            {
                // Автоматичний IP та DNS
                RunNetsh($"interface ip set address \"{adapterName}\" dhcp");
                RunNetsh($"interface ip set dns \"{adapterName}\" dhcp");
            }
            else if (profileType == "Static_Work")
            {
                // Приклад статичного IP (можна змінити під свої потреби)
                RunNetsh($"interface ip set address \"{adapterName}\" static 192.168.1.55 255.255.255.0 192.168.1.1");
                RunNetsh($"interface ip set dns \"{adapterName}\" static 8.8.8.8");
            }
        }

        private void RunNetsh(string arguments)
        {
            var psi = new ProcessStartInfo("netsh", arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                Verb = "runas", // Права адміна
                WindowStyle = ProcessWindowStyle.Hidden
            };
            var p = Process.Start(psi);
            p?.WaitForExit();
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