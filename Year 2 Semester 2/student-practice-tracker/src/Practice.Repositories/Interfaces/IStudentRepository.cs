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
        Task<IEnumerable<Student>> GetStudentsByGroupAsync(int? groupId);
        Task<Student?> GetStudentProfileAsync(int userId);
        Task<List<Course>> GetEnrolledCoursesForStudentAsync(int studentId, int groupId);
    }
}

