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
        public DbSet<InternshipTopic> InternshipTopics => Set<InternshipTopic>();
        public DbSet<AssignmentStatus> AssignmentStatuses => Set<AssignmentStatus>();
        public DbSet<ReportStatus> ReportStatuses => Set<ReportStatus>();
        public DbSet<InternshipAssignment> InternshipAssignments => Set<InternshipAssignment>();
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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Report)
                .WithMany(r => r.Attachments)
                .HasForeignKey(a => a.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.InternshipAssignment)
                .WithMany(a => a.Reports)
                .HasForeignKey(r => r.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseEnrollment>()
                .HasOne(ce => ce.Course)
                .WithMany(c => c.CourseEnrollments)
                .HasForeignKey(ce => ce.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InternshipAssignment>()
                .HasOne(a => a.InternshipTopic)
                .WithMany(t => t.InternshipAssignments)
                .HasForeignKey(a => a.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InternshipAssignment>()
                .HasOne(a => a.Student)
                .WithMany(s => s.InternshipAssignments)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Discipline)
                .WithMany(d => d.Courses)
                .HasForeignKey(c => c.DisciplineId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InternshipTopic>()
                .HasOne(t => t.Organization)
                .WithMany(o => o.InternshipTopics)
                .HasForeignKey(t => t.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuditLog>()
                .HasOne(l => l.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
