using System.ComponentModel.DataAnnotations;

namespace DuckNet.Data.Entities
{
    public class AdapterProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ProfileName { get; set; } = string.Empty; // "Дім", "Робота"

        public bool IsDhcpEnabled { get; set; }
        public string? StaticIp { get; set; }
        public string? SubnetMask { get; set; }
        public string? Gateway { get; set; }
        public string? Dns { get; set; }
    }
}