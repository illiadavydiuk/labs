using DuckNet.Data.Entities;
using DuckNet.Repositories.Interfaces;
using DuckNet.Services.Helpers;
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

        public event Action<string> OnSecurityAlert;

        public DeviceService(IRepository<Device> deviceRepo, IRepository<NetworkEvent> eventRepo, IRepository<ScanSession> scanRepo)
        {
            _deviceRepo = deviceRepo;
            _eventRepo = eventRepo;
            _scanRepo = scanRepo;
        }

        // --- Basic CRUD ---
        public IEnumerable<Device> GetAllDevices() => _deviceRepo.GetAll();

        public void UpdateDevice(Device device)
        {
            _deviceRepo.Update(device);
            _deviceRepo.Save();
        }

        public IEnumerable<NetworkEvent> GetRecentEvents()
        {
            return _eventRepo.GetAll().OrderByDescending(e => e.Timestamp).Take(100).ToList();
        }

        public IEnumerable<NetworkEvent> GetEventsByDate(DateTime date)
        {
            return _eventRepo.GetAll()
                             .Where(e => e.Timestamp.Date == date.Date)
                             .OrderBy(e => e.Timestamp)
                             .ToList();
        }

        public IEnumerable<ScanSession> GetScanHistory()
        {
            return _scanRepo.GetAll().OrderBy(s => s.ScanTime).TakeLast(20).ToList();
        }

        // --- CORE LOGIC ---

        public void UpdateDevices(List<Device> scannedDevices)
        {
            var existingDevices = _deviceRepo.GetAll().ToList();
            var now = DateTime.Now;

            int devicesOnlineCount = 0;
            long totalPingSum = 0;

            // 🔥 Список для накопичення тривог за один цикл сканування
            List<string> currentScanAlerts = new List<string>();

            // 1. ПЕРЕВІРКА ВІДКЛЮЧЕНИХ
            foreach (var existing in existingDevices)
            {
                if (existing.IsOnline && !scannedDevices.Any(d => d.MacAddress == existing.MacAddress))
                {
                    existing.IsOnline = false;
                    _deviceRepo.Update(existing);

                    // fireEvent: false, бо відключення нас не так лякають
                    LogAndNotify(EventType.DeviceDisconnected, $"Пристрій відключився: {existing.IpAddress} ({existing.Hostname})", existing.Id, true, fireEvent: false);
                }
            }

            // 2. ОБРОБКА АКТИВНИХ
            foreach (var scanned in scannedDevices)
            {
                devicesOnlineCount++;
                totalPingSum += scanned.LastPingMs;

                string vendor = MacVendorHelper.GetVendor(scanned.MacAddress);

                if (scanned.Hostname == "Unknown" || scanned.Hostname.StartsWith("Device"))
                {
                    scanned.Hostname = $"{vendor} Device";
                }

                var existing = existingDevices.FirstOrDefault(d => d.MacAddress == scanned.MacAddress);

                if (existing != null)
                {
                    // === ІСНУЮЧИЙ ПРИСТРІЙ ===
                    if (!existing.IsOnline)
                    {
                        // Повернувся в мережу
                        if (!existing.IsTrusted)
                        {
                            string msg = $"!!!! НЕДОВІРЕНИЙ ПОВЕРНУВСЯ: {scanned.IpAddress} ({vendor})";
                            currentScanAlerts.Add(msg); // Додаємо в список тривог

                            // Пишемо в лог, але НЕ викликаємо подію Event одразу (fireEvent: false)
                            LogAndNotify(EventType.DeviceConnected, msg, existing.Id, false, fireEvent: false);
                        }
                        else
                        {
                            LogAndNotify(EventType.DeviceConnected, $"Пристрій повернувся: {scanned.IpAddress} ({existing.Hostname})", existing.Id, true, fireEvent: false);
                        }
                    }

                    existing.IsOnline = true;
                    existing.IpAddress = scanned.IpAddress;
                    existing.LastSeen = now;

                    if (scanned.Hostname != "Unknown" && !scanned.Hostname.Contains("Device ("))
                    {
                        existing.Hostname = scanned.Hostname;
                    }
                    _deviceRepo.Update(existing);
                }
                else
                {
                    // === НОВИЙ ПРИСТРІЙ ===
                    scanned.IsTrusted = false;
                    _deviceRepo.Add(scanned);
                    _deviceRepo.Save();

                    string msg = $"!!!! НОВИЙ ПРИСТРІЙ: {scanned.IpAddress} ({vendor})";
                    currentScanAlerts.Add(msg); // Додаємо в список тривог

                    // Логуємо, але мовчимо поки що
                    LogAndNotify(EventType.DeviceConnected, msg, scanned.Id, false, fireEvent: false);
                }
            }

            // 3. ЗБЕРЕЖЕННЯ СЕСІЇ
            int avgPing = (devicesOnlineCount > 0) ? (int)(totalPingSum / devicesOnlineCount) : 0;
            _scanRepo.Add(new ScanSession { ScanTime = now, DevicesFound = devicesOnlineCount, AveragePingMs = avgPing });
            _scanRepo.Save();
            _deviceRepo.Save();

            // 🔥 4. РОЗУМНЕ СПОВІЩЕННЯ (BATCHING)
            if (currentScanAlerts.Count > 0)
            {
                if (currentScanAlerts.Count == 1)
                {
                    // Якщо тільки один ворог — показуємо деталі
                    OnSecurityAlert?.Invoke(currentScanAlerts[0]);
                }
                else
                {
                    // Якщо їх багато — показуємо одне загальне повідомлення
                    string massAlert = $"⚠️ МАСОВА ТРИВОГА: Виявлено {currentScanAlerts.Count} нових недовірених пристроїв!";
                    OnSecurityAlert?.Invoke(massAlert);

                    // (Деталі вже записані в текстовий файл і базу даних через LogAndNotify)
                }
            }
        }

        // Оновлений метод з параметром fireEvent
        private void LogAndNotify(EventType type, string msg, int? deviceId, bool isTrusted, bool fireEvent = true)
        {
            // 1. БД
            _eventRepo.Add(new NetworkEvent { Timestamp = DateTime.Now, Type = type, Message = msg, DeviceId = deviceId });
            _eventRepo.Save();

            // 2. TXT
            FileLogger.Log(msg);

            // 3. UI Подія (Тільки якщо дозволено параметром fireEvent)
            if (fireEvent && type == EventType.DeviceConnected && !isTrusted)
            {
                OnSecurityAlert?.Invoke(msg);
            }
        }

        public void ClearAllHistory()
        {
            foreach (var d in _deviceRepo.GetAll()) _deviceRepo.Delete(d.Id);
            foreach (var e in _eventRepo.GetAll()) _eventRepo.Delete(e.Id);
            foreach (var s in _scanRepo.GetAll()) _scanRepo.Delete(s.Id);
            _deviceRepo.Save(); _eventRepo.Save(); _scanRepo.Save();
        }
    }
}