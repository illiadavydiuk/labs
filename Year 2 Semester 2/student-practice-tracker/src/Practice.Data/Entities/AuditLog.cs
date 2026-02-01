using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Practice.Data.Entities
{
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string EntityAffected { get; set; }
        public int? EntityId { get; set; }
        public string? EntityName { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    }
}
