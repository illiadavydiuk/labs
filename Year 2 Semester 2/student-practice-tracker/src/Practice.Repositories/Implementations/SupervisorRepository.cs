using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class SupervisorRepository : Repository<Supervisor>, ISupervisorRepository
    {
        public SupervisorRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<Supervisor> GetSupervisorDetailsAsync(int supervisorId)
        {
            return await _dbSet
                .Include(sup => sup.User)
                .Include(sup => sup.Department) 
                .Include(sup => sup.Position)   
                .FirstOrDefaultAsync(sup => sup.SupervisorId == supervisorId);
        }
        public async Task<IEnumerable<Supervisor>> GetSupervisorsByDepartmentAsync(int? departmentId)
        {
            var query = _dbSet
                .Include(s => s.User)
                .Include(s => s.Department)
                .Include(s => s.Position)
                .AsQueryable();

            if (departmentId.HasValue && departmentId.Value > 0)
                query = query.Where(s => s.DepartmentId == departmentId.Value);

            return await query.ToListAsync();
        }

        public async Task<Supervisor> GetByUserIdAsync(int userId)
        {
            return await _dbSet
               .Include(s => s.User)
               .FirstOrDefaultAsync(s => s.UserId == userId);
        }
        public async Task<Supervisor?> GetSupervisorProfileAsync(int userId)
        {
            return await _context.Supervisors
                .Include(s => s.User)
                .Include(s => s.Department)
                .Include(s => s.Position)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<List<InternshipAssignment>> GetStudentsForSupervisorAsync(int supervisorId)
        {
            return await _context.InternshipAssignments
                .Include(a => a.Student).ThenInclude(s => s.User)
                .Include(a => a.Student).ThenInclude(s => s.StudentGroup)
                .Include(a => a.Course)
                .Include(a => a.InternshipTopic).ThenInclude(t => t.Organization)
                .Include(a => a.Reports)
                    .ThenInclude(r => r.Attachments) 
                .Where(a => a.SupervisorId == supervisorId || a.Course.SupervisorId == supervisorId)
                .ToListAsync();
        }
    }
}
