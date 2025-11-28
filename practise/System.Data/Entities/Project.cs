using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Data.Entities
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Developer> Developers { get; set; } = new List<Developer>();
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}