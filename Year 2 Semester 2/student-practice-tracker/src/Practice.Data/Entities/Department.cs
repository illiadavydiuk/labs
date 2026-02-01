using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Practice.Data.Entities
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }
        [Required]
        [MaxLength(200)]
        public string DepartmentName { get; set; }

        public virtual ICollection<Supervisor> Supervisors { get; set; }
        public virtual ICollection<Specialty> Specialties { get; set; }
    }
}
