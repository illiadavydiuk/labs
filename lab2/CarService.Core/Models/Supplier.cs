using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarService.Core.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required, MaxLength(100)]
        public string SupplierName { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        public ICollection<SparePart> SpareParts { get; set; } = new List<SparePart>();
    }
}
