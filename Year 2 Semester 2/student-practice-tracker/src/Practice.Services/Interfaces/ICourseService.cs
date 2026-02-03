using Practice.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAllActiveCoursesAsync();
        Task<Course> CreateCourseAsync(Course course);

        Task UpdateCourseAsync(Course course);
        Task DeleteCourseAsync(int id);

        Task<bool> EnrollStudentToCourseAsync(int studentId, int courseId, int? groupId); 
        Task<IEnumerable<Discipline>> GetAllDisciplinesAsync();
        Task AddDisciplineAsync(string name);
        Task UnenrollGroupFromCourseAsync(int courseId, int groupId);
        Task<IEnumerable<CourseEnrollment>> GetByCourseIdAsync(int courseId);
        Task<Course> GetCourseByIdAsync(int courseId);
    }
}