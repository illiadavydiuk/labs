using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Core.Models
{
    public class PartRequest
    {
        [Key]
        public int RequestId { get; set; }

        public int RepairOrderId { get; set; }

        [ForeignKey(nameof(RepairOrderId))]
        public RepairOrder RepairOrder { get; set; }

        public int PartId { get; set; }

        [ForeignKey(nameof(PartId))]
        public SparePart SparePart { get; set; }

        public int Quantity { get; set; }
        public string Status { get; set; }
        public bool Urgent { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public decimal PriceAtOrder { get; set; }
        public DateTime? ReceivedDate { get; set; }
    }
}
