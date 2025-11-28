using System.Collections.Generic;
using System.Linq;
using TaskManager.Data.Entities;
using TaskManager.Repositories.Implementations;

namespace TaskManager.Services
{
    public class DeveloperService
    {
        private readonly DeveloperRepository _repo;

        public DeveloperService(DeveloperRepository repo) => _repo = repo;

        public IEnumerable<Developer> GetAll() => _repo.GetAll();

        public void Add(string name, string email)
        {
            _repo.Add(new Developer { FullName = name, Email = email });
            _repo.Save();
        }
        public void Update(int id, string newName, string newEmail)
        {
            var dev = _repo.GetById(id);
            if (dev != null)
            {
                dev.FullName = newName;
                dev.Email = newEmail;
                _repo.Update(dev);
                _repo.Save();
            }
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
            _repo.Save();
        }
    }
}