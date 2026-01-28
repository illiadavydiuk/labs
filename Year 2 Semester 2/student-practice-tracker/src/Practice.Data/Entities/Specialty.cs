using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.Entities
{
    public class Specialty
    {
        [Key]
        public int SpecialtyId { get; set; }
        [Required]
        [MaxLength(20)]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }

        public virtual ICollection<StudentGroup> StudentGroups { get; set; }
    }
}
