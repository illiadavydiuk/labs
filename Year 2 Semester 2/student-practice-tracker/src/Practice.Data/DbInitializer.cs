using Practice.Data.Context;
using Practice.Data.Entities;
using System.Linq;
using BCrypt.Net; 

namespace Practice.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { RoleName = "Admin" },
                    new Role { RoleName = "Student" },
                    new Role { RoleName = "Supervisor" }
                );
                context.SaveChanges();
            }

            if (!context.AssignmentStatuses.Any())
            {
                context.AssignmentStatuses.AddRange(
                    new AssignmentStatus { StatusName = "Assigned" },    // ID 1
                    new AssignmentStatus { StatusName = "InProgress" },  // ID 2
                    new AssignmentStatus { StatusName = "Completed" },   // ID 3
                    new AssignmentStatus { StatusName = "Failed" }       // ID 4
                );
                context.SaveChanges();
            }

            if (!context.ReportStatuses.Any())
            {
                context.ReportStatuses.AddRange(
                    new ReportStatus { StatusName = "Submitted" },  // ID 1 (Подано)
                    new ReportStatus { StatusName = "Approved" },   // ID 2 (Прийнято)
                    new ReportStatus { StatusName = "Rejected" },   // ID 3 (Відхилено)
                    new ReportStatus { StatusName = "NeedsRevision" } // ID 4 (На доопрацювання)
                );
                context.SaveChanges();
            }

            if (!context.Users.Any(u => u.Email == "admin@admin.com"))
            {
                var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");

                if (adminRole != null)
                {
                    var adminUser = new User
                    {
                        Email = "admin@admin.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                        FirstName = "System",
                        LastName = "Administrator",
                        RoleId = adminRole.RoleId,
                        CreatedAt = System.DateTime.UtcNow
                    };

                    context.Users.Add(adminUser);
                    context.SaveChanges();
                }
            }
        }
    }
}