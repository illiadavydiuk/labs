using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.Entities
{
    public class Discipline
    {
        [Key]
        public int DisciplineId { get; set; }
        [Required]
        [MaxLength(150)]
        public string DisciplineName { get; set; }

        public virtual ICollection<Course> Courses { get; set; } 
    }
}
