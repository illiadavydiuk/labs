using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using Practice.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly IReportRepository _reportRepo;
        private readonly IAttachmentRepository _attachmentRepo;
        private readonly IAuditService _auditService;

        public ReviewService(IReportRepository reportRepo, IAttachmentRepository attachmentRepo, IAuditService auditService)
        {
            _reportRepo = reportRepo;
            _attachmentRepo = attachmentRepo;
            _auditService = auditService;
        }

        public async Task<bool> SubmitReportAsync(int assignmentId, string? comment, List<Attachment> files)
        {
            var report = new Report
            {
                AssignmentId = assignmentId, 
                StudentComment = comment, 
                StatusId = 1, 
                SubmissionDate = DateTime.UtcNow
            };

            _reportRepo.Add(report);
            await _reportRepo.SaveAsync(); 

            foreach (var file in files)
            {
                file.ReportId = report.ReportId;

                if (string.IsNullOrEmpty(file.FileType)) file.FileType = "binary";

                _attachmentRepo.Add(file);
            }

            await _attachmentRepo.SaveAsync();

            await _auditService.LogActionAsync(null, "SubmitReport", "Подача звіту", "Report", report.ReportId);
            return true;
        }

        public async Task<bool> ReviewReportAsync(int reportId, int statusId, string feedback)
        {
            var report = await _reportRepo.GetByIdAsync(reportId);
            if (report == null) return false;

            report.StatusId = statusId;
            report.SupervisorFeedback = feedback; 
            report.ReviewDate = DateTime.UtcNow;   

            _reportRepo.Update(report);
            await _reportRepo.SaveAsync();
            await _auditService.LogActionAsync(null, "Review", $"Оцінка/статус: {statusId}", "Report", reportId);
            return true;
        }

        public async Task<IEnumerable<Report>> GetReportsHistoryAsync(int assignmentId)
        {
            return await _reportRepo.GetByAssignmentIdAsync(assignmentId);
        }
    }
}