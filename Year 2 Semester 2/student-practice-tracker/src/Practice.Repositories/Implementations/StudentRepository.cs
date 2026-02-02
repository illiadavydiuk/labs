using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        public StudentRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<Student> GetStudentDetailsAsync(int studentId)
        {
            return await _dbSet
                .Include(s => s.User)        // ПІБ та Email
                .Include(s => s.StudentGroup) // Шифр групи
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }
        public async Task<Student> GetByUserIdAsync(int userId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.StudentGroup) // Підтягуємо групу
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
        public async Task<IEnumerable<Student>> GetStudentsByGroupAsync(int? groupId)
        {
            var query = _dbSet
                .Include(s => s.User)
                .Include(s => s.StudentGroup)
                .AsQueryable();

            if (groupId.HasValue && groupId.Value > 0)
                query = query.Where(s => s.GroupId == groupId.Value);

            return await query.ToListAsync();
        }
        public async Task<Student?> GetStudentProfileAsync(int userId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.StudentGroup)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<List<Course>> GetEnrolledCoursesForStudentAsync(int studentId, int groupId)
        {
            return await _context.CourseEnrollments
                .Include(ce => ce.Course).ThenInclude(c => c.Discipline) 
                .Where(ce => ce.StudentId == studentId || ce.GroupId == groupId)
                .Select(ce => ce.Course)
                .Distinct()
                .ToListAsync();
        }
    }
}
