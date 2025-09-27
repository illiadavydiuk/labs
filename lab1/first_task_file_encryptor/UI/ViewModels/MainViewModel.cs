using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using first_task_file_encryptor.Domain.Interfaces;
using first_task_file_encryptor.Domain.Models;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace first_task_file_encryptor.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IEncryptionService _encryptionService;

        [ObservableProperty]
        private string _folderPath = "Папку не вибрано";

        [ObservableProperty]
        private string _statusMessage = "Готовий";

        public ObservableCollection<FileItem> Files { get; set; } = new();

        public MainViewModel(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }

        [RelayCommand]
        private void BrowseFolder()
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                FolderPath = dialog.FolderName;
                LoadFiles();
            }
        }

        [RelayCommand]
        private async Task EncryptAll(PasswordBox passwordBox)
        {
            await ProcessFiles(passwordBox, isEncrypt: true);
        }

        [RelayCommand]
        private async Task DecryptAll(PasswordBox passwordBox)
        {
            await ProcessFiles(passwordBox, isEncrypt: false);
        }

        private async Task ProcessFiles(PasswordBox passwordBox, bool isEncrypt)
        {
            string password = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(FolderPath) || !Directory.Exists(FolderPath) || string.IsNullOrWhiteSpace(password))
            {
                StatusMessage = "Будь ласка, оберіть папку та введіть пароль.";
                return;
            }

            string action = isEncrypt ? "Шифрування" : "Дешифрування";
            StatusMessage = $"{action}...";

            string outputSubFolder = isEncrypt ? "Encrypted" : "Decrypted";
            string baseOutputDir = Path.Combine(Path.GetDirectoryName(FolderPath), Path.GetFileName(FolderPath) + "_" + outputSubFolder);
            Directory.CreateDirectory(baseOutputDir);

            var filesToProcess = Files.ToList();
            int successCount = 0;
            int errorCount = 0;

            foreach (var fileItem in filesToProcess)
            {
                fileItem.Status = $"{action}...";

                string relativePath = Path.GetRelativePath(FolderPath, fileItem.FullPath);
                string outputFilePath = Path.Combine(baseOutputDir, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

                bool result;
                if (isEncrypt)
                {
                    if (fileItem.FileName.EndsWith(".enc", StringComparison.OrdinalIgnoreCase)) continue;
                    result = await _encryptionService.EncryptFileAsync(fileItem.FullPath, outputFilePath + ".enc", password);
                }
                else
                {
                    if (!fileItem.FileName.EndsWith(".enc", StringComparison.OrdinalIgnoreCase)) continue;
                    string decryptedFileName = outputFilePath.Replace(".enc", "");
                    result = await _encryptionService.DecryptFileAsync(fileItem.FullPath, decryptedFileName, password);
                }

                if (result)
                {
                    successCount++;
                }
                else
                {
                    fileItem.Status = isEncrypt ? "Помилка шифрування" : "Помилка (пароль?)";
                    errorCount++;
                }
            }

            StatusMessage = $"Готово. Успішно: {successCount}, Помилки: {errorCount}.";

            if (errorCount > 0 && !isEncrypt)
            {
                MessageBox.Show($"Не вдалося дешифрувати {errorCount} файл(ів). Невірний пароль.", "Помилка дешифрування", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (successCount > 0 || errorCount == 0)
            {
                MessageBox.Show($"Операцію успішно завершено!\nРезультати збережено в папці:\n{baseOutputDir}", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadFiles();
        }

        private void LoadFiles()
        {
            Files.Clear();
            if (Directory.Exists(FolderPath))
            {
                foreach (var file in Directory.GetFiles(FolderPath, "*", SearchOption.AllDirectories))
                {
                    var fileInfo = new FileInfo(file);

                    string initialStatus;
                    if (fileInfo.Extension.Equals(".enc", StringComparison.OrdinalIgnoreCase))
                    {
                        initialStatus = "Зашифровано";
                    }
                    else
                    {
                        initialStatus = "Готовий";
                    }

                    Files.Add(new FileItem
                    {
                        // Показуємо відносний шлях, щоб було зрозуміло, де знаходиться файл
                        FileName = Path.GetRelativePath(FolderPath, fileInfo.FullName),
                        FullPath = fileInfo.FullName,
                        FileSize = fileInfo.Length,
                        Status = initialStatus
                    });
                }
            }
        }
    }
}