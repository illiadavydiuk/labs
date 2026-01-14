using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using TaskManager.Data.Context;

using Task = TaskManager.Data.Entities.Task;

namespace TaskManager.Repositories.Implementations
{
    public class TaskRepository : Repository<Task>
    {
        public TaskRepository(SystemDbContext context) : base(context) { }

        public IEnumerable<Task> GetTasksByProject(int projectId)
        {
            return _context.Tasks
                .Include(t => t.Developers)
                .Where(t => t.ProjectId == projectId)
                .ToList();
        }

        public Task GetTaskWithDevelopers(int id)
        {
            return _context.Tasks
                .Include(t => t.Developers)
                .FirstOrDefault(t => t.Id == id);
        }
    }
}