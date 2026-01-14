using CommunityToolkit.Mvvm.ComponentModel;
using second_task.Data.Dtos;
using System.Collections.ObjectModel;
using System.IO;

namespace second_task.WPF.ViewModels
{
    public partial class FilePreviewViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _fileName = string.Empty;

        [ObservableProperty]
        private int _totalRows;

        [ObservableProperty]
        private int _totalColumns;

        [ObservableProperty]
        private ObservableCollection<ColumnInfo> _columnInfos = new();

        [ObservableProperty]
        private ObservableCollection<MealRawDto> _previewData = new();

        [ObservableProperty]
        private string _fileSize = string.Empty;

        [ObservableProperty]
        private string _encoding = string.Empty;

        public void UpdatePreview(string filePath, IEnumerable<MealRawDto> data)
        {
            FileName = Path.GetFileName(filePath);
            FileSize = GetFileSize(filePath);
            
            var dataList = data.ToList();
            TotalRows = dataList.Count;
            
            PreviewData.Clear();
            foreach (var item in dataList.Take(10)) // Show first 10 rows
            {
                PreviewData.Add(item);
            }

            AnalyzeColumns(dataList);
        }

        private void AnalyzeColumns(List<MealRawDto> data)
        {
            ColumnInfos.Clear();
            
            if (!data.Any()) return;

            var properties = typeof(MealRawDto).GetProperties();
            TotalColumns = properties.Length;

            foreach (var prop in properties)
            {
                var columnInfo = new ColumnInfo
                {
                    Name = prop.Name,
                    Type = GetColumnType(data, prop.Name),
                    NullCount = CountNulls(data, prop.Name),
                    TotalRows = data.Count
                };
                
                ColumnInfos.Add(columnInfo);
            }
        }

        private string GetColumnType(List<MealRawDto> data, string propertyName)
        {
            var property = typeof(MealRawDto).GetProperty(propertyName);
            if (property == null) return "Unknown";

            var propertyType = property.PropertyType;
            
            if (propertyType == typeof(int)) return "Integer";
            if (propertyType == typeof(double)) return "Double";
            if (propertyType == typeof(bool)) return "Boolean";
            if (propertyType == typeof(string)) return "String";
            
            return propertyType.Name;
        }

        private int CountNulls(List<MealRawDto> data, string propertyName)
        {
            var property = typeof(MealRawDto).GetProperty(propertyName);
            if (property == null) return 0;

            return data.Count(item =>
            {
                var value = property.GetValue(item);
                return value == null || (value is string str && string.IsNullOrWhiteSpace(str));
            });
        }


        private string GetFileSize(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var sizeInBytes = fileInfo.Length;
                
                if (sizeInBytes < 1024) return $"{sizeInBytes} B";
                if (sizeInBytes < 1024 * 1024) return $"{sizeInBytes / 1024:F1} KB";
                return $"{sizeInBytes / (1024 * 1024):F1} MB";
            }
            catch
            {
                return "Unknown";
            }
        }
    }

    public class ColumnInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int NullCount { get; set; }
        public int TotalRows { get; set; }
        public double NullPercentage => TotalRows > 0 ? (double)NullCount / TotalRows * 100 : 0;
    }
}
