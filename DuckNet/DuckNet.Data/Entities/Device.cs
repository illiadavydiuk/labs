using System.ComponentModel.DataAnnotations;

namespace DuckNet.Data.Entities
{
    public class Device
    {
        [Key]
        public int Id { get; set; }

        public string IpAddress { get; set; } = string.Empty; // 192.168.1.105
        public string MacAddress { get; set; } = string.Empty; // AA:BB:CC:DD:EE:FF
        public string Hostname { get; set; } = string.Empty;  // DESKTOP-USER

        public bool IsOnline { get; set; }
        public DateTime LastSeen { get; set; }

        // Чи це відомий нам пристрій (наприклад, "Мій телефон")
        public string? CustomName { get; set; }
        public bool IsTrusted { get; set; }

        public virtual ICollection<NetworkEvent> Events { get; set; } = new List<NetworkEvent>();
    }
}