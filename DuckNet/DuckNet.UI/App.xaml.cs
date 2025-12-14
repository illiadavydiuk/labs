using System.Windows;
using DuckNet.Data.Context;
using DuckNet.Data.Entities;
using DuckNet.Repositories.Implementations;
using DuckNet.Services.Implementations;

namespace DuckNet.UI // 🔥 Перевір, щоб тут було UI (великими)
{
    public partial class App : Application
    {
        private DuckNetDbContext _dbContext;
        private MainWindow _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. БД
            _dbContext = new DuckNetDbContext();
            _dbContext.Database.EnsureCreated();

            // ...
            // 2. Репозиторії
            var deviceRepo = new Repository<Device>(_dbContext);
            var eventRepo = new Repository<NetworkEvent>(_dbContext); // 🔥 НОВЕ

            // 3. Сервіси
            var scannerService = new NetworkScannerService();
            var deviceService = new DeviceService(deviceRepo, eventRepo); // 🔥 Передаємо eventRepo
            var adapterService = new AdapterService();
            // ...
            // 4. Головне вікно
            // 🔥 2. Передаємо adapterService третім параметром!
            _mainWindow = new MainWindow(scannerService, deviceService, adapterService);
            _mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _dbContext?.Dispose();
            base.OnExit(e);
        }
    }
}