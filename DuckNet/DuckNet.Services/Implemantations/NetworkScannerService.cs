using DuckNet.Data.Entities;
using DuckNet.Services.Helpers;
using DuckNet.Services.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace DuckNet.Services.Implementations
{
    public class NetworkScannerService : INetworkScanner
    {
        public async Task<List<Device>> ScanNetworkAsync(string baseIpPrefix)
        {
            // Використовуємо потокобезпечний список, бо будемо писати з різних потоків
            var foundDevices = new ConcurrentBag<Device>();

            // Генеруємо список останніх октетів (від 1 до 254)
            var range = Enumerable.Range(1, 254);

            // Запускаємо паралельне сканування
            // Це значно швидше, ніж звичайний цикл foreach
            await Task.Run(() =>
            {
                Parallel.ForEach(range, (i) =>
                {
                    string ip = $"{baseIpPrefix}{i}"; // Наприклад 192.168.1.55

                    if (PingHost(ip))
                    {
                        string mac = ArpHelper.GetMacAddress(ip);
                        string host = GetHostname(ip);

                        // Якщо Dns не дав результату, пробуємо просто повернути IP як ім'я, щоб не було пусто
                        if (host == "Unknown") host = $"Device ({ip})";

                        foundDevices.Add(new Device
                        {
                            IpAddress = ip,
                            MacAddress = mac,
                            Hostname = host,
                            IsOnline = true,
                            LastSeen = System.DateTime.Now
                        });
                    }
                });
            });

            return foundDevices.OrderBy(d => int.Parse(d.IpAddress.Split('.')[3])).ToList();
        }

        private bool PingHost(string ip)
        {
            try
            {
                using (var pinger = new Ping())
                {
                    // Збільшуємо до 2000 мс (2 сек), щоб знайти всіх
                    PingReply reply = pinger.Send(ip, 2000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch { return false; }
        }

        private string GetHostname(string ip)
        {
            try
            {
                return Dns.GetHostEntry(ip).HostName;
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}