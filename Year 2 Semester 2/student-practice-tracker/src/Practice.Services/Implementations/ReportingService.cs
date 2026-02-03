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

namespace Practice.Services.Implementations
{
    public class ReportingService : IReportingService
    {
        private readonly IInternshipAssignmentRepository _assignmentRepo;
        private readonly IStudentGroupRepository _groupRepo;

        public ReportingService(IInternshipAssignmentRepository assignmentRepo, IStudentGroupRepository groupRepo)
        {
            _assignmentRepo = assignmentRepo;
            _groupRepo = groupRepo;
            // Встановлюємо ліцензію (обов'язково для QuestPDF)
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

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Реєстр розподілу");

                // Заголовки
                worksheet.Cell(1, 1).Value = "ПІБ Студента";
                worksheet.Cell(1, 2).Value = "Група";
                worksheet.Cell(1, 3).Value = "Тема практики";
                worksheet.Cell(1, 4).Value = "Статус призначення";
                worksheet.Cell(1, 5).Value = "Статус останнього звіту";
                worksheet.Cell(1, 6).Value = "Дата подачі";
                worksheet.Cell(1, 7).Value = "Коментар керівника";

                var headerRange = worksheet.Range(1, 1, 1, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                int row = 2;
                foreach (var a in assignments)
                {
                    var lastReport = a.Reports?.OrderByDescending(r => r.SubmissionDate).FirstOrDefault();

                    worksheet.Cell(row, 1).Value = $"{a.Student?.User?.LastName} {a.Student?.User?.FirstName}"; 
                    worksheet.Cell(row, 2).Value = a.Student?.StudentGroup?.GroupCode; 
                    worksheet.Cell(row, 3).Value = a.InternshipTopic?.Title; 
                    worksheet.Cell(row, 4).Value = a.AssignmentStatus?.StatusName; 
                    worksheet.Cell(row, 5).Value = lastReport?.ReportStatus?.StatusName ?? "Не подано"; 
                    worksheet.Cell(row, 6).Value = lastReport?.SubmissionDate.ToString("dd.MM.yyyy") ?? "-"; 
                    worksheet.Cell(row, 7).Value = lastReport?.SupervisorFeedback ?? "-"; 
                    row++;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }

        public async Task GeneratePdfStatementAsync(int courseId, int groupId, string filePath)
        {
            var assignments = await _assignmentRepo.GetAssignmentsByCourseAsync(courseId);
            var group = await _groupRepo.GetByIdAsync(groupId); 

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Відомість результатів навчальної практики").FontSize(18).SemiBold().AlignCenter();
                        col.Item().PaddingTop(5).Text($"Група: {group?.GroupCode} | Рік: {DateTime.Now.Year}").AlignCenter();
                        col.Item().PaddingBottom(10).LineHorizontal(1);
                    });

                    page.Content().Table(table =>
                    {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25);  // №
                        columns.RelativeColumn(3);   // ПІБ
                        columns.RelativeColumn(3);   // Тема
                        columns.RelativeColumn(2);   // Організація
                        columns.ConstantColumn(50);  // Оцінка
                        columns.RelativeColumn(1.5f); // Підпис
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("№");
                        header.Cell().Element(CellStyle).Text("ПІБ Студента");
                        header.Cell().Element(CellStyle).Text("Тема практики");
                        header.Cell().Element(CellStyle).Text("Організація");
                        header.Cell().Element(CellStyle).Text("Оцінка");
                        header.Cell().Element(CellStyle).Text("Підпис");

                        static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).AlignCenter();
                    });

                    int i = 1;
                    foreach (var a in assignments.Where(x => x.Student.GroupId == groupId))
                    {
                        table.Cell().Element(RowStyle).Text(i++.ToString());
                        table.Cell().Element(RowStyle).Text($"{a.Student?.User?.LastName} {a.Student?.User?.FirstName}");
                        table.Cell().Element(RowStyle).Text(a.InternshipTopic?.Title ?? "-");
                        table.Cell().Element(RowStyle).Text(a.InternshipTopic?.Organization?.Name ?? "-");
                        table.Cell().Element(RowStyle).AlignCenter().Text(a.FinalGrade?.ToString() ?? "-"); 
                            table.Cell().Element(RowStyle).Text("");

                    static IContainer RowStyle(IContainer container) => container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(2);
                }
                    });

            page.Footer().AlignRight().Text(x =>
            {
                x.Span("Дата формування: ");
                x.Span(DateTime.Now.ToString("dd.MM.yyyy"));
            });
        });
            }).GeneratePdf(filePath);
}
    }
}