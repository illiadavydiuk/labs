using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.Entities
{
    public class ReportStatus
    {
        [Key]
        public int status_id { get; set; }
        [Required]
        public string status_name { get; set; }
    }
}
