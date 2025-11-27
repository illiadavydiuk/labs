using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Core.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        public int? ManagerId { get; set; }

        [ForeignKey(nameof(ManagerId))]
        public Employee? Manager { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        public string Position { get; set; }
        public string Specialization { get; set; }
        public decimal Salary { get; set; }

        public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
        public ICollection<RepairOrder> ResponsibleOrders { get; set; } = new List<RepairOrder>();
        public ICollection<RepairJob> RepairJobs { get; set; } = new List<RepairJob>();

    }
}
