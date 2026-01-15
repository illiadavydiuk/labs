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
        public int student_id { get; set; }
        public int user_id { get; set; }
        [ForeignKey("user_id")]
        public virtual User User { get; set; }
        public int group_id { get; set; }
        [ForeignKey("group_id")]
        public virtual StudentGroup StudentGroup { get; set; }
        public string record_book_number { get; set; }

        public virtual ICollection<IntershipAssignment> IntershipAssignments { get; set; }
    }
}
