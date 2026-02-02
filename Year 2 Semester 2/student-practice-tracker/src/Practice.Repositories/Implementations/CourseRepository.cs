using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Course>> GetAllActiveCoursesAsync()
        {
            return await _context.Courses
                .Where(c => c.IsActive)
                .Include(c => c.Discipline)
                .Include(c => c.Supervisor).ThenInclude(s => s.User)
                .Include(c => c.CourseEnrollments)       
                    .ThenInclude(e => e.StudentGroup)   
                .ToListAsync();
        }
        public async Task<IEnumerable<Course>> GetAllActiveAsync()
        {
            return await _context.Courses
                .AsNoTracking()
                .Include(c => c.Discipline)
                .Include(c => c.Supervisor).ThenInclude(s => s.User)
                .Where(c => c.IsActive)
                .ToListAsync();
        }
        public async Task<IEnumerable<Course>> GetActiveCoursesAsync()
        {
            return await _context.Courses
                .Where(c => c.IsActive)
                .Include(c => c.Discipline)
                .Include(c => c.Supervisor).ThenInclude(s => s.User)
                .Include(c => c.CourseEnrollments)
                    .ThenInclude(e => e.StudentGroup)
                .ToListAsync();
        }
    }
}
