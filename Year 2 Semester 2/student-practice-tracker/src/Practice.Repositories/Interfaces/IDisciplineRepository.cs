using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Interfaces
{
    public interface IDisciplineRepository : IRepository<Discipline>
    {
        Task<Discipline> GetByNameAsync(string disciplineName);
    }
}
