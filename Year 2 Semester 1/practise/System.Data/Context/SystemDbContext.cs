using Microsoft.EntityFrameworkCore;
using TaskManager.Data.Entities;
using Task = TaskManager.Data.Entities.Task;

namespace TaskManager.Data.Context
{
    public class SystemDbContext : DbContext
    {
        public DbSet<Developer> Developers => Set<Developer>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Task> Tasks => Set<Task>();

        public SystemDbContext() { }

        public SystemDbContext(DbContextOptions<SystemDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=localhost,1433;Database=TaskManagerDB;User Id=sa;Password=MyStr0ng!Passw0rd;TrustServerCertificate=True;",
                    sql => sql.EnableRetryOnFailure() // Додає стійкість при короткочасних збоях
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Task>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Developers)
                .WithMany(d => d.Projects)
                .UsingEntity<Dictionary<string, object>>(
                    "ProjectDevelopers", // Ім'я проміжної таблиці
                    j => j.HasOne<Developer>().WithMany().HasForeignKey("DeveloperId").OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<Project>().WithMany().HasForeignKey("ProjectId").OnDelete(DeleteBehavior.Cascade)
                );

            modelBuilder.Entity<Task>()
                .HasMany(t => t.Developers)
                .WithMany(d => d.Tasks)
                .UsingEntity<Dictionary<string, object>>(
                    "TaskDevelopers", // Ім'я проміжної таблиці
                    j => j.HasOne<Developer>().WithMany().HasForeignKey("DeveloperId").OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<Task>().WithMany().HasForeignKey("TaskId").OnDelete(DeleteBehavior.Cascade)
                );
        }
    }
}