using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Services.Interfaces
{
    public interface IReviewService
    {
        Task ReviewReportAsync(int reportId, int supervisorId, int statusId, string feedback, int? grade);
    }
}
