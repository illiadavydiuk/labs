using ClosedXML.Excel;
using second_task.Data.Interfaces;

namespace second_task.Data.Reports
{
    public class XlsxReportService<T> : IReportGenerator<T>
    {
        public void Generate(string filePath, IEnumerable<T> data)
        {
            using var workbook = new XLWorkbook();
            var dataList = data.ToList();

            // Sheet 1: All Data
            var dataSheet = workbook.Worksheets.Add("Data");
            if (dataList.Any())
            {
                dataSheet.Cell(1, 1).InsertTable(dataList, "DataTable", true);
                dataSheet.Columns().AdjustToContents();
            }

            // Sheet 2: Basic Statistics
            var statsSheet = workbook.Worksheets.Add("Statistics");
            statsSheet.Cell("A1").Value = "Metric";
            statsSheet.Cell("B1").Value = "Value";
            
            var total = dataList.Count;
            
            var stats = new (string, object)[]
            {
                ("Total Records", total),
                ("Data Type", typeof(T).Name)
            };

            for (int i = 0; i < stats.Length; i++)
            {
                statsSheet.Cell(i + 2, 1).Value = stats[i].Item1;
                statsSheet.Cell(i + 2, 2).Value = stats[i].Item2?.ToString() ?? "";
            }

            // Apply formatting
            foreach (var sheet in workbook.Worksheets)
            {
                sheet.Row(1).Style.Font.Bold = true;
                sheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
                sheet.Columns().AdjustToContents();
            }

            workbook.SaveAs(filePath);
        }
    }
}
