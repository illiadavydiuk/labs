using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Practice.Data.Entities
{
    public class StudentGroup
    {
        [Key]
        public int GroupId { get; set; }
        public int SpecialtyId { get; set; }
        [ForeignKey("SpecialtyId")]
        public virtual Specialty Specialty { get; set; }
        
        public string GroupCode { get; set; }
        public int EntryYear { get; set; }

        public virtual ICollection<Student> Students { get; set; }
    }
}
