using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Core.Models
{
    public class RepairJob
    {
        [Key]
        public int RepairJobId { get; set; }

        // FK → RepairOrder
        public int RepairOrderId { get; set; }
        [ForeignKey(nameof(RepairOrderId))]
        public RepairOrder RepairOrder { get; set; }

        // FK → Service
        public int ServiceId { get; set; }
        [ForeignKey(nameof(ServiceId))]
        public Service Service { get; set; }

        // FK → Employee (mechanic)
        public int MechanicId { get; set; }
        [ForeignKey(nameof(MechanicId))]
        public Employee Mechanic { get; set; }

        public decimal ActualHours { get; set; }
        public decimal Price { get; set; }
    }
}
