using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.Entities
{
    public class Role
    {
        [Key]
        public int role_id { get; set; }
        [Required, StringLength(50)]
        public string role_name { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
