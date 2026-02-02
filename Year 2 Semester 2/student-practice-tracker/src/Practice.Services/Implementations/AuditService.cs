using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using Practice.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Implementations
{
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditRepo;

        public AuditService(IAuditLogRepository auditRepo)
        {
            _auditRepo = auditRepo;
        }

        public async Task LogActionAsync(int? userId, string action, string details, string entityName = null, int? entityId = null)
        {
            try
            {
                var log = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    Details = details ?? "",

                    EntityAffected = entityName ?? "System",

                    EntityId = entityId,
                    TimeStamp = DateTime.UtcNow
                };

                _auditRepo.Add(log);
                await _auditRepo.SaveAsync();
            }
            catch
            {
            }
        }

        public async Task<List<AuditLog>> GetAllLogsAsync()
        {
            return await _auditRepo.GetAllWithDetailsAsync();
        }
    }
}