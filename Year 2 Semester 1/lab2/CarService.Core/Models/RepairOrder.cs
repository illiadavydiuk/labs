using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Core.Models
{
    public class RepairOrder
    {
        [Key]
        public int RepairOrderId { get; set; }

        [Required, MaxLength(17)]
        public string VIN { get; set; }

        [ForeignKey(nameof(VIN))]
        public Car Car { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }

        public int ResponsibleEmployeeId { get; set; }

        [ForeignKey(nameof(ResponsibleEmployeeId))]
        public Employee ResponsibleEmployee { get; set; }

        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public string ComplaintDescription { get; set; }
        public decimal TotalEstimate { get; set; }
        public string DiagnosticSummary { get; set; }

        public ICollection<RepairJob> RepairJobs { get; set; } = new List<RepairJob>();
        public ICollection<PartRequest> PartRequests { get; set; } = new List<PartRequest>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();


    }
}
