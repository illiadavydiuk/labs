using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public virtual async Task<IEnumerable<T>> GetAllAsync() =>
            await _dbSet.ToListAsync();

        public virtual async Task<T> GetByIdAsync(int id) =>
            await _dbSet.FindAsync(id);

        public void Add(T entity) => _dbSet.Add(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity)
        {
            if (entity != null) _dbSet.Remove(entity);
        }

        public async Task SaveAsync() =>
            await _context.SaveChangesAsync();
    }
}
