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
