using System.ComponentModel.DataAnnotations;

namespace DuckNet.Data.Entities
{
    public class AdapterProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ProfileName { get; set; } = string.Empty;


        public string ActiveAdaptersData { get; set; } = string.Empty;
    }
}