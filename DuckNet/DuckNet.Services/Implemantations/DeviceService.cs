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

        public DeviceService(IRepository<Device> deviceRepo)
        {
            _deviceRepo = deviceRepo;
        }

        public IEnumerable<Device> GetAllDevices() => _deviceRepo.GetAll();

        public void UpdateDevices(List<Device> scannedDevices)
        {
            var existingDevices = _deviceRepo.GetAll().ToList();

            // Спочатку помітимо всі старі як Offline (перед оновленням)
            foreach (var dev in existingDevices)
            {
                dev.IsOnline = false;
                _deviceRepo.Update(dev);
            }

            foreach (var scannedDev in scannedDevices)
            {
                // Шукаємо пристрій за MAC-адресою (вона унікальна)
                var existing = existingDevices.FirstOrDefault(d => d.MacAddress == scannedDev.MacAddress);

                if (existing != null)
                {
                    // Оновлюємо існуючий
                    existing.IsOnline = true;
                    existing.IpAddress = scannedDev.IpAddress; // IP міг змінитися
                    existing.LastSeen = DateTime.Now;
                    if (existing.Hostname == "Unknown" && scannedDev.Hostname != "Unknown")
                        existing.Hostname = scannedDev.Hostname;

                    _deviceRepo.Update(existing);
                }
                else
                {
                    // Додаємо новий
                    _deviceRepo.Add(scannedDev);
                }
            }

            _deviceRepo.Save();
        }
    }
}