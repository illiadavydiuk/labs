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
        public int report_id { get; set; }
        public int assignment_id { get; set; }
        [ForeignKey("assignment_id")]
        public virtual IntershipAssignment IntershipAssignment { get; set; }
        public int status_id { get; set; }
        [ForeignKey("status_id")]
        public virtual ReportStatus ReportStatus { get; set; }
        public string report_file_url { get; set; }// text report
        public string work_archive_url { get; set; } // file with all work
        public string student_comment { get; set; }
        public string supervisor_feedback { get; set; }
        public DateTime submission_date { get; set; } = DateTime.UtcNow;
        public DateTime? review_date { get; set; }
    }
}
