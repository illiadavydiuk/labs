using System.ComponentModel.DataAnnotations;

namespace CarService.Core.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        public string ServiceName { get; set; }

        public decimal BasePrice { get; set; }
        public int EstimatedHours { get; set; }
        public string Category { get; set; }
        public ICollection<RepairJob> RepairJobs { get; set; } = new List<RepairJob>();

    }
}
