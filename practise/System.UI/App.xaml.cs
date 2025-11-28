using System;
using System.Windows;
using TaskManager.Data.Context;
using TaskManager.Repositories.Implementations;
using TaskManager.Services;

namespace TaskManager.UI
{
    public partial class App : Application
    {
        private SystemDbContext _dbContext;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                _dbContext = new SystemDbContext();

                _dbContext.Database.EnsureCreated();

                // 2. Створюємо репозиторії
                var devRepo = new DeveloperRepository(_dbContext);
                var projRepo = new ProjectRepository(_dbContext);
                var taskRepo = new TaskRepository(_dbContext);

                // 3. Створюємо сервіси
                var devService = new DeveloperService(devRepo);
                var projService = new ProjectService(projRepo, devRepo);
                var taskService = new TaskService(taskRepo, devRepo);


                var mainWindow = new MainWindow(projService, devService, taskService);
                mainWindow.Show(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критична помилка запуску:\n{ex.Message}\n\nDetals:\n{ex.InnerException?.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _dbContext?.Dispose();
            base.OnExit(e);
        }
    }
}