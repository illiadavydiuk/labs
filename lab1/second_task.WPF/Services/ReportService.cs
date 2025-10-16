using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using second_task.WPF.Models;
using System.IO;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using WpColor = DocumentFormat.OpenXml.Wordprocessing.Color;
using SysColor = System.Drawing.Color;

namespace second_task.WPF.Services
{
    public class ReportService
    {
        public void GenerateXlsxReport(string filePath, IEnumerable<Meal> meals, List<string>? chartPaths = null)
        {
            using var workbook = new XLWorkbook();
            var mealsList = meals.ToList();

            // Sheet 1: Summary Dashboard
            CreateSummarySheet(workbook, mealsList);
            
            // Sheet 2: All Data
            CreateDataSheet(workbook, mealsList);
            
            // Sheet 3: Cuisine Analysis
            CreateCuisineAnalysisSheet(workbook, mealsList);
            
            // Sheet 4: Diet Analysis  
            CreateDietAnalysisSheet(workbook, mealsList);
            
            // Sheet 5: Charts Data
            CreateChartsSheet(workbook, mealsList);

            workbook.SaveAs(filePath);
        }

        private void CreateSummarySheet(XLWorkbook workbook, List<Meal> meals)
        {
            var sheet = workbook.Worksheets.Add("Summary");
            
            // Title
            sheet.Cell("A1").Value = "HEALTHY EATING DATA ANALYSIS REPORT";
            sheet.Range("A1:F1").Merge().Style.Font.Bold = true;
            sheet.Range("A1:F1").Style.Font.FontSize = 16;
            sheet.Range("A1:F1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Range("A1:F1").Style.Fill.BackgroundColor = XLColor.DarkBlue;
            sheet.Range("A1:F1").Style.Font.FontColor = XLColor.White;

            // Key Metrics
            sheet.Cell("A3").Value = "KEY METRICS";
            sheet.Range("A3:B3").Style.Font.Bold = true;
            sheet.Range("A3:B3").Style.Fill.BackgroundColor = XLColor.LightBlue;

            var total = meals.Count;
            var healthy = meals.Count(m => m.IsHealthy);
            var avgCalories = meals.Any() ? meals.Average(m => m.Calories) : 0;
            var avgRating = meals.Any() ? meals.Average(m => m.Rating) : 0;
            var maxCalories = meals.Any() ? meals.Max(m => m.Calories) : 0;
            var minCalories = meals.Any() ? meals.Min(m => m.Calories) : 0;

            var metrics = new (string, object)[]
            {
                ("Total Meals", total),
                ("Healthy Meals", healthy),
                ("Healthy Percentage", total > 0 ? $"{Math.Round(100.0 * healthy / total, 1)}%" : "0%"),
                ("Average Calories", Math.Round(avgCalories, 1)),
                ("Max Calories", maxCalories),
                ("Min Calories", minCalories),
                ("Average Rating", Math.Round(avgRating, 2)),
                ("Generated On", DateTime.Now.ToString("yyyy-MM-dd HH:mm"))
            };

            for (int i = 0; i < metrics.Length; i++)
            {
                sheet.Cell(i + 4, 1).Value = metrics[i].Item1;
                sheet.Cell(i + 4, 2).Value = metrics[i].Item2?.ToString() ?? "";
                
                // Conditional formatting for healthy percentage
                if (metrics[i].Item1 == "Healthy Percentage" && total > 0)
                {
                    var percentage = 100.0 * healthy / total;
                    if (percentage >= 70) sheet.Cell(i + 4, 2).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    else if (percentage >= 50) sheet.Cell(i + 4, 2).Style.Fill.BackgroundColor = XLColor.Yellow;
                    else sheet.Cell(i + 4, 2).Style.Fill.BackgroundColor = XLColor.LightPink;
                }
            }

            // Top 5 Cuisines
            sheet.Cell("D3").Value = "TOP 5 CUISINES";
            sheet.Range("D3:F3").Style.Font.Bold = true;
            sheet.Range("D3:F3").Style.Fill.BackgroundColor = XLColor.LightGreen;

            var topCuisines = meals.GroupBy(m => m.Cuisine?.Name ?? "Unknown")
                                  .OrderByDescending(g => g.Count())
                                  .Take(5)
                                  .ToList();

            sheet.Cell("D4").Value = "Cuisine";
            sheet.Cell("E4").Value = "Count";
            sheet.Cell("F4").Value = "Avg Calories";
            sheet.Range("D4:F4").Style.Font.Bold = true;

            for (int i = 0; i < topCuisines.Count; i++)
            {
                var group = topCuisines[i];
                sheet.Cell(i + 5, 4).Value = group.Key;
                sheet.Cell(i + 5, 5).Value = group.Count();
                sheet.Cell(i + 5, 6).Value = Math.Round(group.Average(m => m.Calories), 1);
            }

            sheet.Columns().AdjustToContents();
        }

        private void CreateDataSheet(XLWorkbook workbook, List<Meal> meals)
        {
            var sheet = workbook.Worksheets.Add("All Data");
            
            if (meals.Any())
            {
                // Створити анонімні об'єкти з правильними властивостями для експорту
                var exportData = meals.Select(m => new
                {
                    MealId = m.MealId,
                    MealName = m.MealName,
                    Cuisine = m.Cuisine?.Name ?? "Unknown",
                    Diet = m.Diet?.Name ?? "Unknown", 
                    MealType = m.MealType,
                    Calories = m.Calories,
                    ProteinG = m.ProteinG,
                    CarbsG = m.CarbsG,
                    FatG = m.FatG,
                    Rating = m.Rating,
                    IsHealthy = m.IsHealthy
                }).ToList();
                
                // Create table with processed data
                var table = sheet.Cell(1, 1).InsertTable(exportData, "MealsData", true);
                
                // Format headers
                table.HeadersRow().Style.Fill.BackgroundColor = XLColor.DarkBlue;
                table.HeadersRow().Style.Font.FontColor = XLColor.White;
                table.HeadersRow().Style.Font.Bold = true;
                
                // Conditional formatting for IsHealthy column
                try
                {
                    var healthyColumn = table.Column("IsHealthy");
                    foreach (var cell in healthyColumn.Cells())
                    {
                        if (cell.GetValue<bool>())
                            cell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                        else
                            cell.Style.Fill.BackgroundColor = XLColor.LightPink;
                    }
                }
                catch
                {
                    // Ignore if column not found
                }
                
                sheet.Columns().AdjustToContents();
            }
        }

        private void CreateCuisineAnalysisSheet(XLWorkbook workbook, List<Meal> meals)
        {
            var sheet = workbook.Worksheets.Add("Cuisine Analysis");
            
            var cuisineGroups = meals.GroupBy(m => m.Cuisine?.Name ?? "Unknown")
                                    .OrderByDescending(g => g.Count())
                                    .ToList();

            // Headers
            sheet.Cell("A1").Value = "Cuisine";
            sheet.Cell("B1").Value = "Total Meals";
            sheet.Cell("C1").Value = "Healthy Meals";
            sheet.Cell("D1").Value = "Healthy %";
            sheet.Cell("E1").Value = "Avg Calories";
            sheet.Cell("F1").Value = "Avg Rating";
            sheet.Cell("G1").Value = "Avg Protein";
            sheet.Cell("H1").Value = "Avg Carbs";
            sheet.Cell("I1").Value = "Avg Fat";

            // Format headers
            sheet.Range("A1:I1").Style.Font.Bold = true;
            sheet.Range("A1:I1").Style.Fill.BackgroundColor = XLColor.DarkGreen;
            sheet.Range("A1:I1").Style.Font.FontColor = XLColor.White;

            // Data
            int row = 2;
            foreach (var group in cuisineGroups)
            {
                var groupList = group.ToList();
                var healthyCount = groupList.Count(m => m.IsHealthy);
                var healthyPercentage = 100.0 * healthyCount / groupList.Count;

                sheet.Cell(row, 1).Value = group.Key;
                sheet.Cell(row, 2).Value = groupList.Count;
                sheet.Cell(row, 3).Value = healthyCount;
                sheet.Cell(row, 4).Value = Math.Round(healthyPercentage, 1);
                sheet.Cell(row, 5).Value = Math.Round(groupList.Average(m => m.Calories), 1);
                sheet.Cell(row, 6).Value = Math.Round(groupList.Average(m => m.Rating), 2);
                sheet.Cell(row, 7).Value = Math.Round(groupList.Average(m => m.ProteinG), 1);
                sheet.Cell(row, 8).Value = Math.Round(groupList.Average(m => m.CarbsG), 1);
                sheet.Cell(row, 9).Value = Math.Round(groupList.Average(m => m.FatG), 1);

                // Conditional formatting for healthy percentage
                if (healthyPercentage >= 70)
                    sheet.Cell(row, 4).Style.Fill.BackgroundColor = XLColor.LightGreen;
                else if (healthyPercentage >= 50)
                    sheet.Cell(row, 4).Style.Fill.BackgroundColor = XLColor.Yellow;
                else
                    sheet.Cell(row, 4).Style.Fill.BackgroundColor = XLColor.LightPink;

                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        private void CreateDietAnalysisSheet(XLWorkbook workbook, List<Meal> meals)
        {
            var sheet = workbook.Worksheets.Add("Diet Analysis");
            
            var dietGroups = meals.GroupBy(m => m.Diet?.Name ?? "Unknown")
                                 .OrderByDescending(g => g.Count())
                                 .ToList();

            // Headers
            sheet.Cell("A1").Value = "Diet Type";
            sheet.Cell("B1").Value = "Total Meals";
            sheet.Cell("C1").Value = "Healthy Meals";
            sheet.Cell("D1").Value = "Healthy %";
            sheet.Cell("E1").Value = "Avg Calories";
            sheet.Cell("F1").Value = "Avg Rating";

            // Format headers
            sheet.Range("A1:F1").Style.Font.Bold = true;
            sheet.Range("A1:F1").Style.Fill.BackgroundColor = XLColor.DarkOrange;
            sheet.Range("A1:F1").Style.Font.FontColor = XLColor.White;

            // Data
            int row = 2;
            foreach (var group in dietGroups)
            {
                var groupList = group.ToList();
                var healthyCount = groupList.Count(m => m.IsHealthy);
                var healthyPercentage = 100.0 * healthyCount / groupList.Count;

                sheet.Cell(row, 1).Value = group.Key;
                sheet.Cell(row, 2).Value = groupList.Count;
                sheet.Cell(row, 3).Value = healthyCount;
                sheet.Cell(row, 4).Value = Math.Round(healthyPercentage, 1);
                sheet.Cell(row, 5).Value = Math.Round(groupList.Average(m => m.Calories), 1);
                sheet.Cell(row, 6).Value = Math.Round(groupList.Average(m => m.Rating), 2);

                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        private void CreateChartsSheet(XLWorkbook workbook, List<Meal> meals)
        {
            var sheet = workbook.Worksheets.Add("Charts Data");
            
            // Chart 1: Calories Distribution
            sheet.Cell("A1").Value = "CALORIES DISTRIBUTION";
            sheet.Range("A1:B1").Style.Font.Bold = true;
            sheet.Range("A1:B1").Style.Fill.BackgroundColor = XLColor.Purple;
            sheet.Range("A1:B1").Style.Font.FontColor = XLColor.White;

            var calorieRanges = new[]
            {
                ("0-200", meals.Count(m => m.Calories <= 200)),
                ("201-400", meals.Count(m => m.Calories > 200 && m.Calories <= 400)),
                ("401-600", meals.Count(m => m.Calories > 400 && m.Calories <= 600)),
                ("601-800", meals.Count(m => m.Calories > 600 && m.Calories <= 800)),
                ("800+", meals.Count(m => m.Calories > 800))
            };

            sheet.Cell("A2").Value = "Calorie Range";
            sheet.Cell("B2").Value = "Count";
            
            for (int i = 0; i < calorieRanges.Length; i++)
            {
                sheet.Cell(i + 3, 1).Value = calorieRanges[i].Item1;
                sheet.Cell(i + 3, 2).Value = calorieRanges[i].Item2;
            }

            sheet.Columns().AdjustToContents();
        }


        public void GenerateDocxReport(string filePath, IEnumerable<Meal> meals, List<string>? chartPaths = null)
        {
            using var document = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            var mealsList = meals.ToList();

            // 1. Title Page
            CreateSimpleTitlePage(body);
            
            // 2. Key Metrics Table
            CreateSimpleMetricsTable(body, mealsList);
            
            // 3. Charts Section
            if (chartPaths != null && chartPaths.Any())
            {
                CreateChartsSection(body, chartPaths, document);
            }

            document.Save();
        }

        private void CreateTitlePage(Body body)
        {
            // Main Title
            var titleParagraph = body.AppendChild(new Paragraph());
            var titleParagraphProperties = titleParagraph.PrependChild(new ParagraphProperties());
            titleParagraphProperties.AppendChild(new Justification() { Val = JustificationValues.Center });
            titleParagraphProperties.AppendChild(new SpacingBetweenLines() { After = "480" });

            var titleRun = titleParagraph.AppendChild(new Run());
            var titleRunProperties = titleRun.PrependChild(new RunProperties());
            titleRunProperties.AppendChild(new Bold());
            titleRunProperties.AppendChild(new FontSize() { Val = "32" });
            titleRunProperties.AppendChild(new WpColor() { Val = "2F5597" });
            titleRun.AppendChild(new Text("HEALTHY EATING DATA"));
            
            var subtitleParagraph = body.AppendChild(new Paragraph());
            var subtitleParagraphProperties = subtitleParagraph.PrependChild(new ParagraphProperties());
            subtitleParagraphProperties.AppendChild(new Justification() { Val = JustificationValues.Center });
            subtitleParagraphProperties.AppendChild(new SpacingBetweenLines() { After = "960" });

            var subtitleRun = subtitleParagraph.AppendChild(new Run());
            var subtitleRunProperties = subtitleRun.PrependChild(new RunProperties());
            subtitleRunProperties.AppendChild(new Bold());
            subtitleRunProperties.AppendChild(new FontSize() { Val = "28" });
            subtitleRunProperties.AppendChild(new WpColor() { Val = "2F5597" });
            subtitleRun.AppendChild(new Text("ANALYSIS REPORT"));

            // Student Info
            AddCenteredText(body, "Студент: Давидюк Ілля Сергійович", 16, false, "480");
            AddCenteredText(body, "Група: КН-211", 16, false, "240");
            AddCenteredText(body, "Варіант: 36", 16, false, "240");
            AddCenteredText(body, "Джерело даних: Healthy Eating Dataset", 16, false, "480");
            
            // Date
            AddCenteredText(body, DateTime.Now.ToString("dd MMMM yyyy"), 14, false, "960");
        }

        private void CreateExecutiveSummary(Body body, List<Meal> meals)
        {
            AddHeading(body, "EXECUTIVE SUMMARY", 20);
            
            var total = meals.Count;
            var healthy = meals.Count(m => m.IsHealthy);
            var healthyPercentage = total > 0 ? 100.0 * healthy / total : 0;
            
            var summaryText = $"This report presents a comprehensive analysis of {total} meal records from the Healthy Eating Dataset. " +
                            $"The analysis reveals that {healthy} meals ({healthyPercentage:F1}%) are classified as healthy, " +
                            $"with an average caloric content of {(meals.Any() ? meals.Average(m => m.Calories) : 0):F0} calories per meal. " +
                            $"The dataset encompasses {meals.GroupBy(m => m.Cuisine?.Name).Count()} different cuisine types and " +
                            $"{meals.GroupBy(m => m.Diet?.Name).Count()} dietary categories, providing a diverse foundation for nutritional analysis.";
            
            AddNormalText(body, summaryText);
        }

        private void CreateDatasetDescription(Body body, List<Meal> meals)
        {
            AddHeading(body, "DATASET DESCRIPTION", 18);
            
            AddSubheading(body, "Data Structure", 14);
            var structureText = "The dataset contains the following key fields:\n" +
                              "• Meal ID and Name - Unique identifiers and descriptive names\n" +
                              "• Cuisine Type - Categorization by culinary tradition\n" +
                              "• Diet Type - Classification by dietary restrictions or preferences\n" +
                              "• Nutritional Information - Calories, Protein, Carbohydrates, and Fat content\n" +
                              "• Quality Metrics - Rating and health classification";
            AddNormalText(body, structureText);
            
            AddSubheading(body, "Data Quality", 14);
            var qualityText = $"Total Records: {meals.Count}\n" +
                            $"Unique Cuisines: {meals.GroupBy(m => m.Cuisine?.Name ?? "Unknown").Count()}\n" +
                            $"Unique Diets: {meals.GroupBy(m => m.Diet?.Name ?? "Unknown").Count()}\n" +
                            $"Average Rating: {(meals.Any() ? meals.Average(m => m.Rating) : 0):F2}/5.0\n" +
                            $"Calorie Range: {(meals.Any() ? meals.Min(m => m.Calories) : 0)} - {(meals.Any() ? meals.Max(m => m.Calories) : 0)} calories";
            AddNormalText(body, qualityText);
        }

        private void CreateKeyMetricsTable(Body body, List<Meal> meals)
        {
            AddHeading(body, "KEY PERFORMANCE INDICATORS", 18);
            
            var table = new DocumentFormat.OpenXml.Wordprocessing.Table();
            
            // Table properties
            var tableProperties = new TableProperties();
            tableProperties.AppendChild(new TableBorders(
                new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }));
            table.AppendChild(tableProperties);

            // Header row
            var headerRow = new TableRow();
            headerRow.AppendChild(CreateTableCell("Metric", true));
            headerRow.AppendChild(CreateTableCell("Value", true));
            table.AppendChild(headerRow);

            // Data rows
            var metrics = new[]
            {
                ("Total Meals", meals.Count.ToString()),
                ("Healthy Meals", meals.Count(m => m.IsHealthy).ToString()),
                ("Healthy Percentage", $"{(meals.Count > 0 ? 100.0 * meals.Count(m => m.IsHealthy) / meals.Count : 0):F1}%"),
                ("Average Calories", $"{(meals.Any() ? meals.Average(m => m.Calories) : 0):F0}"),
                ("Average Rating", $"{(meals.Any() ? meals.Average(m => m.Rating) : 0):F2}"),
                ("Top Cuisine", meals.GroupBy(m => m.Cuisine?.Name ?? "Unknown").OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "N/A"),
                ("Analysis Date", DateTime.Now.ToString("yyyy-MM-dd"))
            };

            foreach (var (metric, value) in metrics)
            {
                var row = new TableRow();
                row.AppendChild(CreateTableCell(metric, false));
                row.AppendChild(CreateTableCell(value, false));
                table.AppendChild(row);
            }

            body.AppendChild(new Paragraph(new Run(new Text(""))));
            body.AppendChild(table);
            body.AppendChild(new Paragraph(new Run(new Text(""))));
        }

        private void CreateAnalysisResults(Body body, List<Meal> meals)
        {
            AddHeading(body, "ANALYSIS RESULTS", 18);
            
            AddSubheading(body, "Cuisine Analysis", 14);
            var topCuisines = meals.GroupBy(m => m.Cuisine?.Name ?? "Unknown")
                                  .OrderByDescending(g => g.Count())
                                  .Take(5)
                                  .ToList();
            
            var cuisineText = "Top 5 most popular cuisines:\n";
            for (int i = 0; i < topCuisines.Count; i++)
            {
                var group = topCuisines[i];
                var healthyPercentage = 100.0 * group.Count(m => m.IsHealthy) / group.Count();
                cuisineText += $"{i + 1}. {group.Key}: {group.Count()} meals ({healthyPercentage:F1}% healthy)\n";
            }
            AddNormalText(body, cuisineText);
            
            AddSubheading(body, "Nutritional Insights", 14);
            var avgProtein = meals.Any() ? meals.Average(m => m.ProteinG) : 0;
            var avgCarbs = meals.Any() ? meals.Average(m => m.CarbsG) : 0;
            var avgFat = meals.Any() ? meals.Average(m => m.FatG) : 0;
            
            var nutritionText = $"Average macronutrient distribution:\n" +
                              $"• Protein: {avgProtein:F1}g per meal\n" +
                              $"• Carbohydrates: {avgCarbs:F1}g per meal\n" +
                              $"• Fat: {avgFat:F1}g per meal\n\n" +
                              $"The data shows a balanced macronutrient profile across the analyzed meals.";
            AddNormalText(body, nutritionText);
        }

        private void CreateConclusions(Body body, List<Meal> meals)
        {
            AddHeading(body, "CONCLUSIONS AND RECOMMENDATIONS", 18);
            
            var healthy = meals.Count(m => m.IsHealthy);
            var healthyPercentage = meals.Count > 0 ? 100.0 * healthy / meals.Count : 0;
            
            var conclusions = "Based on the comprehensive analysis of the healthy eating dataset, the following key conclusions emerge:\n\n" +
                            $"1. Health Distribution: With {healthyPercentage:F1}% of meals classified as healthy, there is " +
                            (healthyPercentage >= 70 ? "excellent adherence" : healthyPercentage >= 50 ? "moderate adherence" : "room for improvement") +
                            " to healthy eating standards.\n\n" +
                            "2. Cuisine Diversity: The dataset demonstrates significant culinary diversity, providing insights into how different food traditions contribute to nutritional outcomes.\n\n" +
                            "3. Nutritional Balance: The average caloric and macronutrient distributions suggest a generally balanced approach to meal composition across the dataset.\n\n" +
                            "Recommendations for future analysis:\n" +
                            "• Expand the dataset to include seasonal variations\n" +
                            "• Incorporate additional nutritional parameters such as vitamins and minerals\n" +
                            "• Develop predictive models for health classification based on nutritional content";
            
            AddNormalText(body, conclusions);
            
            // Footer
            body.AppendChild(new Paragraph(new Run(new Text(""))));
            AddCenteredText(body, "--- End of Report ---", 12, true, "480");
        }

        private void AddHeading(Body body, string text, int fontSize)
        {
            var paragraph = body.AppendChild(new Paragraph());
            var paragraphProperties = paragraph.PrependChild(new ParagraphProperties());
            paragraphProperties.AppendChild(new SpacingBetweenLines() { Before = "240", After = "120" });
            
            var run = paragraph.AppendChild(new Run());
            var runProperties = run.PrependChild(new RunProperties());
            runProperties.AppendChild(new Bold());
            runProperties.AppendChild(new FontSize() { Val = fontSize.ToString() });
            runProperties.AppendChild(new WpColor() { Val = "1F4E79" });
            run.AppendChild(new Text(text));
        }

        private void AddSubheading(Body body, string text, int fontSize)
        {
            var paragraph = body.AppendChild(new Paragraph());
            var paragraphProperties = paragraph.PrependChild(new ParagraphProperties());
            paragraphProperties.AppendChild(new SpacingBetweenLines() { Before = "160", After = "80" });
            
            var run = paragraph.AppendChild(new Run());
            var runProperties = run.PrependChild(new RunProperties());
            runProperties.AppendChild(new Bold());
            runProperties.AppendChild(new FontSize() { Val = fontSize.ToString() });
            run.AppendChild(new Text(text));
        }

        private void AddNormalText(Body body, string text)
        {
            var paragraph = body.AppendChild(new Paragraph());
            var paragraphProperties = paragraph.PrependChild(new ParagraphProperties());
            paragraphProperties.AppendChild(new SpacingBetweenLines() { After = "120" });
            paragraphProperties.AppendChild(new Justification() { Val = JustificationValues.Both });
            
            var run = paragraph.AppendChild(new Run());
            var runProperties = run.PrependChild(new RunProperties());
            runProperties.AppendChild(new FontSize() { Val = "22" });
            run.AppendChild(new Text(text));
        }

        private void AddCenteredText(Body body, string text, int fontSize, bool italic, string spacingAfter)
        {
            var paragraph = body.AppendChild(new Paragraph());
            var paragraphProperties = paragraph.PrependChild(new ParagraphProperties());
            paragraphProperties.AppendChild(new Justification() { Val = JustificationValues.Center });
            paragraphProperties.AppendChild(new SpacingBetweenLines() { After = spacingAfter });
            
            var run = paragraph.AppendChild(new Run());
            var runProperties = run.PrependChild(new RunProperties());
            runProperties.AppendChild(new FontSize() { Val = fontSize.ToString() });
            if (italic) runProperties.AppendChild(new Italic());
            run.AppendChild(new Text(text));
        }

        private void AddPageBreak(Body body)
        {
            var paragraph = body.AppendChild(new Paragraph());
            var run = paragraph.AppendChild(new Run());
            run.AppendChild(new Break() { Type = BreakValues.Page });
        }

        private TableCell CreateTableCell(string text, bool isHeader)
        {
            var cell = new TableCell();
            
            var cellProperties = new TableCellProperties();
            if (isHeader)
            {
                cellProperties.AppendChild(new Shading() { Val = ShadingPatternValues.Clear, Fill = "D9E1F2" });
            }
            cell.AppendChild(cellProperties);
            
            var paragraph = new Paragraph();
            var run = new Run();
            var runProperties = new RunProperties();
            
            if (isHeader)
            {
                runProperties.AppendChild(new Bold());
            }
            
            run.AppendChild(runProperties);
            run.AppendChild(new Text(text));
            paragraph.AppendChild(run);
            cell.AppendChild(paragraph);
            
            return cell;
        }

        private void CreateChartsSection(Body body, List<string> chartPaths, WordprocessingDocument document)
        {
            AddPageBreak(body);
            AddHeading(body, "DATA VISUALIZATION CHARTS", 18);
            
            foreach (var chartPath in chartPaths)
            {
                if (File.Exists(chartPath))
                {
                    try
                    {
                        InsertImage(body, chartPath, document);
                        AddNormalText(body, $"Chart: {Path.GetFileNameWithoutExtension(chartPath)}");
                        body.AppendChild(new Paragraph(new Run(new Text(""))));
                    }
                    catch (Exception ex)
                    {
                        AddNormalText(body, $"Error loading chart: {ex.Message}");
                    }
                }
            }
        }

        private void CreateSimpleTitlePage(Body body)
        {
            // Main Title
            var titleParagraph = body.AppendChild(new Paragraph());
            var titleParagraphProperties = titleParagraph.PrependChild(new ParagraphProperties());
            titleParagraphProperties.AppendChild(new Justification() { Val = JustificationValues.Center });
            titleParagraphProperties.AppendChild(new SpacingBetweenLines() { After = "480" });

            var titleRun = titleParagraph.AppendChild(new Run());
            var titleRunProperties = titleRun.PrependChild(new RunProperties());
            titleRunProperties.AppendChild(new Bold());
            titleRunProperties.AppendChild(new FontSize() { Val = "32" });
            titleRunProperties.AppendChild(new WpColor() { Val = "2F5597" });
            titleRun.AppendChild(new Text("HEALTHY EATING DATA ANALYSIS"));
            
            // Student Info
            AddCenteredText(body, "Студент: Давидюк Ілля Сергійович", 16, false, "240");
            AddCenteredText(body, "Група: КН-211", 16, false, "240");
            AddCenteredText(body, "Варіант: 36", 16, false, "240");
            AddCenteredText(body, "Датасет: Healthy Eating Dataset", 16, false, "480");
            
            // Date
            AddCenteredText(body, DateTime.Now.ToString("dd MMMM yyyy"), 14, false, "960");
            
            // Page break
            AddPageBreak(body);
        }

        private void CreateSimpleMetricsTable(Body body, List<Meal> meals)
        {
            AddHeading(body, "KEY METRICS", 20);
            
            var table = new Table();
            
            // Table properties
            var tableProperties = new TableProperties(
                new TableBorders(
                    new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                    new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
                )
            );
            table.AppendChild(tableProperties);

            // Header row
            var headerRow = new TableRow();
            headerRow.Append(CreateTableCell("Metric", true));
            headerRow.Append(CreateTableCell("Value", true));
            table.AppendChild(headerRow);

            // Data rows
            var totalMeals = meals.Count;
            var avgCalories = meals.Average(m => m.Calories);
            var avgProtein = meals.Average(m => m.ProteinG);
            var avgCarbs = meals.Average(m => m.CarbsG);
            var avgFat = meals.Average(m => m.FatG);
            var avgRating = meals.Average(m => m.Rating);
            var healthyCount = meals.Count(m => m.IsHealthy);
            var healthyPercentage = (double)healthyCount / totalMeals * 100;

            var metrics = new[]
            {
                ("Total Meals", totalMeals.ToString()),
                ("Average Calories", $"{avgCalories:F1}"),
                ("Average Protein (g)", $"{avgProtein:F1}"),
                ("Average Carbs (g)", $"{avgCarbs:F1}"),
                ("Average Fat (g)", $"{avgFat:F1}"),
                ("Average Rating", $"{avgRating:F1}"),
                ("Healthy Meals", $"{healthyCount} ({healthyPercentage:F1}%)"),
                ("Cuisines", meals.Select(m => m.Cuisine?.Name ?? "Unknown").Distinct().Count().ToString()),
                ("Diet Types", meals.Select(m => m.Diet?.Name ?? "Unknown").Distinct().Count().ToString())
            };

            foreach (var (metric, value) in metrics)
            {
                var row = new TableRow();
                row.Append(CreateTableCell(metric, false));
                row.Append(CreateTableCell(value, false));
                table.AppendChild(row);
            }

            body.AppendChild(table);
            body.AppendChild(new Paragraph(new Run(new Text(""))));
        }

        private void InsertImage(Body body, string imagePath, WordprocessingDocument document)
        {
            try
            {
                LoggingService.LogInfo($"Вставка зображення: {imagePath}");
                
                if (!File.Exists(imagePath))
                {
                    LoggingService.LogError($"Файл зображення не знайдено: {imagePath}");
                    AddNormalText(body, $"[Помилка: Файл зображення не знайдено - {Path.GetFileName(imagePath)}]");
                    return;
                }
                
                var imageBytes = File.ReadAllBytes(imagePath);
                LoggingService.LogInfo($"Розмір файлу зображення: {imageBytes.Length} байт");
                
                // Створити ImagePart
                var imagePart = document.MainDocumentPart.AddImagePart("image/png");
                using (var stream = new MemoryStream(imageBytes))
                {
                    imagePart.FeedData(stream);
                }
                
                var relationshipId = document.MainDocumentPart.GetIdOfPart(imagePart);
                
                // Створити параграф з зображенням
                var paragraph = new Paragraph();
                var paragraphProperties = new ParagraphProperties();
                paragraphProperties.AppendChild(new Justification() { Val = JustificationValues.Center });
                paragraph.AppendChild(paragraphProperties);
                
                var run = new Run();
                var drawing = new Drawing();
                
                // Розміри зображення (в EMUs - English Metric Units)
                long widthEmus = 5486400; // ~6 inches
                long heightEmus = 4114800; // ~4.5 inches
                
                var inline = new DW.Inline()
                {
                    DistanceFromTop = 0U,
                    DistanceFromBottom = 0U,
                    DistanceFromLeft = 0U,
                    DistanceFromRight = 0U
                };
                
                var extent = new DW.Extent() { Cx = widthEmus, Cy = heightEmus };
                var effectExtent = new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L };
                
                var docProperties = new DW.DocProperties()
                {
                    Id = 1U,
                    Name = Path.GetFileNameWithoutExtension(imagePath)
                };
                
                var graphic = new A.Graphic();
                var graphicData = new A.GraphicData() { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" };
                
                var picture = new PIC.Picture();
                var nonVisualPictureProperties = new PIC.NonVisualPictureProperties();
                var nonVisualDrawingProperties = new PIC.NonVisualDrawingProperties()
                {
                    Id = 0U,
                    Name = Path.GetFileName(imagePath)
                };
                var nonVisualPictureDrawingProperties = new PIC.NonVisualPictureDrawingProperties();
                nonVisualPictureProperties.Append(nonVisualDrawingProperties);
                nonVisualPictureProperties.Append(nonVisualPictureDrawingProperties);
                
                var blipFill = new PIC.BlipFill();
                var blip = new A.Blip() { Embed = relationshipId };
                var stretch = new A.Stretch();
                var fillRectangle = new A.FillRectangle();
                stretch.Append(fillRectangle);
                blipFill.Append(blip);
                blipFill.Append(stretch);
                
                var shapeProperties = new PIC.ShapeProperties();
                var transform2D = new A.Transform2D();
                var offset = new A.Offset() { X = 0L, Y = 0L };
                var extents = new A.Extents() { Cx = widthEmus, Cy = heightEmus };
                transform2D.Append(offset);
                transform2D.Append(extents);
                
                var presetGeometry = new A.PresetGeometry() { Preset = A.ShapeTypeValues.Rectangle };
                var adjustValueList = new A.AdjustValueList();
                presetGeometry.Append(adjustValueList);
                
                shapeProperties.Append(transform2D);
                shapeProperties.Append(presetGeometry);
                
                picture.Append(nonVisualPictureProperties);
                picture.Append(blipFill);
                picture.Append(shapeProperties);
                
                graphicData.Append(picture);
                graphic.Append(graphicData);
                
                inline.Append(extent);
                inline.Append(effectExtent);
                inline.Append(docProperties);
                inline.Append(graphic);
                
                drawing.Append(inline);
                run.Append(drawing);
                paragraph.Append(run);
                
                body.AppendChild(paragraph);
                body.AppendChild(new Paragraph(new Run(new Text("")))); // Додати порожній рядок після зображення
                
                LoggingService.LogInfo($"Зображення успішно вставлено: {Path.GetFileName(imagePath)}");
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Помилка при вставці зображення {imagePath}", ex);
                AddNormalText(body, $"[Помилка вставки зображення: {Path.GetFileName(imagePath)} - {ex.Message}]");
            }
        }
    }
}
