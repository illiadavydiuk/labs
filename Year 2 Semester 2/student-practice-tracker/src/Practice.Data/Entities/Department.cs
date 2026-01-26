using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.Entities
{
    public class Department
    {
        [Key]
        public int department_id { get; set; }
        [Required]
        public string department_name { get; set; }
    }
}
