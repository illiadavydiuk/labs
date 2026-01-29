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
    }
}
