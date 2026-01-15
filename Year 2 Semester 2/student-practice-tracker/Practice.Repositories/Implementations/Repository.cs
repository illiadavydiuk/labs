using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class Repository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
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
