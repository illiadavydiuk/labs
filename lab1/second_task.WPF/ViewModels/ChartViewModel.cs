using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using second_task.WPF.Models;
using second_task.WPF.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace second_task.WPF.ViewModels
{
    public partial class ChartViewModel : ObservableObject
    {
        [ObservableProperty]
        private PlotModel? _plotModel;

        [ObservableProperty]
        private ObservableCollection<Meal> _meals = new();

        [ObservableProperty]
        private string _selectedChartType = "Bar Chart";

        [ObservableProperty]
        private string _selectedXAxis = "Cuisine";

        [ObservableProperty]
        private string _selectedYAxis = "Calories";

        [ObservableProperty]
        private string _selectedGroupBy = "None";

        public List<string> ChartTypes { get; } = new()
        {
            "Bar Chart",
            "Line Chart", 
            "Pie Chart"
        };

        public List<string> AxisOptions { get; } = new()
        {
            "Cuisine",
            "Diet",
            "MealType",
            "Calories",
            "ProteinG",
            "CarbsG", 
            "FatG",
            "Rating",
            "IsHealthy"
        };

        public List<string> GroupByOptions { get; } = new()
        {
            "None",
            "Cuisine",
            "Diet",
            "MealType",
            "IsHealthy"
        };

        public ChartViewModel()
        {
            InitializePlot();
        }

        [RelayCommand]
        private void SaveChart()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image (*.png)|*.png",
                Title = "Save Chart As",
                FileName = $"Chart_{DateTime.Now:yyyyMMdd_HHmmss}.png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    SaveChartToFile(saveFileDialog.FileName);
                    LoggingService.LogInfo($"Графік збережено у файл: {saveFileDialog.FileName}");
                    MessageBox.Show($"Chart saved to {saveFileDialog.FileName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    LoggingService.LogError("Помилка при збереженні графіка", ex);
                    MessageBox.Show($"Error saving chart: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void SaveForReport()
        {
            try
            {
                // Створити унікальне ім'я файлу на основі налаштувань графіка
                var fileName = $"ReportChart_{SelectedChartType.Replace(" ", "")}_{SelectedXAxis}_{SelectedYAxis}_{DateTime.Now:yyyyMMddHHmmss}.png";
                var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Outputs", "Charts");
                Directory.CreateDirectory(outputDir);
                var filePath = Path.Combine(outputDir, fileName);
                
                SaveChartForReport(filePath);
                
                LoggingService.LogInfo($"Графік збережено для звіту: {filePath}");
                MessageBox.Show($"Графік збережено для звіту!\nФайл: {fileName}", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Помилка при збереженні графіка для звіту", ex);
                MessageBox.Show($"Помилка при збереженні графіка для звіту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string SaveChartToFile(string filePath)
        {
            var pngExporter = new OxyPlot.Wpf.PngExporter { Width = 800, Height = 600 };
            using var stream = File.Create(filePath);
            pngExporter.Export(PlotModel, stream);
            return filePath;
        }

        public void SaveChartForReport(string filePath)
        {
            try
            {
                var pngExporter = new OxyPlot.Wpf.PngExporter { Width = 800, Height = 600 };
                using var stream = File.Create(filePath);
                pngExporter.Export(PlotModel, stream);
                
                // Перевірити, що файл створено і має розумний розмір
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists || fileInfo.Length < 1024) // менше 1KB
                {
                    throw new InvalidOperationException("Створений файл графіка занадто малий або пошкоджений");
                }
                
                LoggingService.LogInfo($"Графік успішно збережено для звіту. Розмір файлу: {fileInfo.Length} байт");
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Помилка при збереженні графіка для звіту у файл {filePath}", ex);
                throw;
            }
        }

        public string SaveChartToTempFile()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"Chart_{SelectedChartType}_{DateTime.Now:yyyyMMddHHmmss}.png");
            return SaveChartToFile(tempPath);
        }

        public void UpdateData(IEnumerable<Meal> meals)
        {
            Meals.Clear();
            foreach (var meal in meals)
            {
                Meals.Add(meal);
            }
            UpdateChart();
        }

        partial void OnSelectedChartTypeChanged(string value) => UpdateChart();
        partial void OnSelectedXAxisChanged(string value) => UpdateChart();
        partial void OnSelectedYAxisChanged(string value) => UpdateChart();
        partial void OnSelectedGroupByChanged(string value) => UpdateChart();

        private void InitializePlot()
        {
            PlotModel = new PlotModel { Title = "Data Visualization" };
        }

        private void UpdateChart()
        {
            if (!Meals.Any())
            {
                PlotModel = new PlotModel { Title = "No Data Available" };
                return;
            }

            switch (SelectedChartType)
            {
                case "Bar Chart":
                    CreateBarChart();
                    break;
                case "Line Chart":
                    CreateLineChart();
                    break;
                case "Pie Chart":
                    CreatePieChart();
                    break;
                default:
                    CreateBarChart();
                    break;
            }
        }

        private void CreateBarChart()
        {
            var plotModel = new PlotModel { Title = $"{SelectedYAxis} by {SelectedXAxis}" };

            if (IsNumericField(SelectedXAxis))
            {
                // Scatter plot for numeric X axis
                var scatterSeries = new ScatterSeries { Title = SelectedYAxis };
                foreach (var meal in Meals)
                {
                    var x = GetNumericValue(meal, SelectedXAxis);
                    var y = GetNumericValue(meal, SelectedYAxis);
                    scatterSeries.Points.Add(new ScatterPoint(x, y));
                }
                plotModel.Series.Add(scatterSeries);
                plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = SelectedXAxis });
                plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = SelectedYAxis });
            }
            else
            {
                // Bar chart for categorical X axis
                var groups = GroupData();
                var barSeries = new BarSeries { Title = SelectedYAxis };

                var categoryAxis = new CategoryAxis { Position = AxisPosition.Left };
                
                foreach (var group in groups.Take(15)) // Limit to 15 categories
                {
                    categoryAxis.Labels.Add(group.Key);
                    var value = IsNumericField(SelectedYAxis) ? group.Average(m => GetNumericValue(m, SelectedYAxis)) : group.Count();
                    barSeries.Items.Add(new BarItem { Value = value });
                }

                plotModel.Axes.Add(categoryAxis);
                plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = IsNumericField(SelectedYAxis) ? $"Average {SelectedYAxis}" : "Count" });
                plotModel.Series.Add(barSeries);
            }

            PlotModel = plotModel;
        }

        private void CreateLineChart()
        {
            var plotModel = new PlotModel { Title = $"{SelectedYAxis} Trend" };

            if (IsNumericField(SelectedXAxis) && IsNumericField(SelectedYAxis))
            {
                var lineSeries = new LineSeries { Title = SelectedYAxis };
                var sortedMeals = Meals.OrderBy(m => GetNumericValue(m, SelectedXAxis)).ToList();
                
                foreach (var meal in sortedMeals)
                {
                    var x = GetNumericValue(meal, SelectedXAxis);
                    var y = GetNumericValue(meal, SelectedYAxis);
                    lineSeries.Points.Add(new DataPoint(x, y));
                }
                
                plotModel.Series.Add(lineSeries);
                plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = SelectedXAxis });
                plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = SelectedYAxis });
            }
            else
            {
                // Line chart with categorical X axis
                var groups = GroupData();
                var lineSeries = new LineSeries { Title = SelectedYAxis };

                int index = 0;
                foreach (var group in groups.Take(15))
                {
                    var value = IsNumericField(SelectedYAxis) ? group.Average(m => GetNumericValue(m, SelectedYAxis)) : group.Count();
                    lineSeries.Points.Add(new DataPoint(index++, value));
                }

                var categoryAxis = new CategoryAxis { Position = AxisPosition.Bottom };
                foreach (var group in groups.Take(15))
                {
                    categoryAxis.Labels.Add(group.Key);
                }

                plotModel.Axes.Add(categoryAxis);
                plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = IsNumericField(SelectedYAxis) ? $"Average {SelectedYAxis}" : "Count" });
                plotModel.Series.Add(lineSeries);
            }

            PlotModel = plotModel;
        }

        private void CreatePieChart()
        {
            var plotModel = new PlotModel { Title = $"Distribution by {SelectedXAxis}" };

            var groups = GroupData();
            var pieSeries = new PieSeries { Title = SelectedXAxis };

            foreach (var group in groups.Take(10)) // Limit to 10 slices
            {
                var value = IsNumericField(SelectedYAxis) ? group.Sum(m => GetNumericValue(m, SelectedYAxis)) : group.Count();
                pieSeries.Slices.Add(new PieSlice(group.Key, value));
            }

            plotModel.Series.Add(pieSeries);
            PlotModel = plotModel;
        }

        private IEnumerable<IGrouping<string, Meal>> GroupData()
        {
            return SelectedXAxis switch
            {
                "Cuisine" => Meals.GroupBy(m => m.Cuisine?.Name ?? "Unknown").OrderByDescending(g => g.Count()),
                "Diet" => Meals.GroupBy(m => m.Diet?.Name ?? "Unknown").OrderByDescending(g => g.Count()),
                "MealType" => Meals.GroupBy(m => m.MealType ?? "Unknown").OrderByDescending(g => g.Count()),
                "IsHealthy" => Meals.GroupBy(m => m.IsHealthy ? "Healthy" : "Not Healthy").OrderByDescending(g => g.Count()),
                _ => Meals.GroupBy(m => "All").OrderByDescending(g => g.Count())
            };
        }

        private bool IsNumericField(string fieldName)
        {
            return fieldName switch
            {
                "Calories" or "ProteinG" or "CarbsG" or "FatG" or "Rating" => true,
                _ => false
            };
        }

        private double GetNumericValue(Meal meal, string fieldName)
        {
            return fieldName switch
            {
                "Calories" => meal.Calories,
                "ProteinG" => meal.ProteinG,
                "CarbsG" => meal.CarbsG,
                "FatG" => meal.FatG,
                "Rating" => meal.Rating,
                _ => 0
            };
        }
    }
}
