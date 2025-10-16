using CsvHelper;
using second_task.Data.Dtos;
using second_task.Data.Utils;
using System.Globalization;
using System.Text;

namespace second_task.Data.Providers
{
    public class CsvDataProvider
    {
        public IEnumerable<MealRawDto> ReadRawData(string filePath, object importSettings)
        {
            try
            {
                if (FileUtil.IsFileLockedForRead(filePath))
                    throw new Exception("File is locked by another process");
                
                var settings = importSettings as ImportSettings ?? new ImportSettings();
                var encoding = Encoding.GetEncoding(settings.EncodingName);
                
                using var reader = new StreamReader(filePath, encoding: encoding, detectEncodingFromByteOrderMarks: !settings.ForceEncoding);
                var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = settings.Delimiter,
                    HasHeaderRecord = settings.HasHeader
                };
                using var csv = new CsvReader(reader, config);

                var rawMeals = csv.GetRecords<MealRawDto>().ToList();
                return rawMeals;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading CSV file: {ex.Message}", ex);
            }
        }

        public void WriteRawData<T>(string filePath, IEnumerable<T> data, object exportSettings)
        {
            try
            {
                if (FileUtil.IsFileLockedForWrite(filePath))
                    throw new Exception("File is locked by another process");
                
                var settings = exportSettings as ImportSettings ?? new ImportSettings();
                var encoding = settings.Encoding ?? Encoding.UTF8;
                
                using var writer = new StreamWriter(filePath, false, encoding);
                var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = settings.Delimiter,
                    HasHeaderRecord = settings.HasHeaders
                };
                using var csv = new CsvWriter(writer, config);
                csv.WriteRecords(data);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error writing CSV file: {ex.Message}", ex);
            }
        }
    }
}
