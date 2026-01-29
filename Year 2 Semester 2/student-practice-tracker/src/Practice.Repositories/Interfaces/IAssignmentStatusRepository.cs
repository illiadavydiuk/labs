using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Interfaces
{
    public interface IAssignmentStatusRepository : IRepository<AssignmentStatus>
    {
        Task<AssignmentStatus> GetByNameAsync(string statusName);
    }
}
