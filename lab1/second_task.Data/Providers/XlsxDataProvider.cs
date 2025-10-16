using ClosedXML.Excel;
using second_task.Data.Dtos;
using second_task.Data.Utils;

namespace second_task.Data.Providers
{
    public class XlsxDataProvider
    {
        public IEnumerable<MealRawDto> ReadRawData(string filePath, object importSettings)
        {
            try
            {
                if (FileUtil.IsFileLockedForRead(filePath))
                    throw new Exception("File is locked by another process");
                
                using var wb = new XLWorkbook(filePath);
                var ws = wb.Worksheets.First();
                var rangeUsed = ws.RangeUsed();
                if (rangeUsed == null) return new List<MealRawDto>();
                
                var rows = rangeUsed.RowsUsed().ToList();
                if (rows.Count < 2) return new List<MealRawDto>();

                var header = rows[0].Cells().Select(c => c.GetString()).ToList();
                var colIndex = header.Select((h, i) => new { h, i }).ToDictionary(x => x.h.Trim().ToLower(), x => x.i + 1);

                int? ResolveCol(params string[] names)
                {
                    foreach (var n in names)
                    {
                        var key = n.Trim().ToLower();
                        if (colIndex.TryGetValue(key, out var idx)) return idx;
                    }
                    return null;
                }

                var meals = new List<MealRawDto>();

                for (int r = 2; r <= (ws.LastRowUsed()?.RowNumber() ?? 1); r++)
                {
                    string GetStr(params string[] names)
                    {
                        var idx = ResolveCol(names);
                        if (idx == null) return string.Empty;
                        return ws.Cell(r, idx.Value).GetString();
                    }

                    int GetInt(params string[] names)
                    {
                        var idx = ResolveCol(names);
                        if (idx == null) return 0;
                        return ws.Cell(r, idx.Value).GetValue<int>();
                    }

                    double GetDouble(params string[] names)
                    {
                        var idx = ResolveCol(names);
                        if (idx == null) return 0.0;
                        return ws.Cell(r, idx.Value).GetValue<double>();
                    }

                    bool GetBool(params string[] names)
                    {
                        var idx = ResolveCol(names);
                        if (idx == null) return false;
                        var val = ws.Cell(r, idx.Value).GetString().ToLower();
                        return val == "true" || val == "1" || val == "yes";
                    }

                    meals.Add(new MealRawDto
                    {
                        MealId = GetInt("mealid", "meal_id", "id"),
                        MealName = GetStr("mealname", "meal_name", "name"),
                        Cuisine = GetStr("cuisine"),
                        DietType = GetStr("diettype", "diet_type", "diet"),
                        MealType = GetStr("mealtype", "meal_type", "type"),
                        Calories = GetInt("calories"),
                        ProteinG = GetDouble("proteing", "protein_g", "protein"),
                        CarbsG = GetDouble("carbsg", "carbs_g", "carbs"),
                        FatG = GetDouble("fatg", "fat_g", "fat"),
                        Rating = GetDouble("rating"),
                        IsHealthy = GetBool("ishealthy", "is_healthy", "healthy")
                    });
                }

                return meals;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading XLSX file: {ex.Message}", ex);
            }
        }

        public void WriteRawData<T>(string filePath, IEnumerable<T> data, object exportSettings)
        {
            try
            {
                if (FileUtil.IsFileLockedForWrite(filePath))
                    throw new Exception("File is locked by another process");

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Meals");

                // Use reflection to write generic data
                var dataList = data.ToList();
                if (dataList.Any())
                {
                    var properties = typeof(T).GetProperties();
                    
                    // Headers
                    for (int i = 0; i < properties.Length; i++)
                    {
                        ws.Cell(1, i + 1).Value = properties[i].Name;
                    }

                    // Data
                    int row = 2;
                    foreach (var item in dataList)
                    {
                        for (int i = 0; i < properties.Length; i++)
                        {
                            var value = properties[i].GetValue(item);
                            ws.Cell(row, i + 1).Value = value?.ToString() ?? "";
                        }
                        row++;
                    }
                }

                ws.Columns().AdjustToContents();
                wb.SaveAs(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error writing XLSX file: {ex.Message}", ex);
            }
        }
    }
}
