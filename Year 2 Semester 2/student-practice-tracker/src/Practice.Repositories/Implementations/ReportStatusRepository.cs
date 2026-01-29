using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class ReportStatusRepository : Repository<ReportStatus>, IReportStatusRepository
    {
        public ReportStatusRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<ReportStatus> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.StatusName == name);
        }
    }
}
