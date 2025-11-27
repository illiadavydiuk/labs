using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Core.Models
{
    public class SparePart
    {
        [Key]
        public int PartId { get; set; }

        public int SupplierId { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public Supplier Supplier { get; set; }

        [Required, MaxLength(100)]
        public string PartName { get; set; }

        [MaxLength(50)]
        public string PartNumber { get; set; }

        public string Quality { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumStock { get; set; }
        public decimal Price { get; set; }
        public ICollection<PartRequest> PartRequests { get; set; } = new List<PartRequest>();

    }
}
