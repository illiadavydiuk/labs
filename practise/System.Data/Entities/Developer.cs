using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Data.Entities
{
    public class Developer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}