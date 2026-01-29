using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Include(u => u.Role) // Роль для перевірки прав
                .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
