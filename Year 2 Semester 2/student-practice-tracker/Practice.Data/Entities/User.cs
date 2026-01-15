using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Practice.Data.Entities
{
    public class User
    {
        [Key]
        public int user_id { get; set; }

        public int role_id { get; set; }
        [ForeignKey("role_id")]
        public virtual Role Role { get; set; }

        [Required, EmailAddress]
        public string email { get; set; }
        [Required]
        public string password_hash { get; set; }
        [Required]
        public string first_name { get; set; }
        [Required]
        public string last_name { get; set; }
        public DateTime created_at { get; set; } = DateTime.UtcNow;

        public virtual Student Student { get; set; }
        public virtual Supervisor Supervisor { get; set; }
        public virtual ICollection<AuditLog> AuditLogs { get; set; }
    }
}
