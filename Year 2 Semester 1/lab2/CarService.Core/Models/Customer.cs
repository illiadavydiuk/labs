using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarService.Core.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }

        public ICollection<Car> Cars { get; set; } = new List<Car>();
        public ICollection<RepairOrder> RepairOrders { get; set; } = new List<RepairOrder>();
    }
}
