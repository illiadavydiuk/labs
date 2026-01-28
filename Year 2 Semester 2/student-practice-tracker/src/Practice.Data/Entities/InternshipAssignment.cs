using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Practice.Data.Entities
{
    public class InternshipAssignment
    {
        [Key]
        public int AssignmentId { get; set; }
        [Required]
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
        [Required]
        public int TopicId { get; set; }
        [ForeignKey("TopicId")]
        public virtual InternshipTopic InternshipTopic { get; set; }
        public int? SupervisorId { get; set; }
        [ForeignKey("SupervisorId")]
        public virtual Supervisor Supervisor { get; set; }
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public virtual AssignmentStatus AssignmentStatus { get; set; }
        [MaxLength(5000)]
        public string? IndividualTask { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? FinalGrade { get; set; }

        public virtual ICollection<Report> Reports { get; set; }
    }
}
