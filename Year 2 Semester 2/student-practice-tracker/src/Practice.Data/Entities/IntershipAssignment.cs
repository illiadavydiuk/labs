using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Practice.Data.Entities
{
    public class IntershipAssignment
    {
        [Key]
        public int assignment_id { get; set; }
        public int student_id { get; set; }
        [ForeignKey("student_id")]
        public virtual Student Student { get; set; }
        public int topic_id { get; set; }
        [ForeignKey("topic_id")]
        public virtual IntershipTopic IntershipTopic { get; set; }
        public int supervisor_id { get; set; }
        [ForeignKey("supervisor_id")]
        public virtual Supervisor Supervisor { get; set; }
        public int status_id { get; set; }
        [ForeignKey("status_id")]
        public virtual AssignmentStatus AssignmentStatus { get; set; }
        public string individual_task { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public int final_grade { get; set; }

        public virtual ICollection<Report> Reports { get; set; }
    }
}
