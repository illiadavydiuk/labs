using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.Entities
{
    public class Specialty
    {
        [Key]
        public int specialty_id { get; set; }
        [Required]
        public string code { get; set; }
        [Required]
        public string name { get; set; }

        public virtual ICollection<StudentGroup> StudentGroups { get; set; }
    }
}
