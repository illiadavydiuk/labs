using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.Entities
{
    public class Organization
    {
        [Key]
        public int OrganizationId { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        public string Type { get; set; }
        public string Address { get; set; }
        [MaxLength(100)]
        public string ContactEmail { get; set; }

        public virtual ICollection<InternshipTopic> InternshipTopics { get; set; }
    }
}
