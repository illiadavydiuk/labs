using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class InternshipTopicRepository : Repository<InternshipTopic>, IInternshipTopicRepository
    {
        public InternshipTopicRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<InternshipTopic>> GetAvailableTopicsAsync()
        {
            return await _dbSet
                .Where(t => t.IsAvailable)
                .Include(t => t.Organization)
                .ToListAsync();
        }

        public async Task<IEnumerable<InternshipTopic>> GetByOrganizationAsync(int organizationId)
        {
            return await _dbSet
                .Where(t => t.OrganizationId == organizationId)
                .ToListAsync();
        }
    }
}
