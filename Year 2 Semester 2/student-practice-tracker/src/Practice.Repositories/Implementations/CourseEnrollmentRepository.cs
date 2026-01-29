using Practice.Data.Context;
using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Repositories.Implementations
{
    public class CourseEnrollmentRepository : Repository<CourseEnrollment>, ICourseEnrollmentRepository
    {
        public CourseEnrollmentRepository(AppDbContext context) : base(context)
        {
        }
    }
}
