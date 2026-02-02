using Practice.Data.Entities;
using Practice.Repositories.Interfaces;
using Practice.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Practice.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepo;
        private readonly IDisciplineRepository _discRepo;
        private readonly ICourseEnrollmentRepository _enrollRepo;
        private readonly IAuditService _auditService;

        public CourseService(
            ICourseRepository courseRepo,
            IDisciplineRepository discRepo,
            ICourseEnrollmentRepository enrollRepo,
            IAuditService auditService)
        {
            _courseRepo = courseRepo;
            _discRepo = discRepo;
            _enrollRepo = enrollRepo;
            _auditService = auditService;
        }

        public async Task<IEnumerable<Course>> GetAllActiveCoursesAsync()
        {
            return await _courseRepo.GetActiveCoursesAsync();
        }

        public async Task<IEnumerable<Discipline>> GetAllDisciplinesAsync()
        {
            return await _discRepo.GetAllAsync();
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            if (string.IsNullOrWhiteSpace(course.Name)) throw new ArgumentException("Назва курсу обов'язкова");

            _courseRepo.Add(course);
            await _courseRepo.SaveAsync();

            await _auditService.LogActionAsync(null, "Create", $"Створено курс: {course.Name}", "Course", course.CourseId);
            return course;
        }

        public async Task UpdateCourseAsync(Course course)
        {
            var existing = await _courseRepo.GetByIdAsync(course.CourseId);
            if (existing != null)
            {
                existing.Name = course.Name;
                existing.Year = course.Year;
                existing.DisciplineId = course.DisciplineId;
                existing.SupervisorId = course.SupervisorId;
                existing.IsActive = course.IsActive;

                _courseRepo.Update(existing);
                await _courseRepo.SaveAsync();
            }
        }

        public async Task DeleteCourseAsync(int courseId)
        {
            var course = await _courseRepo.GetByIdAsync(courseId);
            if (course != null)
            {
                course.IsActive = false;
                _courseRepo.Update(course);
                await _courseRepo.SaveAsync();
            }
        }

        public async Task AddDisciplineAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            var disc = new Discipline { DisciplineName = name };
            _discRepo.Add(disc);
            await _discRepo.SaveAsync();
        }

        public async Task<bool> EnrollStudentToCourseAsync(int studentId, int courseId, int? groupId)
        {
            var all = await _enrollRepo.GetAllAsync();
            bool exists = all.Any(e => e.CourseId == courseId &&
                                       ((studentId > 0 && e.StudentId == studentId) ||
                                        (groupId.HasValue && e.GroupId == groupId.Value)));

            if (!exists)
            {
                var enroll = new CourseEnrollment
                {
                    StudentId = studentId > 0 ? studentId : null,
                    CourseId = courseId,
                    GroupId = groupId
                };

                _enrollRepo.Add(enroll);
                await _enrollRepo.SaveAsync();
                return true;
            }
            return false;
        }

        public async Task UnenrollGroupFromCourseAsync(int courseId, int groupId)
        {
            var allEnrollments = await _enrollRepo.GetAllAsync();
            var toDelete = allEnrollments.Where(e => e.CourseId == courseId && e.GroupId == groupId).ToList();

            foreach (var item in toDelete)
            {
                _enrollRepo.Delete(item);
            }

            if (toDelete.Any())
            {
                await _enrollRepo.SaveAsync();
            }
        }
    }
}