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
        public async Task<IEnumerable<Course>> GetActiveCoursesAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .Include(c => c.Discipline)
                .ToListAsync();
        }
    }
}
