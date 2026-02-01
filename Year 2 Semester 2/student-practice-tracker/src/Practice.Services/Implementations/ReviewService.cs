using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Practice.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context)
        {
            _context = context;
        }

        public async Task ReviewReportAsync(int reportId, int supervisorId, int statusId, string feedback, int? grade)
        {
            using var context = new AppDbContext();

            var report = await context.Reports
                .Include(r => r.InternshipAssignment) 
                .FirstOrDefaultAsync(r => r.ReportId == reportId);

            if (report == null) throw new Exception("Звіт не знайдено");

            report.StatusId = statusId;
            report.SupervisorFeedback = feedback;

            if (grade.HasValue)
            {
                report.InternshipAssignment.FinalGrade = grade.Value;
            }

            if (statusId == 3)
            {
                report.InternshipAssignment.StatusId = 3;
            }

            await context.SaveChangesAsync();

            // Логування
            string action = statusId == 3 ? "Report Accepted" : "Report Returned";
            string msg = statusId == 3
                ? $"Викладач прийняв звіт. Оцінка: {grade}"
                : $"Викладач повернув звіт. Зауваження: {feedback}";

            var log = new AuditLog
            {
                UserId = supervisorId,
                Action = action,
                Details = msg,
                EntityName = "Assignment",
                EntityId = report.AssignmentId,
                TimeStamp = DateTime.UtcNow
            };
            context.AuditLogs.Add(log);
            await context.SaveChangesAsync();
        }
    }
}