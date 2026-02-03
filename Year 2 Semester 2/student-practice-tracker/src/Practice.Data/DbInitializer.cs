using System;
using System.Collections.Generic;
using System.Linq;
using Practice.Data.Context;
using Practice.Data.Entities;

namespace Practice.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Roles.Any()) return;

            // 1. РОЛІ
            var roleAdmin = new Role { RoleName = "Admin" };
            var roleStudent = new Role { RoleName = "Student" };
            var roleSupervisor = new Role { RoleName = "Supervisor" };
            context.Roles.AddRange(roleAdmin, roleStudent, roleSupervisor);
            context.SaveChanges();

            // 2. СТАТУСИ
            context.AssignmentStatuses.AddRange(
                new AssignmentStatus { StatusName = "Призначено" },
                new AssignmentStatus { StatusName = "В процесі" },
                new AssignmentStatus { StatusName = "Завершено" }
            );

            context.ReportStatuses.AddRange(
                new ReportStatus { StatusName = "На перевірці" },
                new ReportStatus { StatusName = "Повернуто на доопрацювання" },
                new ReportStatus { StatusName = "Прийнято" }
            );
            context.SaveChanges();

            // 3. ДОВІДНИКИ
            var depts = new List<Department> {
                new Department { DepartmentName = "Інженерії ПЗ" },
                new Department { DepartmentName = "Комп'ютерних Наук" },
                new Department { DepartmentName = "Кібербезпеки" }
            };
            context.Departments.AddRange(depts);

            var positions = new List<Position> {
                new Position { PositionName = "Доцент" },
                new Position { PositionName = "Професор" },
                new Position { PositionName = "Старший викладач" }
            };
            context.Positions.AddRange(positions);
            context.SaveChanges();

            var disciplines = new List<Discipline> {
                new Discipline { DisciplineName = "Web-технології" },
                new Discipline { DisciplineName = "Штучний інтелект" },
                new Discipline { DisciplineName = "Мобільна розробка" }
            };
            context.Disciplines.AddRange(disciplines);
            context.SaveChanges();

            // 4. ОРГАНІЗАЦІЇ
            var orgs = new List<Organization> {
                new Organization {
                    Name = "SoftServe",
                    Address = "вул. Садова 2а",
                    Type = "External",
                    ContactEmail = "hr@softserve.com"
                },
                new Organization {
                    Name = "EPAM Systems",
                    Address = "вул. Героїв УПА 73",
                    Type = "External",
                    ContactEmail = "careers@epam.com"
                },
                new Organization {
                    Name = "Кафедра ІПЗ",
                    Address = "Корпус 1",
                    Type = "University",
                    ContactEmail = "ipz_dept@uni.edu" 
                }
            };
            context.Organizations.AddRange(orgs);
            context.SaveChanges(); 
            // 5. СПЕЦІАЛЬНОСТІ ТА ГРУПИ
            var spec121 = new Specialty { Code = "121", Name = "Інженерія ПЗ", DepartmentId = depts[0].DepartmentId };
            context.Specialties.Add(spec121);
            context.SaveChanges();

            var group = new StudentGroup { GroupCode = "ІПЗ-31", EntryYear = 2023, SpecialtyId = spec121.SpecialtyId };
            context.StudentGroups.Add(group);
            context.SaveChanges();

            // 6. КОРИСТУВАЧІ
            string pass = BCrypt.Net.BCrypt.HashPassword("123456");
            var admin = new User { FirstName = "Андрій", LastName = "Адмін", Email = "admin", PasswordHash = pass, RoleId = 1 };
            var teacher = new User { FirstName = "Петро", LastName = "Коваленко", Email = "t1", PasswordHash = pass, RoleId = 3 };
            var studUser = new User { FirstName = "Іван", LastName = "Студент", Email = "s1", PasswordHash = pass, RoleId = 2, IsPasswordChangeRequired = true };

            context.Users.AddRange(admin, teacher, studUser);
            context.SaveChanges();

            var supervisor = new Supervisor { UserId = teacher.UserId, DepartmentId = depts[0].DepartmentId, PositionId = positions[0].PositionId };
            context.Supervisors.Add(supervisor);
            context.SaveChanges();

            var student = new Student { UserId = studUser.UserId, GroupId = group.GroupId, RecordBookNumber = "BC-101" };
            context.Students.Add(student);
            context.SaveChanges();

            // 7. КУРСИ ТА ТЕМИ
            var course = new Course { Name = "Практика 2026", Year = 2026, DisciplineId = disciplines[0].DisciplineId, IsActive = true, SupervisorId = supervisor.SupervisorId };
            context.Courses.Add(course);
            context.SaveChanges();

            var topic = new InternshipTopic
            {
                Title = "Розробка на .NET",
                Description = "Опис теми",
                OrganizationId = orgs[0].OrganizationId,
                DisciplineId = disciplines[0].DisciplineId,
                IsAvailable = true
            };
            context.InternshipTopics.Add(topic);
            context.SaveChanges();

            // 8. ПРИЗНАЧЕННЯ ТА ЗВІТИ
            var assign = new InternshipAssignment
            {
                StudentId = student.StudentId,
                CourseId = course.CourseId,
                TopicId = topic.TopicId,
                SupervisorId = supervisor.SupervisorId,
                StatusId = 2 // В процесі
            };
            context.InternshipAssignments.Add(assign);
            context.SaveChanges();

            context.Reports.Add(new Report
            {
                AssignmentId = assign.AssignmentId,
                SubmissionDate = DateTime.Now,
                StatusId = 1 // На перевірці
            });

            context.SaveChanges();
        }
    }
}