using System.Collections.Generic;
using System.Linq;
using TaskManager.Data.Entities;
using TaskManager.Repositories.Implementations;

// Вирішення конфліктів імен
using Task = TaskManager.Data.Entities.Task;
using TaskStatus = TaskManager.Data.Entities.TaskStatus;

namespace TaskManager.Services
{
    public class TaskService
    {
        private readonly TaskRepository _taskRepo;
        private readonly DeveloperRepository _devRepo;

        public TaskService(TaskRepository taskRepo, DeveloperRepository devRepo)
        {
            _taskRepo = taskRepo;
            _devRepo = devRepo;
        }

        public IEnumerable<Task> GetTasksForProject(int projectId) => _taskRepo.GetTasksByProject(projectId);

        public Task GetTaskDetails(int taskId) => _taskRepo.GetTaskWithDevelopers(taskId);

        public void CreateTask(string title, int projectId)
        {
            _taskRepo.Add(new Task
            {
                Title = title,
                ProjectId = projectId,
                Status = TaskStatus.ToDo,
                Description = ""
            });
            _taskRepo.Save();
        }

        public void DeleteTask(int id)
        {
            _taskRepo.Delete(id);
            _taskRepo.Save();
        }

        public void ToggleStatus(Task task)
        {
            if (task.Status == TaskStatus.ToDo) task.Status = TaskStatus.InProgress;
            else if (task.Status == TaskStatus.InProgress) task.Status = TaskStatus.Done;
            else task.Status = TaskStatus.ToDo;

            _taskRepo.Update(task);
            _taskRepo.Save();
        }

        public void AssignDeveloper(int taskId, int devId)
        {
            var task = _taskRepo.GetTaskWithDevelopers(taskId);
            var dev = _devRepo.GetById(devId);

            if (task != null && dev != null && !task.Developers.Any(d => d.Id == devId))
            {
                task.Developers.Add(dev);
                _taskRepo.Update(task);
                _taskRepo.Save();
            }
        }

        public void RemoveDeveloperFromTask(int taskId, int devId)
        {
            var task = _taskRepo.GetTaskWithDevelopers(taskId);
            var dev = task?.Developers.FirstOrDefault(d => d.Id == devId);

            if (task != null && dev != null)
            {
                task.Developers.Remove(dev);
                _taskRepo.Update(task);
                _taskRepo.Save();
            }
        }
    }
}