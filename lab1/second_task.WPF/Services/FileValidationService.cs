using System.IO;
using System.Text;

namespace second_task.WPF.Services
{
    public class FileValidationService
    {
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public string? DetectedEncoding { get; set; }
            public string? SuggestedDelimiter { get; set; }
            public bool HasHeaders { get; set; }
            public List<string> Issues { get; set; } = new();
            public List<string> Suggestions { get; set; } = new();
        }

        public ValidationResult ValidateFile(string filePath)
        {
            var result = new ValidationResult();
            var extension = Path.GetExtension(filePath).ToLower();

            try
            {
                switch (extension)
                {
                    case ".csv":
                        return ValidateCsvFile(filePath);
                    case ".xlsx":
                        return ValidateXlsxFile(filePath);
                    case ".json":
                        return ValidateJsonFile(filePath);
                    case ".xml":
                        return ValidateXmlFile(filePath);
                    default:
                        result.Issues.Add($"Unsupported file format: {extension}");
                        result.Suggestions.Add("Please use CSV, XLSX, JSON, or XML files");
                        break;
                }
            }
            catch (Exception ex)
            {
                result.Issues.Add($"Error validating file: {ex.Message}");
            }

            return result;
        }

        private ValidationResult ValidateCsvFile(string filePath)
        {
            var result = new ValidationResult();
            
            // Detect encoding
            var encoding = DetectEncoding(filePath);
            result.DetectedEncoding = encoding.WebName;

            try
            {
                using var reader = new StreamReader(filePath, encoding);
                var firstLine = reader.ReadLine();
                var secondLine = reader.ReadLine();

                if (string.IsNullOrEmpty(firstLine))
                {
                    result.Issues.Add("File is empty");
                    return result;
                }

                // Detect delimiter
                var delimiters = new[] { ',', ';', '\t', '|' };
                var delimiterCounts = delimiters.ToDictionary(d => d, d => firstLine.Count(c => c == d));
                var bestDelimiter = delimiterCounts.OrderByDescending(kvp => kvp.Value).First();
                
                result.SuggestedDelimiter = bestDelimiter.Key.ToString();

                // Check if has headers
                if (!string.IsNullOrEmpty(secondLine))
                {
                    var firstFields = firstLine.Split(bestDelimiter.Key);
                    var secondFields = secondLine.Split(bestDelimiter.Key);
                    
                    result.HasHeaders = firstFields.Any(f => !double.TryParse(f, out _)) && 
                                      secondFields.Any(f => double.TryParse(f, out _));
                }

                // Validate structure
                if (bestDelimiter.Value == 0)
                {
                    result.Issues.Add("No delimiter detected");
                    result.Suggestions.Add("Check if file uses comma, semicolon, tab, or pipe as delimiter");
                }

                result.IsValid = !result.Issues.Any();
            }
            catch (Exception ex)
            {
                result.Issues.Add($"Error reading CSV: {ex.Message}");
                result.Suggestions.Add("Try different encoding (UTF-8, Windows-1251, etc.)");
            }

            return result;
        }

        private ValidationResult ValidateXlsxFile(string filePath)
        {
            var result = new ValidationResult { IsValid = true };
            
            try
            {
                using var stream = File.OpenRead(filePath);
                var buffer = new byte[8];
                stream.Read(buffer, 0, 8);
                
                // Check XLSX signature
                if (buffer[0] != 0x50 || buffer[1] != 0x4B) // PK signature
                {
                    result.Issues.Add("Invalid XLSX file format");
                    result.Suggestions.Add("File may be corrupted or not a valid Excel file");
                    result.IsValid = false;
                }
            }
            catch (Exception ex)
            {
                result.Issues.Add($"Error validating XLSX: {ex.Message}");
                result.IsValid = false;
            }

            return result;
        }

        private ValidationResult ValidateJsonFile(string filePath)
        {
            var result = new ValidationResult();
            
            try
            {
                var content = File.ReadAllText(filePath);
                System.Text.Json.JsonDocument.Parse(content);
                result.IsValid = true;
            }
            catch (System.Text.Json.JsonException ex)
            {
                result.Issues.Add($"Invalid JSON format: {ex.Message}");
                result.Suggestions.Add("Check JSON syntax and structure");
            }
            catch (Exception ex)
            {
                result.Issues.Add($"Error reading JSON: {ex.Message}");
            }

            return result;
        }

        private ValidationResult ValidateXmlFile(string filePath)
        {
            var result = new ValidationResult();
            
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.Load(filePath);
                result.IsValid = true;
            }
            catch (System.Xml.XmlException ex)
            {
                result.Issues.Add($"Invalid XML format: {ex.Message}");
                result.Suggestions.Add("Check XML syntax and structure");
            }
            catch (Exception ex)
            {
                result.Issues.Add($"Error reading XML: {ex.Message}");
            }

            return result;
        }

        private Encoding DetectEncoding(string filePath)
        {
            // Read BOM
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var bom = new byte[4];
            stream.Read(bom, 0, 4);

            if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                return Encoding.UTF8;
            if (bom[0] == 0xFF && bom[1] == 0xFE)
                return Encoding.Unicode;
            if (bom[0] == 0xFE && bom[1] == 0xFF)
                return Encoding.BigEndianUnicode;

            // Try to detect by content
            stream.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[1024];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            
            // Check for non-ASCII characters
            var hasNonAscii = buffer.Take(bytesRead).Any(b => b > 127);
            
            return hasNonAscii ? Encoding.UTF8 : Encoding.ASCII;
        }
    }
}
