using System.Collections.Generic;
using System.Linq;
using TaskManager.Data.Entities;
using TaskManager.Repositories.Implementations;

namespace TaskManager.Services
{
    public class ProjectService
    {
        private readonly ProjectRepository _projectRepo;
        private readonly DeveloperRepository _devRepo;

        public ProjectService(ProjectRepository projectRepo, DeveloperRepository devRepo)
        {
            _projectRepo = projectRepo;
            _devRepo = devRepo;
        }

        public IEnumerable<Project> GetAllProjects() => _projectRepo.GetAll();

        public Project GetProjectDetails(int id) => _projectRepo.GetProjectWithDevelopers(id);

        public void CreateProject(string name, string desc)
        {
            _projectRepo.Add(new Project { Name = name, Description = desc });
            _projectRepo.Save();
        }

        public void UpdateProject(int id, string name, string desc)
        {
            var p = _projectRepo.GetById(id);
            if (p != null)
            {
                p.Name = name;
                p.Description = desc;
                _projectRepo.Update(p);
                _projectRepo.Save();
            }
        }

        public void DeleteProject(int id)
        {
            _projectRepo.Delete(id);
            _projectRepo.Save();
        }

        public void AddDeveloperToProject(int projectId, int devId)
        {
            var project = _projectRepo.GetProjectWithDevelopers(projectId);
            var dev = _devRepo.GetById(devId);

            if (project != null && dev != null && !project.Developers.Any(d => d.Id == devId))
            {
                project.Developers.Add(dev);
                _projectRepo.Update(project);
                _projectRepo.Save();
            }
        }

        public void RemoveDeveloperFromProject(int projectId, int devId)
        {
            var project = _projectRepo.GetProjectWithDevelopers(projectId);
            var dev = project?.Developers.FirstOrDefault(d => d.Id == devId);

            if (project != null && dev != null)
            {
                project.Developers.Remove(dev);
                _projectRepo.Update(project);
                _projectRepo.Save();
            }
        }
    }
}