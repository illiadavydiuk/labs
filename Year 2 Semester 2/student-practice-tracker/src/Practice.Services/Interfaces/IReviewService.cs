using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Services.Interfaces
{
    public interface IReviewService
    {
        Task<bool> SubmitReportAsync(int assignmentId, string studentComment, List<Attachment> files);

        Task<bool> ReviewReportAsync(int reportId, int statusId, string feedback);

        Task<IEnumerable<Report>> GetReportsHistoryAsync(int assignmentId);
    }
}
