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
            var foundDevices = new ConcurrentBag<Device>();
            var range = Enumerable.Range(1, 254);

            await Task.Run(() =>
            {
                Parallel.ForEach(range, (i) =>
                {
                    string ip = $"{baseIpPrefix}{i}"; // Наприклад 192.168.1.55

                    // Передаємо змінну для отримання часу пінгу
                    if (PingHost(ip, out long rtt))
                    {
                        string mac = ArpHelper.GetMacAddress(ip);
                        string host = GetHostname(ip);

                        if (host == "Unknown") host = $"Device ({ip})";

                        foundDevices.Add(new Device
                        {
                            IpAddress = ip,
                            MacAddress = mac,
                            Hostname = host,
                            IsOnline = true,
                            LastSeen = System.DateTime.Now,
                            LastPingMs = rtt // 🔥 Записуємо пінг
                        });
                    }
                });
            });
            return foundDevices.OrderBy(d => int.Parse(d.IpAddress.Split('.')[3])).ToList();
        }

        // Оновлений метод з вихідним параметром roundtripTime
        private bool PingHost(string ip, out long roundtripTime)
        {
            roundtripTime = 0;
            try
            {
                using (var pinger = new Ping())
                {
                    // Збільшуємо до 2000 мс
                    PingReply reply = pinger.Send(ip, 2000);
                    if (reply.Status == IPStatus.Success)
                    {
                        roundtripTime = reply.RoundtripTime;
                        return true;
                    }
                    return false;
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