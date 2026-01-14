using Microsoft.EntityFrameworkCore;
using DuckNet.Data.Entities;

namespace DuckNet.Data.Context
{
    public class DuckNetDbContext : DbContext
    {
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<ScanSession> ScanSessions => Set<ScanSession>();
        public DbSet<NetworkEvent> NetworkEvents => Set<NetworkEvent>();
        public DbSet<AdapterProfile> AdapterProfiles => Set<AdapterProfile>();

        public DuckNetDbContext() { }

        public DuckNetDbContext(DbContextOptions<DuckNetDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=localhost,1433;Database=DuckNetDB;User Id=sa;Password=MyStr0ng!Passw0rd;TrustServerCertificate=True;",
                    sql => sql.EnableRetryOnFailure() 
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<NetworkEvent>()
                .HasOne(e => e.Device)          // У події є один Пристрій
                .WithMany(d => d.Events)        // У пристрою є багато Подій
                .HasForeignKey(e => e.DeviceId) // Зовнішній ключ
                .OnDelete(DeleteBehavior.Cascade); // Якщо видаляємо пристрій -> видаляємо його історію
        }
    }
}