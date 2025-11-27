using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Core.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int RepairOrderId { get; set; }

        [ForeignKey(nameof(RepairOrderId))]
        public RepairOrder RepairOrder { get; set; }


        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Comment { get; set; }
        public string InvoiceNumber { get; set; }
        public bool IsRefund { get; set; }
    }
}
