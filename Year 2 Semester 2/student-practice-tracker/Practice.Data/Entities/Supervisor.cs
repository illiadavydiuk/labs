using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Practice.Data.Entities
{
    public class Supervisor
    {
        [Key]
        public int supervisor_id { get; set; }
        public int user_id { get; set; }
        [ForeignKey("user_id")]
        public virtual User User { get; set; }

        public int department_id { get; set; }
        [ForeignKey("department_id")]
        public virtual Department Department { get; set; }

        public int position_id { get; set; }
        [ForeignKey("position_id")]
        public virtual Position Position { get; set; }

        public string phone { get; set; }

        public virtual ICollection<IntershipAssignment> IntershipAssignments { get; set; }
    }
}
