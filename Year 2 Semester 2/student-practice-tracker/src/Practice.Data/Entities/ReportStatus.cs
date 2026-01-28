using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.Entities
{
    public class ReportStatus
    {
        [Key]
        public int StatusId { get; set; }
        [Required, MaxLength(50)]
        public string StatusName { get; set; }

        public virtual ICollection<Report> Reports { get; set; }
    }
}
