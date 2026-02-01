using Practice.Data.Entities; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(int? userId, string action, string details, string entityName, int entityId);

        Task<List<AuditLog>> GetAllLogsAsync();
    }
}