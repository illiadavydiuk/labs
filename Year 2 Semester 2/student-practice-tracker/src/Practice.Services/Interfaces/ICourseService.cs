using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Practice.Services.Interfaces
{
    public interface ICourseService
    {
        // Управління курсами
        Task<Course> CreateCourseAsync(Course course);
        Task<IEnumerable<Course>> GetAllActiveCoursesAsync();

        // Управління дисциплінами
        Task<Discipline> AddDisciplineAsync(string name);
        Task<IEnumerable<Discipline>> GetAllDisciplinesAsync();

        // Зарахування 
        Task<bool> EnrollStudentToCourseAsync(int studentId, int courseId, int groupId);
        Task<IEnumerable<CourseEnrollment>> GetCourseParticipantsAsync(int courseId);
    }
}
