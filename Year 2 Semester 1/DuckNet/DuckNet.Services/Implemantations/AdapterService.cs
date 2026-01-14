using DuckNet.Data.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

namespace DuckNet.Services.Implementations
{
    public class AdapterService
    {
        public List<NetworkAdapterInfo> GetAdapters()
        {
            var adapters = new List<NetworkAdapterInfo>();

            try
            {
                string query = "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID IS NOT NULL";

                using (var searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var adapter = new NetworkAdapterInfo
                        {
                            DeviceId = Convert.ToInt32(obj["DeviceID"]),
                            Name = obj["Name"]?.ToString(),
                            NetConnectionId = obj["NetConnectionID"]?.ToString()
                        };

                        int configCode = 0;
                        if (obj["ConfigManagerErrorCode"] != null)
                        {
                            configCode = Convert.ToInt32(obj["ConfigManagerErrorCode"]);
                        }

                        string netStatus = obj["NetConnectionStatus"]?.ToString();

                        if (configCode == 22 || string.IsNullOrEmpty(netStatus))
                        {
                            adapter.IsEnabled = false;
                            adapter.Status = "Вимкнено";
                        }
                        else
                        {
                            adapter.IsEnabled = true;

                            if (netStatus == "2")
                            {
                                adapter.Status = "Підключено";
                            }
                            else if (netStatus == "7")
                            {
                                adapter.Status = "Кабель відключено";
                            }
                            else if (netStatus == "0")
                            {
                                adapter.Status = "Неактивний";
                            }
                            else
                            {
                                adapter.Status = "Увімкнено";
                            }
                        }

                        adapters.Add(adapter);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Помилка WMI при отриманні адаптерів: " + ex.Message);
            }

            return adapters;
        }

        public void ToggleAdapter(string connectionName, bool enable)
        {
            string status = enable ? "enable" : "disable";
            string arguments = $"interface set interface \"{connectionName}\" admin={status}";
            RunNetsh(arguments);
        }

        public void SetAdapterProfile(string adapterName, string profileType)
        {
            if (profileType == "DHCP")
            {
                RunNetsh($"interface ip set address \"{adapterName}\" dhcp");
                RunNetsh($"interface ip set dns \"{adapterName}\" dhcp");
            }
        }

        private void RunNetsh(string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo("netsh", arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var p = Process.Start(psi);
                p?.WaitForExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Помилка виконання netsh: " + ex.Message);
            }
        }
    }
}
