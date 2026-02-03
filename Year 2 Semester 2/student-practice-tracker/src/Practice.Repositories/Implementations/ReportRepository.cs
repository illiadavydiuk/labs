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
        public async Task<List<Report>> GetReportsByAssignmentIdAsync(int assignmentId)
        {
            return await _context.Reports
                .Where(r => r.AssignmentId == assignmentId)
                .Include(r => r.Attachments) 
                .OrderByDescending(r => r.SubmissionDate) 
                .ToListAsync();
        }

        public async Task AddAsync(Report report)
        {
            await _context.Reports.AddAsync(report);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
