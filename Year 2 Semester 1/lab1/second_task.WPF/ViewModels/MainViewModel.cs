using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using second_task.WPF.Models;
using second_task.WPF.Services;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using second_task.Data.Providers;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Diagnostics;

namespace second_task.WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        private readonly ReportService _reportService;

        [ObservableProperty]
        private ObservableCollection<Meal> _meals = new();

        [ObservableProperty]
        private ObservableCollection<Meal> _filteredMeals = new();

        [ObservableProperty]
        private string _statusText = "Ready";

        [ObservableProperty]
        private string _currentFilePath = string.Empty;

        [ObservableProperty]
        private PlotModel? _plotModel;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedSortColumn = "None";

        [ObservableProperty]
        private bool _sortAscending = true;

        [ObservableProperty]
        private string _selectedGroupColumn = "None";

        [ObservableProperty]
        private string _selectedAggregation = "Count";

        [ObservableProperty]
        private ObservableCollection<string> _recentFiles = new();

        [ObservableProperty]
        private ObservableCollection<AggregationResult> _aggregationResults = new();

        [ObservableProperty]
        private bool _showAggregations = false;

        [ObservableProperty]
        private Meal? _selectedMeal;

        public List<string> SortColumns { get; } = new()
        {
            "None", "MealName", "Cuisine", "Diet", "MealType", "Calories", "ProteinG", "CarbsG", "FatG", "Rating"
        };

        public List<string> GroupColumns { get; } = new()
        {
            "None", "Cuisine", "Diet", "MealType", "IsHealthy"
        };

        public List<string> AggregationTypes { get; } = new()
        {
            "Count", "Sum", "Average", "Min", "Max"
        };

        public MainViewModel()
        {
            _dataService = new DataService();
            _reportService = new ReportService();
            
            // Ініціалізувати логування
            LoggingService.LogInfo("Програма запущена");
            
            // Створити необхідні папки
            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Outputs", "Charts");
            Directory.CreateDirectory(outputDir);
            LoggingService.LogInfo($"Створено папку для графіків: {outputDir}");
            
            LoadRecentFiles();
            InitializePlot();
        }

        [RelayCommand]
        private async Task OpenFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All Supported|*.csv;*.json;*.xml;*.xlsx|CSV files|*.csv|JSON files|*.json|XML files|*.xml|Excel files|*.xlsx",
                Title = "Open Data File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await LoadFileWithPreview(openFileDialog.FileName);
            }
        }

        [RelayCommand]
        private async Task OpenRecentFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                await LoadFileWithPreview(filePath);
            }
            else
            {
                MessageBox.Show("File not found. It may have been moved or deleted.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                RecentFiles.Remove(filePath);
                SaveRecentFiles();
            }
        }

        private async Task LoadFileWithPreview(string filePath)
        {
            try
            {
                StatusText = "Loading preview...";
                LoggingService.LogInfo($"Початок завантаження файлу: {filePath}");
                var settings = new ImportSettings();
                
                // Load preview data
                var previewData = await Task.Run(() => _dataService.LoadPreviewData(filePath, settings));
                
                // Show preview window
                var previewWindow = new Views.FilePreviewWindow(filePath, previewData);
                if (previewWindow.ShowDialog() == true && previewWindow.LoadConfirmed)
                {
                    StatusText = "Loading full data...";
                    var loadedMeals = await Task.Run(() => _dataService.LoadData(filePath, settings));
                    
                    Meals.Clear();
                    foreach (var meal in loadedMeals)
                    {
                        Meals.Add(meal);
                    }
                    
                    ApplyFilter();
                    CurrentFilePath = filePath;
                    AddToRecentFiles(filePath);
                    StatusText = $"Loaded {Meals.Count} meals from {Path.GetFileName(filePath)}";
                    LoggingService.LogInfo($"Успішно завантажено {Meals.Count} страв з файлу {Path.GetFileName(filePath)}");
                    
                    UpdateChart();
                }
                else
                {
                    StatusText = "File loading cancelled";
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Помилка при завантаженні файлу {filePath}", ex);
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Error loading file";
            }
        }

        [RelayCommand]
        private async Task SaveFile()
        {
            if (!Meals.Any())
            {
                MessageBox.Show("No data to save.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "All Supported|*.csv;*.json;*.xml;*.xlsx|CSV files|*.csv|JSON files|*.json|XML files|*.xml|Excel files|*.xlsx",
                Title = "Save Data File"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var fileExtension = Path.GetExtension(saveFileDialog.FileName).ToLower();
                
                // Показувати налаштування тільки для CSV файлів
                if (fileExtension == ".csv")
                {
                    var exportSettingsWindow = new Views.ExportSettingsWindow();
                    if (exportSettingsWindow.ShowDialog() == true && exportSettingsWindow.IsConfirmed)
                    {
                        try
                        {
                            StatusText = "Saving CSV data...";
                            var settings = new ImportSettings
                            {
                                Encoding = System.Text.Encoding.UTF8,
                                DelimiterChar = exportSettingsWindow.GetDelimiterChar(),
                                HasHeaders = exportSettingsWindow.IncludeHeaders
                            };
                            
                            await Task.Run(() => _dataService.SaveData(saveFileDialog.FileName, Meals, settings));
                            
                            StatusText = $"Saved {Meals.Count} meals to {Path.GetFileName(saveFileDialog.FileName)}";
                            LoggingService.LogInfo($"Успішно збережено {Meals.Count} страв у CSV файл {Path.GetFileName(saveFileDialog.FileName)} з розділювачем '{settings.DelimiterChar}'");
                        }
                        catch (Exception ex)
                        {
                            LoggingService.LogError($"Помилка при збереженні CSV файлу {saveFileDialog.FileName}", ex);
                            MessageBox.Show($"Error saving CSV file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            StatusText = "Error saving CSV file";
                        }
                    }
                    else
                    {
                        StatusText = "Save cancelled";
                    }
                }
                else
                {
                    // Для інших форматів (JSON, XML, XLSX) - без налаштувань
                    try
                    {
                        StatusText = $"Saving {fileExtension.ToUpper()} data...";
                        var settings = new ImportSettings
                        {
                            Encoding = System.Text.Encoding.UTF8,
                            DelimiterChar = ',', // За замовчуванням для внутрішніх потреб
                            HasHeaders = true
                        };
                        
                        await Task.Run(() => _dataService.SaveData(saveFileDialog.FileName, Meals, settings));
                        
                        StatusText = $"Saved {Meals.Count} meals to {Path.GetFileName(saveFileDialog.FileName)}";
                        LoggingService.LogInfo($"Успішно збережено {Meals.Count} страв у {fileExtension.ToUpper()} файл {Path.GetFileName(saveFileDialog.FileName)}");
                    }
                    catch (Exception ex)
                    {
                        LoggingService.LogError($"Помилка при збереженні {fileExtension.ToUpper()} файлу {saveFileDialog.FileName}", ex);
                        MessageBox.Show($"Error saving {fileExtension.ToUpper()} file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        StatusText = $"Error saving {fileExtension.ToUpper()} file";
                    }
                }
            }
        }

        [RelayCommand]
        private async Task GenerateXlsxReport()
        {
            if (!Meals.Any())
            {
                MessageBox.Show("No data to generate report.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files|*.xlsx",
                Title = "Save XLSX Report",
                FileName = "MealsReport.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    StatusText = "Collecting charts for XLSX report...";
                    LoggingService.LogInfo("Початок генерації XLSX звіту");
                    
                    // Зібрати збережені графіки
                    var chartPaths = GetSavedReportCharts();
                    
                    StatusText = "Generating XLSX report...";
                    await Task.Run(() => _reportService.GenerateXlsxReport(saveFileDialog.FileName, Meals, chartPaths));
                    
                    StatusText = $"XLSX report saved to {Path.GetFileName(saveFileDialog.FileName)}";
                    LoggingService.LogInfo($"XLSX звіт успішно створено: {saveFileDialog.FileName}");
                    MessageBox.Show("XLSX report generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    LoggingService.LogError("Помилка при генерації XLSX звіту", ex);
                    MessageBox.Show($"Error generating XLSX report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText = "Error generating XLSX report";
                }
            }
        }

        [RelayCommand]
        private async Task GenerateDocxReport()
        {
            if (!Meals.Any())
            {
                MessageBox.Show("No data to generate report.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Document|*.docx",
                Title = "Save DOCX Report",
                FileName = $"HealthyEatingReport_{DateTime.Now:yyyyMMdd}.docx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    StatusText = "Collecting saved charts for report...";
                    LoggingService.LogInfo("Початок генерації DOCX звіту");
                    
                    // Зібрати збережені графіки
                    var chartPaths = GetSavedReportCharts();
                    
                    StatusText = "Generating DOCX report...";
                    await Task.Run(() => _reportService.GenerateDocxReport(saveFileDialog.FileName, Meals, chartPaths));
                    
                    StatusText = $"DOCX report saved to {Path.GetFileName(saveFileDialog.FileName)}";
                    LoggingService.LogInfo($"DOCX звіт успішно створено: {saveFileDialog.FileName}. Включено {chartPaths.Count} графіків");
                    MessageBox.Show($"DOCX report with {chartPaths.Count} charts generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    LoggingService.LogError("Помилка при генерації DOCX звіту", ex);
                    MessageBox.Show($"Error generating DOCX report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText = "Error generating DOCX report";
                }
            }
        }

        private List<string> GetSavedReportCharts()
        {
            var chartPaths = new List<string>();
            
            try
            {
                var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Outputs", "Charts");
                
                if (!Directory.Exists(outputDir))
                {
                    LoggingService.LogInfo("Папка з графіками для звітів не існує. Створюємо порожню папку.");
                    Directory.CreateDirectory(outputDir);
                    return chartPaths;
                }
                
                // Знайти всі файли, що починаються з ReportChart_*.png
                var reportChartFiles = Directory.GetFiles(outputDir, "ReportChart_*.png");
                
                foreach (var file in reportChartFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.Exists && fileInfo.Length > 1024) // Перевірити, що файл не пошкоджений
                    {
                        chartPaths.Add(file);
                    }
                    else
                    {
                        LoggingService.LogInfo($"Пропускаємо пошкоджений або занадто малий файл графіка: {file}");
                    }
                }
                
                LoggingService.LogInfo($"Знайдено {chartPaths.Count} збережених графіків для звіту");
                
                if (chartPaths.Count == 0)
                {
                    LoggingService.LogInfo("Не знайдено збережених графіків для звіту. Користувач може створити графіки через меню 'Show Charts' та зберегти їх кнопкою 'Save for Report'.");
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Помилка при пошуку збережених графіків для звіту", ex);
            }
            
            return chartPaths;
        }

        [RelayCommand]
        private void ShowCharts()
        {
            if (!Meals.Any())
            {
                MessageBox.Show("No data available for charts.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var chartWindow = new Views.ChartWindow(FilteredMeals);
            chartWindow.Show();
        }

        [RelayCommand]
        private void ShowLogs()
        {
            try
            {
                var logFilePath = LoggingService.GetLogFilePath();
                
                if (!File.Exists(logFilePath))
                {
                    MessageBox.Show("Файл логів ще не створено.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                Process.Start(new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = $"\"{logFilePath}\"",
                    UseShellExecute = true
                });
                
                LoggingService.LogInfo("Відкрито файл логів для перегляду");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Помилка при відкритті файлу логів", ex);
                MessageBox.Show($"Помилка при відкритті файлу логів: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AddMeal()
        {
            var editWindow = new Views.MealEditWindow();
            if (editWindow.ShowDialog() == true && editWindow.Result != null)
            {
                var newMeal = editWindow.Result;
                newMeal.MealId = Meals.Count > 0 ? Meals.Max(m => m.MealId) + 1 : 1;
                
                Meals.Add(newMeal);
                ApplyFilter();
                StatusText = "New meal added";
                
                // Focus on new meal
                SelectedMeal = newMeal;
                ScrollToMeal(newMeal);
            }
        }

        [RelayCommand]
        private void EditMeal()
        {
            if (SelectedMeal == null)
            {
                MessageBox.Show("Please select a meal to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editWindow = new Views.MealEditWindow(SelectedMeal);
            if (editWindow.ShowDialog() == true && editWindow.Result != null)
            {
                var editedMeal = editWindow.Result;
                var index = Meals.IndexOf(SelectedMeal);
                if (index >= 0)
                {
                    Meals[index] = editedMeal;
                    ApplyFilter();
                    StatusText = "Meal updated";
                    
                    // Focus on edited meal
                    SelectedMeal = editedMeal;
                    ScrollToMeal(editedMeal);
                }
            }
        }

        private void ScrollToMeal(Meal meal)
        {
            // This will be implemented when we have access to the DataGrid
            var index = FilteredMeals.IndexOf(meal);
            if (index >= 0)
            {
                // Scroll logic will be added in code-behind
            }
        }

        [RelayCommand]
        private void DeleteMeal()
        {
            if (SelectedMeal == null)
            {
                MessageBox.Show("Please select a meal to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete '{SelectedMeal.MealName}'?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Meals.Remove(SelectedMeal);
                ApplyFilter();
                StatusText = "Meal deleted";
                SelectedMeal = null;
            }
        }

        [RelayCommand]
        private void ClearSorting()
        {
            SelectedSortColumn = "None";
            SortAscending = true;
            ApplyFilter();
        }

        [RelayCommand]
        private void CalculateAggregations()
        {
            AggregationResults.Clear();
            
            if (SelectedGroupColumn == "None" || !Meals.Any())
            {
                ShowAggregations = false;
                return;
            }

            var groups = GroupMeals();
            
            foreach (var group in groups)
            {
                var result = new AggregationResult
                {
                    GroupName = group.Key,
                    AggregationType = SelectedAggregation,
                    Count = group.Count()
                };

                switch (SelectedAggregation)
                {
                    case "Count":
                        result.Value = group.Count();
                        break;
                    case "Sum":
                        result.Value = group.Sum(m => m.Calories);
                        break;
                    case "Average":
                        result.Value = group.Average(m => m.Calories);
                        break;
                    case "Min":
                        result.Value = group.Min(m => m.Calories);
                        break;
                    case "Max":
                        result.Value = group.Max(m => m.Calories);
                        break;
                }

                AggregationResults.Add(result);
            }

            ShowAggregations = true;
        }

        private IEnumerable<IGrouping<string, Meal>> GroupMeals()
        {
            return SelectedGroupColumn switch
            {
                "Cuisine" => FilteredMeals.GroupBy(m => m.Cuisine?.Name ?? "Unknown"),
                "Diet" => FilteredMeals.GroupBy(m => m.Diet?.Name ?? "Unknown"),
                "MealType" => FilteredMeals.GroupBy(m => m.MealType),
                "IsHealthy" => FilteredMeals.GroupBy(m => m.IsHealthy ? "Healthy" : "Unhealthy"),
                _ => new List<IGrouping<string, Meal>>()
            };
        }

        partial void OnSearchTextChanged(string value) => ApplyFilter();
        partial void OnSelectedSortColumnChanged(string value) => ApplyFilter();
        partial void OnSortAscendingChanged(bool value) => ApplyFilter();
        partial void OnSelectedGroupColumnChanged(string value) => CalculateAggregations();
        partial void OnSelectedAggregationChanged(string value) => CalculateAggregations();

        private void ApplyFilter()
        {
            FilteredMeals.Clear();
            
            var filtered = Meals.AsEnumerable();
            
            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(m => 
                    m.MealName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (m.Cuisine?.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (m.Diet?.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    m.MealType.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }
            
            // Apply sorting only if not "None"
            if (SelectedSortColumn != "None")
            {
                filtered = SelectedSortColumn switch
                {
                    "MealName" => SortAscending ? filtered.OrderBy(m => m.MealName) : filtered.OrderByDescending(m => m.MealName),
                    "Cuisine" => SortAscending ? filtered.OrderBy(m => m.Cuisine?.Name ?? "") : filtered.OrderByDescending(m => m.Cuisine?.Name ?? ""),
                    "Diet" => SortAscending ? filtered.OrderBy(m => m.Diet?.Name ?? "") : filtered.OrderByDescending(m => m.Diet?.Name ?? ""),
                    "MealType" => SortAscending ? filtered.OrderBy(m => m.MealType) : filtered.OrderByDescending(m => m.MealType),
                    "Calories" => SortAscending ? filtered.OrderBy(m => m.Calories) : filtered.OrderByDescending(m => m.Calories),
                    "ProteinG" => SortAscending ? filtered.OrderBy(m => m.ProteinG) : filtered.OrderByDescending(m => m.ProteinG),
                    "CarbsG" => SortAscending ? filtered.OrderBy(m => m.CarbsG) : filtered.OrderByDescending(m => m.CarbsG),
                    "FatG" => SortAscending ? filtered.OrderBy(m => m.FatG) : filtered.OrderByDescending(m => m.FatG),
                    "Rating" => SortAscending ? filtered.OrderBy(m => m.Rating) : filtered.OrderByDescending(m => m.Rating),
                    _ => filtered
                };
            }
            
            foreach (var meal in filtered)
            {
                FilteredMeals.Add(meal);
            }
        }

        private void LoadRecentFiles()
        {
            try
            {
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "second_task.WPF");
                var recentFilesPath = Path.Combine(appDataPath, "recent_files.txt");
                
                if (File.Exists(recentFilesPath))
                {
                    var files = File.ReadAllLines(recentFilesPath);
                    RecentFiles.Clear();
                    foreach (var file in files.Take(10))
                    {
                        if (!string.IsNullOrWhiteSpace(file))
                        {
                            RecentFiles.Add(file);
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors loading recent files
            }
        }

        private void SaveRecentFiles()
        {
            try
            {
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "second_task.WPF");
                Directory.CreateDirectory(appDataPath);
                var recentFilesPath = Path.Combine(appDataPath, "recent_files.txt");
                
                File.WriteAllLines(recentFilesPath, RecentFiles);
            }
            catch
            {
                // Ignore errors saving recent files
            }
        }

        private void AddToRecentFiles(string filePath)
        {
            if (RecentFiles.Contains(filePath))
            {
                RecentFiles.Remove(filePath);
            }
            
            RecentFiles.Insert(0, filePath);
            
            while (RecentFiles.Count > 10)
            {
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
            }
            
            SaveRecentFiles();
        }

        private void InitializePlot()
        {
            PlotModel = new PlotModel { Title = "Meals Data" };
        }

        private void UpdateChart()
        {
            if (!Meals.Any()) return;

            var plotModel = new PlotModel { Title = "Calories by Cuisine" };
            
            var series = new BarSeries { Title = "Average Calories" };
            
            var cuisineGroups = Meals.GroupBy(m => m.Cuisine?.Name ?? "Unknown")
                                   .OrderByDescending(g => g.Average(m => m.Calories))
                                   .Take(10)
                                   .ToList();
            
            for (int i = 0; i < cuisineGroups.Count; i++)
            {
                var group = cuisineGroups[i];
                var avgCalories = group.Average(m => m.Calories);
                series.Items.Add(new BarItem { Value = avgCalories });
            }
            
            // Add category axis
            var categoryAxis = new CategoryAxis { Position = AxisPosition.Left };
            foreach (var group in cuisineGroups)
            {
                categoryAxis.Labels.Add(group.Key);
            }
            plotModel.Axes.Add(categoryAxis);
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Average Calories" });
            
            plotModel.Series.Add(series);
            PlotModel = plotModel;
        }
    }
}
