using second_task.Data.Dtos;
using second_task.Data.Utils;
using System.Text.Json;

namespace second_task.Data.Providers
{
    public class JsonDataProvider
    {
        public IEnumerable<MealRawDto> ReadRawData(string filePath, object importSettings)
        {
            try
            {
                if (FileUtil.IsFileLockedForRead(filePath))
                    throw new Exception("File is locked by another process");

                var settings = importSettings as ImportSettings ?? new ImportSettings();
                var enc = System.Text.Encoding.GetEncoding(settings.EncodingName);
                using var sr = new StreamReader(filePath, enc, detectEncodingFromByteOrderMarks: !settings.ForceEncoding);
                var json = sr.ReadToEnd();

                // Try to deserialize as MealRawDto array
                var items = JsonSerializer.Deserialize<List<MealRawDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<MealRawDto>();

                return items;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading JSON file: {ex.Message}", ex);
            }
        }

        public void WriteRawData<T>(string filePath, IEnumerable<T> data, object exportSettings)
        {
            try
            {
                if (FileUtil.IsFileLockedForWrite(filePath))
                    throw new Exception("File is locked by another process");

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(data, options);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error writing JSON file: {ex.Message}", ex);
            }
        }
    }
}
