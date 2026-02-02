using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Interfaces
{
    public interface ISupervisorRepository : IRepository<Supervisor>
    {
        Task<Supervisor> GetSupervisorDetailsAsync(int supervisorId);
        Task<IEnumerable<Supervisor>> GetSupervisorsByDepartmentAsync(int? departmentId);
        Task<Supervisor> GetByUserIdAsync(int userId);
        Task<Supervisor?> GetSupervisorProfileAsync(int userId);
    }
}
