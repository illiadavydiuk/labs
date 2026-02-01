using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Practice.Data.Entities
{
    public class InternshipTopic
    {
        [Key]
        public int TopicId { get; set; }
        public int? OrganizationId { get; set; }
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; }
        public int? DisciplineId { get; set; }
        [ForeignKey("DisciplineId")]
        public virtual Discipline Discipline { get; set; }

        [Required, StringLength(1000)]
        public string Title { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }

        public virtual ICollection<InternshipAssignment> InternshipAssignments { get; set; }
    }
}
