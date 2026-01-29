using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class StudentGroupRepository : Repository<StudentGroup>, IStudentGroupRepository
    {
        public StudentGroupRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<StudentGroup>> GetGroupsBySpecialtyAsync(int specialtyId)
        {
            return await _dbSet
                .Where(g => g.SpecialtyId == specialtyId)
                .Include(g => g.Specialty)
                .ToListAsync();
        }
    }
}
