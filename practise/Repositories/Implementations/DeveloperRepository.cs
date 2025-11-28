using TaskManager.Data.Context;
using TaskManager.Data.Entities;

namespace TaskManager.Repositories.Implementations
{
    public class DeveloperRepository : Repository<Developer>
    {
        public DeveloperRepository(SystemDbContext context) : base(context) { }
    }
}