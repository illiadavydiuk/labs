using CarService.Core.Context;
using CarService.Core.Models;
using System;
using System.Linq;

namespace CarService.Core.Initialization
{
    public static class DbInitializer
    {
        public static void SeedData(CarServiceDbContext context)
        {
            if (context.Customers.Any()) return;

            // 1. CUSTOMERS
            var customers = new[]
            {
                new Customer { FirstName="Ivan", LastName="Ivanov", Phone="0931112233" },
                new Customer { FirstName="Oksana", LastName="Shevchenko", Phone="0679988776" },
                new Customer { FirstName="Petro", LastName="Savchuk", Phone="0501239876" },
                new Customer { FirstName="Anna", LastName="Kostenko", Phone="0994455123" },
                new Customer { FirstName="Dmytro", LastName="Bondarenko", Phone="0682345522" },
                new Customer { FirstName="Yulia", LastName="Vasylkiv", Phone="0639987123" },
                new Customer { FirstName="Serhii", LastName="Kochubey", Phone="0967702211" },
                new Customer { FirstName="Roman", LastName="Melnyk", Phone="0936789912" }
            };
            context.Customers.AddRange(customers);
            context.SaveChanges();

            // 2. EMPLOYEES (ManagerId = null)
            var employees = new[]
            {
                new Employee { FirstName="Serhii", LastName="Lysenko", Position="Mechanic", Specialization="Engine", Salary=22000, ManagerId=null },
                new Employee { FirstName="Maksym", LastName="Hnat", Position="Electrician", Specialization="Electrical", Salary=24000, ManagerId=null },
                new Employee { FirstName="Olena", LastName="Rudyk", Position="Master", Specialization="Diagnostics", Salary=26000, ManagerId=null },
                new Employee { FirstName="Ihor", LastName="Tkachenko", Position="Mechanic", Specialization="Suspension", Salary=21000, ManagerId=null },
                new Employee { FirstName="Bohdan", LastName="Sydorenko", Position="Mechanic", Specialization="Brakes", Salary=20500, ManagerId=null }
            };
            context.Employees.AddRange(employees);
            context.SaveChanges();

            // 3. CARS
            var cars = new[]
            {
                new Car { VIN="VIN00000000000001", CustomerId=customers[0].CustomerId, Make="Toyota", Model="Camry", Year=2018, Engine="2.5L", Mileage=150000 },
                new Car { VIN="VIN00000000000002", CustomerId=customers[1].CustomerId, Make="Honda", Model="Civic", Year=2017, Engine="1.8L", Mileage=180000 },
                new Car { VIN="VIN00000000000003", CustomerId=customers[2].CustomerId, Make="Mazda", Model="6", Year=2020, Engine="2.0L", Mileage=80000 },
                new Car { VIN="VIN00000000000004", CustomerId=customers[3].CustomerId, Make="Ford", Model="Focus", Year=2016, Engine="1.6L", Mileage=210000 },
                new Car { VIN="VIN00000000000005", CustomerId=customers[4].CustomerId, Make="BMW", Model="320", Year=2021, Engine="2.0L", Mileage=50000 },
                new Car { VIN="VIN00000000000006", CustomerId=customers[5].CustomerId, Make="Audi", Model="A4", Year=2015, Engine="2.0L", Mileage=240000 },
                new Car { VIN="VIN00000000000007", CustomerId=customers[6].CustomerId, Make="Hyundai", Model="Tucson", Year=2022, Engine="1.6L", Mileage=20000 },
                new Car { VIN="VIN00000000000008", CustomerId=customers[7].CustomerId, Make="Nissan", Model="Leaf", Year=2020, Engine="Electric", Mileage=45000 },
                new Car { VIN="VIN00000000000009", CustomerId=customers[0].CustomerId, Make="Kia", Model="Sportage", Year=2019, Engine="2.0L", Mileage=120000 },
                new Car { VIN="VIN00000000000010", CustomerId=customers[1].CustomerId, Make="Skoda", Model="Octavia", Year=2018, Engine="1.8L", Mileage=140000 }
            };
            context.Cars.AddRange(cars);
            context.SaveChanges();

            // 4. SUPPLIERS
            var suppliers = new[]
            {
                new Supplier { SupplierName="AutoParts UA", Phone="0441122334" },
                new Supplier { SupplierName="EU Parts", Phone="0445566778" },
                new Supplier { SupplierName="Premium OEM", Phone="0443322556" }
            };
            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            // 5. SPARE PARTS
            var parts = new[]
            {
                new SparePart { SupplierId=suppliers[0].SupplierId, PartName="Brake Pads", PartNumber="BP-100", Quality="OEM", StockQuantity=40, MinimumStock=10, Price=1200 },
                new SparePart { SupplierId=suppliers[1].SupplierId, PartName="Oil Filter", PartNumber="OF-20", Quality="Original", StockQuantity=60, MinimumStock=20, Price=300 },
                new SparePart { SupplierId=suppliers[1].SupplierId, PartName="Air Filter", PartNumber="AF-200", Quality="Aftermarket", StockQuantity=35, MinimumStock=10, Price=400 },
                new SparePart { SupplierId=suppliers[2].SupplierId, PartName="Spark Plug", PartNumber="SP-90", Quality="Original", StockQuantity=80, MinimumStock=20, Price=150 },
                new SparePart { SupplierId=suppliers[2].SupplierId, PartName="Suspension Arm", PartNumber="SA-550", Quality="OEM", StockQuantity=15, MinimumStock=4, Price=2400 }
            };
            context.SpareParts.AddRange(parts);
            context.SaveChanges();

            // 6. REPAIR ORDERS
            var ro = new[]
            {
                new RepairOrder { VIN=cars[0].VIN, CustomerId=customers[0].CustomerId, ResponsibleEmployeeId=employees[2].EmployeeId, CreatedDate=new DateTime(2024,9,1), Status="Open", ComplaintDescription="Engine noise", TotalEstimate=3500, DiagnosticSummary="Timing chain wear" },
                new RepairOrder { VIN=cars[1].VIN, CustomerId=customers[1].CustomerId, ResponsibleEmployeeId=employees[0].EmployeeId, CreatedDate=new DateTime(2024,9,2), Status="Open", ComplaintDescription="Brake noise", TotalEstimate=1500, DiagnosticSummary="Pads worn" },
                new RepairOrder { VIN=cars[3].VIN, CustomerId=customers[3].CustomerId, ResponsibleEmployeeId=employees[1].EmployeeId, CreatedDate=new DateTime(2024,9,5), Status="Open", ComplaintDescription="Check engine", TotalEstimate=2800, DiagnosticSummary="Sensor failure" }
            };
            context.RepairOrders.AddRange(ro);
            context.SaveChanges();

            // 7. PART REQUESTS
            var requests = new[]
            {
                new PartRequest { RepairOrderId=ro[0].RepairOrderId, PartId=parts[0].PartId, Quantity=1, Status="Ordered", Urgent=true, ExpectedDeliveryDate=new DateTime(2024,9,4), PriceAtOrder=1200 },
                new PartRequest { RepairOrderId=ro[1].RepairOrderId, PartId=parts[1].PartId, Quantity=1, Status="In Stock", Urgent=false, ExpectedDeliveryDate=new DateTime(2024,9,3), PriceAtOrder=300 },
                new PartRequest { RepairOrderId=ro[2].RepairOrderId, PartId=parts[3].PartId, Quantity=4, Status="Ordered", Urgent=false, ExpectedDeliveryDate=new DateTime(2024,9,7), PriceAtOrder=150 }
            };
            context.PartRequest.AddRange(requests);
            context.SaveChanges();

            // 8. SERVICES
            var services = new[]
            {
                new Service { ServiceName="Engine Diagnostics", BasePrice=800, EstimatedHours=2, Category="Diagnostics" },
                new Service { ServiceName="Brake Replacement", BasePrice=1500, EstimatedHours=1, Category="Mechanic" },
                new Service { ServiceName="Suspension Repair", BasePrice=2000, EstimatedHours=3, Category="Mechanic" },
                new Service { ServiceName="Oil Change", BasePrice=500, EstimatedHours=1, Category="Maintenance" },
                new Service { ServiceName="Battery Replacement", BasePrice=600, EstimatedHours=1, Category="Electrical" }
            };
            context.Services.AddRange(services);
            context.SaveChanges();

            // 9. REPAIR JOBS
            var jobs = new[]
            {
                new RepairJob { RepairOrderId=ro[0].RepairOrderId, ServiceId=services[0].ServiceId, MechanicId=employees[0].EmployeeId, ActualHours=2.3m, Price=900 },
                new RepairJob { RepairOrderId=ro[1].RepairOrderId, ServiceId=services[1].ServiceId, MechanicId=employees[1].EmployeeId, ActualHours=1.2m, Price=1600 },
                new RepairJob { RepairOrderId=ro[2].RepairOrderId, ServiceId=services[2].ServiceId, MechanicId=employees[3].EmployeeId, ActualHours=2.8m, Price=2300 }
            };
            context.RepairJobs.AddRange(jobs);
            context.SaveChanges();

            // 10. PAYMENTS
            var payments = new[]
            {
                new Payment {
                    RepairOrderId = ro[0].RepairOrderId,
                    Amount = 900,
                    PaymentDate = new DateTime(2024, 9, 10),
                    PaymentMethod = "Card",
                    InvoiceNumber = "INV-1001",
                    IsRefund = false,
                    Comment = "Paid in advance"
                },
                new Payment {
                    RepairOrderId = ro[1].RepairOrderId,
                    Amount = 600,
                    PaymentDate = new DateTime(2024, 9, 11),
                    PaymentMethod = "Cash",
                    InvoiceNumber = "INV-1002",
                    IsRefund = false,
                    Comment = "Initial payment"
                },
                new Payment {
                    RepairOrderId = ro[2].RepairOrderId,
                    Amount = 1200,
                    PaymentDate = new DateTime(2024, 9, 12),
                    PaymentMethod = "Card",
                    InvoiceNumber = "INV-1003",
                    IsRefund = false,
                    Comment = "Deposit"
                }
            };

            context.Payments.AddRange(payments);
            context.SaveChanges();

        }
    }
}
