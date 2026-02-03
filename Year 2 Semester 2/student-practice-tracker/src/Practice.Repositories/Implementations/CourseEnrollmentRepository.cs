using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class CourseEnrollmentRepository : Repository<CourseEnrollment>, ICourseEnrollmentRepository
    {
        public CourseEnrollmentRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<CourseEnrollment>> GetByCourseIdAsync(int courseId)
        {
            return await _dbSet
                .Where(e => e.CourseId == courseId)
                .Include(e => e.Student).ThenInclude(s => s.User)
                .Include(e => e.Student).ThenInclude(s => s.StudentGroup)
                .ToListAsync();
        }
        public async Task<bool> IsEnrolledAsync(int studentId, int courseId)
        {
            return await _context.CourseEnrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
        }
    }
}
