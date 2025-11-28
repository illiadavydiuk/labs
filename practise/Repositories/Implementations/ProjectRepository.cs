using Microsoft.EntityFrameworkCore;
using System.Linq;
using TaskManager.Data.Context;
using TaskManager.Data.Entities;

namespace TaskManager.Repositories.Implementations
{
    public class ProjectRepository : Repository<Project>
    {
        public ProjectRepository(SystemDbContext context) : base(context) { }

        public Project GetProjectWithDevelopers(int id)
        {
            return _context.Projects
                .Include(p => p.Developers)
                .FirstOrDefault(p => p.Id == id);
        }
    }
}