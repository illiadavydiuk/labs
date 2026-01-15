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
        public int log_id { get; set; }
        public int user_id { get; set; }
        [ForeignKey("user_id")]
        public virtual User User { get; set; }

        public string action { get; set; }
        public string entity_affected { get; set; }
        public int entity_id { get; set; }
        public DateTime timestamp { get; set; } = DateTime.UtcNow;
    }
}
