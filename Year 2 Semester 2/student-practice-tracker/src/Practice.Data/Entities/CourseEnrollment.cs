using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Practice.Data.Entities
{
    public class CourseEnrollment
    {
        [Key]
        public int EnrollmentId { get; set; }
        public int? GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual StudentGroup StudentGroup { get; set; }
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
        public int? StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
    }
}
