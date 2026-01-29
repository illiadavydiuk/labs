using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class DepartmentRepository : Repository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<Department> GetByNameAsync(string departmentName)
        {
            return await _dbSet
                .FirstOrDefaultAsync(d => d.DepartmentName == departmentName);
        }
    }
}
