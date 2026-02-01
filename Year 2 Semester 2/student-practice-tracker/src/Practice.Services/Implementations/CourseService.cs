using Microsoft.EntityFrameworkCore;
using Practice.Data.Context;
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

        public async Task<IEnumerable<Course>> GetAllActiveCoursesAsync()
        {
            return await _courseRepo.GetAllActiveAsync();
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            if (string.IsNullOrWhiteSpace(course.Name)) throw new ArgumentException("Назва курсу обов'язкова");
            _courseRepo.Add(course);
            await _courseRepo.SaveAsync();
            await _auditService.LogActionAsync(null, "Create", $"Створено курс: {course.Name}", "Course", course.CourseId);
            return course;
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            return await _courseRepo.GetByIdAsync(id);
        }

        public async Task UpdateCourseAsync(Course course)
        {
            using (var context = new AppDbContext())
            {
                var existingCourse = await context.Courses.FindAsync(course.CourseId);

                if (existingCourse != null)
                {
                    existingCourse.Name = course.Name;
                    existingCourse.Year = course.Year;

                    existingCourse.DisciplineId = course.DisciplineId;
                    existingCourse.SupervisorId = course.SupervisorId;

                    context.Entry(existingCourse).State = EntityState.Modified;

                    await context.SaveChangesAsync();

                    if (_auditService != null)
                    {
                        await _auditService.LogActionAsync(null, "Update", $"Оновлено курс ID {course.CourseId}: {course.Name}", "Course", course.CourseId);
                    }
                }
                else
                {
                    throw new Exception($"Курс з ID {course.CourseId} не знайдено в базі даних.");
                }
            }
        }

        public async Task DeleteCourseAsync(int id)
        {
            using var context = new AppDbContext();
            var item = await context.Courses.FindAsync(id);
            if (item != null)
            {
                string name = item.Name;
                context.Courses.Remove(item);
                await context.SaveChangesAsync();
                await _auditService.LogActionAsync(null, "Delete", $"Видалено курс: {name}", "Course", id);
            }
        }

        public async Task<bool> EnrollStudentToCourseAsync(int studentId, int courseId, int? groupId)
        {
            var exists = await _enrollmentRepo.IsEnrolledAsync(studentId, courseId);
            if (exists) return false;

            var enrollment = new CourseEnrollment
            {
                StudentId = studentId,
                CourseId = courseId
            };
            _enrollmentRepo.Add(enrollment);
            await _enrollmentRepo.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<Discipline>> GetAllDisciplinesAsync()
        {
            return await _disciplineRepo.GetAllAsync();
        }

        public async Task AddDisciplineAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Назва дисципліни обов'язкова");
            var d = new Discipline { DisciplineName = name };
            _disciplineRepo.Add(d);
            await _disciplineRepo.SaveAsync();
            await _auditService.LogActionAsync(null, "Create", $"Створено дисципліну: {name}", "Discipline", d.DisciplineId);
        }
    }
}