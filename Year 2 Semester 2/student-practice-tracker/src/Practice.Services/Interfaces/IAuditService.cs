using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(int? userId, string action, string details, string entityName, int entityId);
    }
}
