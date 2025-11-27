using CarService.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CarService.Core.Extensions
{
    public static class ModelBuilderExtension
    {
        public static void SeedOurData(this ModelBuilder modelBuilder)
        {
            SeedCustomers(modelBuilder);
            SeedEmployees(modelBuilder);
            SeedSuppliers(modelBuilder);
            SeedSpareParts(modelBuilder);
            SeedCars(modelBuilder);
            SeedRepairOrders(modelBuilder);
            SeedPartRequests(modelBuilder);
            SeedServices(modelBuilder);
            SeedRepairJobs(modelBuilder);
            SeedPayments(modelBuilder);
        }

        private static void SeedCustomers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, FirstName = "Andrii", LastName = "Shevchenko", Phone = "0671112233" },
                new Customer { CustomerId = 2, FirstName = "Maria", LastName = "Koval", Phone = "0975557788" }
            );
        }

        private static void SeedEmployees(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    EmployeeId = 1,
                    FirstName = "Ivan",
                    LastName = "Petrenko",
                    Position = "Mechanic",
                    Specialization = "Suspension",
                    Salary = 22000m,
                    ManagerId = null
                },
                new Employee
                {
                    EmployeeId = 2,
                    FirstName = "Oleh",
                    LastName = "Kryvonos",
                    Position = "Electrician",
                    Specialization = "Electrics",
                    Salary = 24000m,
                    ManagerId = 1
                },
                new Employee
                {
                    EmployeeId = 3,
                    FirstName = "Taras",
                    LastName = "Melnyk",
                    Position = "Diagnostics",
                    Specialization = "Engine",
                    Salary = 26000m,
                    ManagerId = 1
                }
            );
        }

        private static void SeedSuppliers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Supplier>().HasData(
                new Supplier { SupplierId = 1, SupplierName = "AutoParts UA", Phone = "0441234567" },
                new Supplier { SupplierId = 2, SupplierName = "MotoDetail", Phone = "0509988776" }
            );
        }

        private static void SeedSpareParts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SparePart>().HasData(
                new SparePart
                {
                    PartId = 1,
                    SupplierId = 1,
                    PartName = "Brake Disc",
                    PartNumber = "BD-123",
                    Quality = "OEM",
                    StockQuantity = 10,
                    MinimumStock = 3,
                    Price = 1500m
                },
                new SparePart
                {
                    PartId = 2,
                    SupplierId = 2,
                    PartName = "Oil Filter",
                    PartNumber = "OF-52",
                    Quality = "Aftermarket",
                    StockQuantity = 25,
                    MinimumStock = 5,
                    Price = 300m
                }
            );
        }

        private static void SeedCars(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>().HasData(
                new Car
                {
                    VIN = "VIN00000000000001",
                    CustomerId = 1,
                    Make = "Toyota",
                    Model = "Corolla",
                    Year = 2015,
                    Engine = "1.6",
                    Mileage = 180000
                },
                new Car
                {
                    VIN = "VIN00000000000002",
                    CustomerId = 2,
                    Make = "Honda",
                    Model = "Civic",
                    Year = 2018,
                    Engine = "2.0",
                    Mileage = 120000
                }
            );
        }

        private static void SeedRepairOrders(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RepairOrder>().HasData(
                new RepairOrder
                {
                    RepairOrderId = 1,
                    VIN = "VIN00000000000001",
                    CustomerId = 1,
                    ResponsibleEmployeeId = 1,
                    CreatedDate = new DateTime(2024, 12, 1),
                    Status = "InProgress",
                    ComplaintDescription = "Vibration when braking",
                    TotalEstimate = 3200m,
                    DiagnosticSummary = "Brake disc wear detected"
                },
                new RepairOrder
                {
                    RepairOrderId = 2,
                    VIN = "VIN00000000000002",
                    CustomerId = 2,
                    ResponsibleEmployeeId = 2,
                    CreatedDate = new DateTime(2024, 12, 3),
                    Status = "WaitingParts",
                    ComplaintDescription = "Oil leak observed",
                    TotalEstimate = 2400m,
                    DiagnosticSummary = "Oil filter housing crack"
                }
            );
        }

        private static void SeedPartRequests(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PartRequest>().HasData(
                new PartRequest
                {
                    RequestId = 1,
                    RepairOrderId = 1,
                    PartId = 1,
                    Quantity = 2,
                    Status = "Ordered",
                    Urgent = false,
                    ExpectedDeliveryDate = new DateTime(2024, 12, 5),
                    PriceAtOrder = 1500m,
                    ReceivedDate = null
                },
                new PartRequest
                {
                    RequestId = 2,
                    RepairOrderId = 2,
                    PartId = 2,
                    Quantity = 1,
                    Status = "Ordered",
                    Urgent = true,
                    ExpectedDeliveryDate = new DateTime(2024, 12, 4),
                    PriceAtOrder = 300m,
                    ReceivedDate = null
                }
            );
        }

        private static void SeedServices(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Service>().HasData(
                new Service { ServiceId = 1, ServiceName = "Brake Replacement", BasePrice = 800m, EstimatedHours = 2, Category = "Brakes" },
                new Service { ServiceId = 2, ServiceName = "Oil Change", BasePrice = 400m, EstimatedHours = 1, Category = "Maintenance" }
            );
        }

        private static void SeedRepairJobs(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RepairJob>().HasData(
                new RepairJob
                {
                    RepairJobId = 1,
                    RepairOrderId = 1,
                    ServiceId = 1,
                    MechanicId = 1,
                    ActualHours = 2,
                    Price = 800m
                },
                new RepairJob
                {
                    RepairJobId = 2,
                    RepairOrderId = 2,
                    ServiceId = 2,
                    MechanicId = 2,
                    ActualHours = 1,
                    Price = 400m
                }
            );
        }

        private static void SeedPayments(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>().HasData(
                new Payment
                {
                    PaymentId = 1,
                    RepairOrderId = 1,
                    Amount = 2000m,
                    PaymentDate = new DateTime(2024, 12, 2),
                    PaymentMethod = "Card",
                    Comment = "Advance payment",
                    InvoiceNumber = "INV-001",
                    IsRefund = false
                },
                new Payment
                {
                    PaymentId = 2,
                    RepairOrderId = 2,
                    Amount = 500m,
                    PaymentDate = new DateTime(2024, 12, 3),
                    PaymentMethod = "Cash",
                    Comment = "Partial payment",
                    InvoiceNumber = "INV-002",
                    IsRefund = false
                }
            );
        }
    }
}
