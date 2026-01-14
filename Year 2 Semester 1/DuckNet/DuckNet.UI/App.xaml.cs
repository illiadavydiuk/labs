using System.Windows;
using DuckNet.Data.Context;
using DuckNet.Data.Entities; // Тут лежить AdapterProfile
using DuckNet.Repositories.Implementations;
using DuckNet.Services.Implementations;

namespace DuckNet.UI
{
    public partial class App : Application
    {
        private DuckNetDbContext _dbContext;
        private MainWindow _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _dbContext = new DuckNetDbContext();
            _dbContext.Database.EnsureCreated();

            // Репозиторії
            var deviceRepo = new Repository<Device>(_dbContext);
            var eventRepo = new Repository<NetworkEvent>(_dbContext);
            var scanRepo = new Repository<ScanSession>(_dbContext);
            var profileRepo = new Repository<AdapterProfile>(_dbContext); // 🔥 НОВЕ: Репо для профілів

            // Сервіси
            var scannerService = new NetworkScannerService();
            var deviceService = new DeviceService(deviceRepo, eventRepo, scanRepo);
            var adapterService = new AdapterService();

            // 🔥 Передаємо profileRepo у вікно
            _mainWindow = new MainWindow(scannerService, deviceService, adapterService, profileRepo);
            _mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _dbContext?.Dispose();
            base.OnExit(e);
        }
    }
}