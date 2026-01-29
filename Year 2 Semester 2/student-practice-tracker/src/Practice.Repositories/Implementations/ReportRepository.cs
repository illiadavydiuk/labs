using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class ReportRepository : Repository<Report>, IReportRepository
    {
        public ReportRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Report>> GetByAssignmentIdAsync(int assignmentId)
        {
            return await _dbSet
                .Where(r => r.AssignmentId == assignmentId)
                .Include(r => r.ReportStatus) // Зв'язок з ReportStatus 
                .Include(r => r.Attachments)  // Список файлів 
                .ToListAsync();
        }
    }
}
