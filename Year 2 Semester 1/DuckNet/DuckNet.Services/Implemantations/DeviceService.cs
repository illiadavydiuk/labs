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
        private readonly VendorService _vendorService;

        public event Action<string> OnSecurityAlert;

        public DeviceService(IRepository<Device> deviceRepo, IRepository<NetworkEvent> eventRepo, IRepository<ScanSession> scanRepo)
        {
            _deviceRepo = deviceRepo;
            _eventRepo = eventRepo;
            _scanRepo = scanRepo;
            _vendorService = new VendorService(); 
        }

        public IEnumerable<Device> GetAllDevices() => _deviceRepo.GetAll();
        public IEnumerable<NetworkEvent> GetRecentEvents() => _eventRepo.GetAll().OrderByDescending(e => e.Timestamp).Take(100).ToList();
        public IEnumerable<ScanSession> GetScanHistory() => _scanRepo.GetAll().OrderBy(s => s.ScanTime).TakeLast(20).ToList();
        public IEnumerable<NetworkEvent> GetEventsByDate(DateTime date) => _eventRepo.GetAll().Where(e => e.Timestamp.Date == date.Date).OrderBy(e => e.Timestamp).ToList();

        public void UpdateDevice(Device device)
        {
            _deviceRepo.Update(device);
            _deviceRepo.Save();
        }

        public void UpdateDevices(List<Device> scannedDevices)
        {
            var existingDevices = _deviceRepo.GetAll().ToList();
            var now = DateTime.Now;
            int devicesOnlineCount = 0;
            long totalPingSum = 0;
            List<string> currentScanAlerts = new List<string>();

            // 1. Пошук відключених
            foreach (var existing in existingDevices)
            {
                if (existing.IsOnline && !scannedDevices.Any(d => d.MacAddress == existing.MacAddress))
                {
                    existing.IsOnline = false;
                    _deviceRepo.Update(existing);
                    LogAndNotify(EventType.DeviceDisconnected, $"Пристрій відключився: {existing.IpAddress}", existing.Id, true, fireEvent: false);
                }
            }

            // 2. Обробка знайдених
            foreach (var scanned in scannedDevices)
            {
                devicesOnlineCount++;
                totalPingSum += scanned.LastPingMs;

                // 🔥 ШУКАЄМО У ЛОКАЛЬНОМУ ФАЙЛІ OUI.TXT
                string vendor = _vendorService.GetVendor(scanned.MacAddress);

                if (scanned.Hostname == "Unknown" || scanned.Hostname.StartsWith("Device"))
                {
                    scanned.Hostname = vendor != "Unknown" ? $"{vendor} Device" : "Unknown Device";
                }

                var existing = existingDevices.FirstOrDefault(d => d.MacAddress == scanned.MacAddress);

                if (existing != null)
                {
                    if (!existing.IsOnline)
                    {
                        if (!existing.IsTrusted)
                        {
                            currentScanAlerts.Add($"!!!! НЕДОВІРЕНИЙ ПОВЕРНУВСЯ: {scanned.IpAddress}");
                        }
                        LogAndNotify(EventType.DeviceConnected, $"Пристрій повернувся: {scanned.IpAddress}", existing.Id, existing.IsTrusted, fireEvent: false);
                    }

                    existing.IsOnline = true;
                    existing.IpAddress = scanned.IpAddress;
                    existing.LastSeen = now;

                    if (scanned.Hostname != "Unknown" && !scanned.Hostname.Contains("Device"))
                    {
                        existing.Hostname = scanned.Hostname;
                    }
                    else if (existing.Hostname.Contains("Unknown") && vendor != "Unknown")
                    {
                        existing.Hostname = $"{vendor} Device";
                    }

                    _deviceRepo.Update(existing);
                }
                else
                {
                    // Новий пристрій
                    scanned.IsTrusted = false;
                    _deviceRepo.Add(scanned);
                    _deviceRepo.Save();

                    currentScanAlerts.Add($"!!!! НОВИЙ ПРИСТРІЙ: {scanned.IpAddress} ({vendor})");
                    LogAndNotify(EventType.DeviceConnected, $"Новий пристрій: {scanned.IpAddress}", scanned.Id, false, fireEvent: false);
                }
            }

            // 3. Збереження сесії
            int avgPing = (devicesOnlineCount > 0) ? (int)(totalPingSum / devicesOnlineCount) : 0;
            _scanRepo.Add(new ScanSession { ScanTime = now, DevicesFound = devicesOnlineCount, AveragePingMs = avgPing });
            _scanRepo.Save();
            _deviceRepo.Save();

            // 4. Сповіщення
            if (currentScanAlerts.Count > 0)
            {
                if (currentScanAlerts.Count == 1) OnSecurityAlert?.Invoke(currentScanAlerts[0]);
                else OnSecurityAlert?.Invoke($"⚠️ МАСОВА ТРИВОГА: {currentScanAlerts.Count} нових!");
            }
        }

        private void LogAndNotify(EventType type, string msg, int? deviceId, bool isTrusted, bool fireEvent = true)
        {
            _eventRepo.Add(new NetworkEvent { Timestamp = DateTime.Now, Type = type, Message = msg, DeviceId = deviceId });
            _eventRepo.Save();
            FileLogger.Log(msg);
            if (fireEvent && type == EventType.DeviceConnected && !isTrusted) OnSecurityAlert?.Invoke(msg);
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