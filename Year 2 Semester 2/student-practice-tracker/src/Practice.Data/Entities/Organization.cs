using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.Entities
{
    public class Organization
    {
        [Key]
        public int organization_id { get; set; }
        [Required, StringLength(100)]
        public string name { get; set; }
        public string address { get; set; }
        public string contact_email { get; set; }
    }
}
