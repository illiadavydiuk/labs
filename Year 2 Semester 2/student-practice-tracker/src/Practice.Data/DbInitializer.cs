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
            // 1. Гарантуємо, що БД створена
            context.Database.EnsureCreated();

            // Якщо база вже заповнена - виходимо
            if (context.Roles.Any()) return;

            // ==========================================
            // 1. РОЛІ
            // ==========================================
            var roleAdmin = new Role { RoleName = "Admin" };
            var roleStudent = new Role { RoleName = "Student" };
            var roleSupervisor = new Role { RoleName = "Supervisor" };
            context.Roles.AddRange(roleAdmin, roleStudent, roleSupervisor);
            context.SaveChanges();

            // ==========================================
            // 2. СТАТУСИ
            // ==========================================
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

            // ==========================================
            // 3. ДОВІДНИКИ (Кафедри, Посади)
            // ==========================================
            var deptIpz = new Department { DepartmentName = "Інженерії ПЗ" };
            var deptKn = new Department { DepartmentName = "Комп'ютерних Наук" };
            var deptKib = new Department { DepartmentName = "Кібербезпеки" };
            context.Departments.AddRange(deptIpz, deptKn, deptKib);

            var posDocent = new Position { PositionName = "Доцент" };
            var posProf = new Position { PositionName = "Професор" };
            var posAssist = new Position { PositionName = "Асистент" };
            var posSenior = new Position { PositionName = "Старший викладач" };
            context.Positions.AddRange(posDocent, posProf, posAssist, posSenior);
            context.SaveChanges();

            // ==========================================
            // 4. ДИСЦИПЛІНИ (Для зв'язку курсів і тем)
            // ==========================================
            var discWeb = new Discipline { DisciplineName = "Web-технології" };
            var discAI = new Discipline { DisciplineName = "Штучний інтелект" };
            var discMobile = new Discipline { DisciplineName = "Мобільна розробка" };
            var discSec = new Discipline { DisciplineName = "Інформаційна безпека" };
            var discMan = new Discipline { DisciplineName = "IT Менеджмент" };
            context.Disciplines.AddRange(discWeb, discAI, discMobile, discSec, discMan);
            context.SaveChanges();

            // ==========================================
            // 5. ОРГАНІЗАЦІЇ (>10 шт)
            // ==========================================
            var orgs = new List<Organization>
            {
                new Organization { Name = "SoftServe", Address = "вул. Садова 2а", Type = "External", ContactEmail = "hr@softserve.com" },
                new Organization { Name = "EPAM Systems", Address = "вул. Героїв УПА 73", Type = "External", ContactEmail = "careers@epam.com" },
                new Organization { Name = "GlobalLogic", Address = "вул. Шептицьких 10", Type = "External", ContactEmail = "jobs@globallogic.com" },
                new Organization { Name = "Ciklum", Address = "вул. Амосова 12", Type = "External", ContactEmail = "hr@ciklum.com" },
                new Organization { Name = "Genesis", Address = "вул. Кирилівська 40", Type = "External", ContactEmail = "join@gen.tech" },
                new Organization { Name = "Ajax Systems", Address = "вул. Скляренка 5", Type = "External", ContactEmail = "cv@ajax.systems" },
                new Organization { Name = "Grammarly", Address = "вул. Спортивна 1", Type = "External", ContactEmail = "jobs@grammarly.com" },
                new Organization { Name = "MacPaw", Address = "вул. Велика Васильківська 100", Type = "External", ContactEmail = "hiring@macpaw.com" },
                new Organization { Name = "PrivatBank IT", Address = "вул. Набережна Перемоги 30", Type = "External", ContactEmail = "it_hr@privatbank.ua" },
                new Organization { Name = "N-iX", Address = "вул. Стороженка 32", Type = "External", ContactEmail = "contact@n-ix.com" },
                new Organization { Name = "DataArt", Address = "вул. Хорива 3", Type = "External", ContactEmail = "hr-ua@dataart.com" },
                new Organization { Name = "Кафедра ІПЗ (Внутрішня)", Address = "Університет, корп. 1", Type = "University", ContactEmail = "ipz_dept@uni.edu" }
            };
            context.Organizations.AddRange(orgs);
            context.SaveChanges();

            // ==========================================
            // 6. СТРУКТУРА (Спеціальності та Групи)
            // ==========================================
            var spec121 = new Specialty { Code = "121", Name = "Інженерія ПЗ", DepartmentId = deptIpz.DepartmentId };
            var spec122 = new Specialty { Code = "122", Name = "Комп. Науки", DepartmentId = deptKn.DepartmentId };
            var spec125 = new Specialty { Code = "125", Name = "Кібербезпека", DepartmentId = deptKib.DepartmentId };
            context.Specialties.AddRange(spec121, spec122, spec125);
            context.SaveChanges();

            var groups = new List<StudentGroup>
            {
                new StudentGroup { GroupCode = "IPZ-31", EntryYear = 2023, SpecialtyId = spec121.SpecialtyId },
                new StudentGroup { GroupCode = "IPZ-32", EntryYear = 2023, SpecialtyId = spec121.SpecialtyId },
                new StudentGroup { GroupCode = "KN-31", EntryYear = 2023, SpecialtyId = spec122.SpecialtyId },
                new StudentGroup { GroupCode = "KB-31", EntryYear = 2023, SpecialtyId = spec125.SpecialtyId }
            };
            context.StudentGroups.AddRange(groups);
            context.SaveChanges();

            // ==========================================
            // 7. КОРИСТУВАЧІ (Пароль: 123456)
            // ==========================================
            string pass = BCrypt.Net.BCrypt.HashPassword("123456");

            // --- ADMIN ---
            var admin = new User { FirstName = "Super", LastName = "Admin", Email = "admin", PasswordHash = pass, RoleId = roleAdmin.RoleId };
            context.Users.Add(admin);

            // --- SUPERVISORS (5 викладачів) ---
            var teachers = new List<User>
            {
                new User { FirstName = "Петро", LastName = "Коваленко", Email = "t1", PasswordHash = pass, RoleId = roleSupervisor.RoleId },
                new User { FirstName = "Марія", LastName = "Бондар", Email = "t2", PasswordHash = pass, RoleId = roleSupervisor.RoleId },
                new User { FirstName = "Олег", LastName = "Шевченко", Email = "t3", PasswordHash = pass, RoleId = roleSupervisor.RoleId },
                new User { FirstName = "Ірина", LastName = "Мельник", Email = "t4", PasswordHash = pass, RoleId = roleSupervisor.RoleId },
                new User { FirstName = "Василь", LastName = "Гнатюк", Email = "t5", PasswordHash = pass, RoleId = roleSupervisor.RoleId }
            };
            context.Users.AddRange(teachers);
            context.SaveChanges(); // Щоб отримати ID

            // Профілі викладачів
            var supProfiles = new List<Supervisor>();
            for (int i = 0; i < teachers.Count; i++)
            {
                supProfiles.Add(new Supervisor
                {
                    UserId = teachers[i].UserId,
                    DepartmentId = (i % 2 == 0) ? deptIpz.DepartmentId : deptKn.DepartmentId,
                    PositionId = (i % 3) + 1, // Різні посади
                    Phone = "099-123-45-6" + i
                });
            }
            context.Supervisors.AddRange(supProfiles);

            // --- STUDENTS (15 студентів) ---
            var students = new List<User>();
            string[] fNames = { "Ілля", "Анна", "Максим", "Юлія", "Дмитро", "Олена", "Артем", "Вікторія", "Богдан", "Діана", "Назар", "Софія", "Роман", "Катерина", "Андрій" };
            string[] lNames = { "Студент", "Коваль", "Бойко", "Кравченко", "Козак", "Савченко", "Олійник", "Швець", "Поліщук", "Ткачук", "Мороз", "Марчук", "Лисенко", "Руденко", "Петренко" };

            for (int i = 0; i < 15; i++)
            {
                students.Add(new User
                {
                    FirstName = fNames[i],
                    LastName = lNames[i],
                    Email = $"student{i + 1}", // student1, student2 ...
                    PasswordHash = pass,
                    RoleId = roleStudent.RoleId
                });
            }
            context.Users.AddRange(students);
            context.SaveChanges();

            // Профілі студентів (розподіляємо по групах)
            var studProfiles = new List<Student>();
            for (int i = 0; i < students.Count; i++)
            {
                studProfiles.Add(new Student
                {
                    UserId = students[i].UserId,
                    GroupId = groups[i % groups.Count].GroupId, // Циклічно по групах
                    RecordBookNumber = $"RB-{1000 + i}"
                });
            }
            context.Students.AddRange(studProfiles);
            context.SaveChanges();

            // ==========================================
            // 8. КУРСИ (3 активних курси)
            // ==========================================
            var courseWeb = new Course
            {
                Name = "Виробнича Web-практика 2026",
                Year = 2026,
                DisciplineId = discWeb.DisciplineId, // Зв'язок з темами по WEB
                IsActive = true,
                SupervisorId = supProfiles[0].SupervisorId // Коваленко
            };

            var courseAI = new Course
            {
                Name = "Наукова практика (AI)",
                Year = 2026,
                DisciplineId = discAI.DisciplineId, // Зв'язок з темами по AI
                IsActive = true,
                SupervisorId = supProfiles[1].SupervisorId // Бондар
            };

            var courseMobile = new Course
            {
                Name = "Практика мобільних додатків",
                Year = 2026,
                DisciplineId = discMobile.DisciplineId,
                IsActive = true,
                SupervisorId = supProfiles[2].SupervisorId
            };

            context.Courses.AddRange(courseWeb, courseAI, courseMobile);
            context.SaveChanges();

            // ==========================================
            // 9. ЗАРАХУВАННЯ (Всіх студентів на Web курс, частину на AI)
            // ==========================================
            foreach (var s in studProfiles)
            {
                context.CourseEnrollments.Add(new CourseEnrollment { CourseId = courseWeb.CourseId, StudentId = s.StudentId });

                // Кожного другого запишемо ще й на AI
                if (s.StudentId % 2 == 0)
                {
                    context.CourseEnrollments.Add(new CourseEnrollment { CourseId = courseAI.CourseId, StudentId = s.StudentId });
                }
            }
            context.SaveChanges();

            // ==========================================
            // 10. ТЕМИ ПРАКТИКИ (>15 шт)
            // ==========================================
            // Важливо: DisciplineId має збігатися з курсом, щоб студент їх бачив!

            var topics = new List<InternshipTopic>();

            // Теми для Web (SoftServe, GlobalLogic, Genesis)
            topics.Add(new InternshipTopic { Title = "Розробка Web API на .NET 8", Description = "Backend development", OrganizationId = orgs[0].OrganizationId, DisciplineId = discWeb.DisciplineId, IsAvailable = true });
            topics.Add(new InternshipTopic { Title = "Frontend React Dashboard", Description = "SPA application", OrganizationId = orgs[0].OrganizationId, DisciplineId = discWeb.DisciplineId, IsAvailable = true });
            topics.Add(new InternshipTopic { Title = "E-commerce platform optimization", Description = "High load systems", OrganizationId = orgs[2].OrganizationId, DisciplineId = discWeb.DisciplineId, IsAvailable = true });
            topics.Add(new InternshipTopic { Title = "Cloud migration to Azure", Description = "DevOps tasks", OrganizationId = orgs[1].OrganizationId, DisciplineId = discWeb.DisciplineId, IsAvailable = true });
            topics.Add(new InternshipTopic { Title = "CMS Development for Media", Description = "PHP/Laravel", OrganizationId = orgs[4].OrganizationId, DisciplineId = discWeb.DisciplineId, IsAvailable = true });
            topics.Add(new InternshipTopic { Title = "Corporate Portal", Description = "Angular + Node.js", OrganizationId = orgs[9].OrganizationId, DisciplineId = discWeb.DisciplineId, IsAvailable = true });

            // Теми для AI (Grammarly, DataArt)
            topics.Add(new InternshipTopic { Title = "NLP for text analysis", Description = "Python, PyTorch", OrganizationId = orgs[6].OrganizationId, DisciplineId = discAI.DisciplineId, IsAvailable = true });
            topics.Add(new InternshipTopic { Title = "Computer Vision Security", Description = "OpenCV tasks", OrganizationId = orgs[5].OrganizationId, DisciplineId = discAI.DisciplineId, IsAvailable = true });
            topics.Add(new InternshipTopic { Title = "Predictive Banking Models", Description = "Data Science", OrganizationId = orgs[8].OrganizationId, DisciplineId = discAI.DisciplineId, IsAvailable = true });
            topics.Add(new InternshipTopic { Title = "Chatbot Development", Description = "LLM integration", OrganizationId = orgs[10].OrganizationId, DisciplineId = discAI.DisciplineId, IsAvailable = true });

            // Теми для Mobile (MacPaw, Genesis)
            topics.Add(new InternshipTopic { Title = "iOS Utility App", Description = "Swift/SwiftUI", OrganizationId = orgs[7].OrganizationId, DisciplineId = discMobile.DisciplineId, IsAvailable = true });
            topics.Add(new InternshipTopic { Title = "Android Fitness Tracker", Description = "Kotlin", OrganizationId = orgs[4].OrganizationId, DisciplineId = discMobile.DisciplineId, IsAvailable = true });
            topics.Add(new InternshipTopic { Title = "Cross-platform Delivery App", Description = "Flutter", OrganizationId = orgs[3].OrganizationId, DisciplineId = discMobile.DisciplineId, IsAvailable = true });

            // Теми кафедри (Internal)
            topics.Add(new InternshipTopic { Title = "Розробка сайту кафедри", Description = "Внутрішній проект", OrganizationId = orgs[11].OrganizationId, DisciplineId = discWeb.DisciplineId, IsAvailable = true });
            topics.Add(new InternshipTopic { Title = "Система розкладу занять", Description = "Автоматизація", OrganizationId = orgs[11].OrganizationId, DisciplineId = discWeb.DisciplineId, IsAvailable = true });

            context.InternshipTopics.AddRange(topics);
            context.SaveChanges();
        }
    }
}