using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Practice.Repositories.Implementations
{
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(AppDbContext context) : base(context) { }

        public async Task<List<AuditLog>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(l => l.User) 
                .OrderByDescending(l => l.TimeStamp)
                .ToListAsync();
        }
        public async Task<List<AuditLog>> GetHistoryForAssignmentAsync(int assignmentId)
        {
             return await _context.AuditLogs
                .Where(l => (l.EntityAffected == "InternshipAssignment" && l.RecordId == assignmentId) ||
                            (l.EntityAffected == "Report" && l.Details.Contains($"AssignmentId: {assignmentId}")))
                .OrderByDescending(l => l.TimeStamp)
                .ToListAsync();
        }
    }
}