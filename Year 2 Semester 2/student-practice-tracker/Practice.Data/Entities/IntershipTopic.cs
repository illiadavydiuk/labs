using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Practice.Data.Entities
{
    public class IntershipTopic
    {
        [Key]
        public int topic_id { get; set; }
        public int organization_id { get; set; }
        [ForeignKey("organization_id")]
        public virtual Organization Organization { get; set; }

        [Required, StringLength(1000)]
        public string title { get; set; }
        public string description { get; set; }
        public bool is_available { get; set; }
    }
}
