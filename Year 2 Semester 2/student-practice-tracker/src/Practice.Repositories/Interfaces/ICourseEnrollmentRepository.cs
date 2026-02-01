using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Practice.Repositories.Interfaces
{
    public interface ICourseEnrollmentRepository : IRepository<CourseEnrollment>
    {
        Task<IEnumerable<CourseEnrollment>> GetByCourseIdAsync(int courseId);
        Task<bool> IsEnrolledAsync(int studentId, int courseId);
    }
}
