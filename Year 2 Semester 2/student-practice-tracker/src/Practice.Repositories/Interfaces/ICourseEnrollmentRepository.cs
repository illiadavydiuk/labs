using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Interfaces
{
    public interface ICourseEnrollmentRepository : IRepository<CourseEnrollment>
    {
        Task<IEnumerable<CourseEnrollment>> GetByCourseIdAsync(int courseId);
    }
}
