using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Practice.Data.Entities
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual StudentGroup StudentGroup { get; set; }
        [MaxLength(50)]
        public string RecordBookNumber { get; set; }

        public virtual ICollection<InternshipAssignment> InternshipAssignments { get; set; }
        public virtual ICollection<CourseEnrollment> CourseEnrollments { get; set; }
    }
}
