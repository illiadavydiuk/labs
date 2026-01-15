using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Practice.Data.Entities
{
    public class StudentGroup
    {
        [Key]
        public int group_id { get; set; }
        public int specialty_id { get; set; }
        [ForeignKey("specialty_id")]
        public virtual Specialty Specialty { get; set; }

        public string group_code { get; set; }
        public int entry_year { get; set; }

        public virtual ICollection<Student> Students { get; set; }
    }
}
