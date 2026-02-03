using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Practice.Repositories.Interfaces;
using Practice.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using XLColor = ClosedXML.Excel.XLColor;
using PdfColor = QuestPDF.Infrastructure.Color;

namespace Practice.Services.Implementations
{
    public class ReportingService : IReportingService
    {
        private readonly IInternshipAssignmentRepository _assignmentRepo;
        private readonly IStudentGroupRepository _groupRepo;
        private readonly ICourseService _courseService;

        public ReportingService(IInternshipAssignmentRepository assignmentRepo, IStudentGroupRepository groupRepo, ICourseService courseService)
        {
            _assignmentRepo = assignmentRepo;
            _groupRepo = groupRepo;
            _courseService = courseService;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public void CreateDatabaseBackup(string destinationPath)
        {
            string folderName = "StudentPracticePlatform";
            string path = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folderName); 
            else
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share", folderName);

            string dbPath = Path.Combine(path, "practice_platform.db"); 

            if (File.Exists(dbPath))
            {
                File.Copy(dbPath, destinationPath, true);
            }
            else
            {
                throw new FileNotFoundException("Файл бази даних не знайдено за шляхом: " + dbPath);
            }
        }

        public async Task GenerateExcelStatusReportAsync(string filePath)
        {
            var assignments = await _assignmentRepo.GetAllAsync();

            var groupedAssignments = assignments
                .Where(a => a.Student?.StudentGroup != null)
                .GroupBy(a => a.Student.StudentGroup.GroupCode)
                .OrderBy(g => g.Key);

            using (var workbook = new XLWorkbook())
            {
                foreach (var group in groupedAssignments)
                {
                    string sheetName = group.Key.Length > 31 ? group.Key.Substring(0, 31) : group.Key;
                    var ws = workbook.Worksheets.Add(sheetName);

                    var titleRange = ws.Range(1, 1, 1, 5).Merge();
                    titleRange.Value = "Відомість результатів навчальної практики";
                    titleRange.Style.Font.Bold = true;
                    titleRange.Style.Font.FontSize = 16;
                    titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    var subtitleRange = ws.Range(2, 1, 2, 5).Merge();
                    subtitleRange.Value = $"Група: {group.Key} | Рік: {DateTime.Now.Year}";
                    subtitleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    subtitleRange.Style.Font.Italic = true;

                    string[] headers = { "№", "ПІБ Студента", "Тема практики", "Організація", "Оцінка" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = ws.Cell(4, i + 1);
                        cell.Value = headers[i];

                        var s = cell.Style;
                        s.Font.Bold = true;
                        s.Fill.BackgroundColor = XLColor.LightGray;
                        s.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        s.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    int currentRow = 5; 
                    int counter = 1;

                    foreach (var a in group.OrderBy(x => x.Student?.User?.LastName))
                    {
                        ws.Cell(currentRow, 1).SetValue(counter++);
                        ws.Cell(currentRow, 2).SetValue($"{a.Student?.User?.LastName} {a.Student?.User?.FirstName}");
                        ws.Cell(currentRow, 3).SetValue(a.InternshipTopic?.Title ?? "-");
                        ws.Cell(currentRow, 4).SetValue(a.InternshipTopic?.Organization?.Name ?? "-");

                        var gradeCell = ws.Cell(currentRow, 5);
                        if (a.FinalGrade.HasValue)
                        {
                            gradeCell.SetValue(a.FinalGrade.Value);

                            if (a.FinalGrade.Value < 60)
                            {
                                gradeCell.Style.Font.FontColor = XLColor.Red;
                                gradeCell.Style.Font.Bold = true;
                            }
                        }
                        else gradeCell.SetValue("-");

                        var rowDataRange = ws.Range(currentRow, 1, currentRow, 5);
                        rowDataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        rowDataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        currentRow++;
                    }

                    currentRow += 2;
                    ws.Cell(currentRow, 2).Value = "Керівник практики від кафедри:";
                    ws.Cell(currentRow, 4).Style.Border.BottomBorder = XLBorderStyleValues.Thin;

                    var supervisor = group.FirstOrDefault()?.Course?.Supervisor?.User;
                    if (supervisor != null)
                    {
                        ws.Cell(currentRow, 5).Value = $"{supervisor.LastName} {supervisor.FirstName[0]}.";
                        ws.Cell(currentRow, 5).Style.Font.Bold = true;
                    }

                    ws.Columns(1, 5).AdjustToContents();
                }

                workbook.SaveAs(filePath);
            }
        }

        public async Task GeneratePdfStatementAsync(int courseId, int groupId, string filePath)
        {
            var assignments = await _assignmentRepo.GetAssignmentsByCourseAsync(courseId);
            var group = await _groupRepo.GetByIdAsync(groupId);
            var course = await _courseService.GetCourseByIdAsync(courseId);

            string supervisorName = course?.Supervisor?.User != null
                ? $"{course.Supervisor.User.LastName} {course.Supervisor.User.FirstName[0]}."
                : "Не призначено";

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("Відомість результатів навчальної практики").FontSize(16).SemiBold();
                        col.Item().PaddingTop(2).AlignCenter().Text($"Група: {group?.GroupCode} | {DateTime.Now.Year} рік");
                        col.Item().PaddingBottom(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Black);
                    });

                    page.Content().PaddingTop(10).Column(col =>
                    {
                        // 1. ТАБЛИЦЯ (5 колонок)
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);  // №
                                columns.RelativeColumn(3);   // ПІБ
                                columns.RelativeColumn(4);   // Тема
                                columns.RelativeColumn(3);   // Організація
                                columns.ConstantColumn(50);  // Оцінка
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).AlignCenter().Text("№");
                                header.Cell().Element(CellStyle).Text("ПІБ Студента");
                                header.Cell().Element(CellStyle).AlignCenter().Text("Тема практики");
                                header.Cell().Element(CellStyle).AlignCenter().Text("Організація");
                                header.Cell().Element(CellStyle).AlignCenter().Text("Оцінка");

                                static IContainer CellStyle(IContainer container) =>
                                    container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1);
                            });

                            int i = 1;
                            foreach (var a in assignments.Where(x => x.Student.GroupId == groupId).OrderBy(x => x.Student.User.LastName))
                            {
                                table.Cell().Element(RowStyle).AlignCenter().Text(i++.ToString());
                                table.Cell().Element(RowStyle).Text($"{a.Student?.User?.LastName} {a.Student?.User?.FirstName}");
                                table.Cell().Element(RowStyle).AlignCenter().Text(a.InternshipTopic?.Title ?? "-");
                                table.Cell().Element(RowStyle).AlignCenter().Text(a.InternshipTopic?.Organization?.Name ?? "-");
                                table.Cell().Element(RowStyle).AlignCenter().Text(a.FinalGrade?.ToString() ?? "-");

                                static IContainer RowStyle(IContainer container) =>
                                    container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5);
                            }
                        });

                        col.Item().PaddingTop(30).ShowEntire().Row(row =>
                        {
                            row.AutoItem().Text("Керівник практики від кафедри:");

                            row.RelativeItem();

                            row.ConstantColumn(80).PaddingHorizontal(5).PaddingBottom(2).BorderBottom(1).AlignBottom();

                            row.AutoItem().MinWidth(100).Text(supervisorName).SemiBold();
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Сторінка ");
                        x.CurrentPageNumber();
                        x.Span(" з ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf(filePath);
        }
    }
}