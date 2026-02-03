using System.Threading.Tasks;

namespace Practice.Services.Interfaces
{
    public interface IReportingService
    {
        // Резервна копія файлу SQLite
        void CreateDatabaseBackup(string destinationPath);

        // Excel-звіт: Реєстр розподілу та статусу виконання
        Task GenerateExcelStatusReportAsync(string filePath);

        // PDF-звіт: Підсумкова відомість результатів практики
        Task GeneratePdfStatementAsync(int courseId, int groupId, string filePath);
    }
}