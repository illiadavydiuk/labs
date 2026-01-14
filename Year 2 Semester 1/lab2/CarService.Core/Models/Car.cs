using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Core.Models
{
    public class Car
    {
        [Key]
        [MaxLength(17)]
        public string VIN { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }

        [Required, MaxLength(40)]
        public string Make { get; set; }

        [Required, MaxLength(40)]
        public string Model { get; set; }

        public int Year { get; set; }
        public string Engine { get; set; }
        public int Mileage { get; set; }

        public ICollection<RepairOrder> RepairOrders { get; set; } = new List<RepairOrder>();
    }
}
