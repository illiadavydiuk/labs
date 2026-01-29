using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class OrganizationRepository : Repository<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(AppDbContext context) : base(context)
        {

        }
        public async Task<IEnumerable<Organization>> GetByTypeAsync(string type)
        {
            return await _dbSet
                .Where(o => o.Type == type)
                .ToListAsync();
        }
    }
}
