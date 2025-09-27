using first_task_file_encryptor.Domain;
using first_task_file_encryptor.Domain.Interfaces;
using first_task_file_encryptor.UI.ViewModels;
using System.Windows;

namespace first_task_file_encryptor
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IEncryptionService encryptionService = new AesEncryptionService();
            var mainViewModel = new MainViewModel(encryptionService);

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            mainWindow.Show();
            base.OnStartup(e);
        }
    }
}