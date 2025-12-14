using DuckNet.Data.Entities;
using DuckNet.Repositories.Interfaces;
using DuckNet.Services.Helpers; // Тут лежать MacVendorHelper і FileLogger
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckNet.Services.Implementations
{
    public class DeviceService
    {
        private readonly IRepository<Device> _deviceRepo;
        private readonly IRepository<NetworkEvent> _eventRepo;
        private readonly IRepository<ScanSession> _scanRepo;

        // Подія для сповіщення UI про небезпеку (щоб показати Toast)
        public event Action<string> OnSecurityAlert;

        public DeviceService(IRepository<Device> deviceRepo, IRepository<NetworkEvent> eventRepo, IRepository<ScanSession> scanRepo)
        {
            _deviceRepo = deviceRepo;
            _eventRepo = eventRepo;
            _scanRepo = scanRepo;
        }

        // --- Методи отримання даних (CRUD) ---

        public IEnumerable<Device> GetAllDevices() => _deviceRepo.GetAll();

        // Метод для збереження змін з UI (наприклад, зміна імені або довіри)
        public void UpdateDevice(Device device)
        {
            _deviceRepo.Update(device);
            _deviceRepo.Save();
        }

        public IEnumerable<NetworkEvent> GetRecentEvents()
        {
            return _eventRepo.GetAll()
                             .OrderByDescending(e => e.Timestamp)
                             .Take(100)
                             .ToList();
        }

        // Для експорту в TXT по даті
        public IEnumerable<NetworkEvent> GetEventsByDate(DateTime date)
        {
            return _eventRepo.GetAll()
                             .Where(e => e.Timestamp.Date == date.Date)
                             .OrderBy(e => e.Timestamp)
                             .ToList();
        }

        public IEnumerable<ScanSession> GetScanHistory()
        {
            return _scanRepo.GetAll()
                            .OrderBy(s => s.ScanTime)
                            .TakeLast(20)
                            .ToList();
        }

        // логіка оновлення

        public void UpdateDevices(List<Device> scannedDevices)
        {
            var existingDevices = _deviceRepo.GetAll().ToList();
            var now = DateTime.Now;

            int devicesOnlineCount = 0;
            long totalPingSum = 0; // Для середнього пінгу

            // 1. Обробка тих, хто ВІДКЛЮЧИВСЯ
            foreach (var existing in existingDevices)
            {
                // Якщо пристрій був онлайн, але його немає в новому списку
                if (existing.IsOnline && !scannedDevices.Any(d => d.MacAddress == existing.MacAddress))
                {
                    existing.IsOnline = false;
                    _deviceRepo.Update(existing);

                    // Логуємо відключення (без тривоги)
                    LogAndNotify(EventType.DeviceDisconnected, $"Пристрій відключився: {existing.IpAddress} ({existing.Hostname})", existing.Id, true);
                }
            }

            // 2. Обробка НОВИХ та тих, хто ПОВЕРНУВСЯ
            foreach (var scanned in scannedDevices)
            {
                devicesOnlineCount++;
                totalPingSum += scanned.LastPingMs; // Додаємо пінг до суми

                // Визначаємо виробника
                string vendor = MacVendorHelper.GetVendor(scanned.MacAddress);

                // Якщо ім'я невідоме або стандартне, замінюємо на вендора
                if (scanned.Hostname == "Unknown" || scanned.Hostname.StartsWith("Device"))
                {
                    scanned.Hostname = $"{vendor} Device";
                }

                var existing = existingDevices.FirstOrDefault(d => d.MacAddress == scanned.MacAddress);

                if (existing != null)
                {
                    // пристрій вже є в базі

                    // Якщо він був офлайн, а став онлайн
                    if (!existing.IsOnline)
                    {
                        if (!existing.IsTrusted)
                        {
                            LogAndNotify(EventType.DeviceConnected, $"!!!! НЕДОВІРЕНИЙ ПРИСТРІЙ ПОВЕРНУВСЯ: {scanned.IpAddress} ({vendor})", existing.Id, false);
                        }
                        else
                        {
                            LogAndNotify(EventType.DeviceConnected, $"Пристрій повернувся: {scanned.IpAddress} ({existing.Hostname})", existing.Id, true);
                        }
                    }

                    // Оновлюємо дані
                    existing.IsOnline = true;
                    existing.IpAddress = scanned.IpAddress;
                    existing.LastSeen = now;

                    // Оновлюємо Hostname, тільки якщо нове ім'я краще за старе
                    if (scanned.Hostname != "Unknown" && !scanned.Hostname.Contains("Device ("))
                    {
                        existing.Hostname = scanned.Hostname;
                    }

                    _deviceRepo.Update(existing);
                }
                else
                {
                    scanned.IsTrusted = false; // За замовчуванням не довіряємо
                    _deviceRepo.Add(scanned);
                    _deviceRepo.Save(); // Зберігаємо, щоб отримати ID для логу

                    LogAndNotify(EventType.DeviceConnected, $"!!!! НОВИЙ ПРИСТРІЙ: {scanned.IpAddress} ({vendor})", scanned.Id, false);
                }
            }

            // 3. Зберігаємо статистику сканування (Session)
            int avgPing = (devicesOnlineCount > 0) ? (int)(totalPingSum / devicesOnlineCount) : 0;

            _scanRepo.Add(new ScanSession
            {
                ScanTime = now,
                DevicesFound = devicesOnlineCount,
                AveragePingMs = avgPing
            });

            _scanRepo.Save();
            _deviceRepo.Save();
        }

        private void LogAndNotify(EventType type, string msg, int? deviceId, bool isTrusted)
        {
            // 1. Запис в БД
            _eventRepo.Add(new NetworkEvent
            {
                Timestamp = DateTime.Now,
                Type = type,
                Message = msg,
                DeviceId = deviceId
            });
            _eventRepo.Save();

            // 2. Запис у текстовий файл
            FileLogger.Log(msg);

            // 3. Якщо це підключення і пристрій НЕ довірений -> викликаємо подію для UI
            if (type == EventType.DeviceConnected && !isTrusted)
            {
                OnSecurityAlert?.Invoke(msg);
            }
        }

        // Очищення бази
        public void ClearAllHistory()
        {
            foreach (var d in _deviceRepo.GetAll()) _deviceRepo.Delete(d.Id);
            foreach (var e in _eventRepo.GetAll()) _eventRepo.Delete(e.Id);
            foreach (var s in _scanRepo.GetAll()) _scanRepo.Delete(s.Id);

            _deviceRepo.Save();
            _eventRepo.Save();
            _scanRepo.Save();
        }
    }
}