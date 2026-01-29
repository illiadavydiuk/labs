using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class DisciplineRepository : Repository<Discipline>, IDisciplineRepository
    {
        public DisciplineRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<Discipline> GetByNameAsync(string disciplineName)
        {
            return await _dbSet
                .FirstOrDefaultAsync(d => d.DisciplineName == disciplineName);
        }
    }
}
