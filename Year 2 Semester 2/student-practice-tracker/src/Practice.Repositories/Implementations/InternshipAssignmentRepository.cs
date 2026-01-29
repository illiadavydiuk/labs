using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class InternshipAssignmentRepository : Repository<InternshipAssignment>, IInternshipAssignmentRepository
    {
        public InternshipAssignmentRepository(AppDbContext context) : base(context) { }

        public async Task<InternshipAssignment> GetFullAssignmentDetailsAsync(int assignmentId)
        {
            return await _dbSet
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.InternshipTopic).ThenInclude(t => t.Organization) 
                .Include(a => a.Supervisor).ThenInclude(sup => sup.User)
                .Include(a => a.Course)
                .Include(a => a.AssignmentStatus)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);
        }

        public async Task<IEnumerable<InternshipAssignment>> GetAssignmentsByCourseAsync(int courseId)
        {
            return await _dbSet
                .Where(a => a.CourseId == courseId)
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.InternshipTopic)
                .Include(a => a.AssignmentStatus)
                .ToListAsync();
        }
    }
}
