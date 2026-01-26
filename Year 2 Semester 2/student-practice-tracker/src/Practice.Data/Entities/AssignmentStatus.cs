using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.Entities
{
    public class AssignmentStatus
    {
        [Key]
        public int status_id { get; set;}
        public string status_name { get; set; }
    }
}
