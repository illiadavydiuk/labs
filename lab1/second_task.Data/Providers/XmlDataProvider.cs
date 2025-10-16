using second_task.Data.Dtos;
using second_task.Data.Utils;
using System.Xml.Serialization;

namespace second_task.Data.Providers
{
    public class XmlDataProvider
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
                var xml = sr.ReadToEnd();

                // Try to deserialize as MealRawDto list
                var dtoSerializer = new XmlSerializer(typeof(List<MealRawDto>));
                using var srXml = new StringReader(xml);
                if (dtoSerializer.Deserialize(srXml) is List<MealRawDto> flat)
                {
                    return flat;
                }

                return new List<MealRawDto>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading XML file: {ex.Message}", ex);
            }
        }

        public void WriteRawData<T>(string filePath, IEnumerable<T> data, object exportSettings)
        {
            try
            {
                if (FileUtil.IsFileLockedForWrite(filePath))
                    throw new Exception("File is locked by another process");

                var serializer = new XmlSerializer(typeof(List<T>));
                using var writer = new StreamWriter(filePath);
                serializer.Serialize(writer, data.ToList());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error writing XML file: {ex.Message}", ex);
            }
        }
    }
}
