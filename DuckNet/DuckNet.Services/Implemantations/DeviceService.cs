using DuckNet.Data.Entities;
using DuckNet.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckNet.Services.Implementations
{
    public class DeviceService
    {
        private readonly IRepository<Device> _deviceRepo;
        private readonly IRepository<NetworkEvent> _eventRepo; // Додали репо для подій

        // Оновили конструктор (додай IRepository<NetworkEvent> eventRepo в App.xaml.cs пізніше!)
        public DeviceService(IRepository<Device> deviceRepo, IRepository<NetworkEvent> eventRepo)
        {
            _deviceRepo = deviceRepo;
            _eventRepo = eventRepo;
        }

        public IEnumerable<Device> GetAllDevices() => _deviceRepo.GetAll();

        // Отримати останні 50 подій
        public IEnumerable<NetworkEvent> GetRecentEvents()
        {
            return _eventRepo.GetAll()
                             .OrderByDescending(e => e.Timestamp)
                             .Take(50)
                             .ToList();
        }

        public void UpdateDevices(List<Device> scannedDevices)
        {
            var existingDevices = _deviceRepo.GetAll().ToList();
            var now = DateTime.Now;

            // 1. Перевіряємо тих, хто відключився
            foreach (var existing in existingDevices)
            {
                // Якщо пристрій був онлайн, а зараз його немає в списку сканування
                if (existing.IsOnline && !scannedDevices.Any(d => d.MacAddress == existing.MacAddress))
                {
                    existing.IsOnline = false;
                    _deviceRepo.Update(existing);

                    // 🔥 ЛОГУВАННЯ
                    LogEvent(EventType.DeviceDisconnected, $"Пристрій {existing.IpAddress} ({existing.Hostname}) відключився.", existing.Id);
                }
            }

            // 2. Перевіряємо нових та тих, хто повернувся
            foreach (var scanned in scannedDevices)
            {
                var existing = existingDevices.FirstOrDefault(d => d.MacAddress == scanned.MacAddress);

                if (existing != null)
                {
                    // Якщо був офлайн, а став онлайн
                    if (!existing.IsOnline)
                    {
                        LogEvent(EventType.DeviceConnected, $"Пристрій повернувся: {scanned.IpAddress}", existing.Id);
                    }

                    existing.IsOnline = true;
                    existing.IpAddress = scanned.IpAddress;
                    existing.LastSeen = now;
                    if (scanned.Hostname != "Unknown") existing.Hostname = scanned.Hostname;

                    _deviceRepo.Update(existing);
                }
                else
                {
                    // Абсолютно новий пристрій
                    _deviceRepo.Add(scanned);
                    _deviceRepo.Save(); // Зберігаємо, щоб отримати ID

                    LogEvent(EventType.DeviceConnected, $"Новий пристрій у мережі: {scanned.IpAddress}", scanned.Id);
                }
            }
            _deviceRepo.Save();
        }

        private void LogEvent(EventType type, string msg, int? deviceId = null)
        {
            _eventRepo.Add(new NetworkEvent
            {
                Timestamp = DateTime.Now,
                Type = type,
                Message = msg,
                DeviceId = deviceId
            });
        }
    }
}