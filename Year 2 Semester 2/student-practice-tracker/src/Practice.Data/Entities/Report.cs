using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Practice.Data.Entities
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }
        [Required]
        public int AssignmentId { get; set; }
        [ForeignKey("AssignmentId")]
        public virtual InternshipAssignment IntershipAssignment { get; set; }
        [Required]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public virtual ReportStatus ReportStatus { get; set; }
        public string? StudentComment { get; set; }
        public string SupervisorFeedback { get; set; }
        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewDate { get; set; }

        public virtual ICollection<Attachment> Attachments { get; set; }
    }
}
