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
            // Створення бази даних, якщо вона не існує
            context.Database.EnsureCreated();

            // Якщо база вже заповнена (є ролі), виходимо
            if (context.Roles.Any()) return;

            // 1. РОЛІ
            var roleAdmin = new Role { RoleName = "Admin" };
            var roleStudent = new Role { RoleName = "Student" };
            var roleSupervisor = new Role { RoleName = "Supervisor" };
            context.Roles.AddRange(roleAdmin, roleStudent, roleSupervisor);
            context.SaveChanges();

            // 2. СТАТУСИ (Статуси призначень та звітів)
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

            // 3. ДОВІДНИКИ (Кафедри, Посади, Дисципліни)
            var depts = new List<Department> {
                new Department { DepartmentName = "Інженерії ПЗ" },
                new Department { DepartmentName = "Комп'ютерних Наук" },
                new Department { DepartmentName = "Кібербезпеки" }
            };
            context.Departments.AddRange(depts);

            var positions = new List<Position> {
                new Position { PositionName = "Доцент" },
                new Position { PositionName = "Професор" },
                new Position { PositionName = "Старший викладач" },
                new Position { PositionName = "Асистент" }
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
                new Organization { Name = "SoftServe", Address = "вул. Садова 2а", Type = "External", ContactEmail = "hr@softserve.com" },
                new Organization { Name = "EPAM Systems", Address = "вул. Героїв УПА 73", Type = "External", ContactEmail = "careers@epam.com" },
                new Organization { Name = "Sigma Software", Address = "вул. Наукова 7", Type = "External", ContactEmail = "info@sigma.com" },
                new Organization { Name = "GlobalLogic", Address = "вул. Грінченка 2", Type = "External", ContactEmail = "jobs@globallogic.com" },
                new Organization { Name = "Кафедра ІПЗ", Address = "Корпус 1, ауд. 201", Type = "University", ContactEmail = "ipz_dept@test.com" }
            };
            context.Organizations.AddRange(orgs);
            context.SaveChanges();

            // 5. СПЕЦІАЛЬНОСТІ ТА ГРУПИ
            var spec121 = new Specialty { Code = "121", Name = "Інженерія ПЗ", DepartmentId = depts[0].DepartmentId };
            var spec122 = new Specialty { Code = "122", Name = "Комп'ютерні Науки", DepartmentId = depts[1].DepartmentId };
            context.Specialties.AddRange(spec121, spec122);
            context.SaveChanges();

            var groups = new List<StudentGroup> {
                new StudentGroup { GroupCode = "ІПЗ-31", EntryYear = 2023, SpecialtyId = spec121.SpecialtyId },
                new StudentGroup { GroupCode = "КН-31", EntryYear = 2023, SpecialtyId = spec122.SpecialtyId }
            };
            context.StudentGroups.AddRange(groups);
            context.SaveChanges();

            // 6. КОРИСТУВАЧІ (Адмін та Викладачі)
            string hashedPass = BCrypt.Net.BCrypt.HashPassword("123456");

            var admin = new User { FirstName = "Андрій", LastName = "Адмін", Email = "admin@test.com", PasswordHash = hashedPass, RoleId = roleAdmin.RoleId, CreatedAt = DateTime.Now };
            var t1 = new User { FirstName = "Петро", LastName = "Коваленко", Email = "t1@test.com", PasswordHash = hashedPass, RoleId = roleSupervisor.RoleId, CreatedAt = DateTime.Now };
            var t2 = new User { FirstName = "Олена", LastName = "Світла", Email = "t2@test.com", PasswordHash = hashedPass, RoleId = roleSupervisor.RoleId, CreatedAt = DateTime.Now };

            context.Users.AddRange(admin, t1, t2);
            context.SaveChanges();

            var supervisor1 = new Supervisor { UserId = t1.UserId, DepartmentId = depts[0].DepartmentId, PositionId = positions[0].PositionId, Phone = "+380671112233" };
            var supervisor2 = new Supervisor { UserId = t2.UserId, DepartmentId = depts[1].DepartmentId, PositionId = positions[1].PositionId, Phone = "+380504445566" };
            context.Supervisors.AddRange(supervisor1, supervisor2);
            context.SaveChanges();

            // 7. СТУДЕНТИ (10 осіб)
            var studentList = new List<(string F, string L, string Email, string Record, int GroupIdx)>
            {
                ("Іван", "Мельник", "s1@test.com", "BC-101", 0),
                ("Марія", "Ковальчук", "s2@test.com", "BC-102", 0),
                ("Олександр", "Петренко", "s3@test.com", "BC-103", 0),
                ("Олена", "Шевченко", "s4@test.com", "BC-104", 0),
                ("Максим", "Бондаренко", "s5@test.com", "BC-105", 0),
                ("Юлія", "Лисенко", "s6@test.com", "BC-106", 1),
                ("Дмитро", "Василенко", "s7@test.com", "BC-107", 1),
                ("Тетяна", "Руденко", "s8@test.com", "BC-108", 1),
                ("Артем", "Кравченко", "s9@test.com", "BC-109", 1),
                ("Анастасія", "Поліщук", "s10@test.com", "BC-110", 1)
            };

            foreach (var s in studentList)
            {
                var user = new User
                {
                    FirstName = s.F,
                    LastName = s.L,
                    Email = s.Email,
                    PasswordHash = hashedPass,
                    RoleId = roleStudent.RoleId,
                    CreatedAt = DateTime.Now,
                    IsPasswordChangeRequired = true
                };
                context.Users.Add(user);
                context.SaveChanges();

                context.Students.Add(new Student
                {
                    UserId = user.UserId,
                    GroupId = groups[s.GroupIdx].GroupId,
                    RecordBookNumber = s.Record
                });
            }
            context.SaveChanges();

            // 8. КУРСИ
            var courses = new List<Course> {
                new Course { Name = "Виробнича практика 2026", Year = 2026, DisciplineId = disciplines[0].DisciplineId, IsActive = true, SupervisorId = supervisor1.SupervisorId },
                new Course { Name = "Навчальна практика (КН) 2026", Year = 2026, DisciplineId = disciplines[1].DisciplineId, IsActive = true, SupervisorId = supervisor2.SupervisorId }
            };
            context.Courses.AddRange(courses);
            context.SaveChanges();

            // 9. ТЕМИ ПРАКТИКИ (10 тем)
            var topics = new List<InternshipTopic> {
                new InternshipTopic { Title = "Розробка Backend на ASP.NET Core", Description = "Проектування REST API", OrganizationId = orgs[0].OrganizationId, DisciplineId = disciplines[0].DisciplineId, IsAvailable = true },
                new InternshipTopic { Title = "Frontend на React та TypeScript", Description = "Створення сучасних UI компонентів", OrganizationId = orgs[1].OrganizationId, DisciplineId = disciplines[0].DisciplineId, IsAvailable = true },
                new InternshipTopic { Title = "Автоматизація QA процесів", Description = "Написання автотестів на Selenium", OrganizationId = orgs[2].OrganizationId, DisciplineId = disciplines[0].DisciplineId, IsAvailable = true },
                new InternshipTopic { Title = "Аналіз великих даних (Data Science)", Description = "Обробка датасетів на Python", OrganizationId = orgs[0].OrganizationId, DisciplineId = disciplines[1].DisciplineId, IsAvailable = true },
                new InternshipTopic { Title = "Впровадження AI моделей у бізнес", Description = "Робота з LLM та OpenAI API", OrganizationId = orgs[3].OrganizationId, DisciplineId = disciplines[1].DisciplineId, IsAvailable = true },
                new InternshipTopic { Title = "Розробка мобільних додатків на Flutter", Description = "Кросплатформна мобільна розробка", OrganizationId = orgs[2].OrganizationId, DisciplineId = disciplines[2].DisciplineId, IsAvailable = true },
                new InternshipTopic { Title = "Адміністрування хмарних систем", Description = "Налаштування AWS та Azure сервісів", OrganizationId = orgs[1].OrganizationId, DisciplineId = disciplines[2].DisciplineId, IsAvailable = true },
                new InternshipTopic { Title = "Система електронного документообігу кафедри", Description = "Проєкт для внутрішніх потреб університету", OrganizationId = orgs[4].OrganizationId, DisciplineId = disciplines[0].DisciplineId, IsAvailable = true },
                new InternshipTopic { Title = "Кібербезпека корпоративних мереж", Description = "Аналіз вразливостей та Firewall", OrganizationId = orgs[4].OrganizationId, DisciplineId = disciplines[0].DisciplineId, IsAvailable = true },
                new InternshipTopic { Title = "Машинне навчання в медицині", Description = "Аналіз медичних зображень", OrganizationId = orgs[3].OrganizationId, DisciplineId = disciplines[1].DisciplineId, IsAvailable = true }
            };
            context.InternshipTopics.AddRange(topics);
            context.SaveChanges();
        }
    }
}