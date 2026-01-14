using CarService.Core.Context;
using CarService.Core.Models;
using Microsoft.EntityFrameworkCore;

// Створюємо контекст
using var context = new CarServiceDbContext();

// 1) Простий Where
var expensiveOrders = context.RepairOrders
    .Where(ro => ro.TotalEstimate > 2000)
    .ToList();

Console.WriteLine("=== Repair Orders > 2000 грн ===");
foreach (var r in expensiveOrders)
{
    Console.WriteLine($"{r.RepairOrderId}: {r.VIN} - {r.TotalEstimate} грн");
}
Console.WriteLine();

// 2) FirstOrDefault
var specificCar = context.Cars
    .FirstOrDefault(c => c.Make == "Toyota");

Console.WriteLine("=== First Toyota Car ===");
if (specificCar != null)
{
    Console.WriteLine($"{specificCar.Make} {specificCar.Model} - {specificCar.VIN}");
}
Console.WriteLine();

// 3) Find (пошук по PK)
var firstCustomer = context.Customers.Find(1);

Console.WriteLine("=== Customer with ID = 1 ===");
if (firstCustomer != null)
{
    Console.WriteLine($"{firstCustomer.FirstName} {firstCustomer.LastName}");
}
Console.WriteLine();

// 4) Include навігації
var ordersWithRelations = context.RepairOrders
    .Include(ro => ro.Customer)
    .Include(ro => ro.Car)
    .Include(ro => ro.ResponsibleEmployee)
    .ToList();

Console.WriteLine("=== Repair Orders with Navigation ===");
foreach (var o in ordersWithRelations)
{
    Console.WriteLine($"Order #{o.RepairOrderId}: {o.Customer.FirstName} | {o.Car.Make} {o.Car.Model} | Mechanic: {o.ResponsibleEmployee.LastName}");
}
Console.WriteLine();

// 5) Join/Include у SpareParts
var partsWithSuppliers = context.SpareParts
    .Include(p => p.Supplier)
    .OrderByDescending(p => p.Price)
    .ToList();

Console.WriteLine("=== Spare Parts with Supplier ===");
foreach (var p in partsWithSuppliers)
{
    Console.WriteLine($"{p.PartName} | {p.Supplier.SupplierName} | {p.Price} грн");
}
Console.WriteLine();

// 6) Count + Average

int totalCars = context.Cars.Count();
double avgMileage = context.Cars.Average(c => c.Mileage);

Console.WriteLine("=== Stats ===");
Console.WriteLine($"Cars in system: {totalCars}");
Console.WriteLine($"Average mileage: {avgMileage:F0} km");
Console.WriteLine();

// 7) Сортування по зарплаті
var sortedEmployees = context.Employees
    .OrderByDescending(e => e.Salary)
    .ToList();

Console.WriteLine("=== Employees by Salary DESC ===");
foreach (var e in sortedEmployees)
{
    Console.WriteLine($"{e.FirstName} {e.LastName} — {e.Salary} грн");
}
Console.WriteLine();
