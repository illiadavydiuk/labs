using Practice.Repositories.Interfaces;
using Practice.Services.Interfaces;
using System.Text;

public class AnalyticsService : IAnalyticsService
{
    private readonly IInternshipAssignmentRepository _assignmentRepo;

    public AnalyticsService(IInternshipAssignmentRepository assignmentRepo)
    {
        _assignmentRepo = assignmentRepo;
    }

    public async Task<string> GenerateSimpleReportAsync()
    {
        var assignments = await _assignmentRepo.GetAllAsync();
        var sb = new StringBuilder();
        sb.AppendLine("Прізвище студента;Оцінка;Статус"); 

        foreach (var a in assignments)
        {
            sb.AppendLine($"{a.StudentId};{a.FinalGrade ?? 0};{a.StatusId}");
        }

        return sb.ToString();
    }

    public void ExportDatabase(string destinationPath)
    {
        // Просто копіюємо файл
        string dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "StudentPracticePlatform", "practice_platform.db");

        if (File.Exists(dbPath))
        {
            File.Copy(dbPath, destinationPath, true);
        }
    }
}