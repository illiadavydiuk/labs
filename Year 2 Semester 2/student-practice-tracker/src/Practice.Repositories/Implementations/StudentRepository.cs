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
    }
}
