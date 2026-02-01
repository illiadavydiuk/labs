using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Interfaces
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<Student> GetStudentDetailsAsync(int studentId);
        Task<Student> GetByUserIdAsync(int userId);
    }
}
