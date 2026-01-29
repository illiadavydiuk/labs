using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class AttachmentRepository : Repository<Attachment>, IAttachmentRepository
    {
        public AttachmentRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Attachment>> GetByReportIdAsync(int reportId)
        {
            return await _dbSet
                .Where(a => a.ReportId == reportId)
                .OrderByDescending(a => a.UploadedAt)
                .ToListAsync();
        }
    }
}
