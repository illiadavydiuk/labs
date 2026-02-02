using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using Practice.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Practice.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly IReportRepository _reportRepo;
        private readonly IInternshipAssignmentRepository _assignmentRepo;
        private readonly IAuditService _auditService;

        public ReviewService(
            IReportRepository reportRepo,
            IInternshipAssignmentRepository assignmentRepo,
            IAuditService auditService)
        {
            _reportRepo = reportRepo;
            _assignmentRepo = assignmentRepo;
            _auditService = auditService;
        }

        public async Task ReviewReportAsync(int reportId, int supervisorId, int statusId, string feedback, int? grade)
        {
            var report = await _reportRepo.GetByIdAsync(reportId);
            if (report == null) throw new Exception("Звіт не знайдено");

            report.StatusId = statusId;
            report.SupervisorFeedback = feedback;
            _reportRepo.Update(report);

            var assignment = await _assignmentRepo.GetByIdAsync(report.AssignmentId);
            if (assignment != null)
            {
                if (grade.HasValue) assignment.FinalGrade = grade.Value;
                if (statusId == 3) assignment.StatusId = 3; 
                _assignmentRepo.Update(assignment);
            }

            await _reportRepo.SaveAsync(); 

            string action = statusId == 3 ? "Report Accepted" : "Report Returned";
            await _auditService.LogActionAsync(supervisorId, action, $"Grade: {grade}, Feedback: {feedback}", "Assignment", assignment.AssignmentId);
        }
    }
}