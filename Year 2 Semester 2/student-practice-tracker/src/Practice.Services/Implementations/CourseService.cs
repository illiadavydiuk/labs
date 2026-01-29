using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using Practice.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Practice.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepo;
        private readonly IDisciplineRepository _disciplineRepo;
        private readonly ICourseEnrollmentRepository _enrollmentRepo;
        private readonly IAuditService _auditService;

        public CourseService(
            ICourseRepository courseRepo,
            IDisciplineRepository disciplineRepo,
            ICourseEnrollmentRepository enrollmentRepo,
            IAuditService auditService)
        {
            _courseRepo = courseRepo;
            _disciplineRepo = disciplineRepo;
            _enrollmentRepo = enrollmentRepo;
            _auditService = auditService;
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            _courseRepo.Add(course);
            await _courseRepo.SaveAsync();

            await _auditService.LogActionAsync(null, "Create", $"Курс: {course.Name}", "Course", course.CourseId);
            return course;
        }

        public async Task<IEnumerable<Course>> GetAllActiveCoursesAsync()
        {
            return await _courseRepo.GetActiveCoursesAsync();
        }

        public async Task<Discipline> AddDisciplineAsync(string name)
        {
            var discipline = new Discipline { DisciplineName = name };
            _disciplineRepo.Add(discipline);
            await _disciplineRepo.SaveAsync();

            await _auditService.LogActionAsync(null, "Add", $"Дисципліна: {discipline.DisciplineName}", "Discipline", discipline.DisciplineId);
            return discipline;
        }

        public async Task<IEnumerable<Discipline>> GetAllDisciplinesAsync()
        {
            return await _disciplineRepo.GetAllAsync();
        }

        public async Task<bool> EnrollStudentToCourseAsync(int studentId, int courseId, int groupId)
        {
            var enrollment = new CourseEnrollment
            {
                CourseId = courseId,
                StudentId = studentId,
                GroupId = groupId
            };

            _enrollmentRepo.Add(enrollment);
            await _enrollmentRepo.SaveAsync();

            await _auditService.LogActionAsync(null, "Enroll", $"Зарахування студента {studentId}", "CourseEnrollment", enrollment.EnrollmentId);
            return true;
        }

        public async Task<IEnumerable<CourseEnrollment>> GetCourseParticipantsAsync(int courseId)
        {
            return await _enrollmentRepo.GetByCourseIdAsync(courseId);
        }
    }
}