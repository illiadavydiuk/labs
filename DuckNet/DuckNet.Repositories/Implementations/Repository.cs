using DuckNet.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using DuckNet.Data.Context; // Важливо: тут лежить DuckNetDbContext

namespace DuckNet.Repositories.Implementations // 1. Виправлений namespace
{
    public class Repository<T> : IRepository<T> where T : class
    {
        // 2. Використовуємо DuckNetDbContext замість SystemDbContext
        protected readonly DuckNetDbContext _context;
        protected readonly DbSet<T> _dbSet;

        // 3. Конструктор приймає DuckNetDbContext
        public Repository(DuckNetDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual IEnumerable<T> GetAll() => _dbSet.ToList();

        public virtual T GetById(int id) => _dbSet.Find(id);

        public void Add(T entity) => _dbSet.Add(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(int id)
        {
            var entity = GetById(id);
            if (entity != null) _dbSet.Remove(entity);
        }

        public void Save() => _context.SaveChanges();
    }
}