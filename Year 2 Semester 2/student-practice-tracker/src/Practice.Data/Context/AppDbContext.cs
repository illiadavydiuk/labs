using Microsoft.EntityFrameworkCore;
using Practice.Data.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Practice.Data.Context
{
    public class AppDbContext : DbContext
    {
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Specialty> Specialties => Set<Specialty>();
        public DbSet<StudentGroup> StudentGroups => Set<StudentGroup>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Position> Positions => Set<Position>();
        public DbSet<Supervisor> Supervisors => Set<Supervisor>();
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<InternshipTopic> IntershipTopics => Set<InternshipTopic>();
        public DbSet<AssignmentStatus> AssignmentStatuses => Set<AssignmentStatus>();
        public DbSet<ReportStatus> ReportStatuses => Set<ReportStatus>();
        public DbSet<InternshipAssignment> IntershipAssignments => Set<InternshipAssignment>();
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Discipline> Disciplines => Set<Discipline>();
        public DbSet<Attachment> Attachments => Set<Attachment>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string? path = null;
            string folderName = "StudentPracticePlatform";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    folderName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".local/share", folderName);
            }

            if (path != null)
            {
                Directory.CreateDirectory(path);

                string dbPath = Path.Combine(path, "practice_platform.db");

                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }

            base.OnConfiguring(optionsBuilder);
        }
    }
}
