using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Interfaces
{
    public interface IReportRepository : IRepository<Report>
    {
        Task<List<Report>> GetReportsByAssignmentIdAsync(int assignmentId);
        Task AddAsync(Report report);
        Task SaveAsync();
    }
}
