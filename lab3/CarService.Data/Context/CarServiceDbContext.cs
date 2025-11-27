using CarService.Core.Models;
using CarService.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarService.Core.Context
{
    public class CarServiceDbContext : DbContext
    {
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Car> Cars => Set<Car>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<SparePart> SpareParts => Set<SparePart>();
        public DbSet<RepairOrder> RepairOrders => Set<RepairOrder>();
        public DbSet<PartRequest> PartRequest => Set<PartRequest>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<RepairJob> RepairJobs => Set<RepairJob>();
        public DbSet<Payment> Payments => Set<Payment>();


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Server=localhost,1433;Database=CarService;User Id=sa;Password=MyStr0ng!Passw0rd;TrustServerCertificate=True;",
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "meta")
            );
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.SeedOurData();

            modelBuilder.Entity<Car>()
                .HasOne(c => c.Customer)
                .WithMany(cu => cu.Cars)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RepairOrder>()
                .HasOne(ro => ro.Customer)
                .WithMany(cu => cu.RepairOrders)
                .HasForeignKey(ro => ro.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RepairOrder>()
                .HasOne(ro => ro.Car)
                .WithMany(c => c.RepairOrders)
                .HasForeignKey(ro => ro.VIN)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Manager)
                .WithMany(m => m.Subordinates)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RepairOrder>()
                .HasOne(ro => ro.ResponsibleEmployee)
                .WithMany(e => e.ResponsibleOrders)
                .HasForeignKey(ro => ro.ResponsibleEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SparePart>()
                .HasOne(sp => sp.Supplier)
                .WithMany(su => su.SpareParts)
                .HasForeignKey(sp => sp.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PartRequest>()
                .HasOne(pr => pr.RepairOrder)
                .WithMany(ro => ro.PartRequests)
                .HasForeignKey(pr => pr.RepairOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PartRequest>()
                .HasOne(pr => pr.SparePart)
                .WithMany(sp => sp.PartRequests)
                .HasForeignKey(pr => pr.PartId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.RepairOrder)
                .WithMany(ro => ro.Payments)
                .HasForeignKey(p => p.RepairOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RepairJob>()
                .HasOne(rj => rj.RepairOrder)
                .WithMany(ro => ro.RepairJobs)
                .HasForeignKey(rj => rj.RepairOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RepairJob>()
                .HasOne(rj => rj.Service)
                .WithMany(s => s.RepairJobs)
                .HasForeignKey(rj => rj.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RepairJob>()
                .HasOne(rj => rj.Mechanic)
                .WithMany(e => e.RepairJobs)
                .HasForeignKey(rj => rj.MechanicId)
                .OnDelete(DeleteBehavior.Restrict);
        }


    }
}
