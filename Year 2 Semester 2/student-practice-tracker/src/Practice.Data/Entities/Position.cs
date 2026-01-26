using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.Entities
{
    public class Position
    {
        [Key]
        public int position_id { get; set; }
        [Required]
        public string position_name { get; set; }
    }
}
