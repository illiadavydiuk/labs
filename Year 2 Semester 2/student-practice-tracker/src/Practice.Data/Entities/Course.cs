using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Practice.Data.Entities
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }
        public int DisciplineId { get; set; }
        [ForeignKey("DisciplineId")]
        public virtual Discipline Discipline { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<InternshipAssignment> IntershipAssignments { get; set; }
        public virtual ICollection<CourseEnrollment> CourseEnrollments { get; set; }
    }
}
